using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ipk_first_project;

public class Tcp
{
    private NetworkStream? _stream;

    public void Stream(string host, int port)
    {
        var client = new TcpClient(host, port);
        _stream = client.GetStream();
    }

    public void ListenTcp()
    {
        var data = new byte[256];



        // Read the first batch of the TcpServer response bytes.
        Console.Write("Im stuck");
        Debug.Assert(_stream != null, nameof(_stream) + " != null");
        var bytes = _stream.Read(data, 0, data.Length);
        // String to store the response ASCII representation.
        var responseData = Encoding.ASCII.GetString(data, 0, bytes);
        Console.WriteLine("Received: {0}", responseData);

    }

    public void SendTcp(string message)
    {
        try
        {
            var data = Encoding.ASCII.GetBytes(message);

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

}