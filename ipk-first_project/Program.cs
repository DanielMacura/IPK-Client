using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using Mono.Options;

class MyClass
{
    const char LF = ((char)10);
    
    public static void Main(string[] args)
    {
        bool show_help = false;
        string host = "172.25.102.207";
        string mode = "tcp";
        int port = 4747;

        var p = new OptionSet() {
            { "h|host=", "IP address of AaaS provider.",
                v => host = v },
            { "p|port=", "port of connection",
                (int v) => port = v },
            { "m|mode=", "connection mode.",
                v => mode = v },
            { "i|help",  "show this message and exit",
                v => show_help = v != null },
        };

        try
        {
            _ = p.Parse(args);
        }
        catch (OptionException e)
        {
            Console.Error.Write("greet: ");
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine("Try `greet --help' for more information.");
            return;
        }

        if (show_help)
        {
            ShowHelp(p);
            return;
        }

        if (mode != "tcp" || mode != "udp")
        {
            Console.Error.WriteLine("Not a mode");
            Console.Error.WriteLine("Try `greet --help' for more information.");
        } 

        //ConnectTCP(server, "HELLO"+LF);


        using TcpClient client = new TcpClient(host, port);
        NetworkStream stream = client.GetStream();

        SendTcp(stream, "HELLO"+LF);
        ListenTcp(stream);
        while (true)
        {
            
            SendTcp(stream, Console.ReadLine()+LF);
            ListenTcp(stream);


        }
    }


    static void ShowHelp(OptionSet p)
    {
        Console.WriteLine("Usage: greet [OPTIONS]+ message");
        Console.WriteLine("Greet a list of individuals with an optional message.");
        Console.WriteLine("If no message is specified, a generic greeting is used.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        p.WriteOptionDescriptions(Console.Out);
    }



    static void ListenTcp(NetworkStream stream)
        {
            Byte[] data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Console.Write("Im stuck");
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: {0}", responseData);

        }




    static void SendTcp(NetworkStream stream, string message)
    {
        try
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            // SendTcp the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent: {0}", message);
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine("ArgumentNullException: {0}", e);
        }
    }
}
