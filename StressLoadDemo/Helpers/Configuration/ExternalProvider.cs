using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressLoadDemo.Helpers.Configuration
{
    public class ExternalProvider : IConfigurationProvider
    {
        public string GetConfigValue(string configName)
        {
            try
            {
                return ConfigurationManager.AppSettings[configName];
            }
            catch
            {
                return string.Empty;
            }
        }

        public void PutConfigValue(string configName, string configValue)
        {
            //not implemented.
        }
    }
}
