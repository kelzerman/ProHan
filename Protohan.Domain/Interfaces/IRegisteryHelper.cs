using Protohan.Domain.Models;

namespace Protohan.Domain.Interfaces
{
    public interface IRegistryHelper
    {
        bool Exists(string key);
        ProtocolResult Create(string protocol, string pathToExecutable);
        void Delete(string protocol);
        string GetExecutablePath(string procotol);
    }
}
