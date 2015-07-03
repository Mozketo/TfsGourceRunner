using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using TfsGource.Configuration;
using TfsGource.Extensions;
using TfsGource.Utils;

namespace TfsConverter
{
	internal class Program
	{
		private static bool _ignoreAutoShutdown;

		private static void Main(string[] args)
		{
			var section = ConfigurationManager.GetSection(ProjectsSection.SectionName) as ProjectsSection;
            if (section == null)
            {
                Console.WriteLine("Unable to read ProjectsSection from application config file.");
                return;
            }

			Console.WriteLine(String.Format("Connect to server [{0}]:", Configuration.TfsUrl));
			string tfsUrl = Console.ReadLine();
			if (!String.IsNullOrEmpty(tfsUrl))
				Configuration.TfsUrl = tfsUrl;

			Console.WriteLine(String.Format("Username [{0}]:", Configuration.Username));
			string username = Console.ReadLine();
			if (!string.IsNullOrEmpty(username))
				Configuration.Username = username;

			Console.WriteLine("Password []:");
			string password = ConsoleUtils.ReadPassword();
			Console.WriteLine(String.Empty);
			Console.WriteLine(String.Empty);

		    bool fullscreen = ConsoleUtils.ReadBoolInput(String.Format("Run Fullscreen? [{0}] (T/F)", Configuration.Fullscreen.ToString()[0]), Configuration.Fullscreen);
			Console.WriteLine(String.Empty);
			Console.WriteLine(String.Empty);

			if (DateTime.Now.TimeOfDay > AppSettings.AutoShutdown.TimeOfDay)
			{
				// If the application is started AFTER the AutoShutdown time, the user obviously wants to run the application after the shutdown time.
				_ignoreAutoShutdown = true;
			}

			Console.Clear();

			int index = 0;
			while (true)
			{
				// If the current time has passed the Autoshutdown time then quit the application.
				// If though the user has explicitly expressed their interest to run the app after this time ignore the condition.
				if (!_ignoreAutoShutdown && DateTime.Now.TimeOfDay > AppSettings.AutoShutdown.TimeOfDay)
				{
					Console.WriteLine("Auto Shutdown is at {0}. Application shutting down now", AppSettings.AutoShutdown.TimeOfDay);
					Thread.Sleep(TimeSpan.FromMinutes(5));
					return;
				}

				// Get the next project from the configuration file, generate the log file for the project(s)
				// Then Run gource over it.
				ProjectElement project = section.Projects[index++];
				FileInfo logFile;
				bool success = GenerateLogFile(password, project.ProjectPaths, project.Days, out logFile);
				if (success)
				{
					// Sleep for a moment before running Gource incase the user is wanting to quit this app
					Thread.Sleep(250);
					RunGource(logFile.FullName, project.Title, fullscreen);
				}

				if (index > (section.Projects.Count - 1))
					index = 0;
			}
		}

		private static bool GenerateLogFile(string password, string tfsProjectPaths, int days, out FileInfo logFile)
		{
            string fileName = tfsProjectPaths.Replace(" ", "").GetValidFileName() + ".log";
			logFile = new FileInfo(Path.Combine(Path.GetTempPath(), fileName));
            if (File.Exists(logFile.FullName))
            {
                // If the log file exists, and it's less than x minutes old simply use it
				Console.WriteLine("Log was generated {0} minutes ago.", DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(logFile.FullName)).TotalMinutes);
				if (DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(logFile.FullName)).TotalMinutes < 30 && logFile.Length > 0)
                {
					return true;
                }
				File.Delete(logFile.FullName);
            }

		    using (var fileStream = new FileStream(logFile.FullName, FileMode.Create, FileAccess.ReadWrite))
			{
				DateTime to = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
				DateTime from = to.AddDays(-1 * Math.Abs(days));
			    new Converter(Configuration.TfsUrl, Configuration.Username, password)
			        .Process(tfsProjectPaths.Split(Convert.ToChar(",")), from, fileStream);
			}

			return (File.Exists(logFile.FullName) && logFile.Length > 0);
		}

        private static void RunGource(string logFile, string title, bool fullscreen)
        {
            var args = new StringBuilder();
            args.Append("\"" + logFile + "\"");
            args.Append(" " + "\"" + title + "\"");

            string batch = fullscreen ? AppSettings.FullscreenScriptFileName : AppSettings.WindowedScriptFileName;
            var directoryInfo = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            if (directoryInfo != null)
            {
                string workingDir = directoryInfo.FullName;

                Console.WriteLine("Running Gource with batch file: {0}", Path.Combine(workingDir, batch));

                var process = new Process
                                  {
                                      StartInfo = new ProcessStartInfo(Path.Combine(workingDir, batch))
                                                      {
                                                          Arguments = args.ToString(),
                                                          RedirectStandardOutput = true,
                                                          UseShellExecute = false,
                                                      }
                                  };
                try
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    Console.Write(output);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to run Gource from: {0}", Path.Combine(workingDir, batch));
                }
            }
        }
	}
}