using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Model;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;
using System.Windows.Media;

namespace StressLoadDemo.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class TabDashboardViewModel : ViewModelBase
    {
        //max length of data buffered for drawing graph.
        //Hardcoded to the width of the canvas.
        private const double CanvasWidth = 265;
        private const double CanvasHeight = 111;

        private readonly IStressDataProvider _dataProvider;

        private string _hubOwnerConnectionString;
        private string _eventHubEndpoint;
        private string _batchServiceUrl;
        private string _batchAccountKey;
        private string _storageAccountConnectionString;
        private bool _canStartTest;
        //private readonly System.Timers.Timer _refreshDataTimer;
        string logmsg;
        bool isLogsChangedPropertyInViewModel;
        //private double _deviceRealTimeNumber, _messageRealTimeNumber;
        //private ObservableCollection<MyLine> _deviceLines, _messageLines;
        //private List<MyLine> _deviceLineBuffer, _messageLineBuffer;
        //private Queue<double> _deviceNumberBuffer, _messageNumberBuffer;
        public DeployPhase _currentDeployPhase;
        public PhaseStatus _currentPhaseStatus;

        private int _progressBarValue;
        private Brush[] _lableBgColors;
        /// <summary>
        /// Initializes a new instance of the TabDashboardViewModel class.
        /// </summary>
        public TabDashboardViewModel(IStressDataProvider provider)
        {
            Messenger.Default.Register<IStressDataProvider>(
                this,
                "StartTest",
                data => ProcessRunConfigValue(data)
                );
            Messenger.Default.Register<string>(
                this,
                "RunningLog",
                msg => ShowLog(msg)
                );
            Messenger.Default.Register<DeployStatusUpdateMessage>(
               this,
               "DeployStatus",
               message => SetDeployStatus(message)
               );
            _lableBgColors = new Brush[5] {
                Brushes.White,
                Brushes.White ,
                Brushes.White ,
                Brushes.White ,
                Brushes.White };
            _currentDeployPhase = DeployPhase.DeployStarted;
            _currentPhaseStatus = PhaseStatus.Succeeded;
            _dataProvider = provider;
            _canStartTest = false;

        }

        #region BindingProperties
        public bool CanStartTest
        {
            get
            {
                return _canStartTest;
            }
            set
            {
                _canStartTest = value;
                RaisePropertyChanged();
            }
        }

        public string HubOwnerConnectionString
        {
            get { return _hubOwnerConnectionString; }
            set
            {
                _hubOwnerConnectionString = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string EventHubEndpoint
        {
            get
            {
                return _eventHubEndpoint;
            }
            set
            {
                _eventHubEndpoint = value;
                RaisePropertyChanged();
                TryActivateButton();

            }
        }

        public string BatchServiceUrl
        {
            get { return _batchServiceUrl; }
            set
            {
                _batchServiceUrl = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string BatchAccountKey
        {
            get { return _batchAccountKey; }
            set
            {
                _batchAccountKey = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string StorageAccountConnectionString
        {
            get { return _storageAccountConnectionString; }
            set
            {
                _storageAccountConnectionString = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }
        #endregion
        public int ProgressValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                RaisePropertyChanged();
            }
        }
        public Brush StartLableBgColor
        {
            get
            {
                return _lableBgColors[0];
            }
            set
            {
                _lableBgColors[0] = value;
                RaisePropertyChanged();
            }
        }
        public Brush PoolLableBgColor
        {
            get
            {
                return _lableBgColors[1];
            }
            set
            {
                _lableBgColors[1] = value;
                RaisePropertyChanged();
            }
        }
        public Brush AssemblyLableBgColor
        {
            get
            {
                return _lableBgColors[2];
            }
            set
            {
                _lableBgColors[2] = value;
                RaisePropertyChanged();
            }
        }

        public Brush JobLableBgColor
        {
            get
            {
                return _lableBgColors[3];
            }
            set
            {
                _lableBgColors[3] = value;
                RaisePropertyChanged();
            }
        }
        public Brush FinishLableBgColor
        {
            get
            {
                return _lableBgColors[4];
            }
            set
            {
                _lableBgColors[4] = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand StartTest => new RelayCommand(RunTest);

        public string LogMsg
        {
            get { return logmsg; }
            set
            {
                logmsg = value;
                IsLogsChangedPropertyInViewModel = true;
                RaisePropertyChanged();
            }
        }
        void RunTest()
        {
            new ViewModelLocator().Main.TestStart = true;
            StartLableBgColor = Brushes.DarkGray;
        }
        public bool IsLogsChangedPropertyInViewModel
        {
            get { return isLogsChangedPropertyInViewModel; }
            set
            {
                isLogsChangedPropertyInViewModel = value;
                RaisePropertyChanged();
            }
        }

        void ShowLog(object message)
        {
            LogMsg += message;
            LogMsg += "\n";
        }
        //public ObservableCollection<MyLine> MessageLines
        //{
        //    get { return _messageLines; }
        //    set
        //    {
        //        _messageLines = value;
        //        RaisePropertyChanged();
        //    }
        //}
        //public ObservableCollection<MyLine> DeviceLines
        //{
        //    get
        //    {
        //        return _deviceLines;
        //    }
        //    set
        //    {
        //        _deviceLines = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public double MessageRealTimeNumber
        //{
        //    get { return _messageRealTimeNumber; }
        //    set
        //    {
        //        _messageRealTimeNumber = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public double DeviceRealTimeNumber
        //{
        //    get { return _deviceRealTimeNumber; }
        //    set
        //    {
        //        _deviceRealTimeNumber = value;
        //        RaisePropertyChanged();
        //    }
        //}
        void SetDeployStatus(DeployStatusUpdateMessage status)
        {
            var deployPhase = (int)status.Phase;
            switch (deployPhase)
            {
                case 1:
                    PoolLableBgColor = Brushes.DarkGray;
                    ProgressValue = 1;
                    break;
                case 2:
                    AssemblyLableBgColor = Brushes.DarkGray;
                    ProgressValue = 2;
                    break;
                case 3:
                    JobLableBgColor = Brushes.DarkGray;
                    ProgressValue = 3;
                    break;
                case 4:
                    FinishLableBgColor = Brushes.DarkGray;
                    ProgressValue = 4;
                    MoveOnToMonitor();
                    break;
            }
            var CurrentPhaseStatus = status.Status;

        }
        void MoveOnToMonitor()
        {
            var mainvm = new ViewModelLocator().Main;
            mainvm.SelectedTabIndex = 2;
            mainvm.MonitorStart = true;
        }

        void ProcessRunConfigValue(IStressDataProvider provider)
        {
            provider.BatchKey = _batchAccountKey;
            provider.HubOwnerConectionString = _hubOwnerConnectionString;
            provider.EventHubEndpoint = _eventHubEndpoint;
            provider.BatchUrl = _batchServiceUrl;
            provider.StorageAccountConectionString = _storageAccountConnectionString;
            provider.Run();

        }

        void TryActivateButton()
        {
            if (!(string.IsNullOrEmpty(_hubOwnerConnectionString) ||
                string.IsNullOrEmpty(_eventHubEndpoint) ||
                string.IsNullOrEmpty(_batchAccountKey) ||
                string.IsNullOrEmpty(_batchServiceUrl) ||
                string.IsNullOrEmpty(_storageAccountConnectionString))
                )
            {
                CanStartTest = true;
            }
            else
            {
                CanStartTest = false;
            }
        }
    }
}