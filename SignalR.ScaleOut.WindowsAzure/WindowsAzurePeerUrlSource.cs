using System.Collections.Generic;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace SignalR.ScaleOut.WindowsAzure
{
    public class WindowsAzurePeerUrlSource
        : IPeerUrlSource
    {
        public IEnumerable<string> GetPeerUrls()
        {
            if (RoleEnvironment.IsAvailable)
            {
                foreach (var roleInstance in RoleEnvironment.CurrentRoleInstance.Role.Instances)
                {
                    foreach (var roleInstanceEndpoint in roleInstance.InstanceEndpoints)
                    {
                        if (roleInstanceEndpoint.Value.Protocol == "http")
                        {
                            yield return string.Format("http://{0}:{1}/", roleInstanceEndpoint.Value.IPEndpoint.Address,
                                              roleInstanceEndpoint.Value.IPEndpoint.Port);
                        }
                    }
                }
            }
        }
    }
}