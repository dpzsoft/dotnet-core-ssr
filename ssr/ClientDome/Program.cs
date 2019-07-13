using System;

namespace ClientDome {
    class Program {
        static void Main(string[] args) {

            // 建立标准模式测试客户端连接
            using (ssr.Client client = new ssr.Client(new ClientHost(), "127.0.0.1", 8888)) {
                // 发送长度定义
                client.Sendln("$5");
                // 发送测试数据内容
                client.Send("Test1", (string data) => {

                    // 将回调数据输出到控制台
                    Console.WriteLine($"=> 回调1 -> {data}");

                });
            }

            // 一次性模式测试连接并发送完整数据
            ssr.Client.Send(new ClientHost(), "127.0.0.1", 8888, "$5\r\nTest2", (string data) => {

                // 将回调数据输出到控制台
                Console.WriteLine($"=> 回调2 -> {data}");

            });

        }
    }
}
