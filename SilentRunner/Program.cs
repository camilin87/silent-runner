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

                EventLog.WriteEntry("System", "SilentRunner; Action=Complete; ExitCode=0");
            }
            catch(Exception exc)
            {
                EventLog.WriteEntry("System", $"SilentRunner; Action=Error; Error={exc}");
                throw;
            }
        }
    }
}
