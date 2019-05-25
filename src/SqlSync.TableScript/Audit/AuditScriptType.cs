namespace SqlSync.TableScript.Audit
{
    public enum AuditScriptType
    {
        CreateAuditTable = 1,
        TriggerDisable = 3,
        TriggerEnable = 4, 
        MasterTable = 5,
        CreateInsertTrigger = 6,
        CreateUpdateTrigger = 7,
        CreateDeleteTrigger = 8,
        CreateAllTriggers = 9
     }
}