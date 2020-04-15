using Protohan.Domain.Interfaces;
using System;

namespace Protohan.Business
{
    public class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(Exception exception)
        {
            Console.WriteLine(exception.Message);

            if (exception.InnerException != null)
                Write(exception.InnerException);
        }
    }
}
