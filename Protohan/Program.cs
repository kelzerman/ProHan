using Microsoft.Extensions.DependencyInjection;
using Protohan.Business;
using Protohan.Domain.Interfaces;
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

        private static IRegistryHelper registeryHelper { get; set; }

        static void Main(string[] args)
        {
            Initialize();

            if (args.Length > 0)
            {
                if (options.Contains(args[0]))
                {
                    if (args[0] == "-register")
                        HandleRegistrations(args[1], args[2]);

                    if (args[0] == "-unregister")
                        if (registeryHelper.Exists(args[1]))
                            registeryHelper.Delete(args[1]);
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

            try
            {
                var segments = uri.Split(":");

                var protocol = segments[0];
                var path = uri.Replace(protocol + @":\\", string.Empty);

                WriteLog("Protocol: " + protocol);
                WriteLog("Path: " + path);

                var appPath = registeryHelper.GetExecutablePath(protocol);
                RunApplication(appPath, path);

            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private static void HandleRegistrations(string protocol, string executable)
        {
            if (registeryHelper.Exists(protocol))
                registeryHelper.Delete(protocol);

            var result = registeryHelper.Create(protocol, executable);

            WriteLog(result.Message);
        }

        private static void ShowOptions()
        {
            Console.WriteLine("Usage: LONHandler [options]");
            Console.WriteLine(@"Example: LONHandler -register mymedia c:\path_to\application_to_run.exe");
            Console.WriteLine("");
            Console.WriteLine("Usage: LONHandler [protocol-uri]");
            Console.WriteLine(@"Example: LONHandler mymedia:\\some_uri_here");

            Console.WriteLine("");
            Console.WriteLine("Options:");
            PrintRow("-register <protocol> <path_to_executable>", "Register a new protocol handled by an executable");
            PrintRow("-unregister <protocol> <path_to_executable>", "Removes a register a new protocol handled by an executable");
        }

        private static void WriteLog(string message)
        {
            var debugFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "debug.txt");

            if (File.Exists(debugFile))
                using (StreamWriter sw = File.CreateText(debugFile))
                {
                    sw.WriteLine(message);
                }
            else
                using (StreamWriter sw = File.AppendText(debugFile))
                {
                    sw.WriteLine(message);
                }
        }

        private static void PrintRow(params string[] columns)
        {
            int width = (100 - columns.Length) / columns.Length;
            string row = "";

            foreach (string column in columns)
                row += column.PadRight(width);
        }

        private static void RunApplication(string appPath, string path)
        {
            var commandToRun = "\"" + appPath + "\" " + "\"" + path + "\"";

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

        private static void Initialize()
        {
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IRegistryHelper, RegistryHelper>()
                .BuildServiceProvider();

            registeryHelper = serviceProvider.GetService<IRegistryHelper>();
        }
    }
}
