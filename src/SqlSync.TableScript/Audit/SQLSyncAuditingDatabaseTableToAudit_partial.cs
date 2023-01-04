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
                return _HasAuditTable;
            }
            set
            {
                _HasAuditTable = value;
            }
        }

        [XmlIgnore]
        public virtual bool HasAuditUpdateTrigger
        {
            get
            {
                return _HasAuditUpdateTrigger;
            }
            set
            {
                _HasAuditUpdateTrigger = value;
            }
        }

        [XmlIgnore]
        public virtual bool HasAuditInsertTrigger
        {
            get
            {
                return _HasAuditInsertTrigger;
            }
            set
            {
                _HasAuditInsertTrigger = value;
            }
        }

        [XmlIgnore]
        public virtual bool HasAuditDeleteTrigger
        {
            get
            {
                return _HasAuditDeleteTrigger;
            }
            set
            {
                _HasAuditDeleteTrigger = value;
            }
        }

        [XmlIgnore]
        public virtual string TableName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }
    }
}
