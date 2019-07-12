using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ssr {

    /// <summary>
    /// ssr服务端
    /// </summary>
    public class Server {

        // 基础网络通讯组件
        private Socket _socket;

        /// <summary>
        /// 事件宿主
        /// </summary>
        internal IServerHost Host { get; private set; }

        /// <summary>
        /// 获取工作标识
        /// </summary>        
        public bool Working { get; private set; }

        /// <summary>
        /// 获取工作实体集合
        /// </summary>
        public List<ServerEntity> Entities { get; private set; }

        /// <summary>
        /// 实例化一个新的ssr服务端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public Server(IServerHost host, IPAddress ip, int port) {

            // 设置事件宿主
            this.Host = host;

            // 实例化基础网络通讯组件
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(ip, port));

            // 初始化工作实体集合
            this.Entities = new List<ServerEntity>();

            // 设置初始化工作标识
            this.Working = false;

        }

        /// <summary>
        /// 建立一个新的ssr服务端
        /// </summary>
        /// <param name="host"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Server Build(IServerHost host, IPAddress ip, int port) {
            return new Server(host, ip, port);
        }

        // 接受连接
        private void SocketAccept(IAsyncResult result) {

            // 判断并设置工作标识
            if (!this.Working) return;

            //获取响应的通讯组件
            Socket client = _socket.EndAccept(result);

            try {

                //处理下一个客户端连接
                _socket.BeginAccept(new AsyncCallback(SocketAccept), _socket);

                //新增一个实体
                ServerEntity wsc = new ServerEntity(this, client);
                this.Entities.Add(wsc);

            } catch (Exception ex) {
                //调试输出错误信息
                Debug.WriteLine("-> Error:\r\n" + ex.ToString());
            }
        }

        /// <summary>
        /// 开始服务
        /// </summary>
        public void Start() {

            // 判断并设置工作标识
            if (this.Working) return;
            this.Working = true;

            // 监听基础网络通讯组件
            _socket.Listen(10);

            // 开始接受连接
            _socket.BeginAccept(SocketAccept, _socket);
        }

        /// <summary>
        /// 宿主运行服务
        /// </summary>
        public void Run() {

            // 判断并设置工作标识
            if (this.Working) return;
            this.Working = true;

            // 监听基础网络通讯组件
            _socket.Listen(10);

            // 接受连接
            while (this.Working) {

                //接受一个连接
                Socket socket = _socket.Accept();

                //新增一个实体
                ServerEntity wsc = new ServerEntity(this, socket);
                this.Entities.Add(wsc);
            }

        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop() {

            // 判断并设置工作标识
            if (!this.Working) return;
            this.Working = false;

            // 断开监听
            _socket.Close();
        }

    }
}
