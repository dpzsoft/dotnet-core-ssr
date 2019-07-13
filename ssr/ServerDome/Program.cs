using System;
using System.Net;

namespace ServerDome {
    class Program {
        static void Main(string[] args) {

            // 建立一个新的服务端
            ssr.Server.Build(
                // 关联宿主对象类型
                typeof(ServerHost),
                // 绑定IP
                IPAddress.Any,
                // 设置服务端口
                8888
                // 以独占模式运行服务，如果需要配合其他服务运行，可使用Start开始同步服务模式
                ).Run();

        }
    }
}
