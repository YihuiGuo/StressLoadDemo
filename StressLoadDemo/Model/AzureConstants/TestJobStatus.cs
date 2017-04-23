using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.AzureConstants
{
    public enum TestJobStatus
    {
        Unknown,
        Created,
        Provisioning,
        Enqueued,
        Running,
        Finished,
        Failed,
        VerificationPassed,
        VerificationFailed
    }
}
