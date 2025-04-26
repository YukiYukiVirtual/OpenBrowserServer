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
        public OpenBrowserToken()
        {
            Token = "";
        }
        public void NewToken()
        {
            Token = Guid.NewGuid().ToString();
        }
    }
}
