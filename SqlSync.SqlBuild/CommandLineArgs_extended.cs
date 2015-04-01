﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSync.SqlBuild
{
    public partial class CommandLineArgs
    {
        [System.Runtime.Serialization.DataMember()]
        public virtual bool GetDifference { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual bool Synchronize { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual string GoldDatabase { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual string GoldServer { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public bool ContinueOnFailure { get; set; }
    }
}