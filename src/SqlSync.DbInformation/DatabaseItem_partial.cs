namespace SqlSync.DbInformation
{
    public partial class DatabaseItem
    {
        public override string ToString()
        {
            return DatabaseName;
        }

        public int? SequenceId { get; set; } = null;
    }
}
