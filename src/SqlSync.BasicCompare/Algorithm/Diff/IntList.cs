namespace Algorithm.Diff
{
    using System;

    internal class IntList
    {
        private int[] _items = new int[0x10];
        private int _size;
        private const int DefaultInitialCapacity = 0x10;

        public int Add(int value)
        {
            if (_items.Length <= _size)
            {
                EnsureCapacity(_size + 1);
            }
            _items[_size] = value;
            return _size++;
        }

        public virtual void Clear()
        {
            Array.Clear(_items, 0, _size);
            _size = 0;
        }

        private void EnsureCapacity(int count)
        {
            if (count > _items.Length)
            {
                int num = _items.Length << 1;
                if (num == 0)
                {
                    num = 0x10;
                }
                while (num < count)
                {
                    num = num << 1;
                }
                int[] destinationArray = new int[num];
                Array.Copy(_items, 0, destinationArray, 0, _items.Length);
                _items = destinationArray;
            }
        }

        public virtual void RemoveAt(int index)
        {
            if ((index < 0) || (index >= _size))
            {
                throw new ArgumentOutOfRangeException("index", index, "Less than 0 or more than list count.");
            }
            Shift(index, -1);
            _size--;
        }

        public void Reverse()
        {
            for (int i = 0; i <= (Count / 2); i++)
            {
                int num2 = this[i];
                this[i] = this[(Count - i) - 1];
                this[(Count - i) - 1] = num2;
            }
        }

        private void Shift(int index, int count)
        {
            if (count > 0)
            {
                if ((_size + count) > _items.Length)
                {
                    int num = (_items.Length > 0) ? (_items.Length << 1) : 1;
                    while (num < (_size + count))
                    {
                        num = num << 1;
                    }
                    int[] destinationArray = new int[num];
                    Array.Copy(_items, 0, destinationArray, 0, index);
                    Array.Copy(_items, index, destinationArray, index + count, _size - index);
                    _items = destinationArray;
                }
                else
                {
                    Array.Copy(_items, index, _items, index + count, _size - index);
                }
            }
            else if (count < 0)
            {
                int sourceIndex = index - count;
                Array.Copy(_items, sourceIndex, _items, index, _size - sourceIndex);
            }
        }

        public int Count
        {
            get
            {
                return _size;
            }
        }

        public int this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }
    }
}
