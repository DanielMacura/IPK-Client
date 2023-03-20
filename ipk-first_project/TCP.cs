using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ipkcpc;

public class Tcp
{
    private const int BufSize = 8 * 1024;
    private const char Lf = (char)10;
    private readonly EndPoint _epFrom = new IPEndPoint(IPAddress.Any, 0);
    private readonly State _state = new();
    private AsyncCallback? _recv;
    private NetworkStream? _stream;
    public bool ClientInitiatedExit;

    public void Stream(string host, int port)
    {
        var client = new TcpClient(host, port);
        _stream = client.GetStream();
    }

    public void ListenTcp()
    {
        Debug.Assert(_stream != null, nameof(_stream) + " != null");
        _ = _stream.BeginRead(_state.Buffer, 0, BufSize, _recv = ar =>
        {
            Debug.Assert(ar.AsyncState != null, "ar.AsyncState != null");
            var so = ar.AsyncState as State;
            var bytes = _stream.EndRead(ar);
            //var bytes = _socket.EndReceiveFrom(ar, ref _epFrom);
            Debug.Assert(so != null, nameof(so) + " != null");

            if (ClientInitiatedExit == false) _stream.BeginRead(_state.Buffer, 0, BufSize, _recv, so);

            for (var i = 0; i < bytes; i++) Console.WriteLine(Convert.ToString(so.Buffer[i], 2).PadLeft(8, '0'));
            var message = Encoding.ASCII.GetString(so.Buffer, 0, bytes);

            Console.WriteLine("RECV: {0}: {1}, |{2}|", _epFrom, bytes,
                message /*Encoding.ASCII.GetString(so.buffer, 0, bytes)*/);
            if (message.Trim() is not "BYE") return;
            Console.WriteLine("Got BYE");
            if (ClientInitiatedExit)
            {
                Environment.Exit(0);
            }
            else
            {
                SendTcp("BYE");
                Environment.Exit(0);
            }
        }, _state);


        /*
        var data = new byte[256];
        

        // Read the first batch of the TcpServer response bytes.
        Console.Write("Im stuck");
        Debug.Assert(_stream != null, nameof(_stream) + " != null");
        var bytes = _stream.Read(data, 0, data.Length);

        // String to store the response ASCII representation.
        var responseData = Encoding.ASCII.GetString(data, 0, bytes);
        Console.WriteLine("Received: {0}", responseData);
        */
    }

    public void SendTcp(string message)
    {
        try
        {
            if (message.Trim() is "BYE") ClientInitiatedExit = true;
            var data = Encoding.ASCII.GetBytes(message + Lf);

            // SendTcp the message to the connected TcpServer.
            Debug.Assert(_stream != null, nameof(_stream) + " != null");
            _stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent: {0}", message);
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
    }

    public class State
    {
        public byte[] Buffer = new byte[BufSize];
    }
}