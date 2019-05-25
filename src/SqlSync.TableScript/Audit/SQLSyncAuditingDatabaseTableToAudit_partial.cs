using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace SqlSync.TableScript.Audit
{
    public partial class SQLSyncAuditingDatabaseTableToAudit : IAuditInfo
    {
        private bool _HasAuditTable = false;

        private bool _HasAuditUpdateTrigger = false;

        private bool _HasAuditInsertTrigger = false;

        private bool _HasAuditDeleteTrigger = false;
        [XmlIgnore]
        public virtual bool HasAuditTable
        {
            get
            {
                return this._HasAuditTable;
            }
            set
            {
                this._HasAuditTable = value;
            }
        }

        [XmlIgnore]
        public virtual bool HasAuditUpdateTrigger
        {
            get
            {
                return this._HasAuditUpdateTrigger;
            }
            set
            {
                this._HasAuditUpdateTrigger = value;
            }
        }

        [XmlIgnore]
        public virtual bool HasAuditInsertTrigger
        {
            get
            {
                return this._HasAuditInsertTrigger;
            }
            set
            {
                this._HasAuditInsertTrigger = value;
            }
        }

        [XmlIgnore]
        public virtual bool HasAuditDeleteTrigger
        {
            get
            {
                return this._HasAuditDeleteTrigger;
            }
            set
            {
                this._HasAuditDeleteTrigger = value;
            }
        }

        [XmlIgnore]
        public virtual string TableName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}
