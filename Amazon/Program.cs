/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.05.21
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
using System.Text.RegularExpressions;

namespace Amazon
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));

            String connString = System.Configuration.ConfigurationManager.ConnectionStrings["Amazon"].ConnectionString;
#if DEBUG
            args = new string[] { "AZP" };    //S,U
#endif
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("AZC: Category");
                Console.WriteLine("PA: Polling suppler url according to product category");
                Console.WriteLine("UM: Polling suppler from supplier url");
                Console.WriteLine("UMF: Polling suppler from supplier url by using WebDriver");
            }
            else
            {
                String code = args[0];
                switch (code)
                {
                    case "AZP":    //Product
                        BusinessLogic.PollingProduct(connString, code);
                        break;
                    case "AZC":
                        BusinessLogic.PollingCategory(connString, code);
                        break;
                    case "AZCA":
                        BusinessLogic.PollingCategory(connString);
                        break;
                    case "AZCS":
                        BusinessLogic.PollingCategorySeller(connString);
                        break;
                    case "AZS":
                        BusinessLogic.PollingSeller(connString);
                        break;
                    case "AZSD":
                        BusinessLogic.PollingSellerDetail(connString);
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
            Console.ReadLine();
        }
    }
}
