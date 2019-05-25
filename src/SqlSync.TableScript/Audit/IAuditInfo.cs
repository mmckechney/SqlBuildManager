using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.TableScript.Audit
{
    public interface IAuditInfo
    {
        bool HasAuditTable
        {
            get;
            set;
        }

        bool HasAuditUpdateTrigger
        {
            get;
            set;
        }

        bool HasAuditInsertTrigger
        {
            get;
            set;
        }

        bool HasAuditDeleteTrigger
        {
            get;
            set;
        }

        string TableName
        {
            get;
            set;
        }
    }
}
