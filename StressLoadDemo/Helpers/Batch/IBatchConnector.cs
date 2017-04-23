using Microsoft.Azure.Batch;
using StressLoadDemo.Model.AzureObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Batch
{
    public interface IBatchConnector
    {
        string StorageConnectionString { get; }

        Task<bool> Deploy(TestJob testJob);

        Task<bool> DeleteTest(TestJob testJob);

        Task<bool> DeleteTest(BatchClient client, TestJob testJob);
    }

}
