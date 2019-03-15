using System;
using System.Net;
using System.Net.Sockets;
using MapleLib.Helper;
using MapleLib.MapleCryptoLib;
using Microsoft.Extensions.Logging;

namespace MapleLib.PacketLib
{
    public class Monitor
    {
        public static ILogger Log = LogManager.Log;

        /// <summary>
        /// The Monitor socket
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// Method to handle packets received
        /// </summary>
        public delegate void PacketReceivedHandler(PacketReader packet);

        /// <summary>
        /// Method to handle client disconnected
        /// </summary>
        public delegate void ClientDisconnectedHandler(Monitor monitor);

        /// <summary>
        /// Client disconnected event
        /// </summary>
        public event ClientDisconnectedHandler OnClientDisconnected;

        /// <summary>
        /// The Recieved packet crypto manager
        /// </summary>
        public MapleCrypto RIV { get; set; }

        /// <summary>
        /// The Sent packet crypto manager
        /// </summary>
        public MapleCrypto SIV { get; set; }

        /// <summary>
        /// The Monitor's socket
        /// </summary>
        public Socket Socket => _socket;

        /// <summary>
        /// Creates a new instance of Monitor
        /// </summary>
        public Monitor() => _socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);

        /// <summary>
        /// Starts listening and accepting connections
        /// </summary>
        public void StartMonitoring(IPAddress pIP)
        {
            _socket.Bind(new IPEndPoint(pIP, 0));
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true); //??

            byte[] byIn = {1, 0, 0, 0};
            byte[] byOut = null;

            _socket.IOControl(IOControlCode.ReceiveAll, byIn, byOut);

            WaitForData();
        }

        /// <summary>
        /// Waits for more data to arrive
        /// </summary>
        public void WaitForData() => WaitForData(new SocketInfo(_socket, short.MaxValue));

        /// <summary>
        /// Waits for more data to arrive
        /// </summary>
        /// <param name="pSocketInfo">Info about data to be received</param>
        private void WaitForData(SocketInfo pSocketInfo)
        {
            try
            {
                _socket.BeginReceive(pSocketInfo.DataBuffer,
                    pSocketInfo.Index,
                    pSocketInfo.DataBuffer.Length - pSocketInfo.Index,
                    SocketFlags.None,
                    OnDataReceived,
                    pSocketInfo);
            }
            catch (Exception se)
            {
                Log.LogError("Session.WaitForData", se);
            }
        }

        private void OnDataReceived(IAsyncResult pIAR)
        {
            var socketInfo = (SocketInfo) pIAR.AsyncState;
            try
            {
                var received = socketInfo.Socket.EndReceive(pIAR);
                if (received == 0)
                {
                    OnClientDisconnected?.Invoke(this);
                    return;
                }

                socketInfo.Index += received;


                var data = new byte[received];
                Buffer.BlockCopy(socketInfo.DataBuffer, 0, data, 0, received);

                Log.LogInformation(BitConverter.ToString(data));
                Log.LogInformation(HexEncoding.ToStringFromAscii(data));

                WaitForData();
            }
            catch (ObjectDisposedException e)
            {
                Log.LogError("Socket has been closed", e);
            }
            catch (SocketException se)
            {
                if (se.ErrorCode != 10054)
                {
                    Log.LogError("Session.OnDataReceived", se);
                }
            }
            catch (Exception e)
            {
                Log.LogError("Session.OnDataReceived", e);
            }
        }
    }
}