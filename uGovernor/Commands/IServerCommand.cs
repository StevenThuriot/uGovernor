using uGovernor.Domain;

namespace uGovernor.Commands
{
    interface IServerCommand
    {
        void Run(TorrentServer server);
    }
}