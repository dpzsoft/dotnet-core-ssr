using System;
using System.Collections.Generic;
using System.Text;

namespace ssr {

    /// <summary>
    /// 服务器事件宿主接口
    /// </summary>
    public interface IServerHost {

        void OnRecieve(ServerHostRecieveEventArgs e);

    }
}
