using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace  WebsiteMonitoring
{
    public class csGlobal
    {
        private csGlobal()
        {
        }
        public static bool bCalcelProcessing = true;
        public static string agent_firefox = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:40.0) Gecko/20100101 Firefox";
        public static string strMongodbConnection = "mongodb://192.168.92.200:27017";
        public static string strMongoDatabase = "dbMain";
        public static string strMongoDatabase1 = "BoTuKhoa";
        public static string strUsername="";
        public static string strPermission = "";
        public static string strLastLoginTime = "";
        public static string strLanguage = "English";
        public static bool bConnected = false;
        public static List<string> lstAlertKeyword = new List<string>();
    }
}
