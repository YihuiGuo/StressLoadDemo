using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Model;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;

namespace StressLoadDemo.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        private readonly IStressDataProvider _dataProvider;

        private int _selectedTabIndex;
        private bool _testStart;
        private bool _monitorStart;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                _selectedTabIndex = value;
                RaisePropertyChanged();
            }
        }

        public bool MonitorStart
        {
            get { return _monitorStart; }
            set
            {
                if (value)
                {
                    Messenger.Default.Send<IStressDataProvider>(_dataProvider, "StartMonitor");
                }
            }
        }

        public bool TestStart
        {
            get { return _testStart;}
            set
            {
                if (value)
                {
                    Messenger.Default.Send<IStressDataProvider>(_dataProvider, "StartTest");
                }
            }
        }
        
        public MainViewModel(IStressDataProvider provider)
        {
            _dataProvider = provider;
            Messenger.Default.Register<RequirementMessage>(
                this,
                "AppendRequirementParam",
                AppendToProvider
                );
            Messenger.Default.Register<string>(
                this,
                "BatchJobId",
                AppendBatchJobId
                );
        }

        public void AppendBatchJobId(string batchJobId)
        {
            _dataProvider.BatchJobId = batchJobId;
        }
        public void AppendToProvider(RequirementMessage message)
        {
            _dataProvider.NumOfVm = message.VmCount.ToString();
            _dataProvider.DevicePerVm = message.NumberOfDevicePerVm.ToString();
            _dataProvider.ExpectTestDuration = message.TestDuration.ToString();
            _dataProvider.MessagePerMinute = message.MessagePerMinPerDevice;
            _dataProvider.VmSize = message.AzureVmSize.ToString();
        }
    }
}