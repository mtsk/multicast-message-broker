using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MulticastTest
{
	#region CRC16

	/// <summary>
	/// Any crc should do, this one was taken from: 
	/// http://stackoverflow.com/questions/5059268/c-sharp-crc-implementation
	/// </summary>
	public class Crc16CcittKermit
	{
		private static ushort[] table = {
		  0x0000, 0x1189, 0x2312, 0x329B, 0x4624, 0x57AD, 0x6536, 0x74BF,
		  0x8C48, 0x9DC1, 0xAF5A, 0xBED3, 0xCA6C, 0xDBE5, 0xE97E, 0xF8F7,
		  0x1081, 0x0108, 0x3393, 0x221A, 0x56A5, 0x472C, 0x75B7, 0x643E,
		  0x9CC9, 0x8D40, 0xBFDB, 0xAE52, 0xDAED, 0xCB64, 0xF9FF, 0xE876,
		  0x2102, 0x308B, 0x0210, 0x1399, 0x6726, 0x76AF, 0x4434, 0x55BD,
		  0xAD4A, 0xBCC3, 0x8E58, 0x9FD1, 0xEB6E, 0xFAE7, 0xC87C, 0xD9F5,
		  0x3183, 0x200A, 0x1291, 0x0318, 0x77A7, 0x662E, 0x54B5, 0x453C,
		  0xBDCB, 0xAC42, 0x9ED9, 0x8F50, 0xFBEF, 0xEA66, 0xD8FD, 0xC974,
		  0x4204, 0x538D, 0x6116, 0x709F, 0x0420, 0x15A9, 0x2732, 0x36BB,
		  0xCE4C, 0xDFC5, 0xED5E, 0xFCD7, 0x8868, 0x99E1, 0xAB7A, 0xBAF3,
		  0x5285, 0x430C, 0x7197, 0x601E, 0x14A1, 0x0528, 0x37B3, 0x263A,
		  0xDECD, 0xCF44, 0xFDDF, 0xEC56, 0x98E9, 0x8960, 0xBBFB, 0xAA72,
		  0x6306, 0x728F, 0x4014, 0x519D, 0x2522, 0x34AB, 0x0630, 0x17B9,
		  0xEF4E, 0xFEC7, 0xCC5C, 0xDDD5, 0xA96A, 0xB8E3, 0x8A78, 0x9BF1,
		  0x7387, 0x620E, 0x5095, 0x411C, 0x35A3, 0x242A, 0x16B1, 0x0738,
		  0xFFCF, 0xEE46, 0xDCDD, 0xCD54, 0xB9EB, 0xA862, 0x9AF9, 0x8B70,
		  0x8408, 0x9581, 0xA71A, 0xB693, 0xC22C, 0xD3A5, 0xE13E, 0xF0B7,
		  0x0840, 0x19C9, 0x2B52, 0x3ADB, 0x4E64, 0x5FED, 0x6D76, 0x7CFF,
		  0x9489, 0x8500, 0xB79B, 0xA612, 0xD2AD, 0xC324, 0xF1BF, 0xE036,
		  0x18C1, 0x0948, 0x3BD3, 0x2A5A, 0x5EE5, 0x4F6C, 0x7DF7, 0x6C7E,
		  0xA50A, 0xB483, 0x8618, 0x9791, 0xE32E, 0xF2A7, 0xC03C, 0xD1B5,
		  0x2942, 0x38CB, 0x0A50, 0x1BD9, 0x6F66, 0x7EEF, 0x4C74, 0x5DFD,
		  0xB58B, 0xA402, 0x9699, 0x8710, 0xF3AF, 0xE226, 0xD0BD, 0xC134,
		  0x39C3, 0x284A, 0x1AD1, 0x0B58, 0x7FE7, 0x6E6E, 0x5CF5, 0x4D7C,
		  0xC60C, 0xD785, 0xE51E, 0xF497, 0x8028, 0x91A1, 0xA33A, 0xB2B3,
		  0x4A44, 0x5BCD, 0x6956, 0x78DF, 0x0C60, 0x1DE9, 0x2F72, 0x3EFB,
		  0xD68D, 0xC704, 0xF59F, 0xE416, 0x90A9, 0x8120, 0xB3BB, 0xA232,
		  0x5AC5, 0x4B4C, 0x79D7, 0x685E, 0x1CE1, 0x0D68, 0x3FF3, 0x2E7A,
		  0xE70E, 0xF687, 0xC41C, 0xD595, 0xA12A, 0xB0A3, 0x8238, 0x93B1,
		  0x6B46, 0x7ACF, 0x4854, 0x59DD, 0x2D62, 0x3CEB, 0x0E70, 0x1FF9,
		  0xF78F, 0xE606, 0xD49D, 0xC514, 0xB1AB, 0xA022, 0x92B9, 0x8330,
		  0x7BC7, 0x6A4E, 0x58D5, 0x495C, 0x3DE3, 0x2C6A, 0x1EF1, 0x0F78
		};

		public static ushort ComputeChecksum(params byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException();
			ushort crc = 0;
			for (int i = 0; i < buffer.Length; ++i)
			{
				crc = (ushort)((crc >> 8) ^ table[(crc ^ buffer[i]) & 0xff]);
			}
			return crc;
		}

		public static byte[] ComputeChecksumBytes(params byte[] buffer)
		{
			return BitConverter.GetBytes(ComputeChecksum(buffer));
		}
	}

	#endregion

	/// <summary>
	/// Allows to listen & send messages via IP multicast.
	/// </summary>
	/// <remarks>
	/// Listens & sends multicast messages on all interfaces. All public methods are thread-safe.
	/// </remarks>
	public class MulticastMessageBroker
	{
		#region Constants

		private const int cGuidSize = 16;

		#endregion

		#region Auxiliary

		private class Session
		{
			public Session(UdpClient udpClient)
			{
				UdpClient = udpClient;
				CurrenMessage = new Message();
			}

			public UdpClient UdpClient
			{ get; private set; }

			/// <summary>
			/// lock to ensure that whole chunk of data is processed in receive method before going onto next chunk
			/// </summary>
			public Object CurrenMessageLock = new Object();
			public Message CurrenMessage
			{ get; private set; }
		}

		private class Message
		{
			public byte[] HeaderBytes;
			public int ReadHeaderBytes;
			public byte[] MessageBytes;
			public int ReadMessageBytes;

			public Guid UniqueMessageId;
			public Guid SenderId;
			public Guid SubscriptionId;
			public int MessageLenght;

			public Message()
			{
				HeaderBytes = new byte[3 * cGuidSize + 4 + 2];  // 3 x guid + message length + checksum
			}

			// returns flag if successful
			public bool ParseHeader()
			{
				byte[] guidBytes = new byte[cGuidSize];

				Array.Copy(HeaderBytes, 0, guidBytes, 0, cGuidSize);
				SenderId = new Guid(guidBytes);

				Array.Copy(HeaderBytes, cGuidSize, guidBytes, 0, cGuidSize);
				UniqueMessageId = new Guid(guidBytes);

				Array.Copy(HeaderBytes, 2 * cGuidSize, guidBytes, 0, cGuidSize);
				SubscriptionId = new Guid(guidBytes);

				byte[] messageLengthBytes = new byte[4];
				Array.Copy(HeaderBytes, 3 * cGuidSize, messageLengthBytes, 0, 4);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(messageLengthBytes);
				MessageLenght = BitConverter.ToInt32(messageLengthBytes, 0);

				// calculate checksum & compare
				byte[] headerWithoutChecksum = new byte[HeaderBytes.Length - 2];
				Array.Copy(HeaderBytes, 0, headerWithoutChecksum, 0, HeaderBytes.Length - 2);
				var calculatedChecksum = Crc16CcittKermit.ComputeChecksumBytes((headerWithoutChecksum));
				if (calculatedChecksum[0] == HeaderBytes[HeaderBytes.Length - 2] &&
					calculatedChecksum[1] == HeaderBytes[HeaderBytes.Length - 1])
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Fields

		private IPAddress mMulticastGroup;
		private int mMulticastPort;
		private int mMulticastTTL;
		private int mMaxSendByteCount;

		private Dictionary<Guid, List<Action<string>>> mMessageCallbacks = new Dictionary<Guid, List<Action<string>>>();
		private Object mMessageCallbacksLock = new Object();

		private Dictionary<Guid, Session> mOpenedSessions = new Dictionary<Guid, Session>();
		private Guid mMyself = Guid.NewGuid();
		private AsyncCallback mOnReceive;

		// duplicate message detection
		private int mProcessedMessageHistoryCount;
		private Object mProcessedMessageHistoryLock = new Object();
		private Queue<Guid> mProcessedMessageHistory = new Queue<Guid>();

		volatile bool mIsConnected;
		private Object mIsConnectedLock = new Object();

		#endregion

		#region Private Methods

		void JoinMulticastGroup()
		{
			foreach (IPAddress localAddress in
				Dns.GetHostAddresses(Dns.GetHostName()).Where(i => i.AddressFamily == AddressFamily.InterNetwork))
			{
				var udpClient = new UdpClient(AddressFamily.InterNetwork);
				udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				udpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, mMulticastTTL);
				udpClient.Client.Bind(new IPEndPoint(localAddress, mMulticastPort));
				udpClient.JoinMulticastGroup(mMulticastGroup, localAddress);

				var clientId = Guid.NewGuid();
				mOpenedSessions.Add(clientId, new Session(udpClient));
				udpClient.BeginReceive(mOnReceive,
									   new object[]
									   {		
							  			   clientId
									   });
			}
		}

		void FinalizeMessage(Message message)
		{
			// find appropriate callback & fire
			if (message.SenderId != mMyself)
			{
				// check if message with same uniqueId is not already being processed (packet duplication)
				bool processMessage = true;
				if (mProcessedMessageHistoryCount > 0)
				{
					lock (mProcessedMessageHistoryLock)
					{
						if (mProcessedMessageHistory.Contains(message.UniqueMessageId))
						{
							processMessage = false;
						}
						else
						{							
							mProcessedMessageHistory.Enqueue(message.UniqueMessageId);

							// keep configured size of history
							while (mProcessedMessageHistory.Count > mProcessedMessageHistoryCount) mProcessedMessageHistory.Dequeue();
						}
					}
				}

				if (processMessage)
				{
					// get required message data out of receive buffer (as it may be used to process another incoming message)
					var uniqueMessageId = message.UniqueMessageId;
					var subscriptionId = message.SubscriptionId;
					var messageText = System.Text.Encoding.UTF8.GetString(message.MessageBytes);

					// message callback
					Action<string>[] callbacks = null;
					lock (mMessageCallbacksLock)
					{
						if (mMessageCallbacks.ContainsKey(subscriptionId))
						{
							callbacks = mMessageCallbacks[subscriptionId].ToArray();
						}
					}
					if (callbacks != null)
					{
						foreach (var callback in callbacks)
						{
							var tmp = callback;
							Task.Factory.StartNew(() => tmp(messageText));
						}
					}
				}
			}

			// reset to beginning
			message.ReadHeaderBytes = 0;
			message.ReadMessageBytes = 0;
		}

		// returns -1 if processed all data, otherwise offset where message ended
		int ReadMessage(Message message, int bufferOffset, byte[] receivedBytes)
		{
			int messageEndOffset = -1;

			if (message.ReadMessageBytes == 0)
			{
				message.MessageBytes = new byte[message.MessageLenght];
			}

			var remainingMessageBytesToRead = message.MessageLenght - message.ReadMessageBytes;
			var unprocessedByteCount = receivedBytes.Length - bufferOffset;
			var messageBytesRead = unprocessedByteCount > remainingMessageBytesToRead ? remainingMessageBytesToRead : unprocessedByteCount;
			Array.Copy(receivedBytes, bufferOffset, message.MessageBytes, message.ReadMessageBytes, messageBytesRead);
			message.ReadMessageBytes += messageBytesRead;

			if (messageBytesRead == remainingMessageBytesToRead)
			{
				// finalize
				FinalizeMessage(message);

				// are there some following data?
				if (receivedBytes.Length > bufferOffset + messageBytesRead)
				{
					messageEndOffset = bufferOffset + messageBytesRead;
				}
			}

			return messageEndOffset;
		}

		void OnReceiveMessage(IAsyncResult result)
		{
			if (mIsConnected)
			{
				IPEndPoint ep = null;
				var args = (object[])result.AsyncState;
				var clientId = (Guid)args[0];
				var session = mOpenedSessions[clientId];

				try
				{
					lock (session.CurrenMessageLock)
					{
						byte[] receivedBytes = session.UdpClient.EndReceive(result, ref ep);

						// process all messages inside stream
						int processedOffset = 0;
						while (processedOffset >= 0)
						{
							if (session.CurrenMessage.ReadHeaderBytes < session.CurrenMessage.HeaderBytes.Length)
							{
								// reading header
								var remainingHeaderBytesToRead = session.CurrenMessage.HeaderBytes.Length - session.CurrenMessage.ReadHeaderBytes;
								var unprocessedByteCount = receivedBytes.Length - processedOffset;
								var headerBytesRead = unprocessedByteCount > remainingHeaderBytesToRead ? remainingHeaderBytesToRead : unprocessedByteCount;
								Array.Copy(receivedBytes, processedOffset, session.CurrenMessage.HeaderBytes, session.CurrenMessage.ReadHeaderBytes, headerBytesRead);
								session.CurrenMessage.ReadHeaderBytes += headerBytesRead;

								// header complete?
								if (headerBytesRead == remainingHeaderBytesToRead)
								{
									if (!session.CurrenMessage.ParseHeader())
									{
										// unable to parse header, reset processing pipeline & exit
										session.CurrenMessage.ReadHeaderBytes = 0;
										session.CurrenMessage.ReadMessageBytes = 0;
										processedOffset = -1;
										break;
									}

									processedOffset = ReadMessage(session.CurrenMessage, processedOffset + headerBytesRead, receivedBytes);
								}
								else
								{
									// processed all data in message
									processedOffset = -1;
								}
							}
							else
							{
								// reading message
								processedOffset = ReadMessage(session.CurrenMessage, 0, receivedBytes);
							}
						}
					}
				}
				catch
				{
					// if something happen during processing message, reset the processing state and continue
					// may have not been our message, or it could be corrupted
					session.CurrenMessage.ReadHeaderBytes = 0;
					session.CurrenMessage.ReadMessageBytes = 0;
				}

				// continue listening
				lock (mIsConnectedLock)
				{
				    if (mIsConnected)
				    {
						session.UdpClient.BeginReceive(mOnReceive, args);
					}
				}
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Creates new instance of message broker.
		/// </summary>
		/// <param name="multicastGroup">Group address.</param>
		/// <param name="port">Group port.</param>
		/// <param name="multicastTTL">TTL for sent messages.</param>
		/// <param name="processedMessageHistoryCount">Length of history list used for duplicate message detection.</param>
		/// <param name="maxSendByteCount">Max number of bytes sent in one call to UdpClient.Send.</param>
		public MulticastMessageBroker(
			IPAddress multicastGroup,
			int port,
			int multicastTTL = 255,
			int processedMessageHistoryCount = 30,
			int maxSendByteCount = -1)
		{
			mMulticastGroup = multicastGroup;
			mMulticastPort = port;
			mMulticastTTL = multicastTTL;
			mProcessedMessageHistoryCount = processedMessageHistoryCount;
			mMaxSendByteCount = maxSendByteCount;

			mOnReceive = new AsyncCallback(OnReceiveMessage);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Whether broker is connected to multicast group. If disconnected it cannot receive messages.
		/// </summary>
		public bool IsConnected
		{
			get
			{
				return mIsConnected;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Connects to multicast group on all interfaces. |Enables to receive multicast messages.
		/// </summary>
		public void Connect()
		{
			if (!mIsConnected)
			{
				lock (mIsConnectedLock)
				{
					if (!mIsConnected)
					{
						JoinMulticastGroup();
						mIsConnected = true;
					}
				}
			}
		}

		/// <summary>
		/// Disconnects from multicast group and releases all associated resources.
		/// </summary>
		public void Disconnect()
		{
			if (mIsConnected)
			{
				lock (mIsConnectedLock)
				{
					if (mIsConnected)
					{
						mIsConnected = false; // first set to false, to avoid accessing disposed udp client in receive method

						foreach (var session in mOpenedSessions.Values)
						{
							if (session.UdpClient != null)
							{
								session.UdpClient.Close();
							}
						}
						mOpenedSessions.Clear();
					}
				}
			}
		}

		/// <summary>
		/// Subscribes to receive specific messages.
		/// </summary>
		public void Subscribe(Guid subscriptionId, Action<string> callback)
		{
			if (callback != null)
			{
				lock (mMessageCallbacksLock)
				{
					if (mMessageCallbacks.ContainsKey(subscriptionId))
					{
						mMessageCallbacks[subscriptionId].Add(callback);
					}
					else
					{
						mMessageCallbacks.Add(subscriptionId, new List<Action<string>> { callback });
					}
				}
			}
		}

		/// <summary>
		/// Unsubscribe from message events.
		/// </summary>
		public void Unsubscribe(Guid subscriptionId, Action<string> callback)
		{
			lock (mMessageCallbacksLock)
			{
				if (mMessageCallbacks.ContainsKey(subscriptionId))
				{
					if (mMessageCallbacks[subscriptionId].Contains(callback))
					{
						mMessageCallbacks[subscriptionId].Remove(callback);
					}

					if (mMessageCallbacks[subscriptionId].Count == 0)
					{
						mMessageCallbacks.Remove(subscriptionId);
					}
				}
			}
		}

		/// <summary>
		/// Clears all configured subscribers.
		/// </summary>
		public void ClearAllSubscribers()
		{
			lock (mMessageCallbacksLock)
			{
				mMessageCallbacks.Clear();
			}
		}

		/// <summary>
		/// Sends message to all members subscribed to multicast group.
		/// </summary>
		/// <remarks>
		/// Message format: { Guid:myself(sender) + Guid:uniqueMessageId + Guid:subscriptionId + int:messageLenght + byte[2]:CRC16 + string:message}
		/// </remarks>
		public void SendMessage(Guid subscriptionId, string message)
		{
			if (mIsConnected)
			{
				// create message
				List<byte> buffer = new List<byte>();
				buffer.AddRange(mMyself.ToByteArray());
				buffer.AddRange(Guid.NewGuid().ToByteArray());
				buffer.AddRange(subscriptionId.ToByteArray());

				var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
				var messageLenghtBytes = BitConverter.GetBytes(messageBytes.Length);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(messageLenghtBytes);

				buffer.AddRange(messageLenghtBytes);

				// header checksum
				buffer.AddRange(Crc16CcittKermit.ComputeChecksumBytes(buffer.ToArray()));
				buffer.AddRange(messageBytes);

				lock (mIsConnectedLock)
				{
					if (mIsConnected)
					{
						// send multicast on all interfaces
						foreach (var session in mOpenedSessions)
						{
							var ipep = new IPEndPoint(mMulticastGroup, mMulticastPort);
							int sentBytes = 0;
							while (sentBytes < buffer.Count)
							{
								var remainingNoOfBytesToSend = buffer.Count - sentBytes;
								var bytesToSendCount = remainingNoOfBytesToSend;

								// is max output byte count defined?
								if (mMaxSendByteCount > 0)
								{
									bytesToSendCount = remainingNoOfBytesToSend > mMaxSendByteCount ? mMaxSendByteCount : remainingNoOfBytesToSend;
								}

								var bytesToSend = buffer.Skip(sentBytes).Take(bytesToSendCount).ToArray();
								sentBytes += session.Value.UdpClient.Send(bytesToSend, bytesToSend.Length, ipep);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
