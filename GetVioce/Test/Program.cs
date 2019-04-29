using SchacoRecorderer;
using SchacoVoiceCnversionByBaidu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var list= Recorder.GetAllAudioInputDevices();
            var t = list[0];
            string Filename = @"C:\Users\Administrator\Desktop\out.wav";
            //Recorder r = new Recorder();
            //r.StartCapture(t, Filename,null);


            //r.StopCapture();


           var result= BaiDuClient.AsrData(Filename, LanguageType.CommonChinese);

        }
    }
}
