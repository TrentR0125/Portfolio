using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging.Server
{
    public class Config
    {
        public static IDictionary<string, string> _config = new Dictionary<string, string>()
        {
            // { "eventName", "discord-webhook-url" },
        };
    }
}
