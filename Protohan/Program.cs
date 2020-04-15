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
        private static ILogger logger { get; set; }

        static void Main(string[] args)
        {
            Initialize();

            logger.Write("ProtoHan started.");
            logger.Write(" ");
            if (args.Length > 0)
            {
                if (options.Contains(args[0]))
                {
                    if (args[0] == "-register")
                        RegisterProtocol(args[1], args[2]);

                    if (args[0] == "-unregister")
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
            logger.Write($"Loading protocol for URI {uri}.");

            try
            {
                var segments = uri.Split(":");

                var protocol = segments[0];
                var path = uri.Replace(protocol + @":\\", string.Empty);
                var appPath = registeryHelper.GetExecutablePath(protocol);

                if (appPath == null || string.IsNullOrEmpty(appPath))
                    logger.Write($"No application found for this protocol {protocol}");

                RunApplication(appPath, path);

            }
            catch (Exception ex)
            {
                logger.Write(ex);
            }
        }

        private static void RegisterProtocol(string protocol, string executable)
        {
            logger.Write($"Registering protocol {protocol} with application {executable}.");

            if (registeryHelper.Exists(protocol))
            {
                logger.Write("Protocol found; deleting it first.");
                registeryHelper.Delete(protocol);
            }

           registeryHelper.Create(protocol, executable);
        }

        private static void ShowOptions()
        {
            Console.WriteLine("");
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

            logger.Write("Running application with command: ");
            logger.Write($"\t{commandToRun}");

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
                .AddSingleton<ILogger, ConsoleLogger>() // Will log to the console window.
                                                        //.AddSingleton<ILogger, FileLogger>() // Will log to a debug file, located in the AppData folder.
                .BuildServiceProvider();

            registeryHelper = serviceProvider.GetService<IRegistryHelper>();
            logger = serviceProvider.GetService<ILogger>();
        }
    }
}
