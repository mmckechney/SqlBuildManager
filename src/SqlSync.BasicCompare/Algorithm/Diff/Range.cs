namespace Algorithm.Diff
{
    using System;
    using System.Collections;

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
            if ((count > 0) && (list == null))
            {
                throw new InvalidOperationException("This range does not refer to a list with data.");
            }
        }

        public bool Contains(object obj)
        {
            return (IndexOf(obj) != -1);
        }

        public int IndexOf(object obj)
        {
            for (int i = 0; i < Count; i++)
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
            Check();
            for (int i = 0; i < Count; i++)
            {
                array.SetValue(this[i], (int)(i + index));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if ((count == 0) && (list == null))
            {
                return EmptyList.GetEnumerator();
            }
            Check();
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
                return count;
            }
        }

        public int End
        {
            get
            {
                return ((start + count) - 1);
            }
        }

        public object this[int index]
        {
            get
            {
                Check();
                if ((index < 0) || (index >= count))
                {
                    throw new ArgumentException("index");
                }
                return list[index + start];
            }
        }

        public int Start
        {
            get
            {
                return start;
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
                index++;
                return (index < list.Count);
            }

            public void Reset()
            {
                index = -1;
            }

            public object Current
            {
                get
                {
                    return list[index];
                }
            }
        }
    }
}
