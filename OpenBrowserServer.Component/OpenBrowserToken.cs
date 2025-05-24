using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrowserServer.Component
{
    public class OpenBrowserToken
    {
        public string Token { get; private set; }
        public bool OldInterfaceUsedFlag { get; set; }
        public bool AllowOldInterface { get; set; }
        public OpenBrowserToken()
        {
            NewToken();
            AllowOldInterface = true; // いずれfalseをデフォルトに変える
        }
        public void NewToken()
        {
            Token = Guid.NewGuid().ToString();
            OldInterfaceUsedFlag = false;
        }
    }
}
