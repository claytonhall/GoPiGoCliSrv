using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallE
{
    public partial class Form1 : Form
    {
        bool _clicked = false;
        WallEsBrain _wallEsBrain = new WallEsBrain();
      

        public Form1()
        {
            InitializeComponent();
            _wallEsBrain.OnLog += Log;

            tbServo.MouseDown += (s, e) =>
            {
                _clicked = true;
            };

            tbServo.MouseUp += (s, e) =>
            {
                if (!_clicked)
                    return;

                _clicked = false;
                
            };

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            _wallEsBrain.Connect(txtIPAddress.Text, txtPort.Text);
        }

        private void btnFwd_Click(object sender, EventArgs e)
        {
            _wallEsBrain.SendCommand("fwd()");
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _wallEsBrain.SendCommand("stop()");
        }

        void Log(string s)
        {
            textBox1.AppendText(s + "\r\n");
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _wallEsBrain.Disconnect();
        }

        private void btnDir_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btnDir_MouseDown(object sender, MouseEventArgs e)
        {
            _wallEsBrain.SendCommand(((Button)sender).Text);
        }

        private void btnDir_MouseUp(object sender, MouseEventArgs e)
        {
            _wallEsBrain.SendCommand("stop()");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnUS_Click(object sender, EventArgs e)
        {
            _wallEsBrain.SendCommand("us_dist(15)");
        }

        private void tbServo_ValueChanged(object sender, EventArgs e)
        {
            if (!_clicked)
            {
                _wallEsBrain.SendCommand(String.Format("servo({0})", tbServo.Value));
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            _wallEsBrain.SendCommand(txtCommand.Text);
        }
    }

    public delegate void LoggerDelegate(string s);

    public class WallEsBrain
    {
        public string IP { get; set; }
        public string Port { get; set; }
        public Socket Socket { get; set; }

        public LoggerDelegate OnLog;
        

        public bool Connect(string ip, string port)
        {
            IP = ip;
            Port = port;
            return Connect();
        }

        public bool Connect()
        {
            bool retVal = false;

            try
            {
                this.OnLog(String.Format("Connect to {0}:{1}", IP, Port));

                this.Socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(IP), int.Parse(Port));
                this.Socket.Connect(ipe);

                if (this.Socket.Connected)
                {
                    retVal = true;
                }
            }
            catch(Exception ex)
            {
                OnLog(ex.ToString());
            }
            return retVal;
        }

        public void Disconnect()
        {
            if (this.Socket.Connected)
            {
                this.Socket.Disconnect(true);
            }
        }

        public string SendCommand(string s)
        {
            this.OnLog(String.Format("Sending: {0}", s));

            var base64 = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(s));
            var i = this.Socket.Send(ASCIIEncoding.ASCII.GetBytes(base64));

            byte[] buffer = new byte[1024];
            int result = this.Socket.Receive(buffer);
            
            this.OnLog(String.Format("Received: {0}", ASCIIEncoding.ASCII.GetString(buffer).Substring(0,result)));
            
            return i.ToString();
        }

    }
}
