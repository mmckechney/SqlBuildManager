using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Interfaces.ScriptHandling.Tags
{
    public enum TagInferenceSource
    {
        TextOverName,
        ScriptText,
		ScriptName,
		NameOverText,
        None
    }
}
