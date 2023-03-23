using System.Net;
using System.Net.Sockets;
using System.Text;

//Source
//https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
//

namespace ipkcpc;

public class UdpSocket
{
    private const int BufSize = 8 * 1024;
    private readonly Socket _socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private readonly State _state = new();
    private EndPoint _epFrom = new IPEndPoint(IPAddress.Any, 0);
    private AsyncCallback? _recv;

    public void Server(string address, int port)
    {
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
        Receive();
    }

    public void Client(string address, int port)
    {
        var ipAddress = IPAddress.TryParse(address, out _)
            ? IPAddress.Parse(address)
            : Dns.GetHostEntry(address).AddressList[0].MapToIPv4();
        _socket.Connect(ipAddress, port);
        Receive();
    }

    public void Send(string text)
    {
        if (text.Length > 255)
        {
            Console.Error.Write("ERROR: Message over 255 bytes.");
            return;
        }
        var data = Encoding.ASCII.GetBytes(text);
        var bytes = new byte[data.Length + 2];

        bytes[0] = 0;
        bytes[1] = (byte)data.Length;
        data.CopyTo(bytes, 2);

        _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, ar =>
        {
            _ = _socket.EndSend(ar);
        }, _state);
    }

    private void Receive()
    {
        _=_socket.BeginReceiveFrom(_state.Buffer, 0, BufSize, SocketFlags.None, ref _epFrom, _recv = ar =>
        {
            var so = ar.AsyncState as State;
            var bytes = _socket.EndReceiveFrom(ar, ref _epFrom);

            if (so == null) return;
            _socket.BeginReceiveFrom(so.Buffer, 0, BufSize, SocketFlags.None, ref _epFrom, _recv, so);
            int opcode = so.Buffer[0];
            int statusCode = so.Buffer[1];
            int payloadLength = so.Buffer[2];
            var message = Encoding.ASCII.GetString(so.Buffer, 3, bytes - 3);
            var truncatedToNLength = new string(message.Take(payloadLength).ToArray());
            if (opcode is 1)
            {
                switch (statusCode)
                {
                    case 0:
                        Console.WriteLine("OK:{0}", truncatedToNLength);
                        break;
                    case 1:
                        Console.WriteLine("ERROR:{0}", truncatedToNLength);
                        break;
                }
            }
            else
            {
                Console.Error.WriteLine("Incorrect UPD packet received. Opcode is not 1 - receive.");
            }
        }, _state);
    }

    public class State
    {
        public byte[] Buffer = new byte[BufSize];
    }
}