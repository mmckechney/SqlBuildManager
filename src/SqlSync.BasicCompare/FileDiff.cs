namespace SqlSync.BasicCompare
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Drawing;
    using System.Text;

    [Serializable, DesignerCategory("Component"), DesignTimeVisible(true), ToolboxItem(true)]
    public class FileDiff
    {
        private string _FileName = string.Empty;
        private string _UnifiedDiff = string.Empty;
        private Hashtable _validationDict = new Hashtable();

        public FileDiff()
        {
            this._validationDict.Add("FileName", false);
            this._validationDict.Add("UnifiedDiff", false);
        }

        public virtual bool Fill(FileDiff dataClass)
        {
            try
            {
                this.FileName = dataClass.FileName;
                this.UnifiedDiff = dataClass.UnifiedDiff;
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.Fill(FileDiff) Method", exception);
            }
            return true;
        }

        public virtual bool Fill(Array sourceArray)
        {
            try
            {
                this.FileName = (string) Convert.ChangeType(sourceArray.GetValue(0), typeof(string));
                this.UnifiedDiff = (string) Convert.ChangeType(sourceArray.GetValue(1), typeof(string));
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.Fill(System.Array) Method", exception);
            }
            return true;
        }

        public virtual bool Fill(NameValueCollection nameValueColl)
        {
            try
            {
                if (nameValueColl.GetValues("FileName") != null)
                {
                    this.FileName = (string) Convert.ChangeType(nameValueColl.GetValues("FileName")[0], typeof(string));
                }
                if (nameValueColl.GetValues("UnifiedDiff") != null)
                {
                    this.UnifiedDiff = (string) Convert.ChangeType(nameValueColl.GetValues("UnifiedDiff")[0], typeof(string));
                }
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.Fill(NameValueCollection) Method", exception);
            }
            return true;
        }

        public virtual bool Fill(string fixedString)
        {
            try
            {
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.Fill(string) Method", exception);
            }
            return true;
        }

        public virtual bool Fill(SqlDataReader reader, bool closeReader)
        {
            try
            {
                if (!reader.Read())
                {
                    reader.Close();
                    return false;
                }
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.Fill(SqlDataReader) Method", exception);
            }
            finally
            {
                if (closeReader)
                {
                    reader.Close();
                }
            }
            return true;
        }

        public virtual bool Fill(string delimString, char delimiter)
        {
            string[] textArray = delimString.Split(new char[] { delimiter });
            try
            {
                this.FileName = (string) Convert.ChangeType(textArray[0], typeof(string));
                this.UnifiedDiff = (string) Convert.ChangeType(textArray[1], typeof(string));
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.Fill(string,char) Method", exception);
            }
            return true;
        }

        public virtual string GetCustomDelimitedString(string delimiter)
        {
            string text;
            StringBuilder builder = new StringBuilder();
            try
            {
                builder.Append(this.StrFileName);
                builder.Append(delimiter);
                builder.Append(this.StrUnifiedDiff);
                text = builder.ToString();
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.GetCustomDelimitedString(string) Method", exception);
            }
            return text;
        }

        public virtual string GetDelimitedString(string delimiter)
        {
            string text;
            StringBuilder builder = new StringBuilder();
            try
            {
                builder.Append(this.FileName.ToString());
                builder.Append(delimiter);
                builder.Append(this.UnifiedDiff.ToString());
                text = builder.ToString();
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.GetDelimitedString(string) Method", exception);
            }
            return text;
        }

        public virtual string GetFixedLengthString()
        {
            throw new NotImplementedException("GetFixedLengthString() method had not been implemented. No properties have a subStringLength value set");
        }

        public virtual NameValueCollection GetNameValueCollection()
        {
            NameValueCollection values2;
            NameValueCollection values = new NameValueCollection();
            try
            {
                values.Add("FileName", this.FileName.ToString());
                values.Add("UnifiedDiff", this.UnifiedDiff.ToString());
                values2 = values;
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.GetNameValueCollection() Method", exception);
            }
            return values2;
        }

        public virtual string[] GetStringArray()
        {
            string[] textArray2;
            string[] textArray = new string[2];
            try
            {
                textArray[0] = this._FileName.ToString();
                textArray[1] = this._UnifiedDiff.ToString();
                textArray2 = textArray;
            }
            catch (Exception exception)
            {
                throw new ApplicationException("Error in the Auto-Generated: FileDiff.GetStringArray() Method", exception);
            }
            return textArray2;
        }

        public virtual string[] Validate()
        {
            ArrayList list = new ArrayList();
            if (!((bool) Convert.ChangeType(this._validationDict["FileName"], typeof(bool))))
            {
                list.Add("FileName");
            }
            if (!((bool) Convert.ChangeType(this._validationDict["UnifiedDiff"], typeof(bool))))
            {
                list.Add("UnifiedDiff");
            }
            if (list.Count > 0)
            {
                string[] array = new string[list.Count];
                list.CopyTo(array);
                return array;
            }
            return null;
        }

        public virtual string FileName
        {
            get
            {
                return this._FileName;
            }
            set
            {
                this._FileName = value;
                this._validationDict["FileName"] = true;
            }
        }

        public virtual bool IsValid
        {
            get
            {
                return (this.Validate() == null);
            }
        }

        public virtual string StrFileName
        {
            get
            {
                return this._FileName.ToString();
            }
        }

        public virtual string StrUnifiedDiff
        {
            get
            {
                return this._UnifiedDiff.ToString();
            }
        }

        public virtual string UnifiedDiff
        {
            get
            {
                return this._UnifiedDiff;
            }
            set
            {
                this._UnifiedDiff = value;
                this._validationDict["UnifiedDiff"] = true;
            }
        }
    }
}
