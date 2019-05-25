namespace SqlSync.BasicCompare
{
    using System;

    public class RtfTemplates
    {
        public const string Added = @"\cf4 #Added#\par";
        public const string Deleted = @"\cf5 #Deleted#\par";
        public const string FileFooter = "}";
        public const string FileHeader = "{\\rtf1\\fbidis\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Arial;}{\\f1\\fswiss\\fprq2\\fcharset0 Verdana;}}\r\n{\\colortbl ;\\red51\\green51\\blue153;\\red153\\green153\\blue153;\\red0\\green0\\blue0;\\red0\\green128\\blue0;\\red251\\green87\\blue126;}\r\n\\viewkind4\\uc1\\pard\\ltrpar";
        public const string FileName = @"\cf1\b\f0\fs22 #File Name#\par";
        public const string LinesEffected = @"\cf2\b0\fs16 #Lines Effected#\par";
        public const string ReferenceLines = @"\cf3\fs18 #Reference Lines#\par";
    }
}
