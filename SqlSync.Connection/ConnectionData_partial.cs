using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.Connection
{
    public partial class ConnectionData
    {
        /// <summary>
        /// Quick constructor. Automatically sets UseWindowsAuthentication to true
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="databaseName"></param>
        public ConnectionData(string serverName, string databaseName) : this()
        {
            this._SQLServerName = serverName;
            this._DatabaseName = databaseName;
            this._UseWindowAuthentication = true;
        }
    }
}
