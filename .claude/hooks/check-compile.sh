#!/usr/bin/env bash
# Stop hook: ajan "bitti" demeden once C# derlemesinin gercekten gectigini dogrular.
#
# - Calisma agacinda .cs / .asmdef degisikligi yoksa hicbir sey yapmaz.
# - Acik bir Unity Editor'e (Pipeline) ulasilamiyorsa ENGELLEMEZ, sadece uyarir.
# - Derleme hatasi varsa exit 2 ile Claude'un durmasini engeller ve hatalari stderr'e yazar.
# - En fazla MAX_BLOCKS kez engeller (sonsuz dongu korumasi).
#
# Not: derleyici hatalari Pipeline'in konsol tamponuna dusmez; Editor.log'dan okunur.

cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0

MAX_BLOCKS=3
COUNTER_FILE="${TMPDIR:-/tmp}/rockthegame-compile-gate.count"
EDITOR_LOG="${LOCALAPPDATA:-$HOME/AppData/Local}/Unity/Editor/Editor.log"

# Sadece C# / asmdef degisti mi?
changed=$(git status --porcelain 2>/dev/null | grep -E '\.(cs|asmdef)$' || true)
if [ -z "$changed" ]; then
  rm -f "$COUNTER_FILE"
  exit 0
fi

# Editor erisilebilir mi?
if ! unity status 2>/dev/null | grep -q "ready"; then
  echo "[compile-gate] Unity Editor'e ulasilamadi; derleme dogrulanmadi." >&2
  exit 0
fi

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
  count=$(cat "$COUNTER_FILE" 2>/dev/null || echo 0)
  count=$((count + 1))
  echo "$count" > "$COUNTER_FILE"
  if [ "$count" -gt "$MAX_BLOCKS" ]; then
    echo "[compile-gate] Derleme hala basarisiz ama $MAX_BLOCKS denemeden sonra engelleme birakildi. Kullaniciya bildir." >&2
    rm -f "$COUNTER_FILE"
    exit 0
  fi
  {
    echo "[compile-gate] C# derlemesi BASARISIZ. Bitti demeden once duzelt."
    if [ -f "$EDITOR_LOG" ]; then
      # Derlemeden onceki log konumundan sonrasi; yoksa (log dondurulduyse) son hatalar
      new_errors=$(tail -n +"$((log_lines_before + 1))" "$EDITOR_LOG" | grep -a "error CS" | sort -u | head -15)
      [ -z "$new_errors" ] && new_errors=$(grep -a "error CS" "$EDITOR_LOG" | tail -15 | sort -u)
      echo "$new_errors"
    fi
  } >&2
  exit 2
fi

rm -f "$COUNTER_FILE"
case "$state" in
  *completed*|*up_to_date*) ;;
  *) echo "[compile-gate] Derleme durumu belirsiz ($state); dogrulanamadi." >&2 ;;
esac
exit 0
