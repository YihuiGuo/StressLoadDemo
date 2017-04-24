using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using StressLoadDemo.Model.DataProvider.ReceiverTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StressLoadDemo.Model.DataProvider
{
    public class HubReceiver
    {
        static Settings configSettings;
        private Thread workThread;
        delegate void DoWork();

        public double totalDevice, totalMessage;
        public int partitionNumber;
        public DateTime currentDate;
        public TimeSpan runningTime;
        public int throughPut;
        public string  deviceToHubDelay;
        public string e2EDelay;
        public string sampleContent;
        public bool pause;
        static EventHubClient _eventHubClient;
        public HubReceiver(IStressDataProvider provider)
        {
            var builder = IotHubConnectionStringBuilder.Create(provider.HubOwnerConectionString);
            configSettings = new Settings();
            configSettings.ConnectionString = $"Endpoint={provider.EventHubEndpoint};SharedAccessKeyName={builder.SharedAccessKeyName};SharedAccessKey={builder.SharedAccessKey}";
            configSettings.Path = builder.HostName.Split('.').First();
            configSettings.PartitionId = "0";
            configSettings.GroupName = "$Default";
            configSettings.StartingDateTimeUtc = DateTime.UtcNow - TimeSpan.FromMinutes(1);
            pause = false;
            workThread = new Thread(() => FetchHubData());
            workThread.Start();
        }

        public void StartReceive()
        {
            pause = false;
            workThread = new Thread(() => FetchHubData());
            workThread.Start();
        }

        public void PauseReceive()
        {
            pause = true;
            totalDevice = 0;totalMessage = 0;
        }

        public void SetPartitionId(int targetid)
        {
            configSettings.PartitionId = targetid.ToString();
        }

        public void SetConsumerGroup(string targetConsumerGroupName)
        {
            configSettings.PartitionId = targetConsumerGroupName;
        }

        private void FetchHubData()
        {
            var indicators = new Indicators();
            indicators.Reset();

            var cts = new CancellationTokenSource();
            if (_eventHubClient == null)
            {
                Messenger.Default.Send($"Opening '{configSettings.Path}', partition {configSettings.PartitionId}, consumer group '{configSettings.GroupName}', StartingDateTimeUtc = {configSettings.StartingDateTimeUtc}", "MonitorLog");
                _eventHubClient = EventHubClient.CreateFromConnectionString(configSettings.ConnectionString, configSettings.Path);
            }
            var partition = _eventHubClient.GetRuntimeInformation().PartitionCount;
            partitionNumber = partitionNumber == 0 ? partition : partitionNumber;

            var consumerGroup = _eventHubClient.GetConsumerGroup(configSettings.GroupName);
            var receiver = consumerGroup.CreateReceiver(configSettings.PartitionId, configSettings.StartingDateTimeUtc);
            var stopwatch = Stopwatch.StartNew();
            while (!pause)
            {
                var eventData = receiver.Receive(TimeSpan.FromSeconds(1));
                if (eventData != null)
                {
                    indicators.Push(eventData);
                    totalDevice = indicators.TotalDevices;
                    totalMessage = indicators.TotalMessages;
                    currentDate = DateTime.Now;
                    runningTime = stopwatch.Elapsed;
                    throughPut = (int)(indicators.TotalMessages / stopwatch.Elapsed.TotalMinutes);
                    deviceToHubDelay = FormatDelay(indicators.DeviceToIoTHubDelay.StreamAvg);
                    e2EDelay = FormatDelay(indicators.E2EDelay.StreamAvg);
                    sampleContent = $"from" + $" [{indicators.SampleEventSender}]";
                }
                Thread.Sleep(100);
            }
        }

        private string FormatDelay(double value)
        {
            return double.IsNaN(value) ? "N/A" : TimeSpan.FromMilliseconds(value).ToString();
        }
    }
}
