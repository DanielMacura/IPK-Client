using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

//Sauce
//https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9

namespace ipk_first_project;

public class UdpSocket
{
    private readonly Socket _socket = new (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private const int BufSize = 8 * 1024;
    private readonly State _state = new ();
    private EndPoint _epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback? _recv;

    public class State
    {
        public byte[] Buffer = new byte[BufSize];
    }

    public void Server(string address, int port)
    {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        Receive();
    }

    public void Client(string address, int port)
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
        IPAddress ipAddress = ipHostInfo.AddressList[0].MapToIPv4();
        _socket.Connect(ipAddress, port);
        Receive();
    }

    public void Send(string text)
    {
        //byte[] data = second_byte.Concat(BitConverter.GetBytes(text.Length).Concat(Encoding.ASCII.GetBytes(text))).ToArray();


        var data = Encoding.ASCII.GetBytes(text);
        var fullData = new byte[data.Length + 2];

        fullData[0] = 0;
        fullData[1] = (byte)data.Length;
        data.CopyTo(fullData, 2);


        Console.WriteLine(BitConverter.ToString(fullData));

        foreach (var t in fullData)
        {
            Console.WriteLine(Convert.ToString(t, 2).PadLeft(8, '0'));
        }

        _socket.BeginSend(fullData, 0, fullData.Length, SocketFlags.None, ar =>
        {
            //State? so = ar.AsyncState as State;
            var bytes = _socket.EndSend(ar);
            Console.WriteLine("SEND: {0}, {1}", bytes, text);
        }, _state);
    }

    private void Receive()
    {
        _socket.BeginReceiveFrom(_state.Buffer, 0, BufSize, SocketFlags.None, ref _epFrom, _recv = ar =>
        {
            Debug.Assert(ar.AsyncState != null, "ar.AsyncState != null");
            var so = ar.AsyncState as State;
            var bytes = _socket.EndReceiveFrom(ar, ref _epFrom);
            Debug.Assert(so != null, nameof(so) + " != null");
            _socket.BeginReceiveFrom(so.Buffer, 0, BufSize, SocketFlags.None, ref _epFrom, _recv, so);
            for (var i = 0; i < bytes; i++)
            {
                Console.WriteLine(Convert.ToString(so.Buffer[i], 2).PadLeft(8, '0'));
            }
            // ReSharper disable once UnusedVariable
            int opcode = so.Buffer[0];
            // ReSharper disable once UnusedVariable
            int statusCode = so.Buffer[1];
            // ReSharper disable once UnusedVariable
            int payloadLength = so.Buffer[2];
            var message = Encoding.ASCII.GetString(so.Buffer, 3, bytes - 3);
            Console.WriteLine("RECV: {0}: {1}, |{2}|", _epFrom, bytes, message/*Encoding.ASCII.GetString(so.buffer, 0, bytes)*/);
        }, _state);
    }
}