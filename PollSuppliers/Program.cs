/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.06.10
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

using Alibaba;

namespace PollSuppliers
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            //TimeSpan timeSpan = new TimeSpan(0, 2, 10, 30, 0);
            //timeSpan = new TimeSpan(timeSpan.Ticks / 2);
            //Console.WriteLine("Done [{0:d\\.hh\\:mm\\:ss}]/per.", timeSpan);
            //Console.WriteLine("Done [{0}]/per.", new TimeSpan(0, 2, 3, 4, 5));
#if DEBUG
            args = new string[] { "UM" };    //S,U
#endif
            if (args == null || args.Length != 1)
            {
                Console.WriteLine("SA: Polling suppler url according to supplier category");
                Console.WriteLine("PA: Polling suppler url according to product category");
                Console.WriteLine("UM: Polling suppler from supplier url");
                Console.WriteLine("UMF: Polling suppler from supplier url by using WebDriver");
            }
            else
            {
                String code = args[0];
                switch (code)
                {
                    case "SA":  //Supplier Category to url
                    case "PA":  //Product Category to url
                        BusinessLogic.PollingCategoryUrls(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, code);
                        break;
                    case "PC":
                        BusinessLogic.PollingProductCategory(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, code);
                        break;
                    case "S":
                        BusinessLogic.PollingSuppliers(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);
                        break;
                    case "U":
                        BusinessLogic.PollingSuppliersFromUrl(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);
                        break;
                    case "MU":
                        BusinessLogic.PollingSuppliersMetadataFromUrl(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);
                        break;
                    case "UM":  //by using webClient
                        BusinessLogic.PollingSuppliersFromUrlMetadata(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, false);
                        break;
                    case "UMF": //By using webdriver
                        BusinessLogic.PollingSuppliersFromUrlMetadata(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, true);
                        break;
                    default:
                        //BusinessLogic.PollingSuppliers(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);
                        //BusinessLogic.PollingSuppliersFromUrl(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);
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
    }
}
