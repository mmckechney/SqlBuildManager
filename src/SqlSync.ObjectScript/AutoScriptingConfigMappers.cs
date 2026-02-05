using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.ObjectScript.Models;

#nullable enable

namespace SqlSync.ObjectScript
{
    public static class AutoScriptingConfigMappers
    {
        public static AutoScriptingConfigModel ToModel(this AutoScriptingConfig ds)
        {
            return new AutoScriptingConfigModel(
                AutoScripting: ds?.AutoScripting?.Cast<AutoScriptingConfig.AutoScriptingRow>().Select(r => r.ToModel()).ToList() ?? new List<AutoScripting>(),
                DatabaseScriptConfig: ds?.DatabaseScriptConfig?.Cast<AutoScriptingConfig.DatabaseScriptConfigRow>().Select(r => r.ToModel()).ToList() ?? new List<DatabaseScriptConfig>(),
                PostScriptingAction: ds?.PostScriptingAction?.Cast<AutoScriptingConfig.PostScriptingActionRow>().Select(r => r.ToModel()).ToList() ?? new List<PostScriptingAction>()
            );
        }

        public static AutoScripting ToModel(this AutoScriptingConfig.AutoScriptingRow row)
        {
            return new AutoScripting(
                AllowManualSelection: row.IsAllowManualSelectionNull() ? null : row.AllowManualSelection,
                IncludeFileHeaders: row.IsIncludeFileHeadersNull() ? null : row.IncludeFileHeaders,
                DeletePreExistingFiles: row.IsDeletePreExistingFilesNull() ? null : row.DeletePreExistingFiles,
                ZipScripts: row.IsZipScriptsNull() ? null : row.ZipScripts,
                AutoScripting_Id: row.AutoScripting_Id);
        }

        public static DatabaseScriptConfig ToModel(this AutoScriptingConfig.DatabaseScriptConfigRow row)
        {
            return new DatabaseScriptConfig(
                ServerName: row.IsServerNameNull() ? null : row.ServerName,
                DatabaseName: row.IsDatabaseNameNull() ? null : row.DatabaseName,
                UserName: row.IsUserNameNull() ? null : row.UserName,
                Password: row.IsPasswordNull() ? null : row.Password,
                AuthenticationType: row.IsAuthenticationTypeNull() ? null : row.AuthenticationType,
                ScriptToPath: row.ScriptToPath,
                AutoScripting_Id: row.IsAutoScripting_IdNull() ? null : row.AutoScripting_Id);
        }

        public static PostScriptingAction ToModel(this AutoScriptingConfig.PostScriptingActionRow row)
        {
            return new PostScriptingAction(
                Name: row.IsNameNull() ? null : row.Name,
                Command: row.IsCommandNull() ? null : row.Command,
                Arguments: row.IsArgumentsNull() ? null : row.Arguments,
                AutoScripting_Id: row.IsAutoScripting_IdNull() ? null : row.AutoScripting_Id);
        }

        public static AutoScriptingConfig ToDataSet(this AutoScriptingConfigModel model)
        {
            var ds = new AutoScriptingConfig();

            foreach (var a in model.AutoScripting)
            {
                var row = ds.AutoScripting.NewAutoScriptingRow();
                if (a.AllowManualSelection.HasValue) row.AllowManualSelection = a.AllowManualSelection.Value;
                if (a.IncludeFileHeaders.HasValue) row.IncludeFileHeaders = a.IncludeFileHeaders.Value;
                if (a.DeletePreExistingFiles.HasValue) row.DeletePreExistingFiles = a.DeletePreExistingFiles.Value;
                if (a.ZipScripts.HasValue) row.ZipScripts = a.ZipScripts.Value;
                row.AutoScripting_Id = a.AutoScripting_Id;
                ds.AutoScripting.AddAutoScriptingRow(row);
            }

            foreach (var d in model.DatabaseScriptConfig)
            {
                var row = ds.DatabaseScriptConfig.NewDatabaseScriptConfigRow();
                if (d.ServerName is not null) row.ServerName = d.ServerName;
                if (d.DatabaseName is not null) row.DatabaseName = d.DatabaseName;
                if (d.UserName is not null) row.UserName = d.UserName;
                if (d.Password is not null) row.Password = d.Password;
                if (d.AuthenticationType is not null) row.AuthenticationType = d.AuthenticationType;
                row.ScriptToPath = d.ScriptToPath;
                if (d.AutoScripting_Id.HasValue) row.AutoScripting_Id = d.AutoScripting_Id.Value;
                ds.DatabaseScriptConfig.AddDatabaseScriptConfigRow(row);
            }

            foreach (var p in model.PostScriptingAction)
            {
                var row = ds.PostScriptingAction.NewPostScriptingActionRow();
                if (p.Name is not null) row.Name = p.Name;
                if (p.Command is not null) row.Command = p.Command;
                if (p.Arguments is not null) row.Arguments = p.Arguments;
                if (p.AutoScripting_Id.HasValue) row.AutoScripting_Id = p.AutoScripting_Id.Value;
                ds.PostScriptingAction.AddPostScriptingActionRow(row);
            }

            return ds;
        }
    }
}
