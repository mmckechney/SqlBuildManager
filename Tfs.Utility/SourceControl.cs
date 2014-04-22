using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using Microsoft.TeamFoundation.VersionControl.Client;
using SqlBuildManager.Interfaces.SourceControl;
using System.Linq;
namespace Tfs.Utility
{
    public class SourceControl : ISourceControl
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private TeamFoundationServer tfsServer;
        private string serverName = string.Empty;
        private VersionControlServer versionControlServer;
        private Workspace[] workSpaces;
        public SourceControl(string TfsServerNameUrl)
        {
            serverName = TfsServerNameUrl;
            log.DebugFormat("Retrieved TFS Server name: {0}", serverName);
        }


        public IFileStatus UpdateSourceControl(List<string> fileNames)
        {
            DateTime start = DateTime.Now;
            FileStatus fs = new FileStatus();
            foreach (string file in fileNames)
            {
                SourceControlStatus stat = UpdateSourceControl(file);
                switch (stat)
                {
                    case SourceControlStatus.Added:
                        fs.AddedToSource.Add(file);
                        break;
                    case SourceControlStatus.CheckedOut:
                    case SourceControlStatus.AlreadyPending:
                        fs.CheckedOutFromSource.Add(file);
                        break;
                    case SourceControlStatus.Error:
                    default:
                        fs.SourceError.Add(file);
                        break;
                }
            }
            DateTime end = DateTime.Now;
            TimeSpan duration = end - start;
            log.InfoFormat("Source Control time for {0} files: {1} ms", fileNames.Count().ToString(), duration.Milliseconds.ToString());

            return fs;
        }
        public SourceControlStatus UpdateSourceControl(string fileName)
        {
            DateTime start = DateTime.Now;
            DateTime pendingChange = DateTime.MaxValue;
            DateTime foundStatus = DateTime.MaxValue;
            SourceControlStatus stat = SourceControlStatus.Unknown;
            try
            {

                Workstation myWorkStation = Workstation.Current;
                WorkspaceInfo wsInfo = myWorkStation.GetLocalWorkspaceInfo(Path.GetDirectoryName(fileName));
                if (wsInfo == null)
                {
                    log.WarnFormat("Unable to act on file '{0}'. No active workspace found", fileName);
                    return SourceControlStatus.NotUnderSourceControl;
                }
                Workspace activeWs = this.VersionControlSrv.GetWorkspace(wsInfo);

                string tfsPath = string.Format("$/{0}", this.TfsServer.Name);

                //PendingChange[] pending = activeWs.GetPendingChanges();

                //var x = from p in pending
                //        where p.LocalItem == fileName
                //        select p.LocalItem;
                string lowerCaseName = fileName.ToLowerInvariant();
                PendingChange[] pc = activeWs.GetPendingChanges();
                pendingChange = DateTime.Now;
                var existingPendFound = (from p in pc
                                         where p.LocalItem.ToLowerInvariant() == lowerCaseName && (p.IsAdd || p.IsEdit)
                                         select true).FirstOrDefault();

                if (existingPendFound)
                {
                    foundStatus = DateTime.Now;
                    stat = SourceControlStatus.AlreadyPending;
                    return stat;
                    
                }
                else
                {
                    int add = activeWs.PendAdd(fileName);
                    if (add > 0)
                    {
                        foundStatus = DateTime.Now;
                        log.InfoFormat("Adding file '{0}' to source at '{1}'. PendAdd response {2}", fileName, activeWs.Name, add.ToString());
                        stat = SourceControlStatus.Added;
                        return stat;
                    }
                    else
                    {
                        //int edit = activeWs.PendEdit(new string[] { fileName }, RecursionType.None, null, LockLevel.None, true, PendChangesOptions.GetLatestOnCheckout);
                        int edit = activeWs.PendEdit(fileName);
                        if (edit > 0)
                        {

                            foundStatus = DateTime.Now;
                            log.InfoFormat("Checking out file '{0}' from source at '{1}'. PendEdit response {2}", fileName, activeWs.Name, edit.ToString());
                            stat = SourceControlStatus.CheckedOut;
                        }
                        else
                        {
                            if (!File.Exists(fileName))
                            {
                                log.ErrorFormat("No local copy for file '{0}'. Perform Get Latest to retrive file from source control prior to opening package", fileName);
                                stat = SourceControlStatus.Unknown;
                                try
                                {

                                    //Try to download the file 
                                    string serverPath = activeWs.TryGetServerItemForLocalItem(fileName);
                                    if (serverPath.Length > 0)
                                    {
                                        GetStatus status = activeWs.Get(new GetRequest(fileName, RecursionType.None, VersionSpec.Latest), GetOptions.Overwrite);
                                        log.InfoFormat("Downloaded file '{0}' from source at '{1}'.", fileName, activeWs.Name, edit.ToString());
                                    }
                                }
                                catch
                                {
                                    log.WarnFormat("Unable to download file '{0}' from source at '{1}'.", fileName, activeWs.Name, edit.ToString());
                                }
                            }
                            else
                            {
                                log.WarnFormat("Unable to check out file '{0}' from source at '{1}'. PendEdit response {2}", fileName, activeWs.Name, edit.ToString());
                                stat = SourceControlStatus.Unknown;
                            }
                        }
                        return stat;

                    }
                }
                
            }
            catch (Exception exe)
            {
                log.Error(string.Format("Error Updating source control for file {0}", fileName), exe);
                return SourceControlStatus.Error;
            }
            finally
            {
                log.InfoFormat("Source Control for {0} [{3}]: GetPendingChanges - {1} ms; GetStatus {2} ms", fileName, (pendingChange - start).Milliseconds.ToString(), (foundStatus - pendingChange).Milliseconds.ToString(), stat.ToString());
            }
        }
        public bool FileIsUnderSourceControl(string fileName)
        {
            if (UpdateSourceControl(fileName) == SourceControlStatus.NotUnderSourceControl)
                return false;
            else
                return true;
        }
        
        
        private Workspace GetWorkSpaceForFile(string fileName, Workspace[] workSpaces)
        {
            Workspace[] ws = GetWorkspaces();
            if (ws == null)
                return null;

            for (int i = 0; i < ws.Length; i++)
            {
                try
                {
                    TeamProject tp = ws[i].GetTeamProjectForLocalPath(fileName);
                    log.InfoFormat("Found Team Project {0} ({1}) for file {2}", tp.Name, tp.ArtifactUri, fileName);

                    return ws[i];
                }
                catch (ItemNotMappedException)
                {
                    log.DebugFormat("File '{0}' not mapped to workspace '{1}'", fileName, ws[i].Name);
                }
                catch (Exception exe)
                {
                    log.Error(String.Format("Error getting Workspace for file {0}", fileName), exe);
                    return null;
                }

            }
            return null;

        }
        private Workspace FindAppropriateWorkspaceToAddFile(string fileName, Workspace[] workSpaces)
        {
            string localFilePath = Path.GetDirectoryName(fileName);
            for (int i = 0; i < workSpaces.Length; i++)
            {
                WorkingFolder[] folders = workSpaces[i].Folders;
                for (int x = 0; i < folders.Length; x++)
                {
                    if (String.Equals(folders[x].LocalItem, localFilePath, StringComparison.CurrentCultureIgnoreCase))
                        return workSpaces[i];
                }
            }

            return null;
        }

        /// <summary>
        /// The TFS server we're connected to.
        /// </summary>
        private TeamFoundationServer TfsServer
        {
            get
            {
                try
                {
                    if (tfsServer == null)
                    {
                        tfsServer = TeamFoundationServerFactory.GetServer(serverName);
                        log.InfoFormat("Initialized TFS server {0} at URL {1}", serverName, tfsServer.Uri.ToString());
                        tfsServer.Authenticate();
                    }

                    return tfsServer;
                }
                catch(Exception exe)
                {
                    log.Error(String.Format("Unable to get TFS server for {0}", serverName), exe);
                    return null;
                }
            }
        }

        /// <summary>
        /// The version contorl server we will be using.
        /// </summary>
        private VersionControlServer VersionControlSrv
        {
            get
            {
                try
                {
                    if (versionControlServer == null)
                    {
                        versionControlServer = (VersionControlServer)TfsServer.GetService(typeof(VersionControlServer));
                        log.InfoFormat("Initialized VersionControlServer server GUID {0} from TFS Server {1}", versionControlServer.ServerGuid.ToString(), TfsServer.Name);
                    }

                    return versionControlServer;
                }
                catch(Exception exe)
                {
                    log.Error(String.Format("Unable to get VersionControlServer server for {0}", serverName), exe);
                    return null;
                }
            }
        }


        public Workspace[] GetWorkspaces()
        {

            string hostName = System.Net.Dns.GetHostName().ToString();
            try
            {
                if (this.workSpaces == null)
                {
                    this.workSpaces = VersionControlSrv.QueryWorkspaces(null, VersionControlSrv.AuthorizedUser, hostName);
                    log.DebugFormat("Retrieved {0} workspaces for host {1}", workSpaces.Length.ToString(), hostName);
                    return this.workSpaces;
                }
                else
                {
                    return this.workSpaces;
                }
            }

            catch(Exception exe)
            {
                log.Error(String.Format("Unable to get workspaces for {0}", hostName), exe);
                return null;
            }

        }


    }
}
