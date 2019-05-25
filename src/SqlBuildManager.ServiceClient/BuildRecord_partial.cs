using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.ServiceClient.Sbm.BuildService 
{
   public partial class BuildRecord
    {
       public string ReturnValueString
       {
           get
           {
               return Enum.GetName(typeof(ExecutionReturn), this.returnValue);
           }
       }
       public string RemoteEndPoint
       {
           get;
           set;
       }
       public string RemoteServerName
       {
           get;
           set;
       }
    }
}
