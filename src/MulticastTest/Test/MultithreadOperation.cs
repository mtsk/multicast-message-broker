using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MulticastTest.Test
{
	class MultithreadOperation
	{
		static Guid subscriptionId = new Guid("{BBDAC5DB-9E57-45EB-AD2E-54B2D6CA6E38}");

		public static void Main(string[] args)
		{
			var multicastBroker1 = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000);
			var multicastBroker2 = new MulticastMessageBroker(IPAddress.Parse("239.255.255.20"), 6000);

			multicastBroker1.Connect();
			multicastBroker2.Connect();

			Console.WriteLine("Any key to start, any key again to stop.");
			Console.ReadKey(true);

			int userCount = 20;
			var tasks = new List<Task>();
			var cancellationTokenSource = new CancellationTokenSource();

			Console.WriteLine("Creating {0} workers...",userCount * 2);

			// each user interacts with both message brokers
			for (int i = 0; i < userCount; i++)
			{
				var userId = i;
				tasks.Add(
					Task.Factory.StartNew((obj) => 
						{
							CancellationToken token = (CancellationToken)obj;
							while (!token.IsCancellationRequested)
							{
								Interact(multicastBroker1, "1", userId);
							}
						},
					cancellationTokenSource.Token, TaskCreationOptions.LongRunning)
				);

				tasks.Add(
					Task.Factory.StartNew((obj) =>
					{
						CancellationToken token = (CancellationToken)obj;
						while (!token.IsCancellationRequested)
						{
							Interact(multicastBroker2, "2", userId);
						}
					}, cancellationTokenSource.Token, TaskCreationOptions.LongRunning)
				);
			}

			Console.WriteLine("All workers created and running!");
			Console.ReadKey(true);

			// disconnect to stop message flow
			multicastBroker1.Disconnect();
			multicastBroker2.Disconnect();

			Console.WriteLine();
			Console.WriteLine("Stopping workers...");
			Console.WriteLine();

			// stop worker threads
			cancellationTokenSource.Cancel();
			Task.WaitAll(tasks.ToArray());

			Console.WriteLine("Workers stopped, any key to exit...");
			Console.ReadKey(true);
		}

		public static void Interact(MulticastMessageBroker broker, string brokerId, int userID)
		{
			var callback = new Action<string>(
				(m) =>
				{
					// for higher user count comment out as it slows down too much
					Console.WriteLine(@"Broker '{0}' of user '{1}': ""{2}""", brokerId, userID, m);
				});

			broker.Subscribe(subscriptionId,callback);

			Random rnd = new Random();
			Thread.Sleep(rnd.Next(1, 500));

			broker.SendMessage(subscriptionId, string.Format("User '{0}' sends greetings using broker '{1}'", userID, brokerId));

			broker.Unsubscribe(subscriptionId, callback);
		}
	}
}
