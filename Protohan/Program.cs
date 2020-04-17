using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace Protohan
{
    class Program
    {
        private static List<string> options = new List<string> { "-register", "-unregister" };

        static void Main(string[] args)
        {
            Console.WriteLine("ProtoHan started.");
            Console.WriteLine(" ");

            if (args.Length > 0)
            {
                if (options.Contains(args[0]))
                {
                    if (args[0] == "-register")
                        Create(args[1], args[2]);

                    if (args[0] == "-unregister")
                        Delete(args[1]);
                }
                else
                    LoadProtocol(args[0]);
            }
            else
                ShowOptions();
        }

        private static void LoadProtocol(string uri)
        {
            uri = HttpUtility.UrlDecode(uri);
            Console.WriteLine($"Loading protocol for URI {uri}.");

            try
            {
                var segments = uri.Split(":");

                var protocol = segments[0];
                var path = uri.Replace(protocol + @":\\", string.Empty);
                var appPath = GetExecutablePath(protocol);

                if (appPath == null || string.IsNullOrEmpty(appPath)){
                    WriteError($"No application found for this protocol {protocol}");
                    return;
                }

                RunApplication(appPath, path);

            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private static void ShowOptions()
        {
            Console.WriteLine("Usage: ProtoHan [options]");
            Console.WriteLine(@"Example: ProtoHan -register mymedia c:\path_to\application_to_run.exe");
            Console.WriteLine("");
            Console.WriteLine("Usage: ProtoHan [protocol-uri]");
            Console.WriteLine(@"Example: ProtoHan mymedia:\\some_uri_here");

            Console.WriteLine("");
            Console.WriteLine("Options:");
            PrintRow("-register <protocol> <path_to_executable>", "Register a new protocol handled by an executable");
            PrintRow("-unregister <protocol> <path_to_executable>", "Removes a register a new protocol handled by an executable");
        }

        private static void PrintRow(params string[] columns)
        {
            int width = (100 - columns.Length) / columns.Length;
            string row = "";

            foreach (string column in columns)
                row += column.PadRight(width);

            Console.WriteLine(row);
        }

        private static void RunApplication(string appPath, string path)
        {
            var commandToRun = "\"" + appPath + "\" " + "\"" + path + "\"";

            Console.WriteLine("Running application with command: ");
            Console.WriteLine($"\t{commandToRun}");

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();

            p.StandardInput.WriteLine(commandToRun);
            p.StandardInput.Flush();
            p.StandardInput.Close();
        }

        private static void Create(string protocol, string pathToExecutable)
        {
            Console.WriteLine($"Registering protocol {protocol} with application {pathToExecutable}.");

            if (string.IsNullOrEmpty(protocol)){
                WriteError("Protocol cannot be empty!");
                return;
            }

            if (string.IsNullOrEmpty(pathToExecutable) || !File.Exists(pathToExecutable)){
                WriteError("Executable not found or incorrect.");
                return;
            }

            if (Exists(protocol))
            {
                Console.WriteLine("Protocol found; deleting it first.");
                Delete(protocol);
            }

            AddProtocolToRegistry(protocol, pathToExecutable);

            Console.WriteLine($"Protocol {protocol} registered in the system.");
        }

        private static void Delete(string protocol)
        {
            if (!Exists(protocol))
            {
                WriteError($"Protocol {protocol} not found on system.");
                return;
            }

            var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\");

            key.DeleteSubKey(protocol + "\\DefaultIcon");
            key.DeleteSubKey(protocol + "\\Shell\\open\\command");
            key.DeleteSubKey(protocol + "\\Shell\\open");
            key.DeleteSubKey(protocol + "\\Shell");
            key.DeleteSubKey(protocol);

            Console.WriteLine($"Protocol {protocol} deleted from system.");
        }

        private static bool Exists(string protocol)
        {
            return Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Classes\\" + protocol, true) != null;
        }

        private static string GetExecutablePath(string procotol)
        {
            var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Classes\\" + procotol, true);
            var path = regKey.GetValue("Executable");

            return path.ToString();
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void WriteError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);

            if (ex.InnerException != null)
                WriteError(ex.InnerException);

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void AddProtocolToRegistry(string protocol, string pathToExecutable)
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
