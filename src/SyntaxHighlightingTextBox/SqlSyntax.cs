using System;
using System.Drawing;
using UrielGuy.SyntaxHighlighting;
namespace SqlSync.Highlighting
{
	/// <summary>
	/// Summary description for SqlSyntax.
	/// </summary>
	public class SqlSyntax
	{
	
		private static SeperaratorCollection sqlSeparators = null;
		public static SeperaratorCollection SqlSeparators
		{
			get
			{
				if(sqlSeparators ==  null)
					SetSqlSeparators();

				return sqlSeparators;
			}
		}
		private static void SetSqlSeparators()
		{
			sqlSeparators = new SeperaratorCollection();
			sqlSeparators.Add(' ');
			sqlSeparators.Add('\r');
			sqlSeparators.Add('\n');
			sqlSeparators.Add('\t');
			sqlSeparators.Add(',');
			sqlSeparators.Add('.');
			sqlSeparators.Add('(');
			sqlSeparators.Add('+');

		}

		private static HighLightDescriptorCollection sqlHighlighting;
		public static HighLightDescriptorCollection SqlHighlighting
		{
			get
			{
				if(sqlHighlighting == null)
					SetSqlHighlighting(SqlSync.Highlighting.Keywords.SqlReserved,SqlSync.Highlighting.Keywords.SqlFunctions);

				return sqlHighlighting;
			}
		}
		private static void SetSqlHighlighting(string[] keywords, string[] functions)
		{
			UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection coll = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
			for(int i=0;i<keywords.Length;i++)
			{
				coll.Add(new HighlightDescriptor(keywords[i], Color.Blue, null, DescriptorType.Word, DescriptorRecognition.WholeWord, true));
			}
			for(int i=0;i<functions.Length;i++)
			{
				coll.Add(new HighlightDescriptor(functions[i], Color.Magenta, null, DescriptorType.Word, DescriptorRecognition.WholeWord, true));
			}
			coll.Add(new HighlightDescriptor("GO",Color.Purple,null,DescriptorType.Word, DescriptorRecognition.WholeWord, true));
			coll.Add(new HighlightDescriptor("/*", "*/", Color.Green, null, DescriptorType.ToCloseToken, DescriptorRecognition.StartsWith, false));
			//coll.Add(new HighlightDescriptor("'", "'", Color.Red, null, DescriptorType.ToCloseToken, DescriptorRecognition.StartsWith, false));
			coll.Add(new HighlightDescriptor("--",Color.Green,null,DescriptorType.ToEOL,DescriptorRecognition.StartsWith,false));
			
			sqlHighlighting = coll;
		}
	}
}
