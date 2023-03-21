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
            Debug.Assert(so != null, nameof(so) + " != null");


            var message = Encoding.ASCII.GetString(so.Buffer, 0, bytes);

            //Debug.Write("RECV: {0}: {1}, |{2}|", _epFrom, bytes, message );
            Console.WriteLine(message);
            if (message.Trim() is "BYE")
            {
                Console.WriteLine("Got BYE");
                if (ClientInitiatedExit)
                {
                    Console.WriteLine("Exiting forom tcp");
                    ClientInitiatedExit = true;
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Exiting forom tcp");
                    SendTcp("BYE");
                    ClientInitiatedExit = true;

                    Environment.Exit(0);
                }
            }
            if (ClientInitiatedExit == false) _stream.BeginRead(_state.Buffer, 0, BufSize, _recv, so);
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
            Console.WriteLine("Client initated exit");
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