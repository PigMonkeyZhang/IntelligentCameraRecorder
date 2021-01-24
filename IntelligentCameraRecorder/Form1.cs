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
        private Socket ClientSocket;
        private System.IO.Ports.SerialPort serialPort1;
        private Boolean isConnected = false;
        public void Connect(string IP, Int32 Port)
        {
            if (isConnected)
                return;
            try
            {
                ClientSocket.Connect(IP, Port);//连接到指定服务器
                Console.WriteLine("connect server succeed ok!");//提示信息
                //收到消息是线程会被挂起，创建新线程处理收到的消息
                Thread ThreadReceive = new Thread(Receive);
                ThreadReceive.IsBackground = true;
                ThreadReceive.Start();
                isConnected = true;
            }
            catch (SocketException e)
            {
                Console.WriteLine("无法连接到{0}:{1}{2}", IP, Port, e);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
            ClientSocket.Send(Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        private void Receive()
        {
            try
            {
                byte[] ByteReceive = new byte[1024];
                int ReceiveLenght = ClientSocket.Receive(ByteReceive);//此处线程会被挂起
                Console.WriteLine("{0}", Encoding.UTF8.GetString(ByteReceive, 0, ReceiveLenght));
                System.Diagnostics.Debug.WriteLine("信息:{0}", Encoding.UTF8.GetString(ByteReceive, 0, ReceiveLenght));
                //////////////////////////////
                //线程委托去刷新信息
                Action<String> AsyncUIDelegate = delegate (string n)
                {
                    textBox2.AppendText(n);
                    textBox2.AppendText(System.Environment.NewLine);
                };
                textBox2.Invoke(AsyncUIDelegate, new object[] { Encoding.UTF8.GetString(ByteReceive, 0, ReceiveLenght) });
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
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //添加串口项目
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {//获取有多少个COM口
                //System.Diagnostics.Debug.WriteLine(s);
                this.comboBox1.Items.Add(s);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "请选择需要的目录";
            //folderBrowser.ShowNewFolderButton = true;
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowser.SelectedPath;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // this.label1.Text = "wangbadan";
            Connect("127.0.0.1", 1231);
        }
    }
}
