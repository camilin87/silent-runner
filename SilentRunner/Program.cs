using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentRunner
{
    static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern System.IntPtr GetCommandLine();

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length < 1)
                {
                    throw new InvalidOperationException("Not enough parameters provided");
                }

                var allArgsRaw = Marshal.PtrToStringAuto(GetCommandLine());

                var lastProgramIndex = allArgsRaw.IndexOf(" ");

                if (allArgsRaw.StartsWith("\""))
                {
                    lastProgramIndex = allArgsRaw.IndexOf("\"", 1);
                }

                var argsRaw = allArgsRaw.Substring(lastProgramIndex + 1);

                EventLog.WriteEntry("System", $"SilentRunner; Action=Start; CommandWithArgs={allArgsRaw};");

                var exitCode = 0;

                while (true)
                {
                    exitCode = RunSubtask(argsRaw);

                    if (exitCode != 0)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(30000);
                }

                EventLog.WriteEntry("System", $"SilentRunner; Action=Complete; ExitCode={exitCode}");
                Environment.Exit(exitCode);
            }
            catch (Exception exc)
            {
                EventLog.WriteEntry("System", $"SilentRunner; Action=Error; Error={exc}", EventLogEntryType.Error);
                throw;
            }
        }

        private static int RunSubtask(string argsRaw)
        {
            var logsPath = Path.GetTempFileName();
            //                EventLog.WriteEntry("System", $"SilentRunner; TempLogsFile={logsPath}");

            var subTaskFilename = "cmd";
            var subTaskArgs = $"/c \"{argsRaw}\" > {logsPath} 2>&1";

            EventLog.WriteEntry("System", $"SilentRunner; Action=Spawn; Subtask={subTaskFilename}; Args={subTaskArgs}");

            var startInfo = new ProcessStartInfo(subTaskFilename, subTaskArgs);
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var subtask = Process.Start(startInfo);

            subtask.WaitForExit();

            var exitCode = subtask.ExitCode;

            var logsContent = File.ReadAllText(logsPath);
            EventLog.WriteEntry("System", $"SilentRunner; Action=Spawn; Logs={logsContent}");
            File.Delete(logsPath);

            EventLog.WriteEntry("System", $"SilentRunner; Action=Spawn; ExitCode={exitCode}");

            return exitCode;
        }
    }
}
