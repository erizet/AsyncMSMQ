using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncMSMQ
{
    public interface IMessageService
    {
        /// <summary>
        /// Asynchronous receive messages from this service
        /// </summary>
        /// <returns>A MSMQ messsage</returns>
        Task<IMessage> ReceiveAsync(CancellationToken ct);

        /// <summary>
        /// Asynchronous receive messages from specified MessageQueue 
        /// </summary>
        /// <typeparam name="T">Body stream is of type T</typeparam>
        /// <returns>Object of type T</returns>
        Task<T> ReceiveAsync<T>(CancellationToken ct) where T : new();

        /// <summary>
        /// Just Pings specified MessageQueue and returns true if any Message exists
        /// </summary>
        /// <returns>True or false</returns>
        Task<bool> HasMessagesInQueueAsync();

        /// <summary>
        /// Send Message to designated Queue
        /// </summary>
        /// <param name="messageBody">Message body</param>
        Task<bool> SendAsync(string messageBody);

        /// <summary>
        /// Send Message to designated Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject">Message object body</param>
        Task<bool> SendAsync<T>(T messageObject);

        /// <summary>
        /// Send Message to designated Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject">Message object body</param>
        /// <param name="ttl">Message Time to Live</param>
        Task<bool> SendAsync<T>(T messageObject, TimeSpan ttl);

        /// <summary>
        /// Remove client MessageQueue when app exits
        /// </summary>
        Task<bool> DeleteQueueAsync();
    }
}
