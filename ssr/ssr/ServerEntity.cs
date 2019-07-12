using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace ssr {

    /// <summary>
    /// ssr服务端工作实体
    /// </summary>
    public class ServerEntity {

        // 基础网络通讯流
        private NetworkStream _stream;

        // 缓存
        private byte[] _buffer;

        // 临时命令字节列表
        private List<byte> _command;

        /// <summary>
        /// 获取工作标识
        /// </summary>        
        public bool Working { get; private set; }

        /// <summary>
        /// 获取ssr服务端
        /// </summary>
        public Server Server { get; private set; }

        // 接收数据
        private void Recieve(IAsyncResult result) {

            // 获取数据长度
            int len = _stream.EndRead(result);

            // 设置回车标志
            bool r = false;

            // 遍历数据
            for (int i = 0; i < len; i++) {
                switch (_buffer[i]) {
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
                                this.Server.Host.OnRecieve(e);
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
                        _command.Add(_buffer[i]);
                        break;
                }
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

            // 初始化缓存
            _buffer = new byte[4096];
            _command = new List<byte>();

            //新建流并接收客户端消息
            _stream = new NetworkStream(socket);
            _stream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(Recieve), _stream);
        }

        /// <summary>
        /// 向客户端发送内容
        /// </summary>
        /// <param name="content"></param>
        public void Send(string content) {
            _stream.Write(System.Text.Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 向客户端发送带结束标志内容
        /// </summary>
        /// <param name="content"></param>
        public void Sendln(string content) {
            _stream.Write(System.Text.Encoding.UTF8.GetBytes(content + "\r\n"));
        }

    }
}
