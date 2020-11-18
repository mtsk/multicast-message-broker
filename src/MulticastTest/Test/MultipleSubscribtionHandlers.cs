using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;

namespace MulticastTest.Test
{
	class MultipleSubscribtionHandlers
	{
		static Guid subscriptionId = new Guid("{BBDAC5DB-9E57-45EB-AD2E-54B2D6CA6E38}");

		public static void Main(string[] args)
		{
			var multicastBroker1 = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000);
			var transmitter = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000);
			Console.CancelKeyPress += (s, ev) =>
			{
				// clean-up code
				multicastBroker1.Unsubscribe(subscriptionId, Handler1Callback);
				multicastBroker1.Unsubscribe(subscriptionId, Handler2Callback);
				multicastBroker1.Disconnect();

				transmitter.Disconnect();
			};

			multicastBroker1.Connect();
			transmitter.Connect();

			multicastBroker1.Subscribe(subscriptionId, Handler1Callback);
			multicastBroker1.Subscribe(subscriptionId, Handler2Callback);
			transmitter.Subscribe(subscriptionId, TransmitterCallback);

			Console.WriteLine("Any key to send test message... (press CTRL+C to exit)");
			while (true)
			{
				Console.ReadKey(true);				
				Console.WriteLine("\nSending message...");
				transmitter.SendMessage(subscriptionId, "Hello!");
			}
		}

		public static void Handler1Callback(string message)
		{
			Thread.Sleep(1000);
			Console.WriteLine("MessageBroker1 -> Handler #1: {0}", message);
		}

		public static void Handler2Callback(string message)
		{
			Console.WriteLine("MessageBroker1 -> Handler #2: {0}", message);
		}

		public static void TransmitterCallback(string message)
		{
			Console.WriteLine("This won't get called!");
		}
	}
}
