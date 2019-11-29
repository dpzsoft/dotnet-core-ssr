using System;
using System.Collections.Generic;
using System.Text;

namespace ssr2 {

    /// <summary>
    /// 事件宿主接口
    /// </summary>
    public interface IHost {

        void OnRecieve(HostRecieveEventArgs e);

    }
}
