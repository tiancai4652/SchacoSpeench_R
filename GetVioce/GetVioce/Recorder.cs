using CSCore;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SchacoRecorderer
{
    /// <summary>
    /// 录音机
    /// use for reference from https://github.com/filoe/cscore Sample.Recorder
    /// </summary>
    public class Recorder
    {
      
        static WaveFormat WaveFormatFromBlob(Blob blob)
        {
            if (blob.Length == 40)
                return (WaveFormat)Marshal.PtrToStructure(blob.Data, typeof(WaveFormatExtensible));
            return (WaveFormat)Marshal.PtrToStructure(blob.Data, typeof(WaveFormat));
        }

        /// <summary>
        /// 获取所有音频输入设备
        /// </summary>
        /// <returns></returns>
        public static List<MyAudioInputDevice> GetAllAudioInputDevices(CaptureMode captureMode = CaptureMode.LoopbackCapture)
        {
            CaptureMode CaptureMode = captureMode;
            List<MyAudioInputDevice> result = new List<MyAudioInputDevice>();
            using (var deviceEnumerator = new MMDeviceEnumerator())
            using (var deviceCollection = deviceEnumerator.EnumAudioEndpoints(
                CaptureMode == CaptureMode.Capture ? DataFlow.Capture : DataFlow.Render, DeviceState.Active))
            {
                foreach (var device in deviceCollection)
                {
                    var deviceFormat = WaveFormatFromBlob(device.PropertyStore[
                        new PropertyKey(new Guid(0xf19f064d, 0x82c, 0x4e27, 0xbc, 0x73, 0x68, 0x82, 0xa1, 0xbb, 0x8e, 0x4c), 0)].BlobValue);

                    MyAudioInputDevice temp = new MyAudioInputDevice();
                    temp.Device = device;
                    temp.Channels = deviceFormat.Channels;
                    temp.CaptureMode = CaptureMode;
                    result.Add(temp);
                }
            }
            return result;
        }

        /// <summary>
        /// 开始录音（默认 16k 采样率、16bit 位深、单声道）
        /// </summary>
        /// <param name="Device">选择的录音设备</param>
        /// <param name="fileName">录音文件路径</param>
        /// <param name="sampleRate">采样率(KHz)[1,200]</param>
        /// <param name="bitsPerSample">位深[8,16,24,32]</param>
        /// <param name="channels">声道数[1,2]</param>
        public void StartCapture(MyAudioInputDevice Device, string fileName, EventHandler<SingleBlockReadEventArgs> eh,int sampleRate=16, int bitsPerSample=16, int channels=1)
        {
            if (sampleRate >= 100 && sampleRate <= 200000)
            {
                return;
            }
            int[] list1 = new int[] { 8, 16, 24, 32 };
            if (!list1.Any(x => x == bitsPerSample))
            {
                return;
            }
            int[] list2 = new int[] { 1, 2 };
            if (!list2.Any(x => x == channels))
            {
                return;
            }
            MMDevice SelectedDevice = Device.Device;
            CaptureMode CaptureMode = Device.CaptureMode;
            if (SelectedDevice == null)
                return;


            if (CaptureMode == CaptureMode.Capture)
                _soundIn = new WasapiCapture();
            else
                _soundIn = new WasapiLoopbackCapture();

            _soundIn.Device = SelectedDevice;
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource
                    .ChangeSampleRate(sampleRate) // sample rate
                    .ToSampleSource());
            //_finalSource = singleBlockNotificationStream.ToWaveSource();

            _finalSource = singleBlockNotificationStream
                    .ToWaveSource(bitsPerSample); //bits per sample
            if (channels == 1)
            {
                _finalSource.ToMono();
            }
            else
            {
                _finalSource.ToStereo();
            }
            _writer = new WaveWriter(fileName, _finalSource.WaveFormat);

            byte[] buffer = new byte[_finalSource.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = _finalSource.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, read);
            };
            if (eh != null)
            {
                singleBlockNotificationStream.SingleBlockRead += eh;
            }
            _soundIn.Start();
        }

        public void StartCaptureDefaultSetting(MyAudioInputDevice Device, string fileName, EventHandler<SingleBlockReadEventArgs> eh)
        {
            if (Device.Device == null)
                return;

            if (Device.CaptureMode == CaptureMode.Capture)
                _soundIn = new WasapiCapture();
            else
                _soundIn = new WasapiLoopbackCapture();

            _soundIn.Device = Device.Device;
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ToSampleSource());
            _finalSource = singleBlockNotificationStream.ToWaveSource();
            _writer = new WaveWriter(fileName, _finalSource.WaveFormat);

            byte[] buffer = new byte[_finalSource.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = _finalSource.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, read);
            };

            if (eh != null)
            {
                singleBlockNotificationStream.SingleBlockRead += eh;
            }

            _soundIn.Start();
        }

        public void StartCaptureDefaultSetting2(MyAudioInputDevice Device, string fileName, EventHandler<SingleBlockReadEventArgs> eh,
            int sampleRate = 16, int bitsPerSample = 16, int channels = 2)
        {
            if (Device.Device == null)
                return;

            if (Device.CaptureMode == CaptureMode.Capture)
                _soundIn = new WasapiCapture();
            else
                _soundIn = new WasapiLoopbackCapture();

            _soundIn.Device = Device.Device;
            _soundIn.Initialize();

            var soundInSource = new SoundInSource(_soundIn);
            var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ChangeSampleRate(sampleRate).ToSampleSource());
            _finalSource = singleBlockNotificationStream.ToWaveSource(bitsPerSample);
            _finalSource = channels == 1 ? _finalSource.ToMono() : _finalSource.ToStereo();
            _writer = new WaveWriter(fileName, _finalSource.WaveFormat);

            byte[] buffer = new byte[_finalSource.WaveFormat.BytesPerSecond / 2];
            soundInSource.DataAvailable += (s, e) =>
            {
                int read;
                while ((read = _finalSource.Read(buffer, 0, buffer.Length)) > 0)
                    _writer.Write(buffer, 0, read);
            };

            if (eh != null)
            {
                singleBlockNotificationStream.SingleBlockRead += eh;
            }

            _soundIn.Start();
        }





        WasapiCapture _soundIn = null;
        IWaveSource _finalSource = null;
        IWriteable _writer = null;


        /// <summary>
        /// 开始录音（默认 16k 采样率、16bit 位深、单声道）
        /// </summary>
        /// <param name="Device">选择的录音设备</param>
        /// <param name="fileName">录音文件路径</param>
        /// <param name="sampleRate">采样率(KHz)[1,200]</param>
        /// <param name="bitsPerSample">位深[8,16,24,32]</param>
        /// <param name="channels">声道数[1,2]</param>
        public void StartCapture2(MyAudioInputDevice Device, string fileName, EventHandler<SingleBlockReadEventArgs> eh, 
            int sampleRate = 16, int bitsPerSample = 16, int channels = 1)
        {


            CaptureMode captureMode = Device.CaptureMode;
            DataFlow dataFlow = captureMode == CaptureMode.Capture ? DataFlow.Capture : DataFlow.Render;

            var device = Device.Device;

            WasapiCapture _soundIn = captureMode == CaptureMode.Capture
                ? new WasapiCapture()
                : new WasapiLoopbackCapture();
                if (true)
                {
                    {
                    _soundIn.Device = device;
                    _soundIn.Initialize();
                        SoundInSource soundInSource = new SoundInSource(_soundIn) { FillWithZeros = false };
                        IWaveSource _finalSource = soundInSource
                            .ChangeSampleRate(sampleRate) // sample rate
                            .ToSampleSource()
                            .ToWaveSource(bitsPerSample); //bits per sample
                    _finalSource = channels == 1 ? _finalSource.ToMono() : _finalSource.ToStereo();
                        
                        if(true)
                        {

                         _writer = new WaveWriter(fileName, _finalSource.WaveFormat);
                            
                            if (true)
                            {
                                soundInSource.DataAvailable += (s, e) =>
                                {
                                    byte[] buffer = new byte[_finalSource.WaveFormat.BytesPerSecond / 2];
                                    int read;
                                    while ((read = _finalSource.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        _writer.Write(buffer, 0, read);
                                    }
                                };
                            _soundIn.Start();
                            //    Console.WriteLine("Capturing started ... press any key to stop.");
                            //    Console.ReadKey();
                            //_soundIn.Stop();
                            }
                            
                        
                    }
                    }
                }
        }


        /// <summary>
        /// 停止录音
        /// </summary>
        public void StopCapture()
        {
            if (_soundIn != null)
            {
                _soundIn.Stop();
                _soundIn.Dispose();
                _soundIn = null;
                _finalSource.Dispose();

                if (_writer is IDisposable)
                    ((IDisposable)_writer).Dispose();
            }
        }
    }
}
