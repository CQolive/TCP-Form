using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace WindowsFormsApplication2
{

    public partial class Form1 : Form
    {
        private IPAddress myIP = IPAddress.Parse("127.0.0.1");
        private IPEndPoint MySever;
        private Socket mySocket;
        private Socket handler;
        private static ManualResetEvent myReset = new ManualResetEvent(false);
        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                IPHostEntry myHost = new IPHostEntry();
                myHost = Dns.GetHostByName(textBox1.Text);
                string IPstring = myHost.AddressList[0].ToString();
                myIP = IPAddress.Parse(IPstring);
            }
            catch
            {
                MessageBox.Show("你输入的IP不正确，请重新输入！", "提示");
            }
            try
            {
                MySever = new IPEndPoint(myIP, Int32.Parse(textBox2.Text));
                mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                mySocket.Bind(MySever);
                mySocket.Listen(50);
                textBox3.AppendText("主机" + textBox1.Text + "端口" + textBox2.Text + "开始监听.......\r\n");
                Thread thread = new Thread(new ThreadStart(target));
                thread.Start();
            }
            catch (Exception ee)
            {
                textBox3.AppendText(ee.Message + "\r\n");
            }

        }
        private void target()
        {
            while (true)
            {
                
                //设为非终止
                myReset.Reset();
                mySocket.BeginAccept(new AsyncCallback(AcceptCallback), mySocket);
                //阻塞当前线程，直到收到请求信号
                myReset.WaitOne();
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            //将事件设为终止
            myReset.Set();
            Socket listener = (Socket)ar.AsyncState;
            handler = listener.EndAccept(ar);
            //获取状态
            StateObject state = new StateObject();
            state.workSocket = handler;
            textBox3.AppendText("与客户建立连接.\r\n");
            try
            {
                byte[] byteData = System.Text.Encoding.UTF8.GetBytes("已准备好，请通话！ + \r\n");
                //开始发送数据
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
            Thread thread = new Thread(new ThreadStart(begReceive));
            thread.Start();
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                handler = (Socket)ar.AsyncState;
                int byteSend = handler.EndSend(ar);
            }
            catch (Exception eee)
            {
                MessageBox.Show(eee.Message);
            }
        }

        private void begReceive()
        {
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket tt = state.workSocket;
            //结束读取并获取字节数
            int bytesRead = handler.EndReceive(ar);
            state.sb.Append(System.Text.Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            string content = state.sb.ToString();
            state.sb.Remove(0, content.Length);
            richTextBox1.AppendText(content + "\r\n");
            //重新开始读取数据
            tt.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] byteData = System.Text.Encoding.ASCII.GetBytes(richTextBox2.Text + "\r\n");
                //开始发送数据

                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                mySocket.Close();
                textBox3.AppendText("主机" + textBox1.Text + "端口" + textBox2.Text + "监听停止! \r\n");

            }
            catch
            {
                MessageBox.Show("监听尚未开始,关闭无效!");
            }
        }
    }
}
