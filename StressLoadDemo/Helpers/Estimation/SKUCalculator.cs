using System;
using StressLoadDemo.Model.AzureConstants;

namespace StressLoadDemo.Helpers
{
    public static class SkuCalculator
    {
        public const double IothubS1Speed = 720;
        public const double IothubS2Speed = 7200;
        public const double IothubS3Speed = 360000;

        public const double VmSmallCapacity = 10000;
        public const double VmMediumCapacity = 20000;
        public const double VmLargeCapacity = 40000;
        public const double VmExtralargeCapacity = 80000;

        public static HubSku CalculateHubSku(int messagePerMinute)
        {
            HubSku sku = new HubSku();
            if (messagePerMinute < IothubS2Speed)
            {
                sku.UnitSize=HubSize.S1;
                sku.UnitCount = (int) Math.Ceiling(messagePerMinute/IothubS1Speed);
            }
            else if (messagePerMinute < IothubS3Speed)
            {
                sku.UnitSize = HubSize.S2;
                sku.UnitCount = (int) Math.Ceiling(messagePerMinute/IothubS2Speed);
            }
            else
            {
                sku.UnitSize=HubSize.S3;
                sku.UnitCount= (int)Math.Ceiling(messagePerMinute / IothubS3Speed);
            }
            return sku;
        }

        public static VmSku CalculateVmSku(int deviceCount)
        {
            var sku = new VmSku();
            if (deviceCount > VmExtralargeCapacity)
            {
                sku.Size=VmSize.extralarge;
                sku.VmCount = (int) Math.Ceiling(deviceCount / VmExtralargeCapacity);
            }
            else if (deviceCount > VmLargeCapacity)
            {
                sku.Size=VmSize.large;
                sku.VmCount= (int)Math.Ceiling(deviceCount / VmLargeCapacity);
            }
            else if (deviceCount > VmMediumCapacity)
            {
                sku.Size = VmSize.medium;
                sku.VmCount = (int) Math.Ceiling(deviceCount / VmMediumCapacity);
            }
            else
            {
                sku.Size=VmSize.small;
                sku.VmCount = (int) Math.Ceiling(deviceCount / VmSmallCapacity);

            }
            return sku;
        } 
    }
}
