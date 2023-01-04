namespace Algorithm.Diff
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public class Diff : IDiff, IEnumerable
    {
        private int _End;
        private bool _Same;
        private IntList cdif;
        //private IComparer comparer;
        private IEqualityComparer hashcoder;
        internal IList left;
        internal IList leftRaw;
        internal IList right;
        internal IList rightRaw;

        public Diff(IList left, IList right, IEqualityComparer hashcoder)
        {
            cdif = null;
            this.left = left;
            this.right = right;
            // this.comparer = comparer;
            this.hashcoder = hashcoder;
            init();
        }

        public Diff(string leftFile, string rightFile, bool caseSensitive, bool compareWhitespace) :
            this(UnifiedDiff.LoadFileLines(leftFile), UnifiedDiff.LoadFileLines(rightFile), caseSensitive, compareWhitespace)
        {
        }

        public Diff(string[] left, string[] right, bool caseSensitive, bool compareWhitespace) :
            this(StripWhitespace(left, !compareWhitespace), StripWhitespace(right, !compareWhitespace), caseSensitive ? (IEqualityComparer)StringComparer.Create(CultureInfo.InvariantCulture, false) : (IEqualityComparer)StringComparer.Create(CultureInfo.InvariantCulture, true))
        {
            leftRaw = left;
            rightRaw = right;
        }

        private IntList _longestCommonSubsequence(IList a, IList b)
        {
            Hashtable bMatches;
            int num = 0;
            int num2 = a.Count - 1;
            IntList list = new IntList();
            for (int i = 0; i < a.Count; i++)
            {
                list.Add(-1);
            }
            if (!IsPrepared(out bMatches))
            {
                int start = 0;
                int end = b.Count - 1;
                while (((num <= num2) && (start <= end)) && compare(a[num], b[start]))
                {
                    list[num++] = start++;
                }
                while (((num <= num2) && (start <= end)) && compare(a[num2], b[end]))
                {
                    list[num2--] = end--;
                }
                bMatches = _withPositionsOfInInterval(b, start, end);
            }
            IntList array = new IntList();
            ArrayList list3 = new ArrayList();
            for (int j = num; j <= num2; j++)
            {
                IntList list4 = (IntList)bMatches[a[j]];
                if (list4 != null)
                {
                    int high = 0;
                    for (int k = 0; k < list4.Count; k++)
                    {
                        int num9 = list4[k];
                        if (((high > 0) && (array[high] > num9)) && (array[high - 1] < num9))
                        {
                            array[high] = num9;
                        }
                        else
                        {
                            high = _replaceNextLargerWith(array, num9, high);
                        }
                        if (high != -1)
                        {
                            Trio trio = new Trio((high > 0) ? ((Trio)list3[high - 1]) : null, j, num9);
                            if (high == list3.Count)
                            {
                                list3.Add(trio);
                            }
                            else
                            {
                                list3[high] = trio;
                            }
                        }
                    }
                }
            }
            if (array.Count > 0)
            {
                for (Trio trio2 = (Trio)list3[array.Count - 1]; trio2 != null; trio2 = trio2.a)
                {
                    list[trio2.b] = trio2.c;
                }
            }
            return list;
        }

        private int _replaceNextLargerWith(IntList array, int value, int high)
        {
            if (high <= 0)
            {
                high = array.Count - 1;
            }
            if ((high == -1) || (value > array[array.Count - 1]))
            {
                array.Add(value);
                return (array.Count - 1);
            }
            int num = 0;
            while (num <= high)
            {
                int num2 = (high + num) / 2;
                int num3 = array[num2];
                if (value == num3)
                {
                    return -1;
                }
                if (value > num3)
                {
                    num = num2 + 1;
                }
                else
                {
                    high = num2 - 1;
                }
            }
            array[num] = value;
            return num;
        }

        private Hashtable _withPositionsOfInInterval(IList aCollection, int start, int end)
        {
            Hashtable hashtable = new Hashtable(hashcoder);
            for (int i = start; i <= end; i++)
            {
                object key = aCollection[i];
                if (hashtable.ContainsKey(key))
                {
                    ((IntList)hashtable[key]).Add(i);
                }
                else
                {
                    IntList list2 = new IntList();
                    list2.Add(i);
                    hashtable[key] = list2;
                }
            }
            foreach (IntList list3 in hashtable.Values)
            {
                list3.Reverse();
            }
            return hashtable;
        }

        private IntList compact_diff(IList a, IList b)
        {
            IntList am;
            IntList bm;
            LCSidx(a, b, out am, out bm);
            IntList list3 = new IntList();
            int num = 0;
            int num2 = 0;
            list3.Add(num);
            list3.Add(num2);
            while (true)
            {
                while (((am.Count > 0) && (num == am[0])) && (num2 == bm[0]))
                {
                    am.RemoveAt(0);
                    bm.RemoveAt(0);
                    num++;
                    num2++;
                }
                list3.Add(num);
                list3.Add(num2);
                if (am.Count == 0)
                {
                    break;
                }
                num = am[0];
                num2 = bm[0];
                list3.Add(num);
                list3.Add(num2);
            }
            if ((num < a.Count) || (num2 < b.Count))
            {
                list3.Add(a.Count);
                list3.Add(b.Count);
            }
            return list3;
        }

        private bool compare(object a, object b)
        {
            return hashcoder.Equals(a, b);
            //if (this.comparer == null)
            //{
            //    return a.Equals(b);
            //}

            //return (this.comparer.Compare(a, b) == 0);
        }

        public Patch CreatePatch()
        {
            int rs = 0;
            foreach (Hunk hunk in this)
            {
                if (!hunk.Same)
                {
                    rs += hunk.Right.Count;
                }
            }
            object[] rightData = new object[rs];
            ArrayList list = new ArrayList();
            rs = 0;
            foreach (Hunk hunk2 in this)
            {
                if (hunk2.Same)
                {
                    list.Add(new Patch.Hunk(rightData, hunk2.Left.Start, hunk2.Left.Count, 0, 0, true));
                    continue;
                }
                list.Add(new Patch.Hunk(rightData, hunk2.Left.Start, hunk2.Left.Count, rs, hunk2.Right.Count, false));
                for (int i = 0; i < hunk2.Right.Count; i++)
                {
                    rightData[rs++] = hunk2.Right[i];
                }
            }
            return new Patch((Patch.Hunk[])list.ToArray(typeof(Patch.Hunk)));
        }

        private void init()
        {
            cdif = compact_diff(left, right);
            _Same = true;
            if ((cdif[2] == 0) && (cdif[3] == 0))
            {
                _Same = false;
                cdif.RemoveAt(0);
                cdif.RemoveAt(0);
            }
            _End = (1 + cdif.Count) / 2;
        }

        private bool IsPrepared(out Hashtable bMatches)
        {
            bMatches = null;
            return false;
        }

        private void LCSidx(IList a, IList b, out IntList am, out IntList bm)
        {
            IntList list = _longestCommonSubsequence(a, b);
            am = new IntList();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != -1)
                {
                    am.Add(i);
                }
            }
            bm = new IntList();
            for (int j = 0; j < am.Count; j++)
            {
                bm.Add(list[am[j]]);
            }
        }

        private static string[] StripWhitespace(string[] lines, bool strip)
        {
            if (!strip)
            {
                return lines;
            }
            string[] textArray = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                StringBuilder builder = new StringBuilder();
                foreach (char ch in lines[i])
                {
                    if (!char.IsWhiteSpace(ch))
                    {
                        builder.Append(ch);
                    }
                }
                textArray[i] = builder.ToString();
            }
            return textArray;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (cdif == null)
            {
                throw new InvalidOperationException("No comparison has been performed.");
            }
            return new Enumerator(this);
        }

        public override string ToString()
        {
            StringWriter writer = new StringWriter();
            UnifiedDiff.WriteUnifiedDiff(this, writer);
            return writer.ToString();
        }

        public IList Left
        {
            get
            {
                return left;
            }
        }

        public IList Right
        {
            get
            {
                return right;
            }
        }

        private class Enumerator : IEnumerator
        {
            private int _Off;
            private int _Pos;
            private Algorithm.Diff.Diff diff;

            public Enumerator(Algorithm.Diff.Diff diff)
            {
                this.diff = diff;
                Reset();
            }

            private void _ChkPos()
            {
                if (_Pos == 0)
                {
                    throw new InvalidOperationException("Position is reset.");
                }
            }

            private Algorithm.Diff.Diff.Hunk gethunk()
            {
                _ChkPos();
                int num5 = 1 + _Off;
                int num6 = 2 + _Off;
                int num = diff.cdif[num5 - 2];
                int num2 = diff.cdif[num5] - 1;
                int num3 = diff.cdif[num6 - 2];
                int num4 = diff.cdif[num6] - 1;
                return new Algorithm.Diff.Diff.Hunk(diff.leftRaw, diff.rightRaw, num, num2, num3, num4, same());
            }

            public bool MoveNext()
            {
                return next();
            }

            private bool next()
            {
                reset(_Pos + 1);
                return (_Pos != -1);
            }

            private void reset(int pos)
            {
                if ((pos < 0) || (diff._End <= pos))
                {
                    pos = -1;
                }
                _Pos = pos;
                _Off = (2 * pos) - 1;
            }

            public void Reset()
            {
                reset(0);
            }

            private bool same()
            {
                _ChkPos();
                if (diff._Same != ((1 & _Pos) != 0))
                {
                    return false;
                }
                return true;
            }

            public object Current
            {
                get
                {
                    _ChkPos();
                    return gethunk();
                }
            }
        }

        public class Hunk : Algorithm.Diff.Hunk
        {
            private IList left;
            private IList right;
            private int s1end;
            private int s1start;
            private int s2end;
            private int s2start;
            private bool same;

            internal Hunk(IList left, IList right, int s1start, int s1end, int s2start, int s2end, bool same)
            {
                this.left = left;
                this.right = right;
                this.s1start = s1start;
                this.s1end = s1end;
                this.s2start = s2start;
                this.s2end = s2end;
                this.same = same;
            }

            public override Algorithm.Diff.Range Changes(int index)
            {
                if (index != 0)
                {
                    throw new ArgumentException();
                }
                return Right;
            }

            internal Algorithm.Diff.Diff.Hunk Crop(int shiftstart, int shiftend)
            {
                return new Algorithm.Diff.Diff.Hunk(left, right, Left.Start + shiftstart, Left.End - shiftend, Right.Start + shiftstart, Right.End - shiftend, same);
            }

            public string DiffString()
            {
                if ((left == null) || (right == null))
                {
                    throw new InvalidOperationException("This hunk is based on a patch which does not have the compared data.");
                }
                StringBuilder builder = new StringBuilder();
                if (Same)
                {
                    foreach (object obj2 in Left)
                    {
                        builder.Append(" ");
                        builder.Append(obj2.ToString());
                        builder.Append("\n");
                    }
                }
                else
                {
                    foreach (object obj3 in Left)
                    {
                        builder.Append("<");
                        builder.Append(obj3.ToString());
                        builder.Append("\n");
                    }
                    foreach (object obj4 in Right)
                    {
                        builder.Append(">");
                        builder.Append(obj4.ToString());
                        builder.Append("\n");
                    }
                }
                return builder.ToString();
            }

            public override bool Equals(object o)
            {
                Algorithm.Diff.Diff.Hunk hunk = o as Algorithm.Diff.Diff.Hunk;
                return ((((s1start == hunk.s1start) && (s1start == hunk.s1end)) && ((s1start == hunk.s2start) && (s1start == hunk.s2end))) && (same == hunk.same));
            }

            private Algorithm.Diff.Range get(int seq)
            {
                int start = (seq == 1) ? s1start : s2start;
                int num2 = (seq == 1) ? s1end : s2end;
                IList list = (seq == 1) ? left : right;
                if (num2 < start)
                {
                    return new Algorithm.Diff.Range(list, start, 0);
                }
                return new Algorithm.Diff.Range(list, start, (num2 - start) + 1);
            }

            public override int GetHashCode()
            {
                return (((s1start + s1end) + s2start) + s2end);
            }

            public override bool IsSame(int index)
            {
                if (index != 0)
                {
                    throw new ArgumentException();
                }
                return Same;
            }

            public override Algorithm.Diff.Range Original()
            {
                return Left;
            }

            internal Algorithm.Diff.Diff.Hunk Reverse()
            {
                return new Algorithm.Diff.Diff.Hunk(right, left, Right.Start, Right.End, Left.Start, Left.End, same);
            }

            internal void SetLists(IList left, IList right)
            {
                this.left = left;
                this.right = right;
            }

            public override string ToString()
            {
                if ((left == null) || (right == null))
                {
                    return base.ToString();
                }
                return DiffString();
            }

            public override int ChangedLists
            {
                get
                {
                    return 1;
                }
            }

            public override bool Conflict
            {
                get
                {
                    return false;
                }
            }

            public Algorithm.Diff.Range Left
            {
                get
                {
                    return get(1);
                }
            }

            public Algorithm.Diff.Range Right
            {
                get
                {
                    return get(2);
                }
            }

            public override bool Same
            {
                get
                {
                    return same;
                }
            }
        }

        private class Trio
        {
            public Algorithm.Diff.Diff.Trio a;
            public int b;
            public int c;

            public Trio(Algorithm.Diff.Diff.Trio a, int b, int c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }
        }
    }
}
