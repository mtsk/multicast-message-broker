using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MulticastTest.Test
{
	class OneByteTransmitter
	{
		static Guid subscriptionId = new Guid("{BBDAC5DB-9E57-45EB-AD2E-54B2D6CA6E38}");

		public static void Main(string[] args)
		{
			var receiver = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000, processedMessageHistoryCount: 30);
			var oneByteTransmitter = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000, maxSendByteCount: 1);
			Console.CancelKeyPress += (s, ev) =>
			{
				// clean-up code
				receiver.ClearAllSubscribers();
				receiver.Disconnect();

				oneByteTransmitter.Disconnect();
			};

			receiver.Connect();
			oneByteTransmitter.Connect();

			receiver.Subscribe(subscriptionId,
				(m) =>
				{
					Console.WriteLine("Message received: {0}", m);
				});

			Console.WriteLine("Any key to send test message... (press CTRL+C to exit)");
			while (true)
			{
				Console.ReadKey(true);
				Console.WriteLine("\nSending message...");
				oneByteTransmitter.SendMessage(subscriptionId, "Hello!");
			}
		}
	}
}
