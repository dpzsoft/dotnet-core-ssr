using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ssr {

    /// <summary>
    /// ssr服务端工作实体
    /// </summary>
    public class ServerEntity : IDisposable {

        // 基础网络通讯流
        private NetworkStream _stream;

        // 宿主处理对象
        private IHost _host;

        // 数据接收线程
        System.Threading.Thread _recieveThread;

        // 缓存及关联参数
        private List<byte> _command;//命令迷失缓存字节列表
        private byte[] _buffer;//数据模式缓存数组
        private int _offset;//当前处理偏移

        /// <summary>
        /// 获取当前是否为数据模式
        /// </summary>
        public bool DataMode { get; private set; }

        /// <summary>
        /// 获取工作标识
        /// </summary>        
        public bool Working { get; private set; }

        /// <summary>
        /// 获取ssr服务端
        /// </summary>
        public Server Server { get; private set; }

        /// <summary>
        /// 设置为命令模式
        /// </summary>
        public void SetCommandMode() {
            this.DataMode = false;
        }

        /// <summary>
        /// 设置为数据模式
        /// </summary>
        /// <param name="len">数据长度</param>
        public void SetDataMode(int len) {
            _buffer = new byte[len];
            this.DataMode = true;

            // 初始化偏移
            _offset = 0;
        }

        // 接收数据线程
        private void RecieveThread() {

            try {

                // 初始化回车标志
                bool r = false;

                // 工作正常时循环读取内容
                while (this.Working) {

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
                            using (ServerHostRecieveEventArgs e = new ServerHostRecieveEventArgs()) {
                                e.Server = this.Server;
                                e.Entity = this;
                                e.Content = data;

                                //执行宿主事件
                                _host.OnRecieve(e);
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
                                        using (ServerHostRecieveEventArgs e = new ServerHostRecieveEventArgs()) {
                                            e.Server = this.Server;
                                            e.Entity = this;
                                            e.Content = cmd;

                                            //执行宿主事件
                                            _host.OnRecieve(e);
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

            } catch (Exception ex) {
                // 调试输出错误信息
                Debug.WriteLine($"-> Error:{ex.Message}");
            }

        }

        /// <summary>
        /// 实例化一个工作实体
        /// </summary>
        /// <param name="server"></param>
        /// <param name="socket"></param>
        public ServerEntity(Server server, Socket socket) {

            // 设置关联服务端
            this.Server = server;

            // 初始化宿主对象
            _host = Activator.CreateInstance(server.HostType) as IHost;

            // 初始化缓存
            _command = new List<byte>();

            // 默认设置为命令模式
            SetCommandMode();

            // 新建消息流
            _stream = new NetworkStream(socket);

            // 设置工作标识
            this.Working = true;

            // 建立一个处理接收数据的线程
            _recieveThread = new System.Threading.Thread(RecieveThread);
            _recieveThread.Start();

        }

        /// <summary>
        /// 向客户端发送内容
        /// </summary>
        /// <param name="content"></param>
        public void Send(byte[] bytes) {
            _stream.Write(bytes);
            _stream.Flush();
        }

        /// <summary>
        /// 向客户端发送内容
        /// </summary>
        /// <param name="content"></param>
        public void Send(string content) {
            Send(System.Text.Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 向客户端发送带结束标志内容
        /// </summary>
        /// <param name="content"></param>
        public void Sendln(string content) {
            Send(content + "\r\n");
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close() {

            // 设置工作标识
            if (this.Working) this.Working = false;

            try {
                // 结束线程
                _recieveThread.Abort();

                // 关闭连接
                _stream.Close();
            } catch (Exception ex) {
                //调试输出错误信息
                Debug.WriteLine($"-> Error:{ex.Message}");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() {
            this.Close();
            _host = null;
            _buffer = null;
            _stream.Dispose();
        }
    }
}
