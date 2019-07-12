using System;
using System.Collections.Generic;
using System.Text;

namespace ssr {

    /// <summary>
    /// 服务器宿主事件参数
    /// </summary>
    public class ServerHostRecieveEventArgs : System.EventArgs, IDisposable {

        /// <summary>
        /// 获取服务端对象
        /// </summary>
        public Server Server { get; internal set; }

        /// <summary>
        /// 获取服务端实体对象
        /// </summary>
        public ServerEntity Entity { get; internal set; }

        /// <summary>
        /// 事件内容
        /// </summary>
        public String Content { get; internal set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() {
            this.Server = null;
            this.Entity = null;
            this.Content = null;
        }
    }
}
