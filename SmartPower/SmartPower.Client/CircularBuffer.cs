namespace SmartPower.Client
{
    public class CircularBuffer
    {
        private int[] _buffer;
        private int _head;
        private int _capacity;
        private int _count;

        public CircularBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new int[capacity];
            _head = 0;
            _count = 0;
        }

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                int actualIndex = (_head - _count + index + _capacity) % _capacity;
                return _buffer[actualIndex];
            }
        }

        public void Enqueue(int sample)
        {
            _buffer[_head] = sample;
            _head = (_head + 1) % _capacity;
            if (_count < _capacity)
            {
                _count++;
            }
        }

        public void EnqueueRange(List<int> samples)
        {
            foreach (var sample in samples)
            {
                Enqueue(sample);
            }
        }

        public int Count => _count;
    }
}