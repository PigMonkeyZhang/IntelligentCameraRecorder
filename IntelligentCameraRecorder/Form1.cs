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
        private int showLinesCom = 0;
        private int port;
        private string socketip;
        private CSVFileHelper csvHelper=null;
        private string outputPath = Environment.CurrentDirectory;
        private Thread ThreadReceive;


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
               
                ThreadReceive.Abort();
                clientSocket.Close();
                isConnected = false;
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
                    }
                    textBox2.AppendText(n);
                    textBox2.AppendText(System.Environment.NewLine);
                };
                textBox2.Invoke(AsyncUIDelegate, new object[] { strGet });
                //////////////////////////////
                Receive();
            }
            catch
            {
                Console.WriteLine("服务器已断开");
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
            comboBox1.SelectedItem = Utility.GetValue("com", "portname", "COM3");
            //comboBox1.SelectedIndex = 1;
            ComDevice = new SerialPort();
            ComDevice.DataReceived += new SerialDataReceivedEventHandler(Com_DataReceived);//绑定事件

            //init current output path as current path
            textBox1.Text = Environment.CurrentDirectory;

        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
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
        private void ProcessData(byte[] data)
        {
            //在这里处理数据吧
            string strGet = new UTF8Encoding().GetString(data);
            csvHelper.updateCCDValue(strGet);
            this.BeginInvoke(new MethodInvoker(delegate
            {
                // textBox3.AppendText("\r\n");
                if (showLinesCom++ > 50)
                {
                    showLinesCom = 0;
                    textBox3.Clear();
                }
                textBox3.AppendText(strGet);
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "请选择需要的目录";
            //folderBrowser.ShowNewFolderButton = true;
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowser.SelectedPath;
                outputPath = folderBrowser.SelectedPath;
                if(null == csvHelper)
                    csvHelper = new CSVFileHelper(folderBrowser.SelectedPath);
                else
                {
                    csvHelper.filePath = outputPath;
                    csvHelper.flushCSV();
                }
                    
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (csvHelper == null)
                csvHelper = new CSVFileHelper(Environment.CurrentDirectory);
            else
                csvHelper.flushCSV();
            // this.label1.Text = "wugui";
            socketip = Utility.GetValue("socket", "ip", "127.0.0.1");
            port = int.Parse(Utility.GetValue("socket", "port", "1231"));
            // connect socket here
            connectSocket(socketip, port);
            // connect serial port here
            openSerialPort();
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
                ComDevice.BaudRate = int.Parse(Utility.GetValue("com","baudrate","9600"));
                ComDevice.Parity = (Parity)0;
                ComDevice.DataBits = 8;
                ComDevice.StopBits = (StopBits)1;
                try
                {
                    ComDevice.Open();
                    isComConnected = true;
                   
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
            Utility.SetValue("com", "portname", comboBox1.SelectedItem.ToString());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (null != csvHelper)
                csvHelper.close();
            //disconnect socket
            disConnectSocket();
            //disconnect serial port
            closeSerialPort();
        }
    }
}
