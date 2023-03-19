using System.Net;
using Mono.Options;

namespace ipk_first_project;

internal class MyClass
{
    private const char Lf = (char)10;
        
    public static void Main(string[] args)
    {

        Console.CancelKeyPress += CancelKeyPressHandler;

        var showHelp = false;
        var host = "172.19.173.188";    //172.19.173.188
        var mode = "tcp";
        var port = 4747;

        var p = new OptionSet
        {
            { "h|host=", "IP address of AaaS provider.",
                v => host = v },
            { "p|port=", "port of connection",
                (int v) => port = v },
            { "m|mode=", "connection mode.",
                v => mode = v },
            { "i|help",  "show this message and exit",
                v => showHelp = v != null }
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

        if (showHelp)
        {
            ShowHelp(p);
            return;
        }

        Console.WriteLine("In mode: " + mode);

        if (mode is not ("tcp" or "udp"))
        {
            Console.Error.WriteLine("Not a mode");
            Console.Error.WriteLine("Try `greet --help' for more information.");
        }

        var handler = new NetworkHandler(mode);
        handler.Start(host, port);
        while (true)
        {
            handler.SendMessage(Console.ReadLine() ?? String.Empty);
        }

    }
    protected static void CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Ctrl+C pressed. Exiting...");
        // Cleanup code here.
        Environment.Exit(0);
    }


    private static void ShowHelp(OptionSet p)
    {
        Console.WriteLine("Usage: greet [OPTIONS]+ message");
        Console.WriteLine("Greet a list of individuals with an optional message.");
        Console.WriteLine("If no message is specified, a generic greeting is used.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        p.WriteOptionDescriptions(Console.Out);
    }



    //    static void ListenTcp(NetworkStream stream)
    //    {
    //        Byte[] data = new Byte[256];



    //        // Read the first batch of the TcpServer response bytes.
    //        Console.Write("Im stuck");
    //        Int32 bytes = stream.Read(data, 0, data.Length);
    //        // String to store the response ASCII representation.
    //        String responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
    //        Console.WriteLine("Received: {0}", responseData);

    //    }

    //    static void SendTcp(NetworkStream stream, string message)
    //    {
    //        try
    //        {
    //            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

    //            // SendTcp the message to the connected TcpServer.
    //            stream.Write(data, 0, data.Length);

    //            Console.WriteLine("Sent: {0}", message);
    //        }
    //        catch (ArgumentNullException e)
    //        {
    //            Console.WriteLine("ArgumentNullException: {0}", e);
    //        }
    //    }
}