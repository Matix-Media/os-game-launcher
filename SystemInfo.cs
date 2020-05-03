using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Microsoft.Win32;

namespace OS_Game_Launcher
{
    class SystemInfo
    {
        public static string getOperatingSystemInfo()
        {
            string info_string = "";
            //Create an object of ManagementObjectSearcher class and pass query as parameter.
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject managementObject in mos.Get())
            {
                if (managementObject["Caption"] != null)
                {
                    info_string += "OS-Name: " + managementObject["Caption"].ToString() + "\n";
                }
                if (managementObject["OSArchitecture"] != null)
                {
                    info_string += "OS-Architecture: " + managementObject["OSArchitecture"].ToString() + "\n";
                }
                if (managementObject["CSDVersion"] != null)
                {
                    info_string += "CSD-Version: " + managementObject["CSDVersion"].ToString() + "\n";
                }
            }

            return info_string;
        }

        public static UInt64 getRAMSize()
        {
            try
            {
                ManagementScope Scope;
                Scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", "."), null);

                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);
                UInt64 Capacity = 0;
                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    Capacity += (UInt64)WmiObject["Capacity"];
                }
                return Capacity;
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception {0} Trace {1}", e.Message, e.StackTrace));
                return 0;
            }
        }

        public static string getProcessorInfo()
        {
            string info_string = "";
            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);   //This registry entry contains entry for processor info.

            if (processor_name != null)
            {
                if (processor_name.GetValue("ProcessorNameString") != null)
                {
                    info_string += "Processor-Name: " + processor_name.GetValue("ProcessorNameString") + "\n";
                }
            }

            return info_string;
        }

        public static string getActiveGraphicsCards()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            string graphicsCards = string.Empty;
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["CurrentBitsPerPixel"] != null && obj["CurrentHorizontalResolution"] != null)
                {
                    graphicsCards += obj["Name"].ToString() + " // ";
                }
            }

            return graphicsCards;
        }
    }
}
