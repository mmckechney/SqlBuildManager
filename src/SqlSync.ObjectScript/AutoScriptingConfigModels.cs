using System;
using System.Collections.Generic;

#nullable enable

namespace SqlSync.ObjectScript.Models
{
    public sealed record class AutoScripting(bool? AllowManualSelection, bool? IncludeFileHeaders, bool? DeletePreExistingFiles, bool? ZipScripts, int AutoScripting_Id);

    public sealed record class DatabaseScriptConfig(string? ServerName, string? DatabaseName, string? UserName, string? Password, string? AuthenticationType, string ScriptToPath, int? AutoScripting_Id);

    public sealed record class PostScriptingAction(string? Name, string? Command, string? Arguments, int? AutoScripting_Id);

    public sealed record class AutoScriptingConfigModel(
        IReadOnlyList<AutoScripting> AutoScripting,
        IReadOnlyList<DatabaseScriptConfig> DatabaseScriptConfig,
        IReadOnlyList<PostScriptingAction> PostScriptingAction);
}
