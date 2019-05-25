namespace SqlSync.BasicCompare
{
    using System;

    public class Templates
    {
        public const string addLine = "<INS>#line#</INS>";
        public const string contextLine = "<SPAN class=cx>#line#</SPAN>";
        public const string deleteLine = "<DEL>#line#</DEL>";
        public const string fileDesc = "<H4>#fileDesc#</H4>";
        public const string FileDiffHTML = "<A id=#anchor#></A>\r\n<DIV class=modfile>\r\n#fileDesc#\r\n<PRE class=diff>\r\n#prePost#\r\n<SPAN>\r\n#changeLines#\r\n</SPAN></PRE></DIV>\r\n<a href=#pagetop class=toplink>Return to Top</a>";
        public const string linesIndex = "<SPAN class=lines>#index#</SPAN>";
        public const string prePostVersion = "<SPAN class=info>#prePost#</SPAN>";
    }
}
