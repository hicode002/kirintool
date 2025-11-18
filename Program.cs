using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KirinTool.ImageFlasher;
//using Potato.Fastboot;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Runtime.Intrinsics.Arm;
using System.Collections;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Drawing.Printing;
using Microsoft.Win32;
using System.Runtime.InteropServices;
namespace KirinTool
{
    internal class Program
    {
        static int usbfused=0;
        static void execute()
        {
            if (usbfused == 0)
            {
                System.Threading.Thread.Sleep(2500);
                return;
            }
            //Console.WriteLine("Press any key to continue...(REPLUG)");
            //Console.ReadKey(true);
            //return;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c devcon restart \"USB\\VID_12D1&PID_3609\"";
            startInfo.UseShellExecute = false;
            startInfo.Verb = "runas";
            Process process = Process.Start(startInfo);
            process.WaitForExit();
            
        }
        static  Dictionary<string, int> socid = new Dictionary<string, int>
        {
            {"STOCK",0 },
            {"659",1 },
            {"710",2 },
            {"710F",2 },
            {"710A",2 },
            {"810",3 },
            {"980",4 },
            {"9904G",5 },
            {"9905G",6 },
            {"990E",6 },
            {"9805G",7 },
            {"820",7 },
            {"970",8 },
            {"960",9 },
            {"UNFUSED9000",10 }
        };
        static void Main(string[] args)
        {
            Console.WriteLine("Kirintool Program Public (c) Hicode002 2025");
            Console.WriteLine("Select Platform and then press enter(STOCK does not run any exploit)\n659 710 710F 710A 970 960 980 9904G 9905G 810 820 9805G 990E UNFUSED9000 STOCK");
            
            string platformstr ;
            if (args.Length < 1)
            {
                platformstr = Console.ReadLine();
            }
            else
            {
                platformstr = args[0];
                Console.WriteLine("No need to input,get platform from arguments.");
            }
            
            int platid = socid[platformstr];
            Console.WriteLine("Selected platform " + platformstr + " , id = "+platid.ToString());
            if (platid == 0)
            {
                Console.WriteLine("Supported functions: stockload");
            }
            
        }
    }
}