using AsyncMSMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        class Logger : ILogListener
        {
            public void Error(string errorMessage)
            {
                Console.WriteLine(errorMessage);
            }
        };

        static void Main(string[] args)
        {
            IMessageService service = new MSMQService(@".\Private$\clientTest", true, new Logger());

            CancellationTokenSource cancelSource = new CancellationTokenSource();
            CancellationToken token = cancelSource.Token;

            Console.WriteLine("Waiting for command...");

            ConsoleKeyInfo key;
            while ((key = Console.ReadKey()).Key != ConsoleKey.X)
            {
                switch (key.Key)
                {
                    case ConsoleKey.C:
                        cancelSource.Cancel();
                        Console.WriteLine("Source is cancelled");

                        // Resets cancellation
                        cancelSource = new CancellationTokenSource();
                        token = cancelSource.Token;
                        break;
                    case ConsoleKey.R:
                        StartReceivingFromQueue(service, token);
                        Console.WriteLine("Started receiving...");
                        break;
                    case ConsoleKey.S:
                        service.SendAsync("Eriks testmessage").ContinueWith(t =>
                        {
                            if (t.Result)
                                Console.WriteLine("Sändningen lyckades");
                            else
                                Console.WriteLine("Sändningen misslyckades");
                        });
                        break;
                    case ConsoleKey.P:
                        service.SendAsync<Person>(new Person() { Name = "Kalle" }).ContinueWith(t =>
                        {
                            if (t.Result)
                                Console.WriteLine("Kalle skickad");
                            else
                                Console.WriteLine("Sändningen av person misslyckades");
                        });
                        break;
                    case ConsoleKey.T:
                        service.SendAsync<Person>(new Person() { Name = "Kalle" }).ContinueWith(t =>
                        {
                            if (t.Result)
                                Console.WriteLine("Kalle skickad");
                            else
                                Console.WriteLine("Sändningen av person misslyckades");
                        });
                        break;
                    default:
                        break;
                }

            }
        }


        private static void StartReceivingFromQueue(IMessageService service, CancellationToken token)
        {
            var receiveTask = service.ReceiveAsync(token);

            receiveTask.ContinueWith(t =>
            {
                if (t.IsCanceled)
                    Console.WriteLine("Canceled");
                else if (t.IsFaulted)
                    Console.WriteLine("Faulted");
                else if (t.IsCompleted)
                {
                    IMessage msg = t.Result;
                    Console.WriteLine(msg.ToString());
                }
                else
                    Console.WriteLine("What happend?");
            }).ContinueWith(_ =>
            {
                if (!token.IsCancellationRequested)
                    StartReceivingFromQueue(service, token);
                else
                    Console.WriteLine("Listening stopped");
            });
        }

    }


    public class Person
    {
        public string Name { get; set; }
    }
}
