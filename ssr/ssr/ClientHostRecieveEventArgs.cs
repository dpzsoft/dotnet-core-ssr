using System;
using System.Collections.Generic;
using System.Text;

namespace ssr {

    /// <summary>
    /// 客户端专属接收数据宿主事件参数
    /// </summary>
    public class ClientHostRecieveEventArgs : HostRecieveEventArgs {

        /// <summary>
        /// 获取客户端实体对象
        /// </summary>
        public Client Client { get; internal set; }

        /// <summary>
        /// 获取或设置事件结果
        /// </summary>
        public HostEventResults Result { get; set; } = HostEventResults.Normal;

        /// <summary>
        /// 获取或设置事件结果数据
        /// </summary>
        public String ResultData { get; set; }
    }
}
