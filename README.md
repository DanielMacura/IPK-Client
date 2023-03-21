# IPK - first project

  A simple client for the IPK Calculator Protocol.

- [IPK - first project](#ipk---first-project)
  - [Executive summary of used protocols](#executive-summary-of-used-protocols)
    - [TCP](#tcp)
    - [UDP](#udp)
  - [Implementation](#implementation)
    - [Network Handler Class](#network-handler-class)
    - [TCP communication](#tcp-communication)
      - [TCP Class](#tcp-class)
      - [UDP Class](#udp-class)

## Executive summary of used protocols

### TCP

Transmission Control Protocol is a transport protocol that is used on top of IP (Internet Protocol) to ensure reliable transmission of packets over the internet or other networks. TCP is a connection-oriented protocol, which means that it establishes and maintains a connection between the two parties until the data transfer is complete. TCP provides mechanisms to solve problems that arise from packet-based messaging, e.g. lost packets or out-of-order packets, duplicate packets, and corrupted packets. TCP achieves this by using sequence and acknowledgement numbers, checksums, flow control, error control, and congestion control.

### UDP

User Datagram Protocol is a connectionless and unreliable protocol that provides a simple and efficient way to send and receive datagrams over an IP network. UDP does not guarantee delivery, order, or integrity of the data, but it minimizes the overhead and latency involved in transmitting data when compared to TCP. UDP is suitable for applications that require speed, simplicity, or real-time communication, such as streaming media, online gaming, voice over IP, or DNS queries.

## Implementation

This program is written using [clean code](https://gist.github.com/wojteklu/73c6914cc446146b8b533c0988cf8d29) conventions. The core of the communication is abstracted away and wrapped in multiple classes.

<img  src="./docs src/ipkcp uml.svg">

The program begins by parsing command line arguments. A callback-based option parser called [Mono.Options](https://www.nuget.org/packages/Mono.Options) is used.

``` c#
var p =  new OptionSet{
    {"h=|host=",  "IP address of AaaS provider.",
        v => host = v},
    {"p=|port=",  "port of connection",
        (int v)  => port = v},
    {"m=|mode=",  "connection mode.",
        v => _mode = v},
    {"i|help",  "show this message and exit",
        v => showHelp = v !=  null}};
_ = p.Parse(args);
```

Upon completion of argument parsing, a check is conducted to assess if any required information is missing. In such a case, the user is promptly notified and the program terminates. Conversely, if all necessary arguments are present, the program proceeds to scan and process user input in an ongoing manner.

``` c#
_handler =  new NetworkHandler(_mode);
_handler.Start(host, port);
while  (true){
    var input = Console.ReadLine();
    if  (input !=  null) _handler.SendMessage(input);
}
```

### Network Handler Class

The class acts as a wrapper for the underlaying [Tcp](#tcp-class) and [Udp](#udp-class) classes and helps abstract away more of the connection details. The bellow provided snippet showcases its two classes.

``` c#
public void Start(string host, int port){
        switch (_mode){
            case "tcp":{
                _clientTcp = new Tcp();
                _clientTcp.Stream(host, port);
                _clientTcp.ListenTcp();
                break;
            }
            case "udp":{
                var s = new UdpSocket();
                s.Server(IPAddress.Loopback.ToString(), port);

                _clientUdpSocket = new UdpSocket();
                _clientUdpSocket.Client(host, port);
                break;
            }}}

    public void SendMessage(string message){
        switch (_mode){
            case "tcp":{
                _clientTcp?.SendTcp(message);
                break;
            }

            case "udp":{
                _clientUdpSocket?.Send(message);
                break;
            }}}
```

### TCP communication

When a NetworkHandler is instantiated with the TCP mode, it creates a new instance of the [Tcp class](#tcp-class)  and invokes its Stream method, which takes in the provided host and port as parameters. This process internally creates a new TcpClient object, which then calls the GetStream method. Afterward, the asynchronous method ListenTcp is invoked, and control is then returned to the input parsing loop.

The user is now able to input commands, as demonstrated in the diagram below.

<img  src="./docs src/ipkcp tcp.svg">

#### TCP Class

The class implements the following methods, similar structure is used in the [Udp class](#udp-class), from which it is derived.

- `Stream(string host, int port)`: A public method that takes two arguments, the IP address or hostname of the server and the port number to connect to. It creates a new TcpClient object and obtains its NetworkStream. The stream is stored in the "_stream" field.

    ``` c#
    var client = new TcpClient(host, port);
    _stream = client.GetStream();
    ```

- `ListenTcp()`: A public method that starts listening to the network for incoming data. It uses the "_stream" field to read data asynchronously and pass it to the callback method specified in "_recv". The method loops until "ClientInitiatedExit" is set to true or an exception is thrown. If the received message is "BYE", the method sets "ClientInitiatedExit" to true and terminates the program. Otherwise, it prints the received message and continues listening.

    ``` c#
    _ = _stream.BeginRead(_state.Buffer, 0, BufSize, _recv = ar =>{
        var so = ar.AsyncState as State;
        var bytes = _stream.EndRead(ar);

        var message = Encoding.ASCII.GetString(so.Buffer, 0, bytes);

        Console.WriteLine(message);
        /*
            Code which handles receiving BYE message from server.
        */

        if (ClientInitiatedExit == false) _stream.BeginRead(_state.Buffer, 0, BufSize,_recv, so);
    }, _state);
    ```

- `SendTcp(string message)`: A public method that takes a string argument representing the message to send. It converts the message to a byte array using ASCII encoding and adds a line feed character. It writes the resulting byte array to the network stream stored in "_stream". If the message is "BYE", it sets "ClientInitiatedExit" to true and prints a message indicating that the client initiated the disconnection.

    ``` c#
    if (message.Trim() is "BYE") ClientInitiatedExit = true;
    var data = Encoding.ASCII.GetBytes(message + Lf);
    _stream?.Write(data, 0, data.Length);
    ```

#### UDP Class

This class was adapted from darkguy2008's [gist](https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9). It served as a base and a great learning source, from which the [Tcp class](#tcp-class) was constructed. The [Udp class](#udp-class) was simplified and modified for the purposes of facilitating transition over the IPK Calculator Protocol. 

- `Server(string address, int port)`: Sets up the UDP socket as a server by binding the socket to the specified IP address and port number. This method enables reuse of the address and starts the receive operation to listen for incoming data.
  
    ``` c#
    _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
    _socket.Bind(new IPEndPoint(IPAddress.Parse(address), port));
    Receive();
    ```

- `Client(string address, int port)`: Sets up the UDP socket as a client by connecting the socket to the specified IP address and port number. This method starts the receive operation to listen for incoming data.
  
    ``` c#
    var ipHostInfo = Dns.GetHostEntry(address);
    var ipAddress = ipHostInfo.AddressList[0].MapToIPv4();
    _socket.Connect(ipAddress, port);
    Receive();
    ```

- `Send(string text)`: Sends the specified string over the UDP socket. This method converts the string to bytes and appends two bytes at the beginning of the data, representing the message length, and sends the resulting byte array asynchronously.
  
    ``` c#
    var data = Encoding.ASCII.GetBytes(text);
    var bytes = new byte[data.Length + 2];

    bytes[0] = 0;
    bytes[1] = (byte)data.Length;
    data.CopyTo(bytes, 2);

    _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, ar =>{
        _ = _socket.EndSend(ar);
    }, _state);
    ```

- `Receive()`: Starts the asynchronous receive operation to listen for incoming data from the UDP socket. When data is received, the method processes the received data and calls `BeginReceiveFrom` to start listening for the next data.

    ``` c#
    _=_socket.BeginReceiveFrom(_state.Buffer, 0, BufSize, SocketFlags.None, ref _epFrom, _recv = ar =>{
        var so = ar.AsyncState as State;
        var bytes = _socket.EndReceiveFrom(ar, ref _epFrom);

        _socket.BeginReceiveFrom(so.Buffer, 0, BufSize, SocketFlags.None, ref _epFrom, _recv, so);
        int opcode = so.Buffer[0];
        int statusCode = so.Buffer[1];
        int payloadLength = so.Buffer[2];
        var message = Encoding.ASCII.GetString(so.Buffer, 3, bytes - 3);
        var truncatedToNLength = new string(message.Take(payloadLength).ToArray());
        /*
            Code to handle the propper formating of the output message.
        */
    }, _state);
    ```
