using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Configuration
{
    public interface IConfigurationProvider
    {
        string GetConfigValue(string configName);
        void PutConfigValue(string configName, string configValue);
    }
}
