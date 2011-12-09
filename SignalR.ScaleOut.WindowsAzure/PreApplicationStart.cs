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

            var urlSource = new WindowsAzurePeerUrlSource();
            var idGenerator = new WindowsAzureMasterPeerMessageIdGenerator();
            var busAndStore = new PeerToPeerSignalBusMessageStore(idGenerator);

            // Wire our own stuff in
            DependencyResolver.Register(typeof(IPeerUrlSource), () => urlSource);
            DependencyResolver.Register(typeof(IMessageIdGenerator), () => idGenerator);

            DependencyResolver.Register(typeof(ISignalBus), () => busAndStore);
            DependencyResolver.Register(typeof(IMessageStore), () => busAndStore);
        }

    }
}