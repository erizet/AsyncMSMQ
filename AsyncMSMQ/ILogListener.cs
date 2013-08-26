using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncMSMQ
{
    public interface ILogListener
    {
        void Error(string errorMessage);
    }
}
