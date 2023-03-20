using Mono.Options;

namespace ipkcpc;

internal class Program
{
    private static string? _mode;
    private static NetworkHandler? _handler;
    public static readonly System.Threading.EventWaitHandle waitHandle = new System.Threading.AutoResetEvent(false);
    public static void Main(string[] args)
    {
        Console.CancelKeyPress += CancelKeyPressHandler;                //          TODO            will work when recieve SIGN INT??

        var showHelp = false;
        var host = "merlin.fit.vutbr.cz"; //172.19.173.188
        _mode = "tcp";
        var port = 10002;

        var p = new OptionSet
        {
            {
                "h|host=", "IP address of AaaS provider.",
                v => host = v
            },
            {
                "p|port=", "port of connection",
                (int v) => port = v
            },
            {
                "m|mode=", "connection mode.",
                v => _mode = v
            },
            {
                "i|help", "show this message and exit",
                v => showHelp = v != null
            }
        };

        try
        {
            _ = p.Parse(args);
        }
        catch (OptionException e)
        {
            Console.Error.Write("ipkcpc: ");
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine("Try `ipkcpc --help' for more information.");
            return;
        }

        if (showHelp)
        {
            ShowHelp(p);
            return;
        }

        Console.WriteLine("In mode: " + _mode);

        if (_mode is not ("tcp" or "udp"))
        {
            Console.Error.WriteLine("Connection mode missing.");
            Console.Error.WriteLine("Try `ipkcpc --help' for more information.");
        }

        _handler = new NetworkHandler(_mode);
        _handler.Start(host, port);
        while (true) _handler.SendMessage(Console.ReadLine() ?? string.Empty);
    }

    protected static void CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Ctrl+C pressed. Exiting...");
        if (_mode is "tcp") _handler?.SendMessage("BYE");
        else
        {
            Console.WriteLine("Exiting main");
            Environment.Exit(0);
        }
        waitHandle.WaitOne();
        waitHandle.Reset();
    }


    private static void ShowHelp(OptionSet p)
    {
        Console.WriteLine("Usage: ipkcpc [OPTIONS]");
        Console.WriteLine(
            "Ipkcp is a client facilitating connection to a host conforming to the IPK Calculator Protocol.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        p.WriteOptionDescriptions(Console.Out);
    }
}