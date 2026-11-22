namespace New_Scripts.Flower
{
    /// <summary>
    /// Açılma değerinin zaman içindeki geçmişi: her yaprak <see cref="Sample"/> ile kendi gecikmesi kadar eski değeri okur.
    /// Açılış yayı yaprak ilerlemesinin %50'sini ~17 ms'de geçtiği için açılma-uzayındaki sıralama (indexStagger) gözle
    /// görülmez; yaprakları zamanda kaydırmak sırayı görünür yapar. Halka tampon, ayırma yok.
    /// </summary>
    public sealed class FlowerOpenDelayLine
    {
        private readonly float[] _time;
        private readonly float[] _value;
        private int _head = -1;     // en yeni örnek
        private int _count;

        public FlowerOpenDelayLine(int capacity = 128)
        {
            _time = new float[capacity];
            _value = new float[capacity];
        }

        /// <summary>Geçmişi siler; her gecikmede <paramref name="value"/> okunur.</summary>
        public void Reset(float time, float value)
        {
            _head = -1;
            _count = 0;
            Push(time, value);
        }

        /// <summary>Yeni örnek ekler; zaman azalmaz (aynı zamanda eklenen son değeri ezer).</summary>
        public void Push(float time, float value)
        {
            if (_count > 0 && time <= _time[_head])
            {
                _value[_head] = value;
                return;
            }
            _head = (_head + 1) % _time.Length;
            _time[_head] = time;
            _value[_head] = value;
            if (_count < _time.Length) _count++;
        }

        /// <summary><paramref name="time"/> anındaki değer (örnekler arası doğrusal). En eskiden öncesi en eski, en yeniden sonrası en yeni.</summary>
        public float Sample(float time)
        {
            if (_count == 0) return 0f;
            int newest = _head;
            if (time >= _time[newest]) return _value[newest];

            int cap = _time.Length;
            int next = newest;
            for (int i = 1; i < _count; i++)
            {
                int prev = (newest - i + cap) % cap;
                if (time >= _time[prev])
                {
                    float span = _time[next] - _time[prev];
                    float k = span > 1e-6f ? (time - _time[prev]) / span : 1f;
                    return _value[prev] + (_value[next] - _value[prev]) * k;
                }
                next = prev;
            }
            return _value[next];   // en eskiden de önce
        }
    }
}
