using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using RestSharp;

namespace OS_Game_Launcher
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        static Mutex mutex = null;
        App()
        {
            Console.WriteLine("App registered!");
            AppDomain.CurrentDomain.UnhandledException += GlobalCrashHandler;

            this.Startup += App_Startup;

        }

        public static List<string> startupArgs;

        void App_Startup(object sender, StartupEventArgs e)
        {
            Console.WriteLine("Checking mutex");
            bool createdNew;
            mutex = new Mutex(true, "OSGameLauncher_8ec3a574-c889-4ad8-a3a1-2b4cd6fd9b7d", out createdNew);
            if (!createdNew)
            {
                Console.WriteLine("Mutex already exists");
                mutex = null;
                var manager = new NamedPipeManager("OSGameLauncher");
                manager.Write("CUSTOM_ACTION::SET_MAIN_WND_FOCUS");
                var j_sta = JsonConvert.SerializeObject(e.Args);
                manager.Write("STARTUP_ARGS::" + j_sta);
                Application.Current.Shutdown(0);
                return;
            } else
            {
                startupArgs = new List<string>(Environment.GetCommandLineArgs());
            }
        }

        public void GlobalCrashHandler(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG == false
            Console.WriteLine("\n\n==CRASH HANDLER==");
            Console.WriteLine("FATAL! Fetched unknown Error!");

            Log.Fatal(e.ExceptionObject.ToString());

            Console.WriteLine(e.ExceptionObject.ToString());

            var result = MessageBox.Show("An unknown error occurred!\n" + e.ExceptionObject.ToString() +
                "\n\nWould you like to send the error and some information about your system to our team to improve the usability of the OS Game-Launcher",
                "Fatal", MessageBoxButton.YesNo);

            Console.WriteLine("Report to servers: " + result.ToString());
            if (result == MessageBoxResult.Yes)
            {
                string os_info = SystemInfo.getOperatingSystemInfo();
                string processor_info = SystemInfo.getProcessorInfo();
                string ram_info = "RAM-Size: " + SystemInfo.getRAMSize() + "\n";
                string graphics_cards = "Active-Graphics-Cards: " + SystemInfo.getActiveGraphicsCards() + "\n";
                string system_information = os_info + processor_info + ram_info + graphics_cards;
                Console.WriteLine("System Info:\n" + os_info + processor_info + ram_info);

                var client = new RestClient(OS_Game_Launcher.Properties.Settings.Default.host);
                client.UserAgent = "OSGameLauncherClient/0.0.1";

                try
                {
                    var request = new RestRequest("/crash/report");
                    request.AddParameter("error_message", e.ExceptionObject.ToString());
                    request.AddParameter("system_information", system_information);
                    var response =  client.Get(request);
                } catch (Exception ex)
                {
                    Console.WriteLine("Can not report error to server. " + ex.Message);
                }
                
            }

            Console.WriteLine("=================\n\n");



            Environment.Exit(1);

#endif
        }
    }
}
