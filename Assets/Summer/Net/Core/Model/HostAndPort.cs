using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Spring.Util;

namespace Summer.Net.Core.Model
{
    public class HostAndPort
    {
        public string host;
        public int port;

        public static HostAndPort ValueOf(string host, int port)
        {
            HostAndPort hostAndPort = new HostAndPort();
            hostAndPort.host = host;
            hostAndPort.port = port;
            return hostAndPort;
        }

        /**
         * @param hostAndPort example -> localhost:port
         */
        public static HostAndPort ValueOf(string hostAndPort)
        {
            var split = Regex.Split(hostAndPort.Trim(), StringUtils.COLON_REGEX);
            return ValueOf(split[0].Trim(), int.Parse(split[1].Trim()));
        }

        /**
         * @param hostAndPorts example -> localhost:port,localhost:port,localhost:port
         */
        public static List<HostAndPort> ToHostAndPortList(string hostAndPorts)
        {
            if (StringUtils.IsEmpty(hostAndPorts))
            {
                return CollectionUtils.EmptyList<HostAndPort>();
            }

            var hostAndPortSplits = Regex.Split(hostAndPorts, StringUtils.COMMA_REGEX);
            var hostAndPortList = new List<HostAndPort>();

            foreach (var hostAndPort in hostAndPortSplits)
            {
                hostAndPortList.Add(ValueOf(hostAndPort));
            }

            return hostAndPortList;
        }

        public static List<HostAndPort> ToHostAndPortList(Collection<string> list)
        {
            if (CollectionUtils.IsEmpty(list))
            {
                return CollectionUtils.EmptyList<HostAndPort>();
            }

            var hostAndPortList = new List<HostAndPort>();
            foreach (var str in list)
            {
                hostAndPortList.AddRange(ToHostAndPortList(str));
            }

            return hostAndPortList;
        }

        public static string ToHostAndPortListStr(Collection<HostAndPort> list)
        {
            var urlList = list.Select(it => it.ToHostAndPortStr()).ToList();
            return StringUtils.JoinWith(StringUtils.COMMA, urlList.ToArray());
        }


        public string ToHostAndPortStr()
        {
            return StringUtils.Format("{}:{}", this.host.Trim(), this.port);
        }


        public override string ToString()
        {
            return StringUtils.Format("[{}]", ToHostAndPortStr());
        }
    }
}