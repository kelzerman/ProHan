using Microsoft.Win32;
using Protohan.Domain.Interfaces;
using System;
using System.IO;

namespace Protohan.Business
{
    public class RegistryHelper : IRegistryHelper
    {
        private readonly ILogger logger;

        public RegistryHelper(ILogger logger)
        {
            this.logger = logger;
        }

        public void Create(string protocol, string pathToExecutable)
        {
            if (string.IsNullOrEmpty(protocol))
                logger.Error("Protocol cannot be empty!");

            if (string.IsNullOrEmpty(pathToExecutable) || !File.Exists(pathToExecutable))
                logger.Error("Executable not found or incorrect.");

            RegisterProtocol(protocol, pathToExecutable);

            logger.Write($"Protocol {protocol} registered in the system.");
        }

        public void Delete(string protocol)
        {
            if (!Exists(protocol))
            {
                logger.Error($"Protocol {protocol} not found on system.");
                return;
            }

            var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\");

            key.DeleteSubKey(protocol + "\\DefaultIcon");
            key.DeleteSubKey(protocol + "\\Shell\\open\\command");
            key.DeleteSubKey(protocol + "\\Shell\\open");
            key.DeleteSubKey(protocol + "\\Shell");
            key.DeleteSubKey(protocol);

            logger.Write($"Protocol {protocol} deleted from system.");
        }

        public bool Exists(string protocol)
        {
            return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Classes\\" + protocol, true) != null;
        }

        public string GetExecutablePath(string procotol)
        {
            var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Classes\\" + procotol, true);
            var path = regKey.GetValue("Executable");

            return path.ToString();
        }

        private void RegisterProtocol(string protocol, string pathToExecutable)
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + protocol))
            {
                string applicationLocation = Path.Combine(Environment.CurrentDirectory, "ProtoHan.exe");

                key.SetValue("", "URL:" + protocol);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", applicationLocation + ",1");
                }

                key.SetValue("Executable", pathToExecutable);

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                }
            }
        }
    }
}
