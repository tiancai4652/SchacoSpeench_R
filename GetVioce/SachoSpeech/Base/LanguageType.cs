using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchacoVoiceCnversion
{
    public class LanguageType
    {
        //ev_pid 参数列表
        //dev_pid 语言  模型 是否有标点   备注
        //1536	普通话(支持简单的英文识别)  搜索模型 无标点 支持自定义词库
        //1537	普通话(纯中文识别)  输入法模型 有标点 支持自定义词库
        //1737	英语 无标点 不支持自定义词库
        //1637	粤语 有标点 不支持自定义词库
        //1837	四川话 有标点 不支持自定义词库
        //1936	普通话远场 远场模型    有标点 不支持


        /// <summary>
        /// 普通话(支持简单的英文识别)  搜索模型 无标点 支持自定义词库
        /// </summary>
        public static string CommonChinese = "1536";
        /// <summary>
        /// 普通话(纯中文识别)  输入法模型 有标点 支持自定义词库
        /// </summary>
        public static string CommonChineseWithEnglish = "1537";
        /// <summary>
        /// 英语 无标点 不支持自定义词库
        /// </summary>
        public static string English = "1737";
        /// <summary>
        /// 粤语 有标点 不支持自定义词库
        /// </summary>
        public static string Cantonese = "1637";
        /// <summary>
        /// 四川话 有标点 不支持自定义词库
        /// </summary>
        public static string SiChuan = "1837";
        /// <summary>
        /// 普通话远场 远场模型    有标点 不支持
        /// </summary>
        public static string CommonChineseDistance = "1837";
    }
}
