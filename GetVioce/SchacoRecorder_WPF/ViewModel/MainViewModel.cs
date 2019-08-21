using CSCore.Streams;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Microsoft.Win32;
using SchacoRecorderer;
using SchacoVoiceCnversionByBaidu;
using System;
using System.Collections.ObjectModel;
using System.IO;
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
        }

        #region Property
        public DateTime LastAddPointTime { get; set; }

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
                if (SelectedDevice == null&& DeviceList!=null&& DeviceList.Count>0)
                {
                    SelectedDevice = DeviceList[0];
                }
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

        string _SaveFilePath = Gloable.RecordFileName;
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

        string _Words;
        public string Words
        {
            get
            {
                return _Words;
            }
            set
            {
                _Words = value;
                RaisePropertyChanged(() => Words);
            }
        }


        #endregion

        #region Command
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
            var dic = Path.GetDirectoryName(SaveFilePath);
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            IsPlaying = !IsPlaying;
            if (!IsPlaying)
            {
                Recorder.StopCapture();
                #region 翻译

                var result = BaiDuClient.AsrData(SaveFilePath, LanguageType.CommonChinese);
                if (result.IsCorrect)
                {
                    Words = result.Result.ToString();
                }
                #endregion
            }
            else
            {
                Recorder.StartCaptureDefaultSetting2(SelectedDevice, SaveFilePath, SingleBlockNotificationStreamOnSingleBlockRead);
            }
          
        }

        #endregion

        #region Private

        void GetAllDevices()
        {
            DeviceList = new ObservableCollection<MyAudioInputDevice>(Recorder.GetAllAudioInputDevices(IsCheckCaptureDevice ? SchacoRecorderer.CaptureMode.Capture : SchacoRecorderer.CaptureMode.LoopbackCapture));
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

        void IniChartSourceWithStraightLine(ChartValues<ObservableValue> source,int count,float value)
        {
            for (int i = 0; i < count; i++)
            {
                ObservableValue x = new ObservableValue(value);
                source.Add(x);
            }
        }

        void SingleBlockNotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs e)
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

        bool IsSuitableForLimited(SingleBlockReadEventArgs e)
        {
            bool IsLeftOK = false;
            bool IsRightOK = false;
            IsLeftOK = LeftLimitedUp >= e.Left && LeftLimitedDown <= e.Left;
            IsRightOK = RightLimitedUp >= e.Right && RightLimitedDown <= e.Right;
            return IsLeftOK && IsRightOK;
        }

        #endregion

        #region RecordSetting

        int _DelayTimeToSetStopMS = 2000;
        /// <summary>
        /// 持续多长时间无声音后停止录音
        /// </summary>
        public int DelayTimeToSetStopMS
        {
            get
            {
                return _DelayTimeToSetStopMS;
            }
            set
            {
                _DelayTimeToSetStopMS = value;
                RaisePropertyChanged(() => DelayTimeToSetStopMS);
            }
        }

        float _LeftLimitedUp = 5;
        public float LeftLimitedUp
        {
            get
            {
                return _LeftLimitedUp;
            }
            set
            {
                _LeftLimitedUp = value;
                RaisePropertyChanged(() => LeftLimitedUp);
                IniChartSourceWithStraightLine(LeftLimitUpSource,100, LeftLimitedUp);
            }
        }

        float _LeftLimitedDown = -5;
        public float LeftLimitedDown
        {
            get
            {
                return _LeftLimitedDown;
            }
            set
            {
                _LeftLimitedDown = value;
                RaisePropertyChanged(() => LeftLimitedDown);
                IniChartSourceWithStraightLine(LeftLimitDownSource, 100, LeftLimitedDown);
            }
        }

        float _RightLimitedUp = 5;
        public float RightLimitedUp
        {
            get
            {
                return _RightLimitedUp;
            }
            set
            {
                _RightLimitedUp = value;
                RaisePropertyChanged(() => RightLimitedUp);
                IniChartSourceWithStraightLine(RightLimitUpSource, 100, RightLimitedUp);
            }
        }

        float _RightLimitedDown = -5;
        public float RightLimitedDown
        {
            get
            {
                return _RightLimitedDown;
            }
            set
            {
                _RightLimitedDown = value;
                RaisePropertyChanged(() => RightLimitedDown);
                IniChartSourceWithStraightLine(RightLimitDownSource, 100, RightLimitedDown);
            }
        }

        ChartValues<ObservableValue> _LeftLimitUpSource = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> LeftLimitUpSource
        {
            get
            {
                return _LeftLimitUpSource;
            }
            set
            {
                _LeftLimitUpSource = value;
                RaisePropertyChanged(() => LeftLimitUpSource);
            }
        }

        ChartValues<ObservableValue> _RighLimitUpSource = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> RightLimitUpSource
        {
            get
            {
                return _RighLimitUpSource;
            }
            set
            {
                _RighLimitUpSource = value;
                RaisePropertyChanged(() => RightLimitUpSource);
            }
        }

        ChartValues<ObservableValue> _LeftLimitDownSource = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> LeftLimitDownSource
        {
            get
            {
                return _LeftLimitDownSource;
            }
            set
            {
                _LeftLimitDownSource = value;
                RaisePropertyChanged(() => LeftLimitDownSource);
            }
        }

        ChartValues<ObservableValue> _RighLimitDownSource = new ChartValues<ObservableValue>();
        public ChartValues<ObservableValue> RightLimitDownSource
        {
            get
            {
                return _RighLimitDownSource;
            }
            set
            {
                _RighLimitDownSource = value;
                RaisePropertyChanged(() => RightLimitDownSource);
            }
        }

        string _TranslateText = "";
        public string TranslateText
        {
            get
            {
                return _TranslateText;
            }
            set
            {
                _TranslateText = value;
                RaisePropertyChanged(() => TranslateText);
            }
        }






        #endregion


    }
}