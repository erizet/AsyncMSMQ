using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AsyncMSMQ
{
    public class MSMQMessage : IMessage
    {
        private Message _msmqMessage = null;

        public MSMQMessage(Message msmqMessage)
        {
            _msmqMessage = msmqMessage;
        }

        public object OriginalMessage
        {
            get { return _msmqMessage; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (_msmqMessage != null)
            {
                _msmqMessage.Formatter = new System.Messaging.XmlMessageFormatter(new String[] { });

                StreamReader sr = new StreamReader(_msmqMessage.BodyStream);

                while (sr.Peek() >= 0)
                {
                    sb.AppendLine(sr.ReadLine());
                }
            }

            return sb.ToString();
        }
    }
}
