#!/usr/bin/env bash
# Stop hook: ajan "bitti" demeden once C# derlemesinin VE EditMode testlerinin gectigini dogrular.
#
# - .cs / .asmdef iceriklerinin parmak izi, son basarili dogrulamayla ayniysa hicbir sey yapmaz
#   (ajan commit atsa bile kapi atlanmaz; git status'a bagli degildir).
# - Acik bir Unity Editor'e (Pipeline) ulasilamiyorsa ENGELLEMEZ, sadece uyarir.
# - Derleme veya test hatasinda exit 2 ile Claude'un durmasini engeller ve hatalari stderr'e yazar.
# - En fazla MAX_BLOCKS kez engeller (sonsuz dongu korumasi).
#
# Not: derleyici hatalari Pipeline'in konsol tamponuna dusmez; Editor.log'dan okunur.

cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0

MAX_BLOCKS=3
STATE_DIR="${TMPDIR:-/tmp}"
COUNTER_FILE="$STATE_DIR/rockthegame-compile-gate.count"
VERIFIED_FILE="$STATE_DIR/rockthegame-compile-gate.verified"
EDITOR_LOG="${LOCALAPPDATA:-$HOME/AppData/Local}/Unity/Editor/Editor.log"

# Izlenen + izlenmeyen (ignore edilmeyen) tum .cs/.asmdef iceriklerinin parmak izi
fingerprint=$(git ls-files -co --exclude-standard -- '*.cs' '*.asmdef' 2>/dev/null | sort | xargs -d '\n' sha1sum 2>/dev/null | sha1sum | cut -d' ' -f1)

if [ -n "$fingerprint" ] && [ "$fingerprint" = "$(cat "$VERIFIED_FILE" 2>/dev/null)" ]; then
  exit 0
fi

# Editor erisilebilir mi?
if ! unity status 2>/dev/null | grep -q "ready"; then
  echo "[compile-gate] Unity Editor'e ulasilamadi; derleme/test dogrulanmadi." >&2
  exit 0
fi

block() {
  count=$(cat "$COUNTER_FILE" 2>/dev/null || echo 0)
  count=$((count + 1))
  echo "$count" > "$COUNTER_FILE"
  if [ "$count" -gt "$MAX_BLOCKS" ]; then
    echo "[compile-gate] Hata suruyor ama $MAX_BLOCKS denemeden sonra engelleme birakildi. Kullaniciya bildir." >&2
    rm -f "$COUNTER_FILE"
    exit 0
  fi
  exit 2
}

log_lines_before=0
[ -f "$EDITOR_LOG" ] && log_lines_before=$(wc -l < "$EDITOR_LOG")

unity command recompile >/dev/null 2>&1

state=""
for _ in $(seq 1 45); do
  state=$(unity command recompile_status 2>/dev/null | tail -1)
  case "$state" in
    *completed*|*up_to_date*) break ;;
  esac
  sleep 2
done

status_json=$(unity command console_status 2>/dev/null | tail -1)
if echo "$status_json" | grep -q '"compilationFailed":true'; then
  {
    echo "[compile-gate] C# derlemesi BASARISIZ. Bitti demeden once duzelt."
    if [ -f "$EDITOR_LOG" ]; then
      new_errors=$(tail -n +"$((log_lines_before + 1))" "$EDITOR_LOG" | grep -a "error CS" | sort -u | head -15)
      [ -z "$new_errors" ] && new_errors=$(grep -a "error CS" "$EDITOR_LOG" | tail -15 | sort -u)
      echo "$new_errors"
    fi
  } >&2
  block
fi

case "$state" in
  *completed*|*up_to_date*) ;;
  *) echo "[compile-gate] Derleme durumu belirsiz ($state); dogrulanamadi." >&2; exit 0 ;;
esac

# Derleme temiz: EditMode testlerini kos
test_line=$(unity command run_tests --mode EditMode --timeout 110 2>&1 | tail -1)
if echo "$test_line" | grep -qE '[0-9]+ failed'; then
  {
    echo "[compile-gate] EditMode testleri BASARISIZ: $(echo "$test_line" | cut -f3)"
    echo "Hangi test: 'unity command list_tests --mode EditMode' ile adlari al, 'unity command run_tests --mode EditMode --filter <TamAd>' ile tek tek kos."
  } >&2
  block
fi

rm -f "$COUNTER_FILE"
[ -n "$fingerprint" ] && echo "$fingerprint" > "$VERIFIED_FILE"
exit 0
