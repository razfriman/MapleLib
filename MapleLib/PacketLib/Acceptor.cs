using System;
using System.Net;
using System.Net.Sockets;
using MapleLib.Helper;
using Microsoft.Extensions.Logging;

namespace MapleLib.PacketLib
{
    /// <summary>
    /// A Nework Socket Acceptor (Listener)
    /// </summary>
    public class Acceptor
    {
        public static ILogger Log = LogManager.Log;

        public const int BACKLOG_SIZE = 15;

        /// <summary>
        /// The listener socket
        /// </summary>
        private readonly Socket _listener;

        /// <summary>
        /// Method called when a client is connected
        /// </summary>
        public delegate void ClientConnectedHandler(Session session);

        /// <summary>
        /// Client connected event
        /// </summary>
        public event ClientConnectedHandler OnClientConnected;

        /// <summary>
        /// Creates a new instance of Acceptor
        /// </summary>
        public Acceptor() => _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// Starts listening and accepting connections
        /// </summary>
        /// <param name="port">Port to listen to</param>
        public void StartListening(int port)
        {
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _listener.Listen(BACKLOG_SIZE);
            _listener.BeginAccept(new AsyncCallback(OnClientConnect), null);
        }

        /// <summary>
        /// Stops listening for connections
        /// </summary>
        public void StopListening() => _listener.Disconnect(true);

        /// <summary>
        /// Client connected handler
        /// </summary>
        /// <param name="iar">The IAsyncResult</param>
        private void OnClientConnect(IAsyncResult iar)
        {
            try
            {
                var socket = _listener.EndAccept(iar);
                var session = new Session(socket, SessionType.SERVER_TO_CLIENT);

                OnClientConnected?.Invoke(session);
                session.WaitForData();
                _listener.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException e)
            {
                Log.LogError("OnClientConnect: Socket closed.", e);
            }
            catch (Exception e)
            {
                Log.LogError("OnClientConnect", e);
            }
        }
    }
}