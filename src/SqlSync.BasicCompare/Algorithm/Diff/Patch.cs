namespace Algorithm.Diff
{
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
            return hunks.GetEnumerator();
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
                leftstart = st;
                leftcount = c;
                rightstart = rs;
                rightcount = rc;
                same = s;
            }

            public int Count
            {
                get
                {
                    return leftcount;
                }
            }

            public int End
            {
                get
                {
                    return ((leftstart + leftcount) - 1);
                }
            }

            public IList Right
            {
                get
                {
                    if (same)
                    {
                        return null;
                    }
                    return new Algorithm.Diff.Range(rightData, rightstart, rightcount);
                }
            }

            public bool Same
            {
                get
                {
                    return same;
                }
            }

            public int Start
            {
                get
                {
                    return leftstart;
                }
            }
        }
    }
}
