using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace OpenBrowserServer.Component
{
    public class Setting
    {
        public string Version = "";
        public int IdlePeriod = 500;
        public int HttpRequestPeriod = 5000;
        public int WatchdogTime = 30;
        public List<string> Protocol = new List<string>();
        public List<string> Domain = new List<string>();
        public List<BannedUserInfo> BannedUser = new List<BannedUserInfo>();

        public class BannedUserInfo
        {
            public string Id;
            public string Reason = "未記入";
        }
        public override string ToString()
        {
            return new Serializer().Serialize(this).Trim();
        }
    }
}
