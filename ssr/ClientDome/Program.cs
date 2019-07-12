using System;

namespace ClientDome {
    class Program {
        static void Main(string[] args) {

            // 独立模式发送测试数据
            ssr.Client.Sendln("127.0.0.1", 8888, "Test", (string data) => {

                // 将回调数据输出到控制台
                Console.Write($"-> 接收数据 -> {data}");

            });

        }
    }
}
