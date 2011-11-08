using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;
using SignalR.Web;

namespace SignalR.ScaleOut.WindowsAzure
{
    public class WindowsAzureMasterPeerMessageIdGenerator : IMessageIdGenerator
    {
        private readonly ConcurrentDictionary<string, long> _messageIds = new ConcurrentDictionary<string, long>();

        public Task<long> GenerateMessageId(string key)
        {
            // Slave
            if (!this.IsElectedMaster())
            {
                var electedMasterPeer = this.GetElectedMaster();

                var queryString = "?" + PeerToPeerHelper.RequestKeys.GenerateMessageId + "=" + HttpUtility.UrlEncode(key);
                var messageIdGeneratorUrl = electedMasterPeer + MessageReceiverHandler.HandlerName + queryString;
                return TaskAsyncHelper.Success<HttpWebResponse, long>(HttpHelper.GetAsync(messageIdGeneratorUrl, req => { }), t =>
                             {
                                 var messageId = long.Parse(t.Result.GetResponseHeader(PeerToPeerHelper.HeaderPrefix + PeerToPeerHelper.RequestKeys.GenerateMessageId));
                                 _messageIds[key] = messageId;
                                 return messageId;
                             });
            }

            // Master
            if (!_messageIds.ContainsKey(key))
            {
                _messageIds[key] = 1;
            }

            return TaskAsyncHelper.FromResult<long>(_messageIds[key]++);
        }

        public string GetElectedMaster()
        {
            if (RoleEnvironment.IsAvailable)
            {
                var roleInstance = RoleEnvironment.CurrentRoleInstance.Role.Instances.OrderBy(instance => instance.Id).First();
                foreach (var roleInstanceEndpoint in roleInstance.InstanceEndpoints)
                {
                    if (roleInstanceEndpoint.Value.Protocol == "http")
                    {
                        return string.Format("http://{0}:{1}/", roleInstanceEndpoint.Value.IPEndpoint.Address,
                                                   roleInstanceEndpoint.Value.IPEndpoint.Port);
                    }
                }

            }
            return null;
        }

        public bool IsElectedMaster()
        {
            if (RoleEnvironment.IsAvailable)
            {
                return RoleEnvironment.CurrentRoleInstance == RoleEnvironment.CurrentRoleInstance.Role.Instances.OrderBy(instance => instance.Id).First();
            }
            return true;
        }
    }
}