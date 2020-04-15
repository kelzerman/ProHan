namespace Protohan.Domain.Interfaces
{
    public interface IRegistryHelper
    {
        bool Exists(string key);
        void Create(string protocol, string pathToExecutable);
        void Delete(string protocol);
        string GetExecutablePath(string procotol);
    }
}
