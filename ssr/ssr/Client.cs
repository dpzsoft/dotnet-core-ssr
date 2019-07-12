using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ssr {

    /// <summary>
    /// ssr客户端
    /// </summary>
    public class Client : IDisposable {

        /// <summary>
        /// 发送数据回调
        /// </summary>
        /// <param name="data"></param>
        public delegate void SendCallback(string data);

        // 网络通讯流
        private NetworkStream _stream;

        /// <summary>
        /// 实例化ssr客户端并连接
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Client(string ip, int port) {

            // 初始化基础网络通讯组件并连接
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);

            // 设置网络通讯流
            _stream = new NetworkStream(socket);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="content">待发送的操作命令，以\r\n结尾</param>
        /// <param name="callback">回调函数</param>
        /// <param name="timeout">回调等待超时(秒)，默认为-1，即不限时等待</param>
        public void Send(string content, SendCallback callback = null, int timeout = -1) {

            // 发送信息
            _stream.Write(Encoding.UTF8.GetBytes(content));
            _stream.Flush();

            // 当设置了回调后，开始等待接收数据
            if (callback != null) {

                // 计时器
                int tick = Environment.TickCount;

                // 结束和超时标志
                bool isEnd = false;
                bool isTimeout = false;

                // 设置回车标志
                bool r = false;

                // 设置字节列表
                List<byte> bytes = new List<byte>();

                // 设置当未满足结束和未超时时进行循环
                while (!(isEnd || isTimeout)) {
                    //读取一个数据
                    int bs = _stream.ReadByte();

                    switch (bs) {
                        case 13:// 回车(\r)

                            // 出现两个连续的回车标志则视为非常规
                            if (r) {
                                // 调试输出错误信息
                                Debug.WriteLine("-> Error:规则外的回车符");
                            }

                            r = true;
                            break;
                        case 10:// 换行(\n)

                            if (r) {
                                // 获取内容并重置回车标志
                                string str = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
                                r = false;

                                // 执行业务回调
                                callback(str);

                                // 设置结束标志
                                isEnd = true;
                            } else {
                                // 调试输出错误信息
                                Debug.WriteLine("-> Error:规则外的换行符");
                            }
                            break;
                        default:

                            // 正常情况下，不应该在此处出现回车标志
                            if (r) {
                                //调试输出错误信息
                                Debug.WriteLine("-> Error:规则外的换行符");

                                // 清除命令字节列表并重置回车标志
                                bytes.Clear();
                                r = false;
                            }

                            // 将命令字符加入命令字节列表中
                            bytes.Add((byte)bs);
                            break;
                    }
                }


            }

        }

        /// <summary>
        /// 发送带换行标志的数据
        /// </summary>
        /// <param name="content">待发送的操作命令，以\r\n结尾</param>
        /// <param name="callback">回调函数</param>
        /// <param name="timeout">回调等待超时(秒)，默认为-1，即不限时等待</param>
        public void Sendln(string content, SendCallback callback = null, int timeout = -1) {
            Send(content + "\r\n", callback, timeout);
        }

        /// <summary>
        /// 独立模式发送数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        public static void Send(string ip, int port, string content, SendCallback callback = null, int timeout = -1) {
            // 建立客户端并连接服务器
            using (ssr.Client client = new ssr.Client("127.0.0.1", 8888)) {

                // 发送测试数据
                client.Send(content, callback, timeout);
            }
        }

        /// <summary>
        /// 独立模式发送带换行标志数据
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        public static void Sendln(string ip, int port, string content, SendCallback callback = null, int timeout = -1) {
            Send(ip, port, content + "\r\n", callback, timeout);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() {
            // 断开网络连接
            _stream.Close();
            _stream.Dispose();
            _stream = null;
        }
    }
}
