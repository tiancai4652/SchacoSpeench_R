using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoVoiceCnversion
{
    public class MyBaiduAccount
    {
        static Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private static MyBaiduAccount _Instance = new MyBaiduAccount()
        {
            AppID = GetAppID(),
            APIKey = GetAPIKey(),
            SecretKey = GetSecretKey()
        };
        internal static MyBaiduAccount CreateInstance()
        {
            return _Instance;
        }
        public string AppID { get; set; }
        public string APIKey { get; set; }
        public string SecretKey { get; set; }
        static string GetAppID()
        {
            string result = "16100146";
            if (config.AppSettings.Settings["AppID"] != null)
            {
                result = config.AppSettings.Settings["AppID"].Value;
            }
            return result;
        }
        static string GetAPIKey()
        {
            string result = "F7zgbl4EyPfhiXVRM2gLvtic";
            if (config.AppSettings.Settings["APIKey"] != null)
            {
                result = config.AppSettings.Settings["APIKey"].Value;
            }
            return result;
        }
        static string GetSecretKey()
        {
            string result = "HVWObq0UXAsCjdD2b2BmXo6mK6gNeo20";
            if (config.AppSettings.Settings["SecretKey"] != null)
            {
                result = config.AppSettings.Settings["SecretKey"].Value;
            }
            return result;
        }

        /// <summary>
        /// 更新我的百度账号信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="secretkey"></param>
        public static void UpdateMyAccount(string id, string key, string secretkey)
        {
            if (config.AppSettings.Settings["AppID"] == null)
            {
                config.AppSettings.Settings.Add("AppID", id);
            }
            else
            {
                config.AppSettings.Settings["AppID"].Value = id;
            }

            if (config.AppSettings.Settings["APIKey"] == null)
            {
                config.AppSettings.Settings.Add("APIKey", key);
            }
            else
            {
                config.AppSettings.Settings["APIKey"].Value = key;
            }

            if (config.AppSettings.Settings["SecretKey"] == null)
            {
                config.AppSettings.Settings.Add("SecretKey", secretkey);
            }
            else
            {
                config.AppSettings.Settings["SecretKey"].Value = secretkey;
            }

            config.Save(ConfigurationSaveMode.Modified);
            System.Configuration.ConfigurationManager.RefreshSection("appSettings");

            _Instance = new MyBaiduAccount()
            {
                AppID = GetAppID(),
                APIKey = GetAPIKey(),
                SecretKey = GetSecretKey()
            };
        }
    }
}
