using System;
using System.Collections.Generic;
using System.Text;

namespace ssr2 {

    /// <summary>
    /// 服务端专属接收数据宿主事件参数
    /// </summary>
    public class ServerHostRecieveEventArgs : HostRecieveEventArgs {

        /// <summary>
        /// 获取服务端对象
        /// </summary>
        public Server Server { get; internal set; }

        /// <summary>
        /// 获取服务端实体对象
        /// </summary>
        public ServerEntity Entity { get; internal set; }

    }
}
