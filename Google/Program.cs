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

namespace Google
{
    class Program
    {
        static void Main(string[] args)
        {
            //test();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));

            String connString = System.Configuration.ConfigurationManager.ConnectionStrings["Qway"].ConnectionString;
#if DEBUG
            args = new string[] { "PE", "ALL" };
#endif
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] Google.exe - Arguments:");
            for (Int32 index = 0; index < args.Length; ++index)
            {
                Console.WriteLine($"[{index}].[{args[index]}]");
            }
            if (args == null || args.Length != 2)
            {
                Console.WriteLine("PK: Polling Key Word Details");
                Console.WriteLine("PE: Polling Email");
                Console.WriteLine("SE: Sending Email");
            }
            else
            {
                String process = args[0];
                String code = args[1];
                switch (process)
                {
                    case "PS":
                        BusinessLogic.PollingSearchByAPI(connString, code);
                        break;
                    case "PK":
                        BusinessLogic.PollingSearch(connString, code);
                        break;
                    case "PE":
                        BusinessLogic.PollingEmail(connString);
                        break;
                    case "SE":
                        BusinessLogic.SendingEmail(connString, code, "AIM");
                        break;
                    default:
                        break;
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            //Console.ReadLine();
        }
        static void test()
        {
            Console.WriteLine("First display of filenames to the console:");

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            Console.SetOut(sw);
            Console.Out.WriteLine("Display filenames to a file:");
            Console.Out.WriteLine();

            Console.Out.Close();
            sw.Close();

            StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            Console.WriteLine("Second display of filenames to the console:");
        }
    }
}
