using System.Diagnostics;
using System.Net;

namespace ipkcpc;

public class NetworkHandler
{
    private readonly string _mode;
    private UdpSocket? _c;
    private Tcp? _t;

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
                _t.SendTcp(message);
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