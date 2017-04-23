using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Client;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.DataProvider.ReceiverTool;
using Microsoft.Azure.Batch.Common;
using StressLoadDemo.Model.Utility;
using GalaSoft.MvvmLight.Command;

namespace StressLoadDemo.ViewModel
{
    public class TabMonitorViewModel:ViewModelBase
    {
        private const double CanvasWidth = 373;
        private const double CanvasHeight = 194;
        private readonly Timer _refreshDataTimer,_refreshTaskTimer;
        private double _deviceRealTimeNumber, _messageRealTimeNumber;
        private ObservableCollection<MyLine> _deviceLines, _messageLines;
        private List<MyLine> _deviceLineBuffer, _messageLineBuffer;
        private List<double> _deviceNumberBuffer, _messageNumberBuffer;
        private IStressDataProvider _dataProvider;
        private string _selectedPartition ;
        private HubReceiver _hubDataReceiver;
        private DateTime _firstDataArriveTime;
        private string _consumerGroupName;
        private bool _refreshBtnEnabled;
        private Visibility _shadeVisibility;
        private int _taskActiveCount, _taskRunningCount, _taskCompletedCount,_taskTotalCount;
        private ObservableCollection<string> _partitions { get; set; }
        string _testRunTime, _throughput, _d2hDelay, _e2eDelay;
        public TabMonitorViewModel(IStressDataProvider provider)
        {
            _consumerGroupName = "$Default";
            _selectedPartition = "0";
            _dataProvider = provider;
            _shadeVisibility = Visibility.Hidden;
            _partitions = new ObservableCollection<string>();

            Messenger.Default.Register<IStressDataProvider>(
               this,
               "StartMonitor",
               ProcessMonitorConfig
               );

            _refreshDataTimer = new Timer();
            _refreshTaskTimer = new Timer();
            _refreshDataTimer.Elapsed += ObserveData;
            _refreshTaskTimer.Elapsed += ObserveTask;
            _refreshDataTimer.AutoReset = true;
            _refreshTaskTimer.AutoReset = true;
            //fetch data and refresh UI every sec
            //fetch task and refresh UI every 5 sec
            _refreshDataTimer.Interval = 1000;
            _refreshTaskTimer.Interval = 5000;
            Reload = new RelayCommand(StartCollecting);
        }


        #region UIBindingProperties
        public RelayCommand Reload { get; set; }

        public string TestRunTime
        {
            get { return _testRunTime; }
            set
            {
                _testRunTime = value;
                RaisePropertyChanged();
            }
        }

        public string Throughput
        {
            get { return _throughput; }
            set
            {
                _throughput = value;
                RaisePropertyChanged();
            }
        }

        public string DeviceToHubDelay
        {
            get { return _d2hDelay; }
            set
            {
                _d2hDelay = value;
                RaisePropertyChanged();
            }
        }

        public string EndToEndDelay
        {
            get { return _e2eDelay; }
            set
            {
                _e2eDelay = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MyLine> MessageLines
        {
            get { return _messageLines; }
            set
            {
                _messageLines = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MyLine> DeviceLines
        {
            get
            {
                return _deviceLines;
            }
            set
            {
                _deviceLines = value;
                RaisePropertyChanged();
            }
        }

        public double MessageRealTimeNumber
        {
            get { return _messageRealTimeNumber; }
            set
            {
                _messageRealTimeNumber = value;
                RaisePropertyChanged();
            }
        }

        public double DeviceRealTimeNumber
        {
            get { return _deviceRealTimeNumber; }
            set
            {
                _deviceRealTimeNumber = value;
                RaisePropertyChanged();
            }
        }

        public int TaskTotalCount
        {
            get { return _taskTotalCount; }
            set
            {
                _taskTotalCount = value;
                RaisePropertyChanged();
            }
        }

        public int TaskActiveCount
        {
            get { return _taskActiveCount; }
            set
            {
                _taskActiveCount = value;
                RaisePropertyChanged();
            }
        }

        public int TaskRunningCount
        {
            get { return _taskRunningCount; }
            set
            {
                _taskRunningCount = value;
                RaisePropertyChanged();
            }
        }

        public int TaskCompleteCount
        {
            get { return _taskCompletedCount; }
            set
            {
                _taskCompletedCount = value;
                RaisePropertyChanged();
            }
        }

        public Visibility ShadeVisibility
        {
            get { return _shadeVisibility;}
            set
            {
                _shadeVisibility = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Partitions
        {
            get { return _partitions; }
            set
            {
                //do not refresh the combobox too fast.
                if (_partitions.Count != value.Count)
                {
                    _partitions = value;
                    RaisePropertyChanged();
                }
               
            }
        }

        public string ConsumerGroupName
        {
            get { return _consumerGroupName; }
            set
            {
                _consumerGroupName = value;
                RaisePropertyChanged();
                StopCollecting();
                _hubDataReceiver.SetConsumerGroup(value);
                ShadeVisibility = Visibility.Visible;
            }
        }

        public string SelectedPartition
        {
            get { return _selectedPartition; }
            set
            {
                //if value changed after the partitions are fetched from azure,
                //pause the graph for reloading.
                if (value != _selectedPartition && Partitions != null)
                {
                    StopCollecting();
                    _hubDataReceiver.SetPartitionId(int.Parse(value));
                }
                _selectedPartition = value;
                
                RaisePropertyChanged();
            }
        }

        public bool RefreshBtnEnabled
        {
            get { return _refreshBtnEnabled; }
            set
            {
                _refreshBtnEnabled = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        void StopCollecting()
        {
            _refreshDataTimer.Enabled = false;
            ShadeVisibility = Visibility.Visible;
            _hubDataReceiver.PauseReceive();
            RefreshBtnEnabled = true;
        }

        void StartCollecting()
        {
            _hubDataReceiver.StartReceive();
            ShadeVisibility = Visibility.Hidden;
            RefreshBtnEnabled = false;
            _refreshTaskTimer.Enabled = true;
            _refreshDataTimer.Enabled = true;
            _firstDataArriveTime = DateTime.Now;
            _messageLineBuffer = new List<MyLine>();
            _deviceLineBuffer = new List<MyLine>();
            _messageNumberBuffer = new List<double>();
            _deviceNumberBuffer = new List<double>();
            DeviceLines = new ObservableCollection<MyLine>();
            MessageLines = new ObservableCollection<MyLine>();
            DeviceRealTimeNumber = 0;MessageRealTimeNumber = 0;
            TestRunTime = "N/A";Throughput = "N/A";
            EndToEndDelay = "";DeviceToHubDelay = "";
        }

        public void ProcessMonitorConfig(IStressDataProvider provider)
        {
            _hubDataReceiver = new HubReceiver(provider);
            _hubDataReceiver.StartReceive();
            StartCollecting();
        }

        void ObserveTask(object sender,ElapsedEventArgs e)
        {
            //get task running status
            var builder = new UriBuilder(_dataProvider.BatchUrl);
            var BatchAccountName = builder.Host.Split('.').First();
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(_dataProvider.BatchUrl, BatchAccountName, _dataProvider.BatchKey);
            using (BatchClient batchClient = BatchClient.Open(credentials))
            {
                var job = batchClient.JobOperations.GetJob(_dataProvider.BatchJobId);
                var list = job.ListTasks();
                TaskTotalCount = list.Count();
                TaskActiveCount = list.Count(m => m.State == TaskState.Active || m.State == TaskState.Preparing);
                TaskRunningCount = list.Count(m => m.State == TaskState.Running);
                TaskCompleteCount = list.Count(m => m.State == TaskState.Completed);
            }
        }

        void ObserveData(object sender, ElapsedEventArgs e)
        {

            //get partition counts, fill in combo box("lazy" update)
            var queryPartitionNumber = _hubDataReceiver.partitionNumber;
            if (queryPartitionNumber != Partitions.Count)
            {
                ObservableCollection<string> partitionIds = new ObservableCollection<string>();
                for (int i = 0; i < queryPartitionNumber; i++)
                {
                    partitionIds.Add(i.ToString());
                }
                Partitions = partitionIds;
            }


            //get real time number and calculate the curve
            MessageRealTimeNumber = _hubDataReceiver.totalMessage;
            DeviceRealTimeNumber = _hubDataReceiver.totalDevice;

            //collect data for generating graph
            _messageNumberBuffer.Add(_messageRealTimeNumber);
            _deviceNumberBuffer.Add(_deviceRealTimeNumber);

            TestRunTime = _hubDataReceiver.runningTime.ToString();
            Throughput = _hubDataReceiver.throughPut.ToString()+" messages/minute";
            DeviceToHubDelay = _hubDataReceiver.deviceToHubDelay;
            EndToEndDelay = _hubDataReceiver.e2EDelay;

            if (TaskTotalCount == TaskCompleteCount&&TaskTotalCount!=0)
            {
                //stop working threads
                _hubDataReceiver.PauseReceive();
                _refreshTaskTimer.Enabled = false;
                _refreshDataTimer.Enabled = false;

                TransformDataToLines(_deviceNumberBuffer, ref _deviceLineBuffer);
                TransformDataToLines(_messageNumberBuffer, ref _messageLineBuffer);
            }
            else { 
            TransformDataToLines(
                _deviceNumberBuffer
                .ToList()
                .Skip((int)Math.Max(0, _deviceNumberBuffer.Count - CanvasWidth / 2))
                .ToList()
                , ref _deviceLineBuffer);
            TransformDataToLines(
                _messageNumberBuffer
                .ToList()
                .Skip((int)Math.Max(0, _messageNumberBuffer.Count - CanvasWidth / 2))
                .ToList()
                , ref _messageLineBuffer);
            }
            DeviceLines = new ObservableCollection<MyLine>(_deviceLineBuffer);
            MessageLines = new ObservableCollection<MyLine>(_messageLineBuffer);
            

        }

        void TransformDataToLines(List<double> data, ref List<MyLine> targetLines)
        {
            targetLines = new List<MyLine>();
            if (data.Count > 1)
            {
                var maxY = data.Max();
                var rangeY = maxY - data.Min();
                var scaleY = CanvasHeight / rangeY;
                if (rangeY == 0 && maxY == 0) return;
               
                var verticalShift = maxY > 0 ? scaleY * maxY : -scaleY * maxY;
                var xUnit = CanvasWidth / (data.Count - 1);
                double prevX = 0, prevY = verticalShift - scaleY * data[0];
                var temp = new List<MyLine>();
                data.Take(data.Count - 1).ToList().ForEach(p =>
                {
                    p = verticalShift - p * scaleY;

                    temp.Add(new MyLine() { X1 = prevX, Y1 = prevY, X2 = prevX + xUnit, Y2 = p });

                    prevX += xUnit;
                    prevY = p;
                });
                targetLines = temp;
            }
        }
    }
}
