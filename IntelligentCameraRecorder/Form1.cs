using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.IO.Ports;

namespace IntelligentCameraRecorder
{
    public partial class Form1 : Form
    {
        private Socket clientSocket;
        private SerialPort ComDevice;
        private Boolean isConnected = false;
        private Boolean isComConnected = false;
        private int showLinesSocket = 0;
        private int port;
        private string socketip;
        private CSVFileHelper csvHelper=null;
       // private string outputPath = Environment.CurrentDirectory;
        private Thread ThreadReceive,ComThreadReceive;
        private bool isStarted = false;


        private void connectSocket(string IP, Int32 Port)
        {
            if (isConnected)
                return;
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(IP, Port);//连接到指定服务器
                Console.WriteLine("connect server succeed ok!");//提示信息
                //收到消息是线程会被挂起，创建新线程处理收到的消息
                ThreadReceive = new Thread(Receive);
                ThreadReceive.IsBackground = true;
                ThreadReceive.Start();
                isConnected = true;
            }
            catch (SocketException e)
            {
                Console.WriteLine("无法连接到{0}:{1}{2}", IP, Port, e);
            }
        }

        private void disConnectSocket()
        {
            if (!isConnected)
                return;
            try
            {
                isConnected = false;
                ThreadReceive.Abort();
                clientSocket.Close();
                
            }
            catch (SocketException e)
            {
                Console.WriteLine("无法终止线程{0}", e);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
            clientSocket.Send(Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        private void Receive()
        {
            while (isConnected)
            {
                try
                {
                    byte[] ByteReceive = new byte[1024];
                    int ReceiveLenght = clientSocket.Receive(ByteReceive);//此处线程会被挂起
                    string strGet = Encoding.UTF8.GetString(ByteReceive, 0, ReceiveLenght);
                    Console.WriteLine("{0}", strGet);
                    System.Diagnostics.Debug.WriteLine("信息:{0}", strGet);
                    csvHelper.updateCCDValue(strGet);
                    //////////////////////////////
                    //线程委托去刷新信息
                    Action<String> AsyncUIDelegate = delegate (string n)
                    {
                        if (showLinesSocket++ > 50)
                        {
                            showLinesSocket = 0;
                            textBox2.Clear();
                            textBox3.Clear();
                        }
                        textBox2.AppendText(n);
                        textBox2.AppendText(System.Environment.NewLine);
                    };
                    textBox2.Invoke(AsyncUIDelegate, new object[] { strGet });
                    //////////////////////////////
                }
                catch
                {
                    Console.WriteLine("服务器已断开");
                }
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        private void ComReceive()
        {
            while (isComConnected)
            {
                try
                {
                    // byte[] ByteReceive = new byte[1024];
                    string strGet = ComDevice.ReadLine();
                    Console.WriteLine("{0}", strGet);
                    System.Diagnostics.Debug.WriteLine("信息:{0}", strGet);
                    csvHelper.updateCCDValue(strGet);
                    //////////////////////////////
                    //线程委托去刷新信息
                    Action<String> AsyncUIDelegate = delegate (string n)
                    {
                        if (showLinesSocket++ > 50)
                        {
                            showLinesSocket = 0;
                            textBox3.Clear();
                            textBox2.Clear();
                        }
                        textBox3.AppendText(n);
                        textBox3.AppendText(System.Environment.NewLine);
                    };
                    textBox3.Invoke(AsyncUIDelegate, new object[] { strGet });
                    //////////////////////////////
                }
                catch
                {
                    Console.WriteLine("串口已断开");
                }
            }
        }
        
        public Form1()
        {
            InitializeComponent();
            //创建套接字，ipv4寻址方式，套接字类型，传输协议

            //添加串口项目
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {//获取有多少个COM口
                //System.Diagnostics.Debug.WriteLine(s);
                this.comboBox1.Items.Add(s);
            }
            List<string> sList = Utility.getFileNameList(Environment.CurrentDirectory, ".ini");
            foreach(string s in sList)
            {
                this.comboBox2.Items.Add(s);
            }
            
            
            comboBox1.SelectedItem = Utility.GetValue("com", "portname", "COM3", Utility.getParameterFileName());
           // comboBox2.SelectedItem = csvHelper.getParameterFileName();
            comboBox2.SelectedItem = Utility.GetValue("system", "currentParameterFilePath", "cameraLogger.ini", Utility.getParameterFileName());
            ComDevice = new SerialPort();
            // ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//这个该死的事件害我错乱

            //init current output path as current path
            //currentOutputFilePath
            textBox1.Text = Utility.GetValue("system", "currentOutputFilePath", "wrong", Utility.getParameterFileName());
            if (textBox1.Text.Equals("wrong"))
            {
                textBox1.Text = Environment.CurrentDirectory;
                Utility.SetValue("system", "currentOutputFilePath", textBox1.Text, Utility.getParameterFileName());
            }
                

        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
       /* private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (ComDevice.IsOpen)
            {
                byte[] ReDatas = new byte[ComDevice.BytesToRead];
                ComDevice.Read(ReDatas, 0, ReDatas.Length);//读取数据
               
                this.ProcessData(ReDatas);//输出数据,这里处理数据
            }
            else
            {
                MessageBox.Show("请先打开串口");
            }

        }
        */
       /*
        private void ProcessData(byte[] data)
        {
            //在这里处理数据吧
            string strGet = new UTF8Encoding().GetString(data);
            csvHelper.updateCCDValue(strGet);
            System.Diagnostics.Debug.WriteLine("串口:{1}", strGet);
            this.BeginInvoke(new MethodInvoker(delegate
            {
                textBox3.AppendText("\r\n");
                if (showLinesCom++ > 50)
                {
                    showLinesCom = 0;
                    textBox3.Clear();
                }
                textBox3.AppendText(strGet);
            }));
        }
        */
        private void button1_Click(object sender, EventArgs e)
        {
            //chose output file path
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "请选择需要的目录";
            //folderBrowser.ShowNewFolderButton = true;
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowser.SelectedPath;
              //  outputPath = folderBrowser.SelectedPath;
                /*
                if(null == csvHelper)
                    csvHelper = new CSVFileHelper(folderBrowser.SelectedPath);
                else
                {
                    csvHelper.filePath = outputPath;
                    csvHelper.flushCSV();
                */
                Utility.SetValue("system", "currentOutputFilePath", textBox1.Text, Utility.getParameterFileName());
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //connect buttong
            /* don't need it now 
            if (csvHelper == null)
                csvHelper = new CSVFileHelper(Environment.CurrentDirectory);
            else
                csvHelper.flushCSV();
            */
            // this.label1.Text = "wugui";
            //打开csv文件准备写,移入开始中去吧.
            if (!isStarted)
            {//当前属于关闭状态，切换成开始状态.
                button2.Text = "停止";
                isStarted = true;
                //开始状态时UI上有些选项不可选
                button1.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                if (null == csvHelper)
                    csvHelper = new CSVFileHelper(Environment.CurrentDirectory);
                socketip = Utility.GetValue("socket", "ip", "127.0.0.1", Utility.getParameterFileName());
                port = int.Parse(Utility.GetValue("socket", "port", "1231", Utility.getParameterFileName()));
                // connect socket here
                connectSocket(socketip, port);
                // connect serial port here
                openSerialPort();
            }
            else
            {
                //当前属于开始状态，切换成停止状态.
                button2.Text = "开始";
                //停止状态时UI上有些选项可选
                button1.Enabled = true;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                isStarted = false;
                if (null != csvHelper)
                    csvHelper.close();
                csvHelper = null;
                //disconnect socket
                disConnectSocket();
                //disconnect serial port
                closeSerialPort();
            }
            
        }
        private void openSerialPort()
        {
            if (comboBox1.Items.Count <= 0)
            {
                MessageBox.Show("没有发现串口,请检查线路！");
                return;
            }
            if (isComConnected)   return;
            if (ComDevice.IsOpen == false)
            {
                ComDevice.PortName = comboBox1.SelectedItem.ToString();
                ComDevice.BaudRate = int.Parse(Utility.GetValue("com","baudrate","9600", Utility.getParameterFileName()));
                ComDevice.Parity = (Parity)0;
                ComDevice.DataBits = 8;
                ComDevice.StopBits = (StopBits)1;
                try
                {
                    ComDevice.Open();
                    isComConnected = true;
                    //收到消息是线程会被挂起，创建新线程处理收到的消息
                    ComThreadReceive = new Thread(ComReceive);
                    ComThreadReceive.IsBackground = true;
                    ComThreadReceive.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
            }            
        }

        private void closeSerialPort()
        {
            if (comboBox1.Items.Count <= 0)
            {
                MessageBox.Show("没有发现串口,请检查线路！");
                return;
            }
            if (!isComConnected) return;
            if (ComDevice.IsOpen == true)
            {
                try
                {
                    ComDevice.Close();
                    isComConnected = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            textBox3.Clear();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Utility.SetValue("com", "portname", comboBox1.SelectedItem.ToString(), Utility.getParameterFileName());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            System.Environment.Exit(0);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //这里更新参数文件就好，不要做动作。
            Utility.updateParameterFileName(comboBox2.SelectedItem.ToString());
            //if (null != csvHelper)
             //   csvHelper.updateParameterFileName(comboBox2.SelectedItem.ToString());
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
