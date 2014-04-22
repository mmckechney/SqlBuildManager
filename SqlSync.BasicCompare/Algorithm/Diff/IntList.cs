namespace Algorithm.Diff
{
    using System;
    using System.Reflection;

    internal class IntList
    {
        private int[] _items = new int[0x10];
        private int _size;
        private const int DefaultInitialCapacity = 0x10;

        public int Add(int value)
        {
            if (this._items.Length <= this._size)
            {
                this.EnsureCapacity(this._size + 1);
            }
            this._items[this._size] = value;
            return this._size++;
        }

        public virtual void Clear()
        {
            Array.Clear(this._items, 0, this._size);
            this._size = 0;
        }

        private void EnsureCapacity(int count)
        {
            if (count > this._items.Length)
            {
                int num = this._items.Length << 1;
                if (num == 0)
                {
                    num = 0x10;
                }
                while (num < count)
                {
                    num = num << 1;
                }
                int[] destinationArray = new int[num];
                Array.Copy(this._items, 0, destinationArray, 0, this._items.Length);
                this._items = destinationArray;
            }
        }

        public virtual void RemoveAt(int index)
        {
            if ((index < 0) || (index >= this._size))
            {
                throw new ArgumentOutOfRangeException("index", index, "Less than 0 or more than list count.");
            }
            this.Shift(index, -1);
            this._size--;
        }

        public void Reverse()
        {
            for (int i = 0; i <= (this.Count / 2); i++)
            {
                int num2 = this[i];
                this[i] = this[(this.Count - i) - 1];
                this[(this.Count - i) - 1] = num2;
            }
        }

        private void Shift(int index, int count)
        {
            if (count > 0)
            {
                if ((this._size + count) > this._items.Length)
                {
                    int num = (this._items.Length > 0) ? (this._items.Length << 1) : 1;
                    while (num < (this._size + count))
                    {
                        num = num << 1;
                    }
                    int[] destinationArray = new int[num];
                    Array.Copy(this._items, 0, destinationArray, 0, index);
                    Array.Copy(this._items, index, destinationArray, index + count, this._size - index);
                    this._items = destinationArray;
                }
                else
                {
                    Array.Copy(this._items, index, this._items, index + count, this._size - index);
                }
            }
            else if (count < 0)
            {
                int sourceIndex = index - count;
                Array.Copy(this._items, sourceIndex, this._items, index, this._size - sourceIndex);
            }
        }

        public int Count
        {
            get
            {
                return this._size;
            }
        }

        public int this[int index]
        {
            get
            {
                return this._items[index];
            }
            set
            {
                this._items[index] = value;
            }
        }
    }
}
