using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace IntelligentCameraRecorder
{
    class CSVFileHelper
    {
        public string filePath;
        private string currtFileName = "";
        private string fullFileName;
        private FileStream fs;
        private StreamWriter sw;
        private CCDInfo[] ccdList = null;
        private int ccdnum;
        private int lineCounter = 0;//当前写入的行数
        private string currentParameterFileName = "cameralogger.ini";
        public void updateParameterFileName(string newFileName)
        {
            currentParameterFileName = newFileName;
            //下面要更新所有参数，谢谢
            //1. 刷新默认文件的参数
            Utility.SetValue("system", "currentParameterFilePath", currentParameterFileName, "cameralogger.ini");
            //2. 关闭当前csv文件
            close();

            //3. 重新打开新的csv文件
            openAndWriteHeads();
        }
        
        
        public string getParameterFileName()
        {
            return currentParameterFileName;
        }
        
        public CSVFileHelper(string fPath)
        {
            filePath = fPath;
            currentParameterFileName = Utility.GetValue("system", "currentParameterFilePath", "cameralogger.ini", "cameralogger.ini");
            openAndWriteHeads();
        }
        private void openAndWriteHeads()
        {
            bool isAppend = false;
            string sFilePre = Utility.GetValue("system", "currentMaterialName", "bupi1", currentParameterFileName);
            string fileName = sFilePre+"-"+DateTime.Now.ToString("yyyy-MM-dd-HH");
            if (fileName.Equals(currtFileName))
            {
                //当前文件存在，那么以追加方式打开。

            }
            else
                currtFileName = fileName;
            fullFileName = filePath + "\\" + currtFileName + ".csv";
            FileInfo fi = new FileInfo(fullFileName);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            if (!fi.Exists)
                fs = new FileStream(fullFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            else
            {
                fs = new FileStream(fullFileName, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                isAppend = true;
            }



            //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            sw = new StreamWriter(fs, System.Text.Encoding.UTF8);

            //get heads from parameter file
            ccdnum = int.Parse(Utility.GetValue("camera", "ccdnum", "2",currentParameterFileName));
            ccdList = new CCDInfo[ccdnum];
            string dataLine = "order,time,";
            for (int i = 0; i < ccdnum; i++)
            {
                ccdList[i] = new CCDInfo();
                ccdList[i].ccd_name = "ccd" + (i + 1);
                ccdList[i].columns_names = Utility.GetValue("camera", ccdList[i].ccd_name, null,currentParameterFileName);
                if (null != ccdList[i].columns_names)
                {
                    dataLine += ccdList[i].columns_names;
                    if (i < ccdnum - 1)
                        dataLine += ",";
                }

            }
            if (isAppend)
                return;
            //set heads here
            sw.WriteLine(dataLine);
        }
        public void flushCSV()
        {
            //close current csv
            if (null != sw)
                sw.Close();
            if (null != fs)
                fs.Close();
            sw = null;
            fs = null;

            //reopen new csv file to write
            openAndWriteHeads();
        }

        private void initCCDValues(CCDInfo ccdI)
        {
            if (null == ccdI )
                return;
            if (null == ccdI.columns_names)
                return;
            string[] strL = ccdI.columns_names.Split(',');
            string vals = "";
            for(int i = 0; i < strL.Length; i++)
            {
                vals += " ";
                if (i < strL.Length - 1)
                    vals += ",";
            }
            ccdI.columns_values = vals;
            ccdI.isDirty = false;
        }
        public void updateALine()
        {
            //判断当前时间是否需要更新文件名
            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HH");
            if (!fileName.Equals(currtFileName))
                flushCSV();

            string dL = "";
            dL += lineCounter++ ;
            dL += ",";
            dL += DateTime.Now.ToString("HH:mm:ss");
            dL += ",";
            for (int i = 0; i < ccdList.Length; i++)
            {
                dL += ccdList[i].columns_values;
                initCCDValues(ccdList[i]);
                ccdList[i].isDirty = false;
                if (i < ccdList.Length - 1)
                    dL += ",";
            }
            if (null != sw)
                sw.WriteLine(dL);

        }

        public void close()
        {
            //close current csv
            if (null != sw)
                sw.Close();
            if (null != fs)
                fs.Close();
            sw = null;
            fs = null;
        }

        private string trimHead(string val)
        {
            string retV = "";
            string[] sL = val.Split(',');
            for(int i = 1; i < sL.Length; i++)
            {
                retV += sL[i];
                if (i < sL.Length - 1)
                    retV += ",";
            }
            return retV;
        }
        public void updateCCDValue(string values)
        {
            int i;
            bool isMatched = false;
            //0. 先判断ccd列表是否已经初始化了
            if (null == ccdList) 
            {
                MessageBox.Show("请确认参数文件存在");
                return;
            }
            if(null == ccdList[0])
            {
                MessageBox.Show("请确认参数文件存在");
                return;
            }
            //1. 找到values 锁定的的ccd
            for(i = 0; i< ccdList.Length; i++)
            {
                if (values.Contains(ccdList[i].ccd_name))
                {
                    isMatched = true;
                    break;
                }
                    
            }
            if (isMatched==false)
            {
                MessageBox.Show("数据错误，未找到摄像头信息");
                return;            
            }

            //2. 把values中ccd信息去除
            //values = values.Trim((ccdList[i].ccd_name + ",").ToCharArray());
            values = trimHead(values);
            //3. 判断当前ccd是否脏了
            //3.1 如果脏了，就写入一行数据，更新脏位，当前ccd仍旧为脏
            //3.2 如果没脏，更新脏位和数据值
            if (ccdList[i].isDirty)
            {
                //更新一行数据
                updateALine();
               // MessageBox.Show("更新一行的机会来啦");
            }

            //最后无论如何都要更新这个ccd数据的对吧。
            ccdList[i].isDirty = true;
            ccdList[i].columns_values = values;
            
               
        }
        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static void SaveCSV(DataTable dt, string fullPath)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
            string data = "";
            //写出列名称
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);
            //写出各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    string str = dt.Rows[i][j].ToString();
                    str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                    if (str.Contains(',') || str.Contains('"')
                        || str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                    {
                        str = string.Format("\"{0}\"", str);
                    }

                    data += str;
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
            DialogResult result = MessageBox.Show("CSV文件保存成功！");
            if (result == DialogResult.OK)
            {
            
              //  System.Diagnostics.Process.Start("explorer.exe", Common.PATH_LANG);
            }
        }
        public static System.Text.Encoding GetType(FileStream fs)
        {
            /*byte[] Unicode=new byte[]{0xFF,0xFE};  
            byte[] UnicodeBIG=new byte[]{0xFE,0xFF};  
            byte[] UTF8=new byte[]{0xEF,0xBB,0xBF};*/

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            byte[] ss = r.ReadBytes(3);
            r.Close();
            //编码类型 Coding=编码类型.ASCII;   
            if (ss[0] >= 0xEF)
            {
                if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
                {
                    return System.Text.Encoding.UTF8;
                }
                else if (ss[0] == 0xFE && ss[1] == 0xFF)
                {
                    return System.Text.Encoding.BigEndianUnicode;
                }
                else if (ss[0] == 0xFF && ss[1] == 0xFE)
                {
                    return System.Text.Encoding.Unicode;
                }
                else
                {
                    return System.Text.Encoding.Default;
                }
            }
            else
            {
                return System.Text.Encoding.Default;
            }
        }


        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static DataTable OpenCSV(string filePath)
        {
            
            DataTable dt = new DataTable();
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            Encoding encoding = GetType(fs); //Encoding.ASCII;//
            //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            StreamReader sr = new StreamReader(fs, encoding);
            //string fileContent = sr.ReadToEnd();
            //encoding = sr.CurrentEncoding;
            //记录每次读取的一行记录
            string strLine = "";
            //记录每行记录中的各字段内容
            string[] aryLine = null;
            string[] tableHead = null;
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool IsFirst = true;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                //strLine = Common.ConvertStringUTF8(strLine, encoding);
                //strLine = Common.ConvertStringUTF8(strLine);

                if (IsFirst == true)
                {
                    tableHead = strLine.Split(',');
                    IsFirst = false;
                    columnCount = tableHead.Length;
                    //创建列
                    for (int i = 0; i < columnCount; i++)
                    {
                        DataColumn dc = new DataColumn(tableHead[i]);
                        dt.Columns.Add(dc);
                    }
                }
                else
                {
                    aryLine = strLine.Split(',');
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columnCount; j++)
                    {
                        dr[j] = aryLine[j];
                    }
                    dt.Rows.Add(dr);
                }
            }
            if (aryLine != null && aryLine.Length > 0)
            {
                dt.DefaultView.Sort = tableHead[0] + " " + "asc";
            }

            sr.Close();
            fs.Close();
            return dt;
        }
        
    }
}
