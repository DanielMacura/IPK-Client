using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ipk_first_project
{
    public class NetworkHandler
    {
        private const char Lf = (char)10;
        private string _mode;
        private Tcp? _t;
        private UdpSocket? _c;
        public NetworkHandler(string mode)
        {
            
            _mode = mode;
            

        }
        public void Start(string host, int port)
        {
            switch (_mode)
            {
                case "tcp":
                {
                    /*
                    var ts = new Tcp();
                    ts.Stream(host, port);
                    ts.ListenTcp();*/

                    _t = new Tcp();
                    _t.Stream(host, port);
                    _t.ListenTcp();
                    break;
                }

                case "udp":
                {
                    var s = new UdpSocket();
                    s.Server(IPAddress.Loopback.ToString(), port);

                    _c = new UdpSocket();
                    _c.Client(host, port);       
                    break;
                }
            }
        }

        public void SendMessage(string message)
        {
            switch (_mode)
            {
                case "tcp":
                {
                    Debug.Assert(_t != null, nameof(_t) + " != null");
                    _t.SendTcp(message+Lf);
                    break;
                }

                case "udp":
                {
                    Debug.Assert(_c != null, nameof(_c) + " != null");
                    _c.Send(message);
                    break;
                }
            }
        }
    }
}
