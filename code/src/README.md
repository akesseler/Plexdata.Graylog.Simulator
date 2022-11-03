
## Project Build

Best way to build the whole project is to use _Visual Studio 2022 Community_. Thereafter, download the 
complete sources, open the solution file `Plexdata.Graylog.Simulator.sln`, switch to release and rebuild all.

## Help Generation

An additional help file or API documentation is not generated at this moment. But a short description can be found
in the project's [README.md](https://github.com/akesseler/Plexdata.Graylog.Simulator/blob/master/README.md)

The integrated program help can be shown by calling the executable with option `--help`. 

```
Copyright © 2022 - plexdata.de

This program is a (hopefully useful) simulator for a Graylog server and
supports a UDP interface (including message chunking), a TCP interface
(including zero termination) as well as a WEB interface (HTTP). But note,
HTTPS is not supported for various reasons.

Usage:

  Plexdata.Graylog.Simulator.exe [options]

Options:

  --all [-a]              This option allows to start all supported server
                          types at once. But note, each server is started
                          using it default port, if no explicit port is used!
                          Default value is 'off'.

  --udp-ipv4 [-u4]        This option enables the UDP IPv4 server type.
                          Default value is 'off'.

  --udp-ipv4-port [-u4p]  This option sets the UDP IPv4 server port. The
                          value must be in range of [0..65535]. Default value
                          is '42011'.

  --udp-ipv6 [-u6]        This option enables the UDP IPv6 server type.
                          Default value is 'off'.

  --udp-ipv6-port [-u6p]  This option sets the UDP IPv6 server port. The
                          value must be in range of [0..65535]. Default value
                          is '42012'.

  --tcp-ipv4 [-t4]        This option enables the TCP IPv4 server type.
                          Default value is 'off'.

  --tcp-ipv4-port [-t4p]  This option sets the TCP IPv4 server port. The
                          value must be in range of [0..65535]. Default value
                          is '42021'.

  --tcp-ipv6 [-t6]        This option enables the TCP IPv6 server type.
                          Default value is 'off'.

  --tcp-ipv6-port [-t6p]  This option sets the TCP IPv6 server port. The
                          value must be in range of [0..65535]. Default value
                          is '42022'.

  --web-http [-wh]        This option enables the WEB HTTP server type.
                          Default value is 'off'.

  --web-http-port [-whp]  This option sets the WEB HTTP server port. The
                          value must be in range of [0..65535]. Default value
                          is '42031'.

  --version               This option shows copyright and program version at
                          start up.

  --debug                 This option enables the debug mode. In this mode
                          some additional information are printed out during
                          runtime.

  --trace                 This option enables the trace mode. In this mode
                          some of the called methods are printed out during
                          runtime.

  --help [-h,-?]          This option just displays this help screen.
```

## Trouble Shooting

Nothing known at the moment.
