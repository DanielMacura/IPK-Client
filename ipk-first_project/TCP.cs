﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ipkcpc;

public class Tcp
{
    private const int BufSize = 8 * 1024;
    private const char Lf = (char)10;
    private readonly State _state = new();
    private AsyncCallback? _recv;
    private NetworkStream? _stream;
    public bool ClientInitiatedExit;

    public void Stream(string host, int port)
    {
        var ipAddress = IPAddress.TryParse(host, out _)
            ? IPAddress.Parse(host)
            : Dns.GetHostEntry(host).AddressList[0].MapToIPv4();
        var client = new TcpClient(ipAddress.ToString(), port);
        _stream = client.GetStream();
    }

    public void ListenTcp()
    {
        if (_stream == null) return;
        _ = _stream.BeginRead(_state.Buffer, 0, BufSize, _recv = ar =>
        {
            var so = ar.AsyncState as State;
            var bytes = _stream.EndRead(ar);

            if (so == null) return;
            var message = Encoding.ASCII.GetString(so.Buffer, 0, bytes);

            Console.WriteLine(message.Trim());
            if (message.Trim() is "BYE")
            {
                if (ClientInitiatedExit)
                {
                    ClientInitiatedExit = true;
                    Environment.Exit(0);
                }
                else
                {
                    SendTcp("BYE");
                    ClientInitiatedExit = true;
                    Environment.Exit(0);
                }
            }

            if (ClientInitiatedExit == false) _stream.BeginRead(_state.Buffer, 0, BufSize, _recv, so);
        }, _state);
    }

    public void SendTcp(string message)
    {
        try
        {
            if (message.Trim() is "BYE") ClientInitiatedExit = true;
            var data = Encoding.ASCII.GetBytes(message + Lf);
            _stream?.Write(data, 0, data.Length);
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