using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentRunner
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                EventLog.WriteEntry("System", $"SilentRunner; Action=Start; Args={string.Join(" ", args)}");

                if (args.Length < 1)
                {
                    throw new InvalidOperationException("Not enough parameters provided");
                }

                var subTaskFilename = args[0];
                var subTaskArgs = string.Join(" ", args.Skip(1).Take(args.Length - 1));

                EventLog.WriteEntry("System", $"SilentRunner; Action=Spawn; Subtask={subTaskFilename}; Args={subTaskArgs}");

                var startInfo = new ProcessStartInfo(subTaskFilename, subTaskArgs);
                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                var subtask = Process.Start(startInfo);

                subtask.WaitForExit();

                var exitCode = subtask.ExitCode;

                EventLog.WriteEntry("System", $"SilentRunner; Action=Complete; ExitCode={exitCode}");
                Environment.Exit(exitCode);
            }
            catch(Exception exc)
            {
                EventLog.WriteEntry("System", $"SilentRunner; Action=Error; Error={exc}");
                throw;
            }
        }
    }
}
