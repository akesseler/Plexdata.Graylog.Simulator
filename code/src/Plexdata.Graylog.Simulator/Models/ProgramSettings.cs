/*
 * MIT License
 * 
 * Copyright (c) 2022 plexdata.de
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Plexdata.ArgumentParser.Attributes;
using Plexdata.ArgumentParser.Constants;
using System;
using System.ComponentModel;

namespace Plexdata.Graylog.Simulator.Models
{
    [HelpLicense("Graylog Simulator (Version " + Placeholders.Version + ") " + Placeholders.Copyright)]
    [HelpUtilize]
    [HelpPreface]
    [ParametersGroup]
    internal class ProgramSettings
    {
        #region Private Fields

        private const UInt16 DefaulPortUdpIPv4 = 42011;
        private const UInt16 DefaulPortUdpIPv6 = 42012;
        private const UInt16 DefaulPortTcpIPv4 = 42021;
        private const UInt16 DefaulPortTcpIPv6 = 42022;
        private const UInt16 DefaulPortWebHttp = 42031;

        #endregion

        #region Construction

        public ProgramSettings()
            : base()
        {
        }

        #endregion

        #region Public Properties

        [DefaultValue(false)]
        [HelpSummary(
            "This option allows to start all supported server types at once. But note, each server " +
            "is started using it default port, if no explicit port is used! Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "all", BriefLabel = "a")]
        public Boolean IsRunAllTypes { get; set; } = false;

        [DefaultValue(false)]
        [HelpSummary("This option enables the UDP IPv4 server type. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "udp-ipv4", BriefLabel = "u4")]
        public Boolean IsUdpIPv4Type { get; set; } = false;

        [DefaultValue(ProgramSettings.DefaulPortUdpIPv4)]
        [HelpSummary(
            "This option sets the UDP IPv4 server port. The value must be in range of [0..65535]. " +
            "Default value is '42011'.")]
        [OptionParameter(SolidLabel = "udp-ipv4-port", BriefLabel = "u4p")]
        public UInt16 UdpIPv4Prot { get; set; } = ProgramSettings.DefaulPortUdpIPv4;

        [DefaultValue(false)]
        [HelpSummary("This option enables the UDP IPv6 server type. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "udp-ipv6", BriefLabel = "u6")]
        public Boolean IsUdpIPv6Type { get; set; } = false;

        [DefaultValue(ProgramSettings.DefaulPortUdpIPv6)]
        [HelpSummary(
            "This option sets the UDP IPv6 server port. The value must be in range of [0..65535]. " +
            "Default value is '42012'.")]
        [OptionParameter(SolidLabel = "udp-ipv6-port", BriefLabel = "u6p")]
        public UInt16 UdpIPv6Prot { get; set; } = ProgramSettings.DefaulPortUdpIPv6;

        [DefaultValue(false)]
        [HelpSummary("This option enables the TCP IPv4 server type. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "tcp-ipv4", BriefLabel = "t4")]
        public Boolean IsTcpIPv4Type { get; set; } = false;

        [DefaultValue(ProgramSettings.DefaulPortTcpIPv4)]
        [HelpSummary(
            "This option sets the TCP IPv4 server port. The value must be in range of [0..65535]. " +
            "Default value is '42021'.")]
        [OptionParameter(SolidLabel = "tcp-ipv4-port", BriefLabel = "t4p")]
        public UInt16 TcpIPv4Prot { get; set; } = ProgramSettings.DefaulPortTcpIPv4;

        [DefaultValue(false)]
        [HelpSummary("This option enables the TCP IPv6 server type. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "tcp-ipv6", BriefLabel = "t6")]
        public Boolean IsTcpIPv6Type { get; set; } = false;

        [DefaultValue(ProgramSettings.DefaulPortTcpIPv6)]
        [HelpSummary(
            "This option sets the TCP IPv6 server port. The value must be in range of [0..65535]. " +
            "Default value is '42022'.")]
        [OptionParameter(SolidLabel = "tcp-ipv6-port", BriefLabel = "t6p")]
        public UInt16 TcpIPv6Prot { get; set; } = ProgramSettings.DefaulPortTcpIPv6;

        [DefaultValue(false)]
        [HelpSummary("This option enables the WEB HTTP server type. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "web-http", BriefLabel = "wh")]
        public Boolean IsWebHttpType { get; set; } = false;

        [DefaultValue(ProgramSettings.DefaulPortWebHttp)]
        [HelpSummary(
            "This option sets the WEB HTTP server port. The value must be in range of [0..65535]. " +
            "Default value is '42031'.")]
        [OptionParameter(SolidLabel = "web-http-port", BriefLabel = "whp")]
        public UInt16 WebHttpProt { get; set; } = ProgramSettings.DefaulPortWebHttp;

        [HelpSummary("This option shows copyright and program version at start up.")]
        [SwitchParameter(SolidLabel = "version")]
        public Boolean IsVersion { get; set; } = false;

        [HelpSummary(
            "This option enables the debug mode. In this mode some additional information (e.g. binary " +
            "dumps) is printed out at runtime. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "debug")]
        public Boolean IsDebug { get; set; } = false;

        [HelpSummary(
            "This option enables the trace mode. In this mode some of the called methods (including call " +
            "duration) are printed out during runtime. Default value is 'off'.")]
        [SwitchParameter(SolidLabel = "trace")]
        public Boolean IsTrace { get; set; } = false;

        [HelpSummary("This option just displays this help screen.")]
        [SwitchParameter(SolidLabel = "help", BriefLabel = "h,?")]
        public Boolean IsHelp { get; set; } = false;

        #endregion
    }
}
