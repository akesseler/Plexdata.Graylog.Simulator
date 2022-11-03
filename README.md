<p align="center">
  <a href="https://github.com/akesseler/Plexdata.Graylog.Simulator/blob/master/LICENSE.md" alt="license">
    <img src="https://img.shields.io/github/license/akesseler/Plexdata.Graylog.Simulator.svg" />
  </a>
  <a href="https://github.com/akesseler/Plexdata.Graylog.Simulator/releases/latest" alt="latest">
    <img src="https://img.shields.io/github/release/akesseler/Plexdata.Graylog.Simulator.svg" />
  </a>
  <a href="https://github.com/akesseler/Plexdata.Graylog.Simulator/archive/master.zip" alt="master">
    <img src="https://img.shields.io/github/languages/code-size/akesseler/Plexdata.Graylog.Simulator.svg" />
  </a>
</p>

# Plexdata Graylog Simulator

Weeks ago I asked the Graylog support for a Sandbox where I can test my own logger implementation. 
The answer was _Download and install your own version_. This answer was really unsatisfying. So I 
came up with the idea of implementing my own Graylog simulator.

Fortunately I found the official GELF documentation at [https://docs.graylog.org/docs/gelf](https://docs.graylog.org/docs/gelf) 
and I decided to implement my own server. And now I proudly present the first true Graylog simulator 
written in C#.

## Table of Contents

1. [Licensing](#licensing)
1. [Features](#features)
1. [Examples](#examples)
1. [Limitations](#limitations)
1. [Downloads](#downloads)
1. [Known Issues](#known-issues)

## Licensing <a name="licensing"></a>

The software has been published under the terms of _MIT License_.

## Features <a name="features"></a>

This Graylog simulator accepts messages over UDP, TCP as well as over HTTP requests. Listening on UDP 
and TCP is possible for IPv4 and also for IPv6. Each of these listeners can be started independently. 
Additionally, message chunking and payload zipping (GZip only) for UDP is also possible.

## Examples <a name="examples"></a>

Showing integrated help can be done by executing the simulator with option `--help` like this.

```
Plexdata.Graylog.Simulator.exe --help
```

Running all listeners at once with all default ports is possible bei calling the simulator with option 
`--all`.

```
Plexdata.Graylog.Simulator.exe --all
```

Enabling method tracing as well as payload dumping can be accomplished by starting the simulator with 
options `--trace` and `--debug`.

```
Plexdata.Graylog.Simulator.exe --all --trace --debug
```

Printing additional program version and copyright statement is possible by appending option `--version` 
like this.

```
Plexdata.Graylog.Simulator.exe --all --version
```

Starting one single listener on a user-defined port (e.g. the UDP listener for IPv4) is possible by 
using listener-specific options like this.

```
Plexdata.Graylog.Simulator.exe --udp-ipv4 --udp-ipv4-port 54321
```

The same as above but using short options instead.

```
Plexdata.Graylog.Simulator.exe -u4 -u4p 54321
```

Starting all listeners by using different ports can be done like shown here.

```
Plexdata.Graylog.Simulator.exe -a -u4p 11111 -u6p 22222 -t4p 33333 -t6p 44444 -whp 55555
```

## Limitations <a name="limitations"></a>

This Graylog simulator supports only HTTP protocol. The other way round, protocol HTTPS is not yet 
supported. In addition, HTTP is only possible for `localhost`. But this could be a firewall setting.

## Downloads <a name="downloads"></a>

The latest release can be obtained from [https://github.com/akesseler/Plexdata.Graylog.Simulator/releases/latest](https://github.com/akesseler/Plexdata.Graylog.Simulator/releases/latest).

The main branch can be downloaded as ZIP from [https://github.com/akesseler/Plexdata.Graylog.Simulator/archive/master.zip](https://github.com/akesseler/Plexdata.Graylog.Simulator/archive/master.zip).

## Known Issues <a name="known-issues"></a>

Issues, except the HTTP and `localhost` limitations, are not known at the moment.