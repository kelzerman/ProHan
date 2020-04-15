using Microsoft.Win32;
using Protohan.Domain.Interfaces;
using Protohan.Domain.Models;
using System;
using System.IO;

namespace Protohan.Business
{
    public class RegistryHelper : IRegistryHelper
    {
        public ProtocolResult Create(string protocol, string pathToExecutable)
        {
            if (string.IsNullOrEmpty(protocol))
            {
                return new ProtocolResult
                {
                    Message = "Protocol cannot be empty!",
                    Succes = false
                };
            }

            if (string.IsNullOrEmpty(pathToExecutable) || !File.Exists(pathToExecutable))
            {
                return new ProtocolResult
                {
                    Message = "Executable not found.",
                    Succes = false
                };
            }

            RegisterProtocol(protocol, pathToExecutable);

            return new ProtocolResult
            {
                Message = $"Protocol {protocol} registered in the system.",
                Succes = true
            };
        }

        public void Delete(string protocol)
        {
            if (!Exists(protocol))
                return;

            var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\");

            key.DeleteSubKey(protocol + "\\DefaultIcon");
            key.DeleteSubKey(protocol + "\\Shell\\open\\command");
            key.DeleteSubKey(protocol + "\\Shell\\open");
            key.DeleteSubKey(protocol + "\\Shell");
            key.DeleteSubKey(protocol);
        }

        public bool Exists(string key)
        {
            return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Classes\\" + key, true) != null;
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
                string applicationLocation = Path.Combine(Environment.CurrentDirectory, "LonHandler.exe");

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
