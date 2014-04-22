namespace Algorithm.Diff
{
    using System;
    using System.Collections;
    using System.IO;

    public class UnifiedDiff
    {
        private UnifiedDiff()
        {
        }

        internal static string[] LoadFileLines(string file)
        {
            ArrayList list = new ArrayList();
            using (StreamReader reader = new StreamReader(file))
            {
                string text;
                while ((text = reader.ReadLine()) != null)
                {
                    list.Add(text);
                }
            }
            return (string[]) list.ToArray(typeof(string));
        }

        private static void WriteBlock(TextWriter writer, char prefix, Algorithm.Diff.Range items)
        {
            if ((items.Count > 0) && (items[0] is char))
            {
                WriteCharBlock(writer, prefix, items);
            }
            else
            {
                WriteStringBlock(writer, prefix, items);
            }
        }

        private static void WriteCharBlock(TextWriter writer, char prefix, Algorithm.Diff.Range items)
        {
            bool flag = true;
            int num = 0;
            foreach (char ch in items)
            {
                if ((ch == '\n') && !flag)
                {
                    writer.WriteLine();
                    flag = true;
                }
                if (flag)
                {
                    writer.Write(prefix);
                    flag = false;
                    num = 0;
                }
                if (ch == '\n')
                {
                    writer.WriteLine("[newline]");
                    flag = true;
                    continue;
                }
                writer.Write(ch);
                num++;
                if (num == 60)
                {
                    writer.WriteLine();
                    flag = true;
                }
            }
            if (!flag)
            {
                writer.WriteLine();
            }
        }

        private static void WriteStringBlock(TextWriter writer, char prefix, Algorithm.Diff.Range items)
        {
            foreach (object obj2 in items)
            {
                writer.Write(prefix);
                writer.WriteLine(obj2.ToString());
            }
        }

        public static void WriteUnifiedDiff(Algorithm.Diff.Diff diff, TextWriter writer)
        {
            WriteUnifiedDiff(diff, writer, "Left", "Right", 2);
        }

        public static void WriteUnifiedDiff(Algorithm.Diff.Diff diff, TextWriter writer, string fromfile, string tofile, int context)
        {
            writer.Write("--- ");
            writer.WriteLine(fromfile);
            writer.Write("+++ ");
            writer.WriteLine(tofile);
            ArrayList hunks = new ArrayList();
            foreach (Algorithm.Diff.Diff.Hunk hunk in diff)
            {
                Algorithm.Diff.Diff.Hunk hunk2 = null;
                if (hunks.Count > 0)
                {
                    hunk2 = (Algorithm.Diff.Diff.Hunk) hunks[hunks.Count - 1];
                }
                if (hunk.Same)
                {
                    if (hunk2 == null)
                    {
                        if (hunk.Left.Count > context)
                        {
                            hunks.Add(hunk.Crop(hunk.Left.Count - context, 0));
                        }
                        else
                        {
                            hunks.Add(hunk);
                        }
                    }
                    else if (hunk.Left.Count <= (context * 2))
                    {
                        hunks.Add(hunk);
                    }
                    else
                    {
                        hunks.Add(hunk.Crop(0, hunk.Left.Count - context));
                        WriteUnifiedDiffSection(writer, hunks);
                        hunks.Clear();
                        if (hunk.Left.Count > context)
                        {
                            hunks.Add(hunk.Crop(hunk.Left.Count - context, 0));
                        }
                        else
                        {
                            hunks.Add(hunk);
                        }
                    }
                    continue;
                }
                hunks.Add(hunk);
            }
            if ((hunks.Count > 0) && ((hunks.Count != 1) || !((Algorithm.Diff.Diff.Hunk) hunks[0]).Same))
            {
                WriteUnifiedDiffSection(writer, hunks);
            }
        }

        public static void WriteUnifiedDiff(string leftFile, string rightFile, TextWriter writer, int context, bool caseSensitive, bool compareWhitespace)
        {
            WriteUnifiedDiff(LoadFileLines(leftFile), leftFile, LoadFileLines(rightFile), rightFile, writer, context, caseSensitive, compareWhitespace);
        }

        public static void WriteUnifiedDiff(string[] leftLines, string leftName, string[] rightLines, string rightName, TextWriter writer, int context, bool caseSensitive, bool compareWhitespace)
        {
            Algorithm.Diff.Diff diff = new Algorithm.Diff.Diff(leftLines, rightLines, caseSensitive, compareWhitespace);
            WriteUnifiedDiff(diff, writer, leftName, rightName, context);
        }

        private static void WriteUnifiedDiffSection(TextWriter writer, ArrayList hunks)
        {
            Algorithm.Diff.Diff.Hunk hunk = (Algorithm.Diff.Diff.Hunk) hunks[0];
            Algorithm.Diff.Diff.Hunk hunk2 = (Algorithm.Diff.Diff.Hunk) hunks[hunks.Count - 1];
            writer.Write("@@ -");
            writer.Write((int) (hunk.Left.Start + 1));
            writer.Write(",");
            writer.Write((int) ((hunk2.Left.End - hunk.Left.Start) + 1));
            writer.Write(" +");
            writer.Write((int) (hunk.Right.Start + 1));
            writer.Write(",");
            writer.Write((int) ((hunk2.Right.End - hunk.Right.Start) + 1));
            writer.WriteLine(" @@");
            foreach (Algorithm.Diff.Diff.Hunk hunk3 in hunks)
            {
                if (hunk3.Same)
                {
                    WriteBlock(writer, ' ', hunk3.Left);
                    continue;
                }
                WriteBlock(writer, '-', hunk3.Left);
                WriteBlock(writer, '+', hunk3.Right);
            }
        }
    }
}
