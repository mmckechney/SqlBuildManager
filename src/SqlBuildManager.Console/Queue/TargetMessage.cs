using Azure.ResourceManager.Network.Models;
using SqlSync.Connection;
using System.Collections.Generic;

namespace SqlBuildManager.Console.Queue
{
    public class TargetMessage
    {
        public string ServerName { get; set; } = string.Empty;
        public List<DatabaseOverride> DbOverrideSequence { get; set; } = null!;

        private string concurrencyTag = string.Empty;
        public string ConcurrencyTag
        {
            get
            {
                if (this.concurrencyTag.Length > 0)
                {
                    return this.concurrencyTag;
                }
                else
                {
                    if (this.DbOverrideSequence != null && this.DbOverrideSequence.Count > 0)
                    {
                        if (this.DbOverrideSequence[0].ConcurrencyTag.Length > 0)
                        {
                            return this.DbOverrideSequence[0].ConcurrencyTag;
                        }

                    }
                }
                return string.Empty;
            }
            set
            {
                this.concurrencyTag = value;
            }
        }

    }
}
