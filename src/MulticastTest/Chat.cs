using System;
using System.Text;
using System.Net;

namespace MulticastTest
{
	class Chat
	{
		static Guid subscriptionId = new Guid("{BBDAC5DB-9E57-45EB-AD2E-54B2D6CA6E38}");

		public static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.Unicode;
			Console.InputEncoding = System.Text.Encoding.Unicode;

			var multicastBroker = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000);
			Console.CancelKeyPress += (s, ev) =>
			{
				// clean-up code
				multicastBroker.ClearAllSubscribers();
				multicastBroker.Disconnect();
			};
			
			multicastBroker.Connect();

			multicastBroker.Subscribe(subscriptionId,
				(m) =>
				{
					Console.WriteLine("Message received: {0}", m);
				});

			Console.WriteLine("Type the message then press Enter to send (CTRL+C to exit):");
			while (true)
			{
				var message = Console.ReadLine();
				if (message != null)
				{
					multicastBroker.SendMessage(subscriptionId, message);
				}
			}
		}
	}
}
