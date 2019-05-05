using CSCore;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRecord
{
    class Program
    {
        static WasapiCapture soundIn = null;
        static IWaveSource convertedSource = null;
        static SoundInSource soundInSource = null;
        static WaveWriter waveWriter = null;

        static void Main(string[] args)
        {
            Play();
            //PlayWithStream();
        }

        static void Play()
        {

            Console.WriteLine("Select capturing mode:");
            Console.WriteLine("- 1: Capture");
            Console.WriteLine("- 2: LoopbackCapture");

            CaptureMode captureMode = (CaptureMode)ReadInteger(1, 2);
            DataFlow dataFlow = captureMode == CaptureMode.Capture ? DataFlow.Capture : DataFlow.Render;

            var devices = MMDeviceEnumerator.EnumerateDevices(dataFlow, DeviceState.Active);
            if (!devices.Any())
            {
                Console.WriteLine("No devices found.");
                return;
            }

            Console.WriteLine("Select device:");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine("- {0:#00}: {1}", i, devices[i].FriendlyName);
            }
            int selectedDeviceIndex = ReadInteger(Enumerable.Range(0, devices.Count).ToArray());
            var device = devices[selectedDeviceIndex];

            Console.WriteLine("Enter sample rate:");
            int sampleRate;
            do
            {
                sampleRate = ReadInteger();
                if (sampleRate >= 100 && sampleRate <= 200000)
                    break;
                Console.WriteLine("Must be between 1kHz and 200kHz.");
            } while (true);

            Console.WriteLine("Choose bits per sample (8, 16, 24 or 32):");
            int bitsPerSample = ReadInteger(8, 16, 24, 32);

            Console.WriteLine("Choose number of channels (1, 2):");
            int channels = ReadInteger(1, 2);

            using ( soundIn = captureMode == CaptureMode.Capture
                ? new WasapiCapture()
                : new WasapiLoopbackCapture())
            {
                soundIn.Device = device;
                soundIn.Initialize();
                 soundInSource = new SoundInSource(soundIn) { FillWithZeros = false };
                 convertedSource = soundInSource
                    .ChangeSampleRate(sampleRate) // sample rate
                    .ToSampleSource()
                    .ToWaveSource(bitsPerSample); //bits per sample
                using (convertedSource = channels == 1 ? convertedSource.ToMono() : convertedSource.ToStereo())
                {
                    using ( waveWriter = new WaveWriter("out.wav", convertedSource.WaveFormat))
                    {
                        soundInSource.DataAvailable += (s, e) =>
                        {
                            byte[] buffer = new byte[convertedSource.WaveFormat.BytesPerSecond / 2];
                            int read;
                            while ((read = convertedSource.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                waveWriter.Write(buffer, 0, read);
                            }
                        };
                        soundIn.Start();
                        Console.WriteLine("Capturing started ... press any key to stop.");
                        Console.ReadKey();
                        soundIn.Stop();
                    }
                }
            }

            Process.Start("out.wav");
        }

        static void PlayWithStream()
        {

            Console.WriteLine("Select capturing mode:");
            Console.WriteLine("- 1: Capture");
            Console.WriteLine("- 2: LoopbackCapture");

            CaptureMode captureMode = (CaptureMode)ReadInteger(1, 2);
            DataFlow dataFlow = captureMode == CaptureMode.Capture ? DataFlow.Capture : DataFlow.Render;

            var devices = MMDeviceEnumerator.EnumerateDevices(dataFlow, DeviceState.Active);
            if (!devices.Any())
            {
                Console.WriteLine("No devices found.");
                return;
            }

            Console.WriteLine("Select device:");
            for (int i = 0; i < devices.Count; i++)
            {
                Console.WriteLine("- {0:#00}: {1}", i, devices[i].FriendlyName);
            }
            int selectedDeviceIndex = ReadInteger(Enumerable.Range(0, devices.Count).ToArray());
            var device = devices[selectedDeviceIndex];

            Console.WriteLine("Enter sample rate:");
            int sampleRate;
            do
            {
                sampleRate = ReadInteger();
                if (sampleRate >= 100 && sampleRate <= 200000)
                    break;
                Console.WriteLine("Must be between 1kHz and 200kHz.");
            } while (true);

            Console.WriteLine("Choose bits per sample (8, 16, 24 or 32):");
            int bitsPerSample = ReadInteger(8, 16, 24, 32);

            Console.WriteLine("Choose number of channels (1, 2):");
            int channels = ReadInteger(1, 2);

            using ( soundIn = captureMode == CaptureMode.Capture
                ? new WasapiCapture()
                : new WasapiLoopbackCapture())
            {
                soundIn.Device = device;
                soundIn.Initialize();
                 soundInSource = new SoundInSource(soundIn) { FillWithZeros = false };
               

                var singleBlockNotificationStream = new SingleBlockNotificationStream(soundInSource.ChangeSampleRate(sampleRate).ToSampleSource());
                 convertedSource = singleBlockNotificationStream
                    .ToWaveSource(bitsPerSample); //bits per sample


                using (convertedSource = channels == 1 ? convertedSource.ToMono() : convertedSource.ToStereo())
                {
                    using ( waveWriter = new WaveWriter("out.wav", convertedSource.WaveFormat))
                    {
                        soundInSource.DataAvailable += (s, e) =>
                        {
                            byte[] buffer = new byte[convertedSource.WaveFormat.BytesPerSecond / 2];
                            int read;
                            while ((read = convertedSource.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                waveWriter.Write(buffer, 0, read);
                            }
                        };
                        singleBlockNotificationStream.SingleBlockRead += SingleBlockNotificationStreamOnSingleBlockRead;
                        soundIn.Start();
                        Console.WriteLine("Capturing started ... press any key to stop.");
                        Console.ReadKey();
                        soundIn.Stop();
                    }
                }
            }

            Process.Start("out.wav");
        }

        static void Stop()
        {
            if (soundIn != null)
            {
                soundIn.Stop();
                soundIn.Dispose();
                soundIn = null;
                convertedSource.Dispose();
                convertedSource = null;
                soundInSource.Dispose();
                soundInSource = null;
                if (waveWriter is IDisposable)
                    ((IDisposable)waveWriter).Dispose();
            }
        }

        static void SingleBlockNotificationStreamOnSingleBlockRead(object sender, SingleBlockReadEventArgs e)
        {
         
        }

        private static int ReadInteger(params int[] validValues)
        {
            int value;

            do
            {
                value = ReadInteger();
                if (validValues == null || validValues.Any(x => x == value))
                    return value;
                Console.WriteLine("Invalid value");
            } while (true);
        }

        private static int ReadInteger()
        {
            int value;
            while (!Int32.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Invalid value");
            }
            return value;
        }

        enum CaptureMode
        {
            Capture = 1,
            // ReSharper disable once UnusedMember.Local
            LoopbackCapture = 2
        }
    }
}
