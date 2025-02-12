﻿using System.Diagnostics;
using Mono.Options;

namespace ipkcpc;

internal class Program
{
    private static string? _mode;
    private static NetworkHandler? _handler;
    public static readonly EventWaitHandle WaitHandle = new AutoResetEvent(false);
    public static void Main(string[] args)
    {
        Console.CancelKeyPress += CancelKeyPressHandler;

        var showHelp = false;
        var host = "";
        _mode = "";
        var port = -1;

        var p = new OptionSet
        {
            {
                "h=|host=", "IP address of AaaS provider.",
                v => host = v
            },
            {
                "p=|port=", "port of connection",
                (int v) => port = v
            },
            {
                "m=|mode=", "connection mode.",
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

        Debug.WriteLine("In mode: " + _mode);
        var incompleteArguments = false;
        if (host is "")
        {
            Console.Error.WriteLine("Host address not specified.");
            incompleteArguments = true;
        }
        if (port is -1)
        {
            Console.Error.WriteLine("Port not specified.");
            incompleteArguments = true;
        }
        if (_mode is not ("tcp" or "udp"))
        {
            Console.Error.WriteLine("Connection mode not specified.");
            incompleteArguments = true;
        }

        if (incompleteArguments)
        {
            Console.Error.WriteLine("Try `ipkcpc --help' for more information.");
            Environment.Exit(1);
        }

        _handler = new NetworkHandler(_mode);
        _handler.Start(host, port);
        while (true)
        {
            // TODO add EOF check
            var input =  Console.ReadLine();
            if (input != null) _handler.SendMessage(input);
        }
    }

    protected static void CancelKeyPressHandler(object? sender, ConsoleCancelEventArgs args)
    {
        if (_mode is "tcp") _handler?.SendMessage("BYE");
        else
        {
            Environment.Exit(0);
        }
        WaitHandle.WaitOne();
        WaitHandle.Reset();
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