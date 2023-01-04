using System.Drawing;
using UrielGuy.SyntaxHighlighting;
namespace SqlSync.Highlighting
{
    public class LogFileSyntax
    {
        private static SeperaratorCollection logFileSeparators = null;
        public static SeperaratorCollection LogFileSeparators
        {
            get
            {
                if (logFileSeparators == null)
                    SetLogFileSeparators();

                return logFileSeparators;
            }
        }
        private static void SetLogFileSeparators()
        {
            logFileSeparators = new SeperaratorCollection();
            logFileSeparators.Add(' ');
            logFileSeparators.Add('\r');
            logFileSeparators.Add('\n');
            logFileSeparators.Add('\t');
        }

        private static HighLightDescriptorCollection logFileHighlighting;
        public static HighLightDescriptorCollection LogFileHighlighting
        {
            get
            {
                if (logFileHighlighting == null)
                    SetLogFileHighlighting();

                return logFileHighlighting;
            }
        }
        private static void SetLogFileHighlighting()
        {
            string[] goodLevels = new string[] { "DEBUG", "INFO" };
            string[] warnLevels = new string[] { "WARN" };
            string[] badLevels = new string[] { "ERROR", "FATAL" };

            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection coll = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            for (int i = 0; i < goodLevels.Length; i++)
                coll.Add(new HighlightDescriptor(goodLevels[i], Color.LawnGreen, null, DescriptorType.Word, DescriptorRecognition.WholeWord, true));

            for (int i = 0; i < warnLevels.Length; i++)
                coll.Add(new HighlightDescriptor(warnLevels[i], Color.Orange, null, DescriptorType.Word, DescriptorRecognition.WholeWord, true));

            for (int i = 0; i < badLevels.Length; i++)
                coll.Add(new HighlightDescriptor(badLevels[i], Color.Red, null, DescriptorType.Word, DescriptorRecognition.WholeWord, true));


            coll.Add(new HighlightDescriptor(@"\[Thread:", "]", Color.Gray, null, DescriptorType.ToCloseToken, DescriptorRecognition.RegularExpression, false));
            coll.Add(new HighlightDescriptor(@"\d\d\d\d-\d\d-\d\d", Color.Gray, null, DescriptorType.Word, DescriptorRecognition.RegularExpression, false));
            coll.Add(new HighlightDescriptor(@"\d\d:\d\d:\d\d,\d\d\d", Color.Gray, null, DescriptorType.Word, DescriptorRecognition.RegularExpression, false));
            coll.Add(new HighlightDescriptor(@"(\(\w*\.\w*\))|(\(\w*\.\w*\.\w*\))|(\(\w*\.\w*\.\w*\.\w*\))|(\(\w*\.\w*\.\w*\.\w*\.\w*\))", Color.Gray, null, DescriptorType.Word, DescriptorRecognition.RegularExpression, false));

            logFileHighlighting = coll;
        }
    }
}
