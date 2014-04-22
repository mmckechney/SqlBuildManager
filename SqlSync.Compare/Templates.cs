using System;

namespace SqlSync.Compare
{
	/// <summary>
	/// Summary description for Templates.
	/// </summary>
	public class Templates
	{
		public const string deleteLine = "<DEL>#line#</DEL>"; // -
		public const string addLine = "<INS>#line#</INS>"; // +
		public const string contextLine = "<SPAN class=cx>#line#</SPAN>"; // blank
		public const string linesIndex = "<SPAN class=lines>#index#</SPAN>"; //@@
		public const string fileDesc = "<H4>#fileDesc#</H4>"; //Modified; Added; Deleted
		public const string prePostVersion = "<SPAN class=info>#prePost#</SPAN>"; // --- ; +++

		public const string FileDiffHTML = @"<A id=#anchor#></A>
<DIV class=modfile>
#fileDesc#
<PRE class=diff>
#prePost#
<SPAN>
#changeLines#
</SPAN></PRE></DIV>
<a href=#pagetop class=toplink>Return to Top</a>";

	}
	public class RtfTemplates
	{
		public const string FileHeader = @"{\rtf1\fbidis\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fswiss\fprq2\fcharset0 Arial;}{\f1\fswiss\fprq2\fcharset0 Verdana;}}
{\colortbl ;\red51\green51\blue153;\red153\green153\blue153;\red0\green0\blue0;\red0\green128\blue0;\red251\green87\blue126;}
\viewkind4\uc1\pard\ltrpar";

		public const string FileName = @"\cf1\b\f0\fs22 #File Name#\par";
		public const string LinesEffected = @"\cf2\b0\fs16 #Lines Effected#\par";
		public const string ReferenceLines = @"\cf3\fs18 #Reference Lines#\par";
		public const string Added = @"\cf4 #Added#\par";
		public const string Deleted = @"\cf5 #Deleted#\par";
		public const string FileFooter = "}";

	}
}
