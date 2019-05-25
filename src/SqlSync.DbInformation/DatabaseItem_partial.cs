using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.DbInformation
{
    public partial class DatabaseItem
    {
        public override string ToString()
        {
            return this.DatabaseName;
        }
    }
}
