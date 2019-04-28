using CSCore.Streams;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;
using SchacoRecorderer;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchacoRecorder_WPF.ViewModel
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
        public SeriesCollection SeriesCollection { get; set; }

        public DateTime LastAddPointTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            LastAddPointTime = DateTime.Now;
            GetAllDevices();
            OpenDialogCommand = new RelayCommand(OpenDialog);
            PlayPauseCommand = new RelayCommand(PlayPause);
            IniChartSource();


            SeriesCollection = new SeriesCollection();
            SeriesCollection.Add(GetS(LeftSource));
            SeriesCollection.Add(GetS(RightSource));

        }

        LineSeries GetS(ChartValues<ObservableValue> source)
        {
            //实例化一条折线图
            LineSeries mylineseries = new LineSeries();
            //设置折线的标题
            mylineseries.Title = "Temp";
            //折线图直线形式
            mylineseries.LineSmoothness = 0;
            //折线图的无点样式
            mylineseries.PointGeometry = null;
            //添加折线图的数据
            mylineseries.Values = source;
            return mylineseries;
        }

        void GetAllDevices()
        {
            DeviceList = new ObservableCollection<MyAudioInputDevice>(Recorder.GetAllAudioInputDevices(IsCheckCaptureDevice ? SchacoRecorderer.CaptureMode.Capture : SchacoRecorderer.CaptureMode.LoopbackCapture));
        }

        public Recorder Recorder { get; set; }

        bool _IsPlaying = false;
        public bool IsPlaying
        {
            get
            {
                return _IsPlaying;
            }
            set
            {
                _IsPlaying = value;
                RaisePropertyChanged(() => IsPlaying);
            }
        }

        ObservableCollection<MyAudioInputDevice> _DeviceList = new ObservableCollection<MyAudioInputDevice>();
        public ObservableCollection<MyAudioInputDevice> DeviceList
        {
            get
            {
                return _DeviceList;
            }
            set
            {
                _DeviceList = value;
                RaisePropertyChanged(() => DeviceList);
            }
        }

        MyAudioInputDevice _SelectedDevice;
        public MyAudioInputDevice SelectedDevice
        {
            get
            {
                return _SelectedDevice;
            }
            set
            {
                _SelectedDevice = value;
                RaisePropertyChanged(() => SelectedDevice);
            }
        }

        string _SaveFilePath = "";
        public string SaveFilePath
        {
            get
            {
                return _SaveFilePath;
            }
            set
            {
                _SaveFilePath = value;
                RaisePropertyChanged(() => SaveFilePath);
            }
        }

        bool _IsCheckCaptureDevice = false;
        public bool IsCheckCaptureDevice
        {
            get
            {
                return _IsCheckCaptureDevice;
            }
            set
            {
                _IsCheckCaptureDevice = value;
                RaisePropertyChanged(() => IsCheckCaptureDevice);
                GetAllDevices();
            }
        }

        public ICommand OpenDialogCommand { get; set; }
        void OpenDialog()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "WAV (*.wav)|*.wav";
            sfd.Title = "Save";
            sfd.FileName = "";
            if (sfd.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(sfd.FileName))
                {
                    SaveFilePath = sfd.FileName;
                }
            }
        }

        public ICommand PlayPauseCommand { get; set; }
        void PlayPause()
        {
            if (SelectedDevice == null || string.IsNullOrEmpty(SaveFilePath))
            {
                return;
            }
            if (Recorder == null)
            {
                Recorder = new Recorder();
            }
            IsPlaying = !IsPlaying;
            if (!IsPlaying)
            {
                Recorder.StopCapture();
            }
            else
            {
                Recorder.StartCaptureDefaultSetting(SelectedDevice, SaveFilePath, SingleBlockNotificationStreamOnSingleBlockRead);
            }
          
        }

        void IniChartSource()
        {
            for (int i = 0; i < 20; i++)
            {
                ObservableValue x = new ObservableValue(0);
                _LeftSource.Add(x);
                ObservableValue y = new ObservableValue(0);
                _RightSource.Add(y);
            }
        }

        ChartValues<ObservableValue> _LeftSource = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> LeftSource
        {
            get
            {
                return _LeftSource;
            }
            set
            {
                _LeftSource = value;
                RaisePropertyChanged(() => LeftSource);
            }
        }

        ChartValues<ObservableValue> _RightSource = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> RightSource
        {
            get
            {
                return _RightSource;
            }
            set
            {
                _RightSource = value;
                RaisePropertyChanged(() => RightSource);
            }
        }

        private void SingleBlockNotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
            if ((DateTime.Now - LastAddPointTime).TotalMilliseconds > 200)
            {
                LastAddPointTime = DateTime.Now;
                LeftSource.Add(new ObservableValue(e.Left));
                RightSource.Add(new ObservableValue(e.Right));
                if (LeftSource.Count > 100)
                {
                    LeftSource.RemoveAt(0);
                }
                if (RightSource.Count > 100)
                {
                    RightSource.RemoveAt(0);
                }
            }
        }
    }
}