
  

# IPK - first project

  A simple client for the IPK Calculator Protocol.

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

After the arguments are parsed, they are evaluated for missing information. The user is informed of this eventuality and the program exits. If all the necessary arguments are provided, the flow of the program continues and user input is constantly scanned and further processed.

``` c#

_handler =  new NetworkHandler(_mode);

_handler.Start(host, port);

while  (true)

{

var input = Console.ReadLine();

if  (input !=  null) _handler.SendMessage(input);

}

```

### TCP communication

When a NetworkHandler is instantiated with the TCP mode, it creates a new instance of the Tcp class and invokes its Stream method, which takes in the provided host and port as parameters. This process internally creates a new TcpClient object, which then calls the GetStream method. Afterward, the asynchronous method ListenTcp is invoked, and control is then returned to the input parsing loop.

  
  
  

The user is now able to input commands, as demonstrated in the diagram below.

  

<img  src="./docs src/ipkcp tcp.svg">