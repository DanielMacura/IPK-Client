# Changelog

## Program execution

The program can be executed using the following command:

`ipkcpc -h <host> -p <port> -m <mode>`

Where `host` refers to the IPv4 address of the server, `port` denotes the server port, and `mode` can be either "tcp" or "udp". The order of the flags is arbitrary, and the flags are case-insensitive. An error will be raised if `port` is not a number within the range of 0-65535.

Executing the program with the `--help` flag will display the help information.

The program will terminate with a status code of either 0 (SUCCESS) or 1 (ERROR).

## Known issues / TODO

- [ ] Checking for EOF to exit program flow.
- [ ] Write documentation for more tests.
- [ ] On Nix, when parsing a non numerical address, an exception is thrown.
- [x] 23/3/23 Fix UDP and TCP trying to unnecessarily resolve ip address using DNS.
- [x] 23/3/23 Fix not trimming newline character from incoming TCP messages.
- [ ] ...
- [ ] Sleep... but it was fun.
