using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoVoiceCnversionByBaidu
{
   public class BaiduSpeechResult
    {
        public bool IsCorrect { get; set; }
        public string ErrorMsg { get; set; }
        public JArray Result { get; set; }

        public static BaiduSpeechResult Convertor(JObject obj)
        {
            BaiduSpeechResult resutl = new BaiduSpeechResult();
            resutl.IsCorrect = obj["err_no"].Equals("0");
            resutl.ErrorMsg = obj["err_msg"].ToString();
            resutl.Result = obj["result"] as JArray;
            return resutl;
        }
    }
}
