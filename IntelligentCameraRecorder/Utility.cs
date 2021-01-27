using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace IntelligentCameraRecorder
{
    class Utility
    {
        /// <summary>
        /// ////////////////////read and write parameters
        /// 
        /// 
        /// 
        /// </summary>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string deVal, StringBuilder retVal,
           int size, string filePath);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        public static void SetValue(string section, string key, string value)
        {
            string strPath = Environment.CurrentDirectory + "\\cameralogger.ini";
            WritePrivateProfileString(section, key, value, strPath);
        }

        public static string GetValue(string section, string key, string defaultVal)
        {
            StringBuilder sb = new StringBuilder(255);
            string strPath = Environment.CurrentDirectory + "\\cameralogger.ini";
            //最好初始缺省值设置为非空，因为如果配置文件不存在，取不到值，程序也不会报错
            GetPrivateProfileString(section, key, defaultVal, sb, 255, strPath);
            return sb.ToString();

        }
        ///
    }
}
