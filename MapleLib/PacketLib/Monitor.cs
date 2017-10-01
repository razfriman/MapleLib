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
		private readonly Socket mSocket;

		/// <summary>
		/// The Recieved packet crypto manager
		/// </summary>
		private MapleCrypto mRIV;

		/// <summary>
		/// The Sent packet crypto manager
		/// </summary>
		private MapleCrypto mSIV;

		/// <summary>
		/// Method to handle packets received
		/// </summary>
		public delegate void PacketReceivedHandler(PacketReader pPacket);

		/// <summary>
		/// Packet received event
		/// </summary>
		//public event PacketReceivedHandler OnPacketReceived;//Unused

		/// <summary>
		/// Method to handle client disconnected
		/// </summary>
		public delegate void ClientDisconnectedHandler(Monitor pMonitor);

		/// <summary>
		/// Client disconnected event
		/// </summary>
		public event ClientDisconnectedHandler OnClientDisconnected;

		/// <summary>
		/// The Recieved packet crypto manager
		/// </summary>
		public MapleCrypto RIV
		{
			get { return mRIV; }
			set { mRIV = value; }
		}

		/// <summary>
		/// The Sent packet crypto manager
		/// </summary>
		public MapleCrypto SIV
		{
			get { return mSIV; }
			set { mSIV = value; }
		}

		/// <summary>
		/// The Monitor's socket
		/// </summary>
		public Socket Socket
		{
			get { return mSocket; }
		}

		/// <summary>
		/// Creates a new instance of Monitor
		/// </summary>
		public Monitor()
		{
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
		}

		/// <summary>
		/// Starts listening and accepting connections
		/// </summary>
		public void StartMonitoring(IPAddress pIP)
		{

			mSocket.Bind(new IPEndPoint(pIP, 0));

			mSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);//??

			byte[] byIn = { 1, 0, 0, 0 };
			byte[] byOut = null;

			mSocket.IOControl(IOControlCode.ReceiveAll, byIn, byOut);

			WaitForData();
		}

		/// <summary>
		/// Waits for more data to arrive
		/// </summary>
		public void WaitForData()
		{
			WaitForData(new SocketInfo(mSocket, short.MaxValue));
		}

		/// <summary>
		/// Waits for more data to arrive
		/// </summary>
		/// <param name="pSocketInfo">Info about data to be received</param>
		private void WaitForData(SocketInfo pSocketInfo)
		{
			try
			{
				mSocket.BeginReceive(pSocketInfo.DataBuffer,
					pSocketInfo.Index,
					pSocketInfo.DataBuffer.Length - pSocketInfo.Index,
					SocketFlags.None,
					new AsyncCallback(OnDataReceived),
					pSocketInfo);
			}
			catch (Exception se)
			{
                Log.LogError("Session.WaitForData", se);
			}
		}

		private void OnDataReceived(IAsyncResult pIAR)
		{
			SocketInfo socketInfo = (SocketInfo)pIAR.AsyncState;
			try
			{
				int received = socketInfo.Socket.EndReceive(pIAR);
				if (received == 0)
				{
                    OnClientDisconnected?.Invoke(this);
					return;
				}

				socketInfo.Index += received;


				byte[] dataa = new byte[received];
				Buffer.BlockCopy(socketInfo.DataBuffer, 0, dataa, 0, received);

                Log.LogInformation(BitConverter.ToString(dataa));
				Log.LogInformation(HexEncoding.ToStringFromAscii(dataa));
				WaitForData();
				/*if (socketInfo.Index == socketInfo.DataBuffer.Length) {
					switch (socketInfo.State) {
						case SocketInfo.StateEnum.Header:
							PacketReader headerReader = new PacketReader(socketInfo.DataBuffer);
							byte[] packetHeaderB = headerReader.ToArray();
							int packetHeader = headerReader.ReadInt();
							short packetLength = (short)MapleCrypto.getPacketLength(packetHeader);
							if (!_RIV.checkPacket(packetHeader)) {
								Console.WriteLine("[Error] Packet check failed. Disconnecting client.");
								this.Socket.Close();
								}
							socketInfo.State = SocketInfo.StateEnum.Content;
							socketInfo.DataBuffer = new byte[packetLength];
							socketInfo.Index = 0;
							WaitForData(socketInfo);
							break;
						case SocketInfo.StateEnum.Content:
							byte[] data = socketInfo.DataBuffer;

							_RIV.crypt(data);
							MapleCustomEncryption.Decrypt(data);

							if (data.Length != 0 && OnPacketReceived != null) {
								OnPacketReceived(new PacketReader(data));
								}
							WaitForData();
							break;
						}
					} else {
					Console.WriteLine("[Warning] Not enough data");
					WaitForData(socketInfo);
					}*/
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