namespace Algorithm.Diff
{
    using System;

    public abstract class Hunk
    {
        internal Hunk()
        {
        }

        public abstract Range Changes(int index);
        public abstract bool IsSame(int index);
        public int MaxLines()
        {
            int count = this.Original().Count;
            for (int i = 0; i < this.ChangedLists; i++)
            {
                if (this.Changes(i).Count > count)
                {
                    count = this.Changes(i).Count;
                }
            }
            return count;
        }

        public abstract Range Original();

        public abstract int ChangedLists { get; }

        public abstract bool Conflict { get; }

        public abstract bool Same { get; }
    }
}
