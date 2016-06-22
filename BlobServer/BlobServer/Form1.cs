using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
/**
 * Form1.java
 * Assignment: Final Project / Blob Suurvival
 * Purpose: gets everything started, initalizes socket listeners.
 * displays IP address, calls runGame()
 *
 * @version 6/21/2016
 * @author Alex Maclean
 */
namespace BlobServer
{
    public partial class BlobServer : Form
    {
        private readonly IPAddress _localAdd;
        private readonly List<PlayerCommunications> _players = new List<PlayerCommunications>();
        private readonly GameState _game;

        public BlobServer()
        {
            _localAdd = IPAddress.Parse(GetLocalIPAddress());
            var t = new Timer
            {
                Enabled = true,
                Interval = 200
            };
            t.Tick += t_Tick;
         _game = new GameState();
            InitializeComponent();
            StartListeners();
            t.Start();
        }

        //initalizes 100 threads each listening at a port for incoming clients
        public void StartListeners()
        {
            for (var i = 0; i < 100; i+=2)
            {
                var port = i;
                var readerThread = new Thread(() => Listen(port)) {Name = string.Format("Reader {0}", port)};
                readerThread.Start();
            }
        }

        //method run by each thread, it accepts clients and creates a dedicated PlayerCommunications for each
        public void Listen(int port)
        {

            var listener = new TcpListener(_localAdd, port+5000);
            listener.Start();
            var client = listener.AcceptTcpClient();
            var blobStream = client.GetStream();

            var listener2 = new TcpListener(_localAdd, port+5000+1);
            listener2.Start();
            var client2 = listener2.AcceptTcpClient();
            var moveStream = client2.GetStream();
            lock (_players)
            {
                _players.Add(new PlayerCommunications(moveStream,blobStream,_game));
            }

            
        }
        //called periodicaly by the timer (t) calls RunGame and Redraws
        void t_Tick(object sender, EventArgs e)
        {
            _game.RunGame();
             Invalidate();
        }

        //displays the LAN IP address
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.DrawString("IP:"+GetLocalIPAddress(),new Font(FontFamily.GenericSerif, 20,FontStyle.Italic),Brushes.Aqua, new PointF(0f,0f) );
        }

        //get the LAN IP address
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

    }
}
