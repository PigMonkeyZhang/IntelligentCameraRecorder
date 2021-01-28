using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

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

        public static void SetValue(string section, string key, string value,string fPath)
        {
            string strPath = Environment.CurrentDirectory + "\\" + fPath;
            WritePrivateProfileString(section, key, value, strPath);
        }

        public static string GetValue(string section, string key, string defaultVal,string fPath)
        {
            StringBuilder sb = new StringBuilder(255);
            string strPath = Environment.CurrentDirectory +"\\"+ fPath;
            //最好初始缺省值设置为非空，因为如果配置文件不存在，取不到值，程序也不会报错
            GetPrivateProfileString(section, key, defaultVal, sb, 255, strPath);
            return sb.ToString();

        }

        public static bool csvStringMatched(string source, string key)
        {
            string[] sL = source.Split(',');
            foreach(string s in sL)
            {
                if (s.Equals(key))
                    return true;
            }
            return false;
        }
        ///
        public static List<string> getFileNameList(string path, string extName)
        {
            try
            {
                List<string> lst = new List<string>();
                string[] dir = Directory.GetDirectories(path); //文件夹列表
                DirectoryInfo fdir = new DirectoryInfo(path);
                FileInfo[] file = fdir.GetFiles();
                //FileInfo[] file = Directory.GetFiles(path); //文件列表
                if (file.Length != 0 || dir.Length != 0) //当前目录文件或文件夹不为空
                {
                    foreach (FileInfo f in file) //显示当前目录所有文件
                    {
                        if (extName.ToLower().IndexOf(f.Extension.ToLower()) >= 0)
                        {
                            lst.Add(f.Name);
                        }
                    }
                }
                return lst;
            }
            catch (Exception ex)
            {
                MessageBox.Show("有错误了");
                throw ex;
            }
        }
    }
}
