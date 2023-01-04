namespace SqlBuildManager.Console.Shared
{
    public class ConnectionValidator
    {
        public static bool IsEventHubConnectionString(string input)
        {
            string lc = input.ToLower();
            if (lc.StartsWith("endpoint") && lc.Contains("sharedaccesskeyname") && lc.Contains("sharedaccesskey"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsServiceBusConnectionString(string input)
        {
            return IsEventHubConnectionString(input);
        }

        public static bool IsStorageConnectionString(string input)
        {
            string lc = input.ToLower();
            if (lc.StartsWith("defaultendpointsprotocol") && lc.Contains("accountname") && lc.Contains("accountkey"))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
