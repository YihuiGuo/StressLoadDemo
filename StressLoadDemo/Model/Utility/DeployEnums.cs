using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.Utility
{
    public enum DeployPhase
    {
        DeployStarted,
        PoolCreated,
        AssemblyUploaded,
        JobCreated,
        JobStarted
    }

    public enum PhaseStatus
    {
        Succeeded,
        Failed
    }
 
}
