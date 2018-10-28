/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.06.21
 *   Description: 
 *       Company: Qway Inc.
 *  Project Name: 
 *     Reference: 
 * Code Reviewer: 
 *   Review Date: 
 *        Remark: 
 * ****************************************************************************
 * Revision History:                                                          *
 * ========================================================================== *
 *          Name: 
 * Modified Date: 
 *   Description: 
 * --------------------------------------------------------------------------
 * ****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;

using System.Net;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Web;
//using System.Web.HttpUtility;
using Utilities;


namespace ProcessProgram
{
    class Program
    {
        private static Boolean _Stop = false;
        private static Boolean _End = false;
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
#if DEBUG
            args = new string[] {
                @"C:\Qway\Google\Google.exe",
                "PE ALL",
                "0",
                "0",
                "10"
            };
            //args = new string[] {
            //    @"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\ProcessSample\bin\Debug\ProcessSample.exe",
            //    "PE ALL",
            //    "0",
            //    "0",
            //    "1"
            //};
#endif
            if (args != null || args.Length == 5)
            {
                doProcess(args[0], args[1], Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]));
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            Console.ReadLine();
        }

        private static void doProcess(string fileName, string arguments, Int32 hours, Int32 minutes, Int32 seconds)
        {
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}]: Process Satrt ...");
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}]:\n\tFile Name: {fileName}\n\t Arguments: {arguments}\n\t Hours: {hours}\n\t Minutes: {minutes}");
            _Stop = false;
            Boolean success = false;
            TimeSpan wait = new TimeSpan(0, 0, 0);
            Int32 count = 0;
            do
            {
                //++count;
                //arguments = count.ToString();
                success = StartProcess(fileName, arguments, wait);
                wait = new TimeSpan(hours, minutes, seconds);
            } while (success);
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}]: Process End");
        }

        public static Boolean StartProcess(String fileName, String arguments, TimeSpan wait)
        {
            Boolean success = false;
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(fileName);
            //process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //process.StartInfo.CreateNoWindow = true; //not display a windows
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.Exited += Process_Exited;
            process.StartInfo.Arguments = arguments;
            Thread.Sleep(wait);
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                //string output = process.StandardOutput.ReadToEnd(); //The output result
                process.WaitForExit();
                Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] ExitCode: {process.ExitCode}");
                success = process.ExitCode != 0;
            }
            catch (Exception ex)
            {
            }
            process.Dispose();
            return success;
        }
        private static void Process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}]: Exited");
        }

        private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Process process = sender as Process;
            if (e.Data != null)
            {
                Console.WriteLine($"[X]{e.Data}");
                if (e.Data == "END")
                    _Stop = true;
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Console.WriteLine($"{e.Data}");
            }
        }
    }
}
