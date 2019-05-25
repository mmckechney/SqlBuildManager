namespace Algorithm.Diff
{
    using System;
    using System.Collections;

    public class Patch : IEnumerable
    {
        private Hunk[] hunks;

        internal Patch(Hunk[] hunks)
        {
            this.hunks = hunks;
        }

        public IList Apply(IList original)
        {
            ArrayList list = new ArrayList();
            foreach (Hunk hunk in this)
            {
                if (hunk.Same)
                {
                    list.AddRange(new Algorithm.Diff.Range(original, hunk.Start, hunk.Count));
                    continue;
                }
                list.AddRange(hunk.Right);
            }
            return list;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.hunks.GetEnumerator();
        }

        public class Hunk
        {
            private int leftcount;
            private int leftstart;
            private int rightcount;
            private object[] rightData;
            private int rightstart;
            private bool same;

            internal Hunk(object[] rightData, int st, int c, int rs, int rc, bool s)
            {
                this.rightData = rightData;
                this.leftstart = st;
                this.leftcount = c;
                this.rightstart = rs;
                this.rightcount = rc;
                this.same = s;
            }

            public int Count
            {
                get
                {
                    return this.leftcount;
                }
            }

            public int End
            {
                get
                {
                    return ((this.leftstart + this.leftcount) - 1);
                }
            }

            public IList Right
            {
                get
                {
                    if (this.same)
                    {
                        return null;
                    }
                    return new Algorithm.Diff.Range(this.rightData, this.rightstart, this.rightcount);
                }
            }

            public bool Same
            {
                get
                {
                    return this.same;
                }
            }

            public int Start
            {
                get
                {
                    return this.leftstart;
                }
            }
        }
    }
}
