namespace Algorithm.Diff
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class Range : IList, ICollection, IEnumerable
    {
        private int count;
        private static ArrayList EmptyList = new ArrayList();
        private IList list;
        private int start;

        public Range(IList list, int start, int count)
        {
            this.list = list;
            this.start = start;
            this.count = count;
        }

        private void Check()
        {
            if ((this.count > 0) && (this.list == null))
            {
                throw new InvalidOperationException("This range does not refer to a list with data.");
            }
        }

        public bool Contains(object obj)
        {
            return (this.IndexOf(obj) != -1);
        }

        public int IndexOf(object obj)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (obj.Equals(this[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.Check();
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], (int) (i + index));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if ((this.count == 0) && (this.list == null))
            {
                return EmptyList.GetEnumerator();
            }
            this.Check();
            return new Enumer(this);
        }

        int IList.Add(object obj)
        {
            throw new InvalidOperationException();
        }

        void IList.Clear()
        {
            throw new InvalidOperationException();
        }

        void IList.Insert(int index, object obj)
        {
            throw new InvalidOperationException();
        }

        void IList.Remove(object obj)
        {
            throw new InvalidOperationException();
        }

        void IList.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public int End
        {
            get
            {
                return ((this.start + this.count) - 1);
            }
        }

        public object this[int index]
        {
            get
            {
                this.Check();
                if ((index < 0) || (index >= this.count))
                {
                    throw new ArgumentException("index");
                }
                return this.list[index + this.start];
            }
        }

        public int Start
        {
            get
            {
                return this.start;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        private class Enumer : IEnumerator
        {
            private int index = -1;
            private Algorithm.Diff.Range list;

            public Enumer(Algorithm.Diff.Range list)
            {
                this.list = list;
            }

            public bool MoveNext()
            {
                this.index++;
                return (this.index < this.list.Count);
            }

            public void Reset()
            {
                this.index = -1;
            }

            public object Current
            {
                get
                {
                    return this.list[this.index];
                }
            }
        }
    }
}
