using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrowserServer.Component
{
    public class OpenBrowserToken
    {
        public string Token { get; private set; } // 各自の環境で一意に決まるトークン、このアプリ起動時に決定し、VRChatからHttpリクエストを介して取得される。
        public bool OldInterfaceUsedFlag { get; set; } // トークンを使用しない古いインターフェイスからのリクエストがあったことを保持するフラグ
        public bool AllowOldInterface { get; set; } // トークンを使用しない古いインターフェイスからのリクエストを許可するフラグ
        public OpenBrowserToken()
        {
            Token = Guid.NewGuid().ToString();
            AllowOldInterface = true; // いずれfalseをデフォルトに変える
            OldInterfaceUsedFlag = false;
        }
    }
}
