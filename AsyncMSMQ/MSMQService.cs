using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AsyncMSMQ
{
    public class MSMQService : IMessageService
    {
        private ILogListener _log;
        public string MessageQueuePath { get; private set; }

        public MSMQService(string queuePath, bool createIfNotExists, ILogListener logListener)
        {
            if (logListener == null) throw new ArgumentNullException("LogListener cannot be null");

            _log = logListener;
            MessageQueuePath = queuePath;
            if (createIfNotExists) CreateQueueIfNotExists();
        }


        /// <summary>
        /// Asynchronous receive messages from specified MessageQueue
        /// </summary>
        /// <typeparam name="T">Body stream is of type T</typeparam>
        /// <returns>Object of type T</returns>
        public Task<IMessage> ReceiveAsync(CancellationToken ct)
        {
            return Task<IMessage>.Factory.StartNew(() =>
            {
                MSMQMessage msmqMessage = null;
                using (var ReceieveQueue = new MessageQueue(MessageQueuePath))
                {
                    msmqMessage = new MSMQMessage(Receive(ReceieveQueue, ct));
                }

                return msmqMessage;
            }, ct);
        }



        /// <summary>
        /// Asynchronous receive messages from specified MessageQueue
        /// </summary>
        /// <typeparam name="T">Body stream is of type T</typeparam>
        /// <returns>Object of type T</returns>
        public Task<T> ReceiveAsync<T>(CancellationToken ct) where T : new()
        {
            return Task<T>.Factory.StartNew(() =>
            {
                T result = new T();

                using (var receiveQueue = new MessageQueue(MessageQueuePath))
                {
                    receiveQueue.Formatter = new XmlMessageFormatter(new Type[] { typeof(T) });
                    Message message = Receive(receiveQueue, ct);
                    if (message != null && message.Body is T)
                    {
                        result = (T)message.Body;
                    }
                }

                return result;
            }, ct);
        }


        private Message Receive(MessageQueue receiveQueue, CancellationToken ct)
        {
            IAsyncResult ar = receiveQueue.BeginReceive();
            int waitForEvent = WaitHandle.WaitAny(new WaitHandle[] { ar.AsyncWaitHandle, ct.WaitHandle });


            if (ct.IsCancellationRequested)
            {
                receiveQueue.Close();          // remember to close the queue
                ct.ThrowIfCancellationRequested();
            }

            // If not cancelled, BeginReceived must be finished
            Message message = receiveQueue.EndReceive(ar);

            return message;
        }


        /// <summary>
        /// Just Pings specified MessageQueue and returns true if any Message exists
        /// </summary>
        /// <returns>True or false</returns>
        public Task<bool> HasMessagesInQueueAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool hasMessageInQueue = false;
                try
                {
                    using (var ReceiveQueue = new MessageQueue(MessageQueuePath))
                    {
                        var message = ReceiveQueue.Receive();

                        if (message != null)
                        {
                            hasMessageInQueue = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }
                return hasMessageInQueue;
            });
        }


        /// <summary>
        /// Send Message to designated Queue
        /// </summary>
        /// <param name="messageBody">Message body</param>
        public Task<bool> SendAsync(string messageBody)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool ret = false;

                try
                {
                    using (var SendQueue = new MessageQueue(MessageQueuePath))
                    {
                        Message message = new Message();
                        message.Body = messageBody;
                        SendQueue.Send(message);
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }

                return ret;
            });
        }



        /// <summary>
        /// Send Message to designated Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject">Message object body</param>
        public Task<bool> SendAsync<T>(T messageObject)
        {
            return SendAsync<T>(messageObject, Message.InfiniteTimeout);
        }

        /// <summary>
        /// Send Message to designated Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageObject">Message object body</param>
        /// <param name="ttl">Message Time to Live</param>
        public Task<bool> SendAsync<T>(T messageObject, TimeSpan ttl)
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool ret = false;

                try
                {
                    using (var SendQueue = new MessageQueue(MessageQueuePath))
                    {
                        Message message = new Message();
                        message.TimeToBeReceived = ttl;
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        serializer.Serialize(message.BodyStream, messageObject);

                        SendQueue.Send(message);
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }

                return ret;
            });
        }


        /// <summary>
        /// Remove client MessageQueue when app exits
        /// </summary>
        public Task<bool> DeleteQueueAsync()
        {
            return Task<bool>.Factory.StartNew(() =>
            {
                bool ret = false;
                try
                {
                    if (MessageQueue.Exists(MessageQueuePath))
                    {
                        MessageQueue.Delete(MessageQueuePath);
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                }

                return ret;
            });
        }

        private void CreateQueueIfNotExists()
        {
            try
            {
                if (!MessageQueue.Exists(MessageQueuePath))
                {
                    MessageQueue.Create(MessageQueuePath);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("createIfNotExists cannot be used on private queues or multicast addresses.");
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

    }
}

