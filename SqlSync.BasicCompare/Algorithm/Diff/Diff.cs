namespace Algorithm.Diff
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class Diff : IDiff, IEnumerable
    {
        private int _End;
        private bool _Same;
        private IntList cdif;
        private IComparer comparer;
        private IHashCodeProvider hashcoder;
        internal IList left;
        internal IList leftRaw;
        internal IList right;
        internal IList rightRaw;

        public Diff(IList left, IList right, IComparer comparer, IHashCodeProvider hashcoder)
        {
            this.cdif = null;
            this.left = left;
            this.right = right;
            this.comparer = comparer;
            this.hashcoder = hashcoder;
            this.init();
        }

        public Diff(string leftFile, string rightFile, bool caseSensitive, bool compareWhitespace) : this(UnifiedDiff.LoadFileLines(leftFile), UnifiedDiff.LoadFileLines(rightFile), caseSensitive, compareWhitespace)
        {
        }

        public Diff(string[] left, string[] right, bool caseSensitive, bool compareWhitespace) : this(StripWhitespace(left, !compareWhitespace), StripWhitespace(right, !compareWhitespace), caseSensitive ? ((IComparer) Comparer.Default) : ((IComparer) CaseInsensitiveComparer.Default), caseSensitive ? null : ((IHashCodeProvider) CaseInsensitiveHashCodeProvider.Default))
        {
            this.leftRaw = left;
            this.rightRaw = right;
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
            if (!this.IsPrepared(out bMatches))
            {
                int start = 0;
                int end = b.Count - 1;
                while (((num <= num2) && (start <= end)) && this.compare(a[num], b[start]))
                {
                    list[num++] = start++;
                }
                while (((num <= num2) && (start <= end)) && this.compare(a[num2], b[end]))
                {
                    list[num2--] = end--;
                }
                bMatches = this._withPositionsOfInInterval(b, start, end);
            }
            IntList array = new IntList();
            ArrayList list3 = new ArrayList();
            for (int j = num; j <= num2; j++)
            {
                IntList list4 = (IntList) bMatches[a[j]];
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
                            high = this._replaceNextLargerWith(array, num9, high);
                        }
                        if (high != -1)
                        {
                            Trio trio = new Trio((high > 0) ? ((Trio) list3[high - 1]) : null, j, num9);
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
                for (Trio trio2 = (Trio) list3[array.Count - 1]; trio2 != null; trio2 = trio2.a)
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
            Hashtable hashtable = new Hashtable(this.hashcoder, this.comparer);
            for (int i = start; i <= end; i++)
            {
                object key = aCollection[i];
                if (hashtable.ContainsKey(key))
                {
                    ((IntList) hashtable[key]).Add(i);
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
            this.LCSidx(a, b, out am, out bm);
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
            if (this.comparer == null)
            {
                return a.Equals(b);
            }
            return (this.comparer.Compare(a, b) == 0);
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
            return new Patch((Patch.Hunk[]) list.ToArray(typeof(Patch.Hunk)));
        }

        private void init()
        {
            this.cdif = this.compact_diff(this.left, this.right);
            this._Same = true;
            if ((this.cdif[2] == 0) && (this.cdif[3] == 0))
            {
                this._Same = false;
                this.cdif.RemoveAt(0);
                this.cdif.RemoveAt(0);
            }
            this._End = (1 + this.cdif.Count) / 2;
        }

        private bool IsPrepared(out Hashtable bMatches)
        {
            bMatches = null;
            return false;
        }

        private void LCSidx(IList a, IList b, out IntList am, out IntList bm)
        {
            IntList list = this._longestCommonSubsequence(a, b);
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
            if (this.cdif == null)
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
                return this.left;
            }
        }

        public IList Right
        {
            get
            {
                return this.right;
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
                this.Reset();
            }

            private void _ChkPos()
            {
                if (this._Pos == 0)
                {
                    throw new InvalidOperationException("Position is reset.");
                }
            }

            private Algorithm.Diff.Diff.Hunk gethunk()
            {
                this._ChkPos();
                int num5 = 1 + this._Off;
                int num6 = 2 + this._Off;
                int num = this.diff.cdif[num5 - 2];
                int num2 = this.diff.cdif[num5] - 1;
                int num3 = this.diff.cdif[num6 - 2];
                int num4 = this.diff.cdif[num6] - 1;
                return new Algorithm.Diff.Diff.Hunk(this.diff.leftRaw, this.diff.rightRaw, num, num2, num3, num4, this.same());
            }

            public bool MoveNext()
            {
                return this.next();
            }

            private bool next()
            {
                this.reset(this._Pos + 1);
                return (this._Pos != -1);
            }

            private void reset(int pos)
            {
                if ((pos < 0) || (this.diff._End <= pos))
                {
                    pos = -1;
                }
                this._Pos = pos;
                this._Off = (2 * pos) - 1;
            }

            public void Reset()
            {
                this.reset(0);
            }

            private bool same()
            {
                this._ChkPos();
                if (this.diff._Same != ((1 & this._Pos) != 0))
                {
                    return false;
                }
                return true;
            }

            public object Current
            {
                get
                {
                    this._ChkPos();
                    return this.gethunk();
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
                return this.Right;
            }

            internal Algorithm.Diff.Diff.Hunk Crop(int shiftstart, int shiftend)
            {
                return new Algorithm.Diff.Diff.Hunk(this.left, this.right, this.Left.Start + shiftstart, this.Left.End - shiftend, this.Right.Start + shiftstart, this.Right.End - shiftend, this.same);
            }

            public string DiffString()
            {
                if ((this.left == null) || (this.right == null))
                {
                    throw new InvalidOperationException("This hunk is based on a patch which does not have the compared data.");
                }
                StringBuilder builder = new StringBuilder();
                if (this.Same)
                {
                    foreach (object obj2 in this.Left)
                    {
                        builder.Append(" ");
                        builder.Append(obj2.ToString());
                        builder.Append("\n");
                    }
                }
                else
                {
                    foreach (object obj3 in this.Left)
                    {
                        builder.Append("<");
                        builder.Append(obj3.ToString());
                        builder.Append("\n");
                    }
                    foreach (object obj4 in this.Right)
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
                return ((((this.s1start == hunk.s1start) && (this.s1start == hunk.s1end)) && ((this.s1start == hunk.s2start) && (this.s1start == hunk.s2end))) && (this.same == hunk.same));
            }

            private Algorithm.Diff.Range get(int seq)
            {
                int start = (seq == 1) ? this.s1start : this.s2start;
                int num2 = (seq == 1) ? this.s1end : this.s2end;
                IList list = (seq == 1) ? this.left : this.right;
                if (num2 < start)
                {
                    return new Algorithm.Diff.Range(list, start, 0);
                }
                return new Algorithm.Diff.Range(list, start, (num2 - start) + 1);
            }

            public override int GetHashCode()
            {
                return (((this.s1start + this.s1end) + this.s2start) + this.s2end);
            }

            public override bool IsSame(int index)
            {
                if (index != 0)
                {
                    throw new ArgumentException();
                }
                return this.Same;
            }

            public override Algorithm.Diff.Range Original()
            {
                return this.Left;
            }

            internal Algorithm.Diff.Diff.Hunk Reverse()
            {
                return new Algorithm.Diff.Diff.Hunk(this.right, this.left, this.Right.Start, this.Right.End, this.Left.Start, this.Left.End, this.same);
            }

            internal void SetLists(IList left, IList right)
            {
                this.left = left;
                this.right = right;
            }

            public override string ToString()
            {
                if ((this.left == null) || (this.right == null))
                {
                    return base.ToString();
                }
                return this.DiffString();
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
                    return this.get(1);
                }
            }

            public Algorithm.Diff.Range Right
            {
                get
                {
                    return this.get(2);
                }
            }

            public override bool Same
            {
                get
                {
                    return this.same;
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
