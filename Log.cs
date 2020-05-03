using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace OS_Game_Launcher
{
    class Log
    {
        private static string sLogFormat;
        private static string sErrorTime;
        private static string sPathNameError = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OS Game-Launcher Logs", "errors.log");
        private static string sPathNameWarning = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OS Game-Launcher Logs", "warnings.log");

        private static void GenerateTimestamps()
        {
            //sLogFormat used to create log files format :
            // dd/mm/yyyy hh:mm:ss AM/PM ==> Log Message
            sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " :: ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            sErrorTime = sYear + sMonth + sDay;
        }


        public static void Fatal(string sErrMsg)
        {
            GenerateTimestamps();
            
            Utils.CreateDirectoryIfNotExists(Path.GetDirectoryName(sPathNameError));
            
            StreamWriter sw = new StreamWriter(sPathNameError, true);
            
            sw.WriteLine(sLogFormat + sErrMsg + "\n\n");
            sw.Flush();
            sw.Close();
        }


        public static void Warning(string sWarnMsg)
        {
            GenerateTimestamps();
            
            Utils.CreateDirectoryIfNotExists(Path.GetDirectoryName(sPathNameError));
            
            StreamWriter sw = new StreamWriter(sPathNameWarning, true);
            
            sw.WriteLine(sLogFormat + sWarnMsg + "\n\n");
            sw.Flush();
            sw.Close();
        }
        
    }
}
