using System;

namespace Protohan.Domain.Interfaces
{
    public interface ILogger
    {
        void Write(string message);
        void Error(string message);
        void Write(Exception exception);
    }
}
