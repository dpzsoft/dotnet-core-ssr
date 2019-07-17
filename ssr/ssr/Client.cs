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

        // 缓存及关联参数
        private List<byte> _command;//命令迷失缓存字节列表
        private byte[] _buffer;//数据模式缓存数组
        private int _offset;//当前处理偏移

        /// <summary>
        /// 获取当前是否为数据模式
        /// </summary>
        public bool DataMode { get; private set; }

        // 宿主处理对象
        private IHost _host;

        /// <summary>
        /// 设置为命令模式
        /// </summary>
        public void SetCommandMode() {
            DataMode = false;
        }

        /// <summary>
        /// 设置为数据模式
        /// </summary>
        /// <param name="len">数据长度</param>
        public void SetDataMode(int len) {
            _buffer = new byte[len];
            DataMode = true;

            // 初始化偏移
            _offset = 0;
        }

        /// <summary>
        /// 实例化ssr客户端并连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Client(IHost host, string ip, int port) {

            // 绑定宿主处理对象
            _host = host;

            // 初始化基础网络通讯组件并连接
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ip, port);

            // 设置网络通讯流
            _stream = new NetworkStream(socket);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes">待发送的字节数组</param>
        /// <param name="callback">回调函数</param>
        public void Send(byte[] bytes, SendCallback callback = null) {

            // 发送信息
            _stream.Write(bytes);
            _stream.Flush();

            // 当设置了回调后，开始等待接收数据
            if (callback != null) {

                // 结束和超时标志
                bool isEnd = false;

                // 设置回车标志
                bool r = false;

                // 设置字节列表
                _command = new List<byte>();

                // 设置当未满足结束和未超时时进行循环
                while (!isEnd) {

                    // 根据当前模式读取数据
                    if (this.DataMode) {
                        #region [=====数据模式=====]

                        // 根据缓存大小读取数据
                        int len = _stream.Read(_buffer, _offset, _buffer.Length - _offset);

                        // 增加数据偏移
                        _offset += len;

                        // 判断数据是否读取完毕,如已读取完毕，则开始处理业务
                        if (_offset >= _buffer.Length) {

                            // 获取数据字符串
                            string data = System.Text.Encoding.UTF8.GetString(_buffer);

                            // 执行业务调用
                            using (ClientHostRecieveEventArgs e = new ClientHostRecieveEventArgs()) {
                                e.Client = this;
                                e.Content = data;

                                //执行宿主事件
                                _host.OnRecieve(e);

                                if (e.Result == HostEventResults.Finished) {
                                    // 设置回调
                                    callback(e.ResultData);

                                    // 设置结束标志
                                    isEnd = true;
                                }
                            }

                            // 设置为命令模式
                            SetCommandMode();

                        }

                        #endregion
                    } else {
                        #region [=====命令模式=====]
                        //命令模式,判断换行标志提取命令

                        // 读取一个字节
                        int bs = _stream.ReadByte();

                        if (bs >= 0) {
                            // 读入正常数据后进行数据解析
                            switch (bs) {
                                case 13://回车(\r)

                                    // 出现两个连续的回车标志则视为非常规
                                    if (r) {
                                        //调试输出错误信息
                                        Debug.WriteLine("-> Error:规则外的回车符");
                                    }

                                    r = true;
                                    break;
                                case 10://换行(\n)

                                    if (r) {
                                        //获取行命令并重置回车标志
                                        string cmd = System.Text.Encoding.UTF8.GetString(_command.ToArray());
                                        r = false;

                                        //执行业务调用
                                        using (ClientHostRecieveEventArgs e = new ClientHostRecieveEventArgs()) {
                                            e.Client = this;
                                            e.Content = cmd;

                                            //执行宿主事件
                                            _host.OnRecieve(e);

                                            if (e.Result == HostEventResults.Finished) {
                                                // 设置回调
                                                callback(e.ResultData);

                                                // 设置结束标志
                                                isEnd = true;
                                            }
                                        }

                                    } else {
                                        //调试输出错误信息
                                        Debug.WriteLine("-> Error:规则外的换行符");
                                    }

                                    // 清除命令字节列表
                                    _command.Clear();
                                    break;
                                default:

                                    // 正常情况下，不应该在此处出现回车标志
                                    if (r) {
                                        //调试输出错误信息
                                        Debug.WriteLine("-> Error:规则外的换行符");

                                        // 清除命令字节列表并重置回车标志
                                        _command.Clear();
                                        r = false;
                                    }

                                    // 将命令字符加入命令字节列表中
                                    _command.Add((byte)bs);
                                    break;
                            }
                        } else {
                            // 未读取数据，则线程等待毫秒，防止线程阻塞
                            System.Threading.Thread.Sleep(10);
                        }

                        #endregion
                    }

                }


            }

        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="content">待发送的操作命令，以\r\n结尾</param>
        /// <param name="callback">回调函数</param>
        public void Send(string content, SendCallback callback = null) {

            // 发送信息
            Send(Encoding.UTF8.GetBytes(content), callback);

        }

        /// <summary>
        /// 发送带换行标志的数据
        /// </summary>
        /// <param name="content">待发送的操作命令，以\r\n结尾</param>
        /// <param name="callback">回调函数</param>
        public void Sendln(string content, SendCallback callback = null) {
            Send(content + "\r\n", callback);
        }

        /// <summary>
        /// 独立模式发送数据
        /// </summary>
        /// <param name="host"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        public static void Send(IHost host, string ip, int port, string content, SendCallback callback = null) {
            // 建立客户端并连接服务器
            using (ssr.Client client = new ssr.Client(host, "127.0.0.1", 8888)) {

                // 发送测试数据
                client.Send(content, callback);
            }
        }

        /// <summary>
        /// 独立模式发送带换行标志数据
        /// </summary>
        /// <param name="host"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        public static void Sendln(IHost host, string ip, int port, string content, SendCallback callback = null) {
            Send(host, ip, port, content + "\r\n", callback);
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
