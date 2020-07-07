using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.CommandLine;
using System.Linq;

namespace SqlBuildManager.Console
{
    public static class CommandLineExtension
    {
        public static Option Copy(this Option opt, bool reqired)
        {
            Option newOpt = new Option(opt.RawAliases.ToArray(), opt.Description);
            newOpt.Argument = opt.Argument;
            newOpt.Name = opt.Name;
            newOpt.Required = reqired;

            return newOpt;
        }
    }
}
