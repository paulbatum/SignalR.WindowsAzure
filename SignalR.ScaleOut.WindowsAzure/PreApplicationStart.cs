using System.Web;
using SignalR.Infrastructure;
using PreApplicationStart = SignalR.ScaleOut.WindowsAzure.PreApplicationStart;

[assembly: PreApplicationStartMethod(typeof(PreApplicationStart), "Start")]

namespace SignalR.ScaleOut.WindowsAzure
{
    public static class PreApplicationStart
    {

        public static void Start()
        {
            // Ensure SignalR.ScaleOut has started
            SignalR.ScaleOut.PreApplicationStart.Start();

            // Wire our own stuff in
            DependencyResolver.Register(typeof(IPeerUrlSource), () => new WindowsAzurePeerUrlSource());
            DependencyResolver.Register(typeof(IMessageIdGenerator), () => new WindowsAzureMasterPeerMessageIdGenerator());

            DependencyResolver.Register(typeof(ISignalBus), () => new PeerToPeerSignalBusMessageStore(DependencyResolver.Resolve<IMessageIdGenerator>()));
            DependencyResolver.Register(typeof(IMessageStore), () => new PeerToPeerSignalBusMessageStore(DependencyResolver.Resolve<IMessageIdGenerator>()));
        }

    }
}