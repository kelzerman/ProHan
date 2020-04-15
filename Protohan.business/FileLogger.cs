using Protohan.Domain.Interfaces;
using System;
using System.IO;

namespace Protohan.Business
{
    public class FileLogger : ILogger
    {
        public void Error(string message)
        {
            throw new NotImplementedException();
        }

        public void Write(string message)
        {
            DoWrite(message);
        }

        public void Write(Exception exception)
        {
            DoWrite(exception.Message);

            if (exception.InnerException != null)
                Write(exception.InnerException);
        }

        private void DoWrite(string message)
        {
            var fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "ProtoHan", "debug.txt");

            if (File.Exists(fileName))
                using (StreamWriter sw = File.CreateText(fileName))
                    sw.WriteLine(message);
            else
                using (StreamWriter sw = File.AppendText(fileName))
                    sw.WriteLine(message);
        }
    }
}
