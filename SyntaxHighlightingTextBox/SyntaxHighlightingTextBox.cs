using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
//using System.Linq;
//http://www.codeproject.com/cs/miscctrl/SyntaxHighlighting.asp
namespace UrielGuy.SyntaxHighlighting
{
	/// <summary>
	/// A textbox the does syntax highlighting.
	/// </summary>
	public class SyntaxHighlightingTextBox :	System.Windows.Forms.RichTextBox 
	{
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool suspendHighlighting;

        public bool SuspendHighlighting
        {
            get { return suspendHighlighting; }
            set
            {
                suspendHighlighting = value;
                if (value == true)
                    this.RefreshHighlighting();

            }
        }
		private SqlSync.Highlighting.SyntaxHightlightType highlightType;
		[Category("Behavior")]
		public SqlSync.Highlighting.SyntaxHightlightType HighlightType
		{
			get
			{
				return this.highlightType;
			}
			set
			{
				this.highlightType = value;
				switch(this.highlightType)
				{
					case SqlSync.Highlighting.SyntaxHightlightType.Sql:
						this.HighlightDescriptors = SqlSync.Highlighting.SqlSyntax.SqlHighlighting;
						this.Separators = SqlSync.Highlighting.SqlSyntax.SqlSeparators;
						break;
                    case SqlSync.Highlighting.SyntaxHightlightType.LogFile:
                        this.HighlightDescriptors = SqlSync.Highlighting.LogFileSyntax.LogFileHighlighting;
                        this.Separators = SqlSync.Highlighting.LogFileSyntax.LogFileSeparators;
                        break;
                    case SqlSync.Highlighting.SyntaxHightlightType.None:
                    case SqlSync.Highlighting.SyntaxHightlightType.RemoteServiceLog:
                        this.suspendHighlighting = true;
                        break;
 				}
			}
		}
		public SyntaxHighlightingTextBox(SqlSync.Highlighting.SyntaxHightlightType type)
		{
			this.HighlightType = type;

            // Otherwise, non-standard links get lost when user starts typing
            // next to a non-standard link
            this.DetectUrls = false;
		}
		public SyntaxHighlightingTextBox()
		{
			this.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;

            // Otherwise, non-standard links get lost when user starts typing
            // next to a non-standard link
            this.DetectUrls = false;
		}

		#region Members

		//Members exposed via properties
		private SeperaratorCollection mSeperators = new SeperaratorCollection();  
		private HighLightDescriptorCollection mHighlightDescriptors = new HighLightDescriptorCollection();
		private bool mCaseSesitive = false;
		private bool mFilterAutoComplete = false;

		//Internal use members
		private bool mAutoCompleteShown = false;
		private bool mParsing = false;
		private bool mIgnoreLostFocus = false;

		private AutoCompleteForm mAutoCompleteForm = new AutoCompleteForm();

		//Undo/Redo members
		private ArrayList mUndoList = new ArrayList();
		private Stack mRedoStack = new Stack();
		private bool mIsUndo = false;
		private UndoRedoInfo mLastInfo = new UndoRedoInfo("", new Win32.POINT(), 0);
		private int mMaxUndoRedoSteps = 50;

		#endregion

		#region Properties
		/// <summary>
		/// Determines if token recognition is case sensitive.
		/// </summary>
		[Category("Behavior")]
		public bool CaseSensitive 
		{ 
			get 
			{ 
				return mCaseSesitive; 
			}
			set 
			{ 
				mCaseSesitive = value;
			}
		}


		/// <summary>
		/// Sets whether or not to remove items from the Autocomplete window as the user types...
		/// </summary>
		[Category("Behavior")]
		public bool FilterAutoComplete 
		{
			get 
			{
				return mFilterAutoComplete;
			}
			set 
			{
				mFilterAutoComplete = value;
			}
		}

		/// <summary>
		/// Set the maximum amount of Undo/Redo steps.
		/// </summary>
		[Category("Behavior")]
		public int MaxUndoRedoSteps 
		{
			get 
			{
				return mMaxUndoRedoSteps;
			}
			set
			{
				mMaxUndoRedoSteps = value;
			}
		}

			/// <summary>
			/// A collection of charecters. a token is every string between two seperators.
			/// </summary>
			/// 
			public SeperaratorCollection Separators 
		{
			get 
			{
				return mSeperators;
			}
				set
				{
					this.mSeperators = value;
				}
		}
		
		/// <summary>
		/// The collection of highlight descriptors.
		/// </summary>
		/// 
		public HighLightDescriptorCollection HighlightDescriptors 
		{
			get 
			{
				return mHighlightDescriptors;
			}
			set
			{
				this.mHighlightDescriptors = value;
			}
		}

		#endregion

		public void RefreshHighlighting()
		{
			this.OnTextChanged(EventArgs.Empty);
		}
		#region Overriden methods

		/// <summary>
		/// The on text changed overrided. Here we parse the text into RTF for the highlighting.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTextChanged(EventArgs e)
		{
            try
            {
                if (this.suspendHighlighting)
                    return;
                if (mParsing) return;
                mParsing = true;
                Win32.LockWindowUpdate(Handle);
                base.OnTextChanged(e);

                if (!mIsUndo)
                {
                    mRedoStack.Clear();
                    mUndoList.Insert(0, mLastInfo);
                    this.LimitUndo();
                    mLastInfo = new UndoRedoInfo(Text, GetScrollPos(), SelectionStart);
                }

                //Save scroll bar an cursor position, changeing the RTF moves the cursor and scrollbars to top positin
                Win32.POINT scrollPos = GetScrollPos();
                int cursorLoc = SelectionStart;

                //Created with an estimate of how big the stringbuilder has to be...
                StringBuilder sb = new
                    StringBuilder((int)(Text.Length * 1.5 + 150));

                //Adding RTF header
                sb.Append(@"{\rtf1\fbidis\ansi\ansicpg1255\deff0\deflang1037{\fonttbl{");

                //Font table creation
                int fontCounter = 0;
                Hashtable fonts = new Hashtable();
                AddFontToTable(sb, Font, ref fontCounter, fonts);
                foreach (HighlightDescriptor hd in mHighlightDescriptors)
                {
                    if ((hd.Font != null) && !fonts.ContainsKey(hd.Font.Name))
                    {
                        AddFontToTable(sb, hd.Font, ref fontCounter, fonts);
                    }
                }
                sb.Append("}\n");

                //ColorTable

                sb.Append(@"{\colortbl ;");
                Hashtable colors = new Hashtable();
                int colorCounter = 1;
                AddColorToTable(sb, ForeColor, ref colorCounter, colors);
                AddColorToTable(sb, BackColor, ref colorCounter, colors);

                foreach (HighlightDescriptor hd in mHighlightDescriptors)
                {
                    if (!colors.ContainsKey(hd.Color))
                    {
                        AddColorToTable(sb, hd.Color, ref colorCounter, colors);
                    }
                }

                //Parsing text

                sb.Append("}\n").Append(@"\viewkind4\uc1\pard\ltrpar");
                SetDefaultSettings(sb, colors, fonts);

                char[] sperators = mSeperators.GetAsCharArray();

                //Replacing "\" to "\\" for RTF...
                string[] lines = Text.Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").Split('\n');
                for (int lineCounter = 0; lineCounter < lines.Length; lineCounter++)
                {
                    if (lineCounter != 0)
                    {
                        AddNewLine(sb);
                    }
                    string line = lines[lineCounter];
                    string[] tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                    if (tokens.Length == 0)
                    {
                        sb.Append(line);
                        AddNewLine(sb);
                        continue;
                    }

                    int tokenCounter = 0;
                    for (int i = 0; i < line.Length; )
                    {
                        char curChar = line[i];
                        if (mSeperators.Contains(curChar))
                        {
                            sb.Append(curChar);
                            i++;
                        }
                        else
                        {
                            string curToken = tokens[tokenCounter++];
                            bool bAddToken = true;
                            foreach (HighlightDescriptor hd in mHighlightDescriptors)
                            {
                                string compareStr = mCaseSesitive ? hd.Token : hd.Token.ToUpper();
                                bool match = false;

                                //Check if the highlight descriptor matches the current toker according to the DescriptoRecognision property.
                                switch (hd.DescriptorRecognition)
                                {
                                    case DescriptorRecognition.WholeWord:
                                        if (curToken == compareStr)
                                        {
                                            match = true;
                                        }
                                        break;
                                    case DescriptorRecognition.StartsWith:
                                        if (curToken.StartsWith(compareStr))
                                        {
                                            match = true;
                                        }
                                        break;
                                    case DescriptorRecognition.Contains:
                                        if (curToken.IndexOf(compareStr) != -1)
                                        {
                                            match = true;
                                        }
                                        break;
                                    case DescriptorRecognition.RegularExpression:
                                        Regex pattern = new Regex(hd.Token, RegexOptions.IgnoreCase);
                                        if (pattern.Match(curToken).Success)
                                        {
                                            match = true;
                                        }
                                        break;
                                }
                                if (!match)
                                {
                                    //If this token doesn't match chech the next one.
                                    continue;
                                }

                                //printing this token will be handled by the inner code, don't apply default settings...
                                bAddToken = false;

                                //Set colors to current descriptor settings.
                                SetDescriptorSettings(sb, hd, colors, fonts);

                                //Print text affected by this descriptor.
                                switch (hd.DescriptorType)
                                {
                                    case DescriptorType.Word:
                                        sb.Append(line.Substring(i, curToken.Length));
                                        SetDefaultSettings(sb, colors, fonts);
                                        i += curToken.Length;
                                        break;
                                    case DescriptorType.ToEOL:
                                        sb.Append(line.Remove(0, i));
                                        i = line.Length;
                                        SetDefaultSettings(sb, colors, fonts);
                                        break;
                                    case DescriptorType.ToCloseToken:
                                        //while( (i+1< line.Length) && (line.IndexOf(hd.CloseToken, i+1) == -1) && (lineCounter < lines.Length) )
                                        while ((line.IndexOf(hd.CloseToken, i) == -1) && (lineCounter < lines.Length))
                                        {
                                            sb.Append(line.Remove(0, i));
                                            lineCounter++;
                                            if (lineCounter < lines.Length)
                                            {
                                                AddNewLine(sb);
                                                line = lines[lineCounter];
                                                i = 0;
                                            }
                                            else
                                            {
                                                i = line.Length;
                                            }
                                        }
                                        if (line.IndexOf(hd.CloseToken, i) != -1)
                                        {
                                            sb.Append(line.Substring(i, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length - i));
                                            line = line.Remove(0, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length);
                                            tokenCounter = 0;
                                            tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
                                            SetDefaultSettings(sb, colors, fonts);
                                            i = 0;
                                        }
                                        break;
                                }
                                break;
                            }
                            if (bAddToken)
                            {
                                //Print text with default settings...
                                sb.Append(line.Substring(i, curToken.Length));
                                i += curToken.Length;
                            }
                        }
                    }
                }

                //			System.Diagnostics.Debug.WriteLine(sb.ToString());
                Rtf = sb.ToString();

                //Restore cursor and scrollbars location.
                SelectionStart = cursorLoc;

                mParsing = false;

                SetScrollPos(scrollPos);
                Win32.LockWindowUpdate((IntPtr)0);
                Invalidate();

                if (mAutoCompleteShown)
                {
                    if (mFilterAutoComplete)
                    {
                        SetAutoCompleteItems();
                        SetAutoCompleteSize();
                        SetAutoCompleteLocation(false);
                    }
                    SetBestSelectedAutoCompleteItem();
                }
            }
            catch (Exception exe)
            {
                log.Error("Error setting SyntaxHighlighting. Type=" + this.highlightType.ToString(), exe);
            }
		}


		protected override void OnVScroll(EventArgs e)
		{
			if (mParsing) return;
			base.OnVScroll (e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			HideAutoCompleteForm();
			base.OnMouseDown (e);
		}

		/// <summary>
		/// Taking care of Keyboard events
		/// </summary>
		/// <param name="m"></param>
		/// <remarks>
		/// Since even when overriding the OnKeyDown methoed and not calling the base function 
		/// you don't have full control of the input, I've decided to catch windows messages to handle them.
		/// </remarks>
		protected override void WndProc(ref Message m)
		{
            switch (m.Msg)
            {
                //				case Win32.WM_LBUTTONDBLCLK:
                //					AcceptAutoCompleteItem();
                //					return;
                case Win32.WM_PAINT:
                    {
                        //Don't draw the control while parsing to avoid flicker.
                        if (mParsing)
                        {
                            return;
                        }
                        break;
                    }
                case Win32.WM_KEYDOWN:
                    {
                        if (mAutoCompleteShown)
                        {
                            switch ((Keys)(int)m.WParam)
                            {
                                case Keys.Down:
                                    {
                                        if (mAutoCompleteForm.Items.Count != 0)
                                        {
                                            mAutoCompleteForm.SelectedIndex = (mAutoCompleteForm.SelectedIndex + 1) % mAutoCompleteForm.Items.Count;
                                        }
                                        return;
                                    }
                                case Keys.Up:
                                    {
                                        if (mAutoCompleteForm.Items.Count != 0)
                                        {
                                            if (mAutoCompleteForm.SelectedIndex < 1)
                                            {
                                                mAutoCompleteForm.SelectedIndex = mAutoCompleteForm.Items.Count - 1;
                                            }
                                            else
                                            {
                                                mAutoCompleteForm.SelectedIndex--;
                                            }
                                        }
                                        return;
                                    }
                                case Keys.Enter:
                                case Keys.Space:
                                case Keys.Tab:
                                    {
                                        AcceptAutoCompleteItem();
                                        return;
                                    }
                                case Keys.Escape:
                                    {
                                        HideAutoCompleteForm();
                                        return;
                                    }

                            }
                        }
                        else
                        {
                            if (((Keys)(int)m.WParam == Keys.Space) &&
                                ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
                            {
                                CompleteWord();
                            }
                            else if (((Keys)(int)m.WParam == Keys.Z) &&
                                ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
                            {
                                Undo();
                                return;
                            }
                            else if (((Keys)(int)m.WParam == Keys.Y) &&
                                ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
                            {
                                Redo();
                                return;
                            }
                        }
                        break;
                    }
                case Win32.WM_CHAR:
                    {
                        switch ((Keys)(int)m.WParam)
                        {
                            case Keys.Space:
                                if ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0)
                                {
                                    return;
                                }
                                break;
                            case Keys.Enter:
                                if (mAutoCompleteShown) return;
                                break;
                        }
                    }
                    break;

            }
			base.WndProc (ref m);
		}


		/// <summary>
		/// Hides the AutoComplete form when losing focus on textbox.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLostFocus(EventArgs e)
		{
			if (!mIgnoreLostFocus)
			{
				HideAutoCompleteForm();
			}
			base.OnLostFocus (e);
		}


		#endregion

		#region Undo/Redo Code
		public new bool CanUndo 
		{
			get 
			{
				return mUndoList.Count > 0;
			}
		}
		public new bool CanRedo
		{
			get 
			{
				return mRedoStack.Count > 0;
			}
		}

		private void LimitUndo()
		{
			while (mUndoList.Count > mMaxUndoRedoSteps)
			{
				mUndoList.RemoveAt(mMaxUndoRedoSteps);
			}
		}

		public new void Undo()
		{
			if (!CanUndo)
				return;
			mIsUndo = true;
			mRedoStack.Push(new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
			UndoRedoInfo info = (UndoRedoInfo)mUndoList[0];
			mUndoList.RemoveAt(0);
			Text = info.Text;
			SelectionStart = info.CursorLocation;
			SetScrollPos(info.ScrollPos);
			mLastInfo = info;
			mIsUndo = false;
		}
		public new void Redo()
		{
			if (!CanRedo)
				return;
			mIsUndo = true;
			mUndoList.Insert(0,new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
			LimitUndo();
			UndoRedoInfo info = (UndoRedoInfo)mRedoStack.Pop();
			Text = info.Text;
			SelectionStart = info.CursorLocation;
			SetScrollPos(info.ScrollPos);
			mIsUndo = false;
		}

		private class UndoRedoInfo
		{
			public UndoRedoInfo(string text, Win32.POINT scrollPos, int cursorLoc)
			{
				Text = text;
				ScrollPos = scrollPos;
				CursorLocation = cursorLoc;
			}
			public readonly Win32.POINT ScrollPos;
			public readonly int CursorLocation;
			public readonly string Text;
		}
		#endregion

		#region AutoComplete functions

		/// <summary>
		/// Entry point to autocomplete mechanism.
		/// Tries to complete the current word. if it fails it shows the AutoComplete form.
		/// </summary>
		private void CompleteWord()
		{
			int curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1))+1;
			int curTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
			if(curTokenStartIndex > curTokenEndIndex)
				curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(),Math.Min(SelectionStart-1,Text.Length-1))+1;
			
			if (curTokenEndIndex == -1) 
			{
				curTokenEndIndex = Text.Length;
			}
			string curTokenString = Text.Substring(curTokenStartIndex, Math.Max(curTokenEndIndex - curTokenStartIndex,0)).ToUpper();
			
			string token = null;
			foreach (HighlightDescriptor hd in mHighlightDescriptors)
			{
				if (hd.UseForAutoComplete && hd.Token.ToUpper().StartsWith(curTokenString))
				{
					if (token == null)
					{
						token = hd.Token;
					}
					else
					{
						token = null;
						break;
					}
				}
			}
			if (token == null)
			{
				ShowAutoComplete();
			}
			else
			{
				SelectionStart = curTokenStartIndex;
				SelectionLength = curTokenEndIndex - curTokenStartIndex;
				SelectedText = token;
				SelectionStart = SelectionStart + SelectionLength;
				SelectionLength = 0;
			}
		}

		/// <summary>
		/// replace the current word of the cursor with the one from the AutoComplete form and closes it.
		/// </summary>
		/// <returns>If the operation was succesful</returns>
		private bool AcceptAutoCompleteItem()
		{
			
			if (mAutoCompleteForm.SelectedItem == null)
			{
				return false;
			}
			
			int curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1)) + 1;
			int curTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
			if(curTokenStartIndex > curTokenEndIndex)
				curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart-1, Text.Length - 1)) + 1;
			
			if (curTokenEndIndex == -1) 
			{
				curTokenEndIndex = Text.Length;
			}
			SelectionStart = Math.Max(curTokenStartIndex, 0);
			SelectionLength = Math.Max(0,curTokenEndIndex - curTokenStartIndex);
			SelectedText = mAutoCompleteForm.SelectedItem;
			SelectionStart = SelectionStart + SelectionLength;
			SelectionLength = 0;
			
			HideAutoCompleteForm();
			return true;
		}



		/// <summary>
		/// Finds the and sets the best matching token as the selected item in the AutoCompleteForm.
		/// </summary>
		private void SetBestSelectedAutoCompleteItem()
		{
			int curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1))+1;
			int curTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
			if (curTokenEndIndex == -1) 
			{
				curTokenEndIndex = Text.Length;
			}
			string curTokenString = Text.Substring(curTokenStartIndex, Math.Max(curTokenEndIndex - curTokenStartIndex,0)).ToUpper();
			
			if ((mAutoCompleteForm.SelectedItem != null) && 
				mAutoCompleteForm.SelectedItem.ToUpper().StartsWith(curTokenString))
			{
				return;
			}

			int matchingChars = -1;
			string bestMatchingToken = null;

			foreach (string item in mAutoCompleteForm.Items)
			{
				bool isWholeItemMatching = true;
				for (int i = 0 ; i < Math.Min(item.Length, curTokenString.Length); i++)
				{
					if (char.ToUpper(item[i]) != char.ToUpper(curTokenString[i]))
					{
						isWholeItemMatching = false;
						if (i-1 > matchingChars)
						{
							matchingChars = i;
							bestMatchingToken = item;
							break;
						}
					}
				}
				if (isWholeItemMatching &&
					(Math.Min(item.Length, curTokenString.Length) > matchingChars))
				{
					matchingChars = Math.Min(item.Length, curTokenString.Length);
					bestMatchingToken = item;
				}
			}
			
			if (bestMatchingToken != null)
			{
				mAutoCompleteForm.SelectedIndex = mAutoCompleteForm.Items.IndexOf(bestMatchingToken);
			}


		}

		/// <summary>
		/// Sets the items for the AutoComplete form.
		/// </summary>
		private void SetAutoCompleteItems()
		{
			mAutoCompleteForm.Items.Clear();
			string filterString = "";
			if (mFilterAutoComplete)
			{
			
				int filterTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1))+1;
				int filterTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
				if(filterTokenStartIndex > filterTokenEndIndex)
					filterTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart-1, Text.Length - 1))+1;
				if (filterTokenEndIndex == -1) 
				{
					filterTokenEndIndex = Text.Length;
				}
				if(filterTokenEndIndex - filterTokenStartIndex < 0)
					return;

				filterString = Text.Substring(filterTokenStartIndex, filterTokenEndIndex - filterTokenStartIndex).ToUpper();
			}
		
			foreach (HighlightDescriptor hd in mHighlightDescriptors)
			{
				if (hd.Token.ToUpper().StartsWith(filterString) && hd.UseForAutoComplete)
				{
					mAutoCompleteForm.Items.Add(hd.Token);
				}
			}
			mAutoCompleteForm.UpdateView();
		}
		
		/// <summary>
		/// Sets the size. the size is limited by the MaxSize property in the form itself.
		/// </summary>
		private void SetAutoCompleteSize()
		{
			mAutoCompleteForm.Height = Math.Min(
				Math.Max(mAutoCompleteForm.Items.Count, 1) * mAutoCompleteForm.ItemHeight + 4, 
				mAutoCompleteForm.MaximumSize.Height);
		}

		/// <summary>
		/// closes the AutoCompleteForm.
		/// </summary>
		private void HideAutoCompleteForm()
		{
			mAutoCompleteForm.Visible = false;
			mAutoCompleteShown = false;
		}
		

		/// <summary>
		/// Sets the location of the AutoComplete form, maiking sure it's on the screen where the cursor is.
		/// </summary>
		/// <param name="moveHorizontly">determines wheather or not to move the form horizontly.</param>
		private void SetAutoCompleteLocation(bool moveHorizontly)
		{
			Point cursorLocation = GetPositionFromCharIndex(SelectionStart);
			Screen screen = Screen.FromPoint(cursorLocation);
			Point optimalLocation = new Point(PointToScreen(cursorLocation).X-15, (int)(PointToScreen(cursorLocation).Y + Font.Size*2 + 2));
			Rectangle desiredPlace = new Rectangle(optimalLocation , mAutoCompleteForm.Size);
			desiredPlace.Width = 152;
			if (desiredPlace.Left < screen.Bounds.Left) 
			{
				desiredPlace.X = screen.Bounds.Left;
			}
			if (desiredPlace.Right > screen.Bounds.Right)
			{
				desiredPlace.X -= (desiredPlace.Right - screen.Bounds.Right);
			}
			if (desiredPlace.Bottom > screen.Bounds.Bottom)
			{
				desiredPlace.Y = cursorLocation.Y - 2 - desiredPlace.Height;
			}
			if (!moveHorizontly)
			{
				desiredPlace.X = mAutoCompleteForm.Left;
			}

			mAutoCompleteForm.Bounds = desiredPlace;
		}

		/// <summary>
		/// Shows the Autocomplete form.
		/// </summary>
		public void ShowAutoComplete()
		{
			SetAutoCompleteItems();
			SetAutoCompleteSize();
			SetAutoCompleteLocation(true);
			mIgnoreLostFocus = true;
			mAutoCompleteForm.Visible = true;
			SetBestSelectedAutoCompleteItem();
			mAutoCompleteShown = true;
			Focus();
			mIgnoreLostFocus = false;
		}

		#endregion 

		#region Rtf building helper functions

		/// <summary>
		/// Set color and font to default control settings.
		/// </summary>
		/// <param name="sb">the string builder building the RTF</param>
		/// <param name="colors">colors hashtable</param>
		/// <param name="fonts">fonts hashtable</param>
		private void SetDefaultSettings(StringBuilder sb, Hashtable colors, Hashtable fonts)
		{
			SetColor(sb, ForeColor, colors);
			SetFont(sb, Font, fonts);
			SetFontSize(sb, (int)Font.Size);
			EndTags(sb);
		}

		/// <summary>
		/// Set Color and font to a highlight descriptor settings.
		/// </summary>
		/// <param name="sb">the string builder building the RTF</param>
		/// <param name="hd">the HighlightDescriptor with the font and color settings to apply.</param>
		/// <param name="colors">colors hashtable</param>
		/// <param name="fonts">fonts hashtable</param>
		private void SetDescriptorSettings(StringBuilder sb, HighlightDescriptor hd, Hashtable colors, Hashtable fonts)
		{
			SetColor(sb, hd.Color, colors);
			if (hd.Font != null)
			{
				SetFont(sb, hd.Font, fonts);
				SetFontSize(sb, (int)hd.Font.Size);
			}
			EndTags(sb);

		}
		/// <summary>
		/// Sets the color to the specified color
		/// </summary>
		private void SetColor(StringBuilder sb, Color color, Hashtable colors)
		{
			sb.Append(@"\cf").Append(colors[color]);
		}
		/// <summary>
		/// Sets the backgroung color to the specified color.
		/// </summary>
		private void SetBackColor(StringBuilder sb, Color color, Hashtable colors)
		{
			sb.Append(@"\cb").Append(colors[color]);
		}
		/// <summary>
		/// Sets the font to the specified font.
		/// </summary>
		private void SetFont(StringBuilder sb, Font font, Hashtable fonts)
		{
			if (font == null) return;
			sb.Append(@"\f").Append(fonts[font.Name]);
		}
		/// <summary>
		/// Sets the font size to the specified font size.
		/// </summary>
		private void SetFontSize(StringBuilder sb, int size)
		{
			sb.Append(@"\fs").Append(size*2);
		}
		/// <summary>
		/// Adds a newLine mark to the RTF.
		/// </summary>
		private void AddNewLine(StringBuilder sb)
		{
			sb.Append("\\par\n");
		}

		/// <summary>
		/// Ends a RTF tags section.
		/// </summary>
		private void EndTags(StringBuilder sb)
		{
			sb.Append(' ');
		}

		/// <summary>
		/// Adds a font to the RTF's font table and to the fonts hashtable.
		/// </summary>
		/// <param name="sb">The RTF's string builder</param>
		/// <param name="font">the Font to add</param>
		/// <param name="counter">a counter, containing the amount of fonts in the table</param>
		/// <param name="fonts">an hashtable. the key is the font's name. the value is it's index in the table</param>
		private void AddFontToTable(StringBuilder sb, Font font, ref int counter, Hashtable fonts)
		{
	
			sb.Append(@"\f").Append(counter).Append(@"\fnil\fcharset0").Append(font.Name).Append(";}");
			fonts.Add(font.Name, counter++);
		}

		/// <summary>
		/// Adds a color to the RTF's color table and to the colors hashtable.
		/// </summary>
		/// <param name="sb">The RTF's string builder</param>
		/// <param name="color">the color to add</param>
		/// <param name="counter">a counter, containing the amount of colors in the table</param>
		/// <param name="colors">an hashtable. the key is the color. the value is it's index in the table</param>
		private void AddColorToTable(StringBuilder sb, Color color, ref int counter, Hashtable colors)
		{
	
			sb.Append(@"\red").Append(color.R).Append(@"\green").Append(color.G).Append(@"\blue")
				.Append(color.B).Append(";");
			colors.Add(color, counter++);
		}

		#endregion

		#region Scrollbar positions functions
		/// <summary>
		/// Sends a win32 message to get the scrollbars' position.
		/// </summary>
		/// <returns>a POINT structore containing horizontal and vertical scrollbar position.</returns>
		private unsafe Win32.POINT GetScrollPos()
		{
			Win32.POINT res = new Win32.POINT();
			IntPtr ptr = new IntPtr(&res);
			Win32.SendMessage(Handle, Win32.EM_GETSCROLLPOS, 0, ptr);
			return res;

		}

		/// <summary>
		/// Sends a win32 message to set scrollbars position.
		/// </summary>
		/// <param name="point">a POINT conatining H/Vscrollbar scrollpos.</param>
		private unsafe void SetScrollPos(Win32.POINT point)
		{
			IntPtr ptr = new IntPtr(&point);
			Win32.SendMessage(Handle, Win32.EM_SETSCROLLPOS, 0, ptr);

		}
		#endregion

        #region Custom Links Courtesty of http://www.codeproject.com/KB/edit/RichTextBoxLinks.aspx

        #region Interop-Defines
        [StructLayout(LayoutKind.Sequential)]
        private struct CHARFORMAT2_STRUCT
        {
            public UInt32 cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
            public UInt16 wWeight;
            public UInt16 sSpacing;
            public int crBackColor; // Color.ToArgb() -> int
            public int lcid;
            public int dwReserved;
            public Int16 sStyle;
            public Int16 wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_USER = 0x0400;
        private const int EM_GETCHARFORMAT = WM_USER + 58;
        private const int EM_SETCHARFORMAT = WM_USER + 68;

        private const int SCF_SELECTION = 0x0001;
        private const int SCF_WORD = 0x0002;
        private const int SCF_ALL = 0x0004;

        #region CHARFORMAT2 Flags
        private const UInt32 CFE_BOLD = 0x0001;
        private const UInt32 CFE_ITALIC = 0x0002;
        private const UInt32 CFE_UNDERLINE = 0x0004;
        private const UInt32 CFE_STRIKEOUT = 0x0008;
        private const UInt32 CFE_PROTECTED = 0x0010;
        private const UInt32 CFE_LINK = 0x0020;
        private const UInt32 CFE_AUTOCOLOR = 0x40000000;
        private const UInt32 CFE_SUBSCRIPT = 0x00010000;		/* Superscript and subscript are */
        private const UInt32 CFE_SUPERSCRIPT = 0x00020000;		/*  mutually exclusive			 */

        private const int CFM_SMALLCAPS = 0x0040;			/* (*)	*/
        private const int CFM_ALLCAPS = 0x0080;			/* Displayed by 3.0	*/
        private const int CFM_HIDDEN = 0x0100;			/* Hidden by 3.0 */
        private const int CFM_OUTLINE = 0x0200;			/* (*)	*/
        private const int CFM_SHADOW = 0x0400;			/* (*)	*/
        private const int CFM_EMBOSS = 0x0800;			/* (*)	*/
        private const int CFM_IMPRINT = 0x1000;			/* (*)	*/
        private const int CFM_DISABLED = 0x2000;
        private const int CFM_REVISED = 0x4000;

        private const int CFM_BACKCOLOR = 0x04000000;
        private const int CFM_LCID = 0x02000000;
        private const int CFM_UNDERLINETYPE = 0x00800000;		/* Many displayed by 3.0 */
        private const int CFM_WEIGHT = 0x00400000;
        private const int CFM_SPACING = 0x00200000;		/* Displayed by 3.0	*/
        private const int CFM_KERNING = 0x00100000;		/* (*)	*/
        private const int CFM_STYLE = 0x00080000;		/* (*)	*/
        private const int CFM_ANIMATION = 0x00040000;		/* (*)	*/
        private const int CFM_REVAUTHOR = 0x00008000;


        private const UInt32 CFM_BOLD = 0x00000001;
        private const UInt32 CFM_ITALIC = 0x00000002;
        private const UInt32 CFM_UNDERLINE = 0x00000004;
        private const UInt32 CFM_STRIKEOUT = 0x00000008;
        private const UInt32 CFM_PROTECTED = 0x00000010;
        private const UInt32 CFM_LINK = 0x00000020;
        private const UInt32 CFM_SIZE = 0x80000000;
        private const UInt32 CFM_COLOR = 0x40000000;
        private const UInt32 CFM_FACE = 0x20000000;
        private const UInt32 CFM_OFFSET = 0x10000000;
        private const UInt32 CFM_CHARSET = 0x08000000;
        private const UInt32 CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
        private const UInt32 CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

        private const byte CFU_UNDERLINENONE = 0x00000000;
        private const byte CFU_UNDERLINE = 0x00000001;
        private const byte CFU_UNDERLINEWORD = 0x00000002; /* (*) displayed as ordinary underline	*/
        private const byte CFU_UNDERLINEDOUBLE = 0x00000003; /* (*) displayed as ordinary underline	*/
        private const byte CFU_UNDERLINEDOTTED = 0x00000004;
        private const byte CFU_UNDERLINEDASH = 0x00000005;
        private const byte CFU_UNDERLINEDASHDOT = 0x00000006;
        private const byte CFU_UNDERLINEDASHDOTDOT = 0x00000007;
        private const byte CFU_UNDERLINEWAVE = 0x00000008;
        private const byte CFU_UNDERLINETHICK = 0x00000009;
        private const byte CFU_UNDERLINEHAIRLINE = 0x0000000A; /* (*) displayed as ordinary underline	*/

        #endregion

        #endregion

        [DefaultValue(false)]
        public new bool DetectUrls
        {
            get { return base.DetectUrls; }
            set { base.DetectUrls = value; }
        }

        /// <summary>
        /// Insert a given text as a link into the RichTextBox at the current insert position.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        public void InsertLink(string text)
        {
            InsertLink(text, this.SelectionStart);
        }

        /// <summary>
        /// Insert a given text at a given position as a link. 
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="position">Insert position</param>
        public void InsertLink(string text, int position)
        {
            if (position < 0 || position > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = position;
            this.SelectedText = text;
            this.Select(position, text.Length);
            this.SetSelectionLink(true);
            this.Select(position + text.Length, 0);
        }
        /// <summary>
        /// Selects the text at the specified start and length and creates a hyperlink out of it
        /// </summary>
        /// <param name="startIndex">Start index for the start of the hyperlink</param>
        /// <param name="length">Lenght of the text to select for the hyperlink</param>
        public void AddLinkAt(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = startIndex;
            this.Select(startIndex, length);
            this.SetSelectionLink(true);
            this.Select(startIndex + length, 0);
        }
        /// <summary>
        /// Selects the text at the specified start and length and adds the specified hyperlink text as the link value
        /// </summary>
        /// <param name="startIndex">Start index for the start of the hyperlink</param>
        /// <param name="length">Lenght of the text to select for the hyperlink</param>
        /// <param name="hyperlink">The hyperlink value</param>
        public void AddLinkAt(int startIndex, int length, string hyperlink)
        {
            if (startIndex < 0 || startIndex > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = startIndex;
            this.Select(startIndex, length);
            string text = this.SelectedText;
            //escape out any backslashes
            text = text.Replace(@"\", @"\'5c");
            this.SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";

            int highlightLength = text.Replace(@"\'5c", "").Length;
            this.Select(startIndex, highlightLength + hyperlink.Length +2);
            this.SetSelectionLink(true);
            this.Select(startIndex + highlightLength + hyperlink.Length, 0);
        }

        /// <summary>
        /// Insert a given text at at the current input position as a link.
        /// The link text is followed by a hash (#) and the given hyperlink text, both of
        /// them invisible.
        /// When clicked on, the whole link text and hyperlink string are given in the
        /// LinkClickedEventArgs.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
        public void InsertLink(string text, string hyperlink)
        {
            InsertLink(text, hyperlink, this.SelectionStart);
        }

        /// <summary>
        /// Insert a given text at a given position as a link. The link text is followed by
        /// a hash (#) and the given hyperlink text, both of them invisible.
        /// When clicked on, the whole link text and hyperlink string are given in the
        /// LinkClickedEventArgs.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
        /// <param name="position">Insert position</param>
        public void InsertLink(string text, string hyperlink, int position)
        {
            if (position < 0 || position > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = position;
            this.SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";
            this.Select(position, text.Length + hyperlink.Length + 1);
            this.SetSelectionLink(true);
            this.Select(position + text.Length + hyperlink.Length + 1, 0);
        }

        /// <summary>
        /// Set the current selection's link style
        /// </summary>
        /// <param name="link">true: set link style, false: clear link style</param>
        public void SetSelectionLink(bool link)
        {
            SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
        }
        /// <summary>
        /// Get the link style for the current selection
        /// </summary>
        /// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
        public int GetSelectionLink()
        {
            return GetSelectionStyle(CFM_LINK, CFE_LINK);
        }

        private void SetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.dwMask = mask;
            cf.dwEffects = effect;

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

            Marshal.FreeCoTaskMem(lpar);
        }

        private int GetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.szFaceName = new char[32];

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

            cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

            int state;
            // dwMask holds the information which properties are consistent throughout the selection:
            if ((cf.dwMask & mask) == mask)
            {
                if ((cf.dwEffects & effect) == effect)
                    state = 1;
                else
                    state = 0;
            }
            else
            {
                state = -1;
            }

            Marshal.FreeCoTaskMem(lpar);
            return state;
        }
        #endregion
    }

}