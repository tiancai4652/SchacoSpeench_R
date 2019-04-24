using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoVoiceCnversionByBaidu
{
    public class BaiDuClient
    {
        static Baidu.Aip.Speech.Asr _client = null;
        static Baidu.Aip.Speech.Asr client
        {
            get
            {
                if (_client == null)
                {
                    _client=Initiall();
                }
                return _client;
            }
        }
        static Baidu.Aip.Speech.Asr Initiall()
        {
            MyBaiduAccount account = MyBaiduAccount.CreateInstance();
            // 设置APPID/AK/SK
            var APP_ID = account.AppID;
            var API_KEY = account.APIKey;
            var SECRET_KEY = account.SecretKey;
            var _client = new Baidu.Aip.Speech.Asr(APP_ID,API_KEY, SECRET_KEY);
            _client.Timeout = 60000;  // 修改超时时间
            return _client;
        }

        // 识别本地录音文件
        public static BaiduSpeechResult AsrData(string file,string language,string format= "pcm")
        {
            string Format = format;
            var data = File.ReadAllBytes(file);
            // 可选参数
            var options = new Dictionary<string, object> {{"dev_pid", language } };
            client.Timeout = 120000; // 若语音较长，建议设置更大的超时时间. ms
            var JObj = client.Recognize(data, Format, 16000, options) ;
            return BaiduSpeechResult.Convertor(JObj);
        }
    }
}
