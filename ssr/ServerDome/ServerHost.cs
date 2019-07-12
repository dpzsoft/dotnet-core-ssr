using System;
using System.Collections.Generic;
using System.Text;
using ssr;

namespace ServerDome {

    /// <summary>
    /// 服务器事件宿主
    /// </summary>
    public class ServerHost : ssr.IServerHost {

        public void OnRecieve(ServerHostRecieveEventArgs e) {

            // 输出内容
            Console.WriteLine($"-> 接受数据 -> {e.Content}");

            // 进行内容回显,这里可根据实际业务处理后发送处理结果
            e.Entity.Sendln(e.Content);

        }
    }
}
