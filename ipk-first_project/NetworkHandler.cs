using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ipk_first_project
{
    public class NetworkHandler
    {
        private string _mode;
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
                    break;
                }

                case "udp":
                {
                    var s = new UdpSocket();
                    s.Server(IPAddress.Loopback.ToString(), port);

                    var c = new UdpSocket();
                    c.Client(host, port);       
                    break;
                }
            }
        }
    }
}
