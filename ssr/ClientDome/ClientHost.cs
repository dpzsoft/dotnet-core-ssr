using System;
using System.Collections.Generic;
using System.Text;
using ssr;

namespace ClientDome {

    /// <summary>
    /// 服务器事件宿主
    /// </summary>
    public class ClientHost : ssr.IHost {

        // 数据模式定义
        private bool _data = false;

        public void OnRecieve(HostRecieveEventArgs e) {

            // 此demo进行了一次简单的定长数据获取示例
            if (_data) {
                // 数据模式

                // 输出内容
                Console.WriteLine($"-> 接受数据 -> {e.Content}");

                // 测试协议，原封内容发回客户端
                ClientHostRecieveEventArgs args = (ClientHostRecieveEventArgs)e;
                args.ResultData = e.Content;
                args.Result = HostEventResults.Finished;
            } else {
                //命令模式

                // 输出内容
                Console.WriteLine($"-> 接受定义命令 -> {e.Content}");

                //此处以$开头定义数据长度
                if (e.Content.StartsWith("$")) {
                    _data = true;
                    int len = int.Parse(e.Content.Substring(1));

                    // 输出内容
                    Console.WriteLine($"-> 定义数据长度：{len}");

                    ClientHostRecieveEventArgs args = (ClientHostRecieveEventArgs)e;
                    args.Client.SetDataMode(len);
                }
            }


        }
    }
}
