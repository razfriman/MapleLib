using System;
using System.Net.Sockets;
using MapleLib.Helper;
using MapleLib.MapleCryptoLib;
using Microsoft.Extensions.Logging;

namespace MapleLib.PacketLib
{
    /// <summary>
    /// Class to a network session socket
    /// </summary>
    public class Session
    {
        public static ILogger Log = LogManager.Log;

        /// <summary>
        /// The Session's socket
        /// </summary>
        private readonly Socket _socket;

        /// <summary>
        /// The Recieved packet crypto manager
        /// </summary>
        private MapleCrypto _riv;

        /// <summary>
        /// The Sent packet crypto manager
        /// </summary>
        private MapleCrypto _siv;

        /// <summary>
        /// Method to handle packets received
        /// </summary>
        public delegate void PacketReceivedHandler(PacketReader packet, bool isInit);

        /// <summary>
        /// Packet received event
        /// </summary>
        public event PacketReceivedHandler OnPacketReceived;

        /// <summary>
        /// Method to handle client disconnected
        /// </summary>
        public delegate void ClientDisconnectedHandler(Session session);

        /// <summary>
        /// Client disconnected event
        /// </summary>
        public event ClientDisconnectedHandler OnClientDisconnected;

        public delegate void InitPacketReceived(short version, byte serverIdentifier);

        public event InitPacketReceived OnInitPacketReceived;

        /// <summary>
        /// The Recieved packet crypto manager
        /// </summary>
        public MapleCrypto RIV
        {
            get => _riv;
            set => _riv = value;
        }

        /// <summary>
        /// The Sent packet crypto manager
        /// </summary>
        public MapleCrypto SIV
        {
			get => _siv;
			set => _siv = value;
        }

        /// <summary>
        /// The Session's socket
        /// </summary>
        public Socket Socket => _socket;

        public SessionType Type { get; }

        /// <summary>
        /// Creates a new instance of a Session
        /// </summary>
        /// <param name="socket">Socket connection of the session</param>

        public Session(Socket socket, SessionType type)
        {
            _socket = socket;
            Type = type;
        }

        /// <summary>
        /// Waits for more data to arrive
        /// </summary>
        public void WaitForData() => WaitForData(new SocketInfo(_socket, 4));

        public void WaitForDataNoEncryption() => WaitForData(new SocketInfo(_socket, 2, true));

        /// <summary>
        /// Waits for more data to arrive
        /// </summary>
        /// <param name="socketInfo">Info about data to be received</param>
        private void WaitForData(SocketInfo socketInfo)
        {
            try
            {
                _socket.BeginReceive(socketInfo.DataBuffer,
                    socketInfo.Index,
                    socketInfo.DataBuffer.Length - socketInfo.Index,
                    SocketFlags.None,
                    new AsyncCallback(OnDataReceived),
                    socketInfo);
            }
            catch (Exception se)
            {
                Log.LogError("Session.WaitForData", se);
            }
        }

        /// <summary>
        /// Data received event handler
        /// </summary>
        /// <param name="iar">IAsyncResult of the data received event</param>
        private void OnDataReceived(IAsyncResult iar)
        {
            var socketInfo = (SocketInfo)iar.AsyncState;
            try
            {
                var received = socketInfo.Socket.EndReceive(iar);
                if (received == 0)
                {
                    OnClientDisconnected?.Invoke(this);
                    return;
                }

                socketInfo.Index += received;

                if (socketInfo.Index == socketInfo.DataBuffer.Length)
                {
                    switch (socketInfo.State)
                    {
                        case SocketInfo.StateEnum.Header:
                            if (socketInfo.IsNoEncryption)
                            {
                                var headerReader = new PacketReader(socketInfo.DataBuffer);
                                var packetHeader = headerReader.ReadShort();
                                socketInfo.State = SocketInfo.StateEnum.Content;
                                socketInfo.DataBuffer = new byte[packetHeader];
                                socketInfo.Index = 0;
                                WaitForData(socketInfo);
                            }
                            else
                            {
                                var headerReader = new PacketReader(socketInfo.DataBuffer);
                                var packetHeaderB = headerReader.ToArray();
                                var packetHeader = headerReader.ReadInt();
                                var packetLength = (short)MapleCrypto.GetPacketLength(packetHeader);
                                if (Type == SessionType.SERVER_TO_CLIENT && !_riv.CheckPacketToServer(BitConverter.GetBytes(packetHeader)))
                                {
                                    Log.LogError("Packet check failed. Disconnecting client");
                                    Socket.Close();
                                }
                                socketInfo.State = SocketInfo.StateEnum.Content;
                                socketInfo.DataBuffer = new byte[packetLength];
                                socketInfo.Index = 0;
                                WaitForData(socketInfo);
                            }
                            break;
                        case SocketInfo.StateEnum.Content:
                            var data = socketInfo.DataBuffer;

                            if (socketInfo.IsNoEncryption)
                            {
                                socketInfo.IsNoEncryption = false;
                                var reader = new PacketReader(data);
                                var version = reader.ReadShort();
                                var unknown = reader.ReadMapleString();
                                _siv = new MapleCrypto(reader.ReadBytes(4), version);
                                _riv = new MapleCrypto(reader.ReadBytes(4), version);
                                var serverType = reader.ReadByte();

                                if (Type == SessionType.CLIENT_TO_SERVER)
                                {
                                    OnInitPacketReceived(version, serverType);
                                }

                                OnPacketReceived(new PacketReader(data), true);
                                WaitForData();
                            }
                            else
                            {
                                _riv.Crypt(data);
                                MapleCustomEncryption.Decrypt(data);

                                if (data.Length != 0)
                                {
                                    OnPacketReceived?.Invoke(new PacketReader(data), false);
                                }

                                WaitForData();
                            }
                            break;
                    }
                }
                else
                {
                    Log.LogWarning("Not enough data");
                    WaitForData(socketInfo);
                }
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

        public void SendInitialPacket(int version, string patchLoc, byte[] riv, byte[] siv, byte serverType)
        {
            var writer = new PacketWriter();
            writer.WriteShort(patchLoc == "" ? 0x0D : 0x0E);
            writer.WriteShort(version);
            writer.WriteMapleString(patchLoc);
            writer.WriteBytes(riv);
            writer.WriteBytes(siv);
            writer.WriteByte(serverType);
            SendRawPacket(writer);
        }

        /// <summary>
        /// Encrypts the packet then send it to the client.
        /// </summary>
        /// <param name="pPacket">The PacketWrtier object to be sent.</param>
        public void SendPacket(PacketWriter pPacket) => SendPacket(pPacket.ToArray());

        /// <summary>
        /// Encrypts the packet then send it to the client.
        /// </summary>
        /// <param name="input">The byte array to be sent.</param>
        public void SendPacket(byte[] input)
        {
            var cryptData = input;
            var sendData = new byte[cryptData.Length + 4];
            var header = Type == SessionType.SERVER_TO_CLIENT ? _siv.GetHeaderToClient(cryptData.Length) : _siv.GetHeaderToServer(cryptData.Length);

            MapleCustomEncryption.Encrypt(cryptData);
            _siv.Crypt(cryptData);

            Buffer.BlockCopy(header, 0, sendData, 0, 4);
            Buffer.BlockCopy(cryptData, 0, sendData, 4, cryptData.Length);
            SendRawPacket(sendData);
        }

        /// <summary>
        /// Sends a raw packet to the client
        /// </summary>
        /// <param name="pPacket">The PacketWriter</param>
        public void SendRawPacket(PacketWriter pPacket) => SendRawPacket(pPacket.ToArray());

        /// <summary>
        /// Sends a raw buffer to the client.
        /// </summary>
        /// <param name="buffer">The buffer to be sent.</param>
        public void SendRawPacket(byte[] buffer) => _socket.Send(buffer);
    }
}