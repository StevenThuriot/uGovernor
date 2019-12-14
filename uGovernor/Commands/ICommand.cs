using uGovernor.Domain;

namespace uGovernor.Commands
{
    interface ICommand
    {
        void Run(Torrent torrent);
    }
}