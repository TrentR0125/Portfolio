using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Logging.Server
{
    public class Server : BaseScript
    {
        #region Variables
        static readonly Dictionary<int, PendingRequest> _pendingRequests = new Dictionary<int, PendingRequest>();
        #endregion

        #region HTTP & API Event Handling
        [EventHandler("__cfx_internal:httpResponse")]
        private void OnHttpResponse(int token, int statusCode, string body, dynamic headers)
        {
            if (_pendingRequests.TryGetValue(token, out var req))
            {
                if (statusCode == 200)
                {
                    req.SetResult(body);
                }
                else
                {
                    req.SetException(new Exception($"Server returned status code: {statusCode}"));
                }

                _pendingRequests.Remove(token);
            }
        }

        public static async Task<string> DownloadString(string url)
        {
            var args = new Dictionary<string, object>()
            {
                { "url", url }
            };

            var argsJson = JsonConvert.SerializeObject(args);
            var id = API.PerformHttpRequestInternal(argsJson, argsJson.Length);
            var req = _pendingRequests[id] = new PendingRequest(id);

            return await req.Task;
        }

        public static async Task<string> UploadString(string url, string body)
        {
            var args = new Dictionary<string, object>()
            {
                { "url", url },
                { "method", "POST" },
                { "data", body },
                { "headers", new Dictionary<string, string> { 
                    { 
                        "Content-Type", 
                        "application/json" 
                    } 
                } }
            };

            var argsJson = JsonConvert.SerializeObject(args);
            var id = API.PerformHttpRequestInternal(argsJson, argsJson.Length);
            var req = _pendingRequests[id] = new PendingRequest(id);

            return await req.Task;
        }
        #endregion

        #region Event Handlers
        [EventHandler("DiscordLogging:Send")]
        private void OnLogRequest([FromSource] Player plyr, string caller, string msg)
        {
            string discordId;

            discordId = GetDiscordId(plyr);

            string GetDiscordId(Player player)
            {
                for (int i = 0; i < 6; i++)
                {
                    string[] id = API.GetPlayerIdentifier(plyr.Handle, i).Split(':');
                    if (id[0] == "discord")
                    {
                        return id[1];
                    }
                    else
                    {
                        continue;
                    }
                };
                return "Error finding Discord ID";
            }

            string playerInfo = $"**Player:** {plyr.Name} **|** <@{discordId}>\n";
            string action = $"**Action:** {msg}\n";
            string field = playerInfo + "" + action;

            SendWebhook(caller, $"<t:{Epoch()}:F>", DiscordEmbed("Logging System", field, "Server 1"));
        }
        #endregion

        #region Epoch Time
        private static int Epoch()
        {
            return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
        #endregion

        #region Pending Request
        private class PendingRequest : TaskCompletionSource<string>
        {
            public int Token;
            public PendingRequest(int token)
            {
                Token = token;
            }
        }
        #endregion

        #region Discord Methods
        public static dynamic DiscordEmbed(string title, string field, string footerText)
        {
            return new
            {
                title = title,
                description = field,
                color = "1127128",
                footer = new
                {
                    text = footerText
                },
                thumbnail = new
                {
                    url = ""
                }
            };
        }

        public static async Task SendWebhook(string webhook, string content, object embed)
        {
            string getWebhook = Config._config[webhook];
            try
            {
                await UploadString(getWebhook,
                    JsonConvert.SerializeObject(new
                    {
                        username = "Discord Logs",
                        content = content,
                        embeds = new[]
                        {
                            embed
                        }
                    }
                    ));
            }
            catch 
            {
                
            }
        }

        public static async Task SendWebhook(string webhook, string content, object[] embeds)
        {
            string getWebhook = Config._config[webhook];
            try
            {
                await UploadString(getWebhook,
                    JsonConvert.SerializeObject(new
                    {
                        content = content,
                        embeds = embeds
                    }
                    ));
            }
            catch 
            {
                
            }
        }
        #endregion
    }
}
