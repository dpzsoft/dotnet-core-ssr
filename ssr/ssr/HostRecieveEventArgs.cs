using System;
using System.Collections.Generic;
using System.Text;

namespace ssr {

    /// <summary>
    /// 宿主事件返回
    /// </summary>
    public enum HostEventResults {

        /// <summary>
        /// 默认返回
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 事件完成
        /// </summary>
        Finished = 1,

        /// <summary>
        /// 事件处理发生异常
        /// </summary>
        Error = -1
    }

    /// <summary>
    /// 服务器宿主事件参数
    /// </summary>
    public class HostRecieveEventArgs : System.EventArgs, IDisposable {

        /// <summary>
        /// 事件内容
        /// </summary>
        public String Content { get; internal set; }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() {
            this.Content = null;
        }
    }
}
