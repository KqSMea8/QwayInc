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

using Alibaba;
namespace PollEmails
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));

            BusinessLogic.PollingEmails(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString);

            //if (args.Length > 0)
            //    BusinessLogic.PollingEmails(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, getMAPIFolders());
            //else
            //    BusinessLogic.PollingEmailsFromMailServer(System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString, getMailServers());
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine(String.Format("{0}{1}{0}", new String('=', 5), DateTime.Now));
            Console.WriteLine(String.Format("Elapsed: {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            Console.WriteLine("THE END");
            Console.ReadLine();
        }
        //private static Dictionary<String, String> getMAPIFolders()
        //{
        //    Dictionary<String, String> dic = new Dictionary<String, String>();
        //    String[] keyValues = Properties.Settings.Default.MAPIFolders.Split(';');
        //    foreach (String keyValue in keyValues)
        //    {
        //        String[] kv = keyValue.Split(',');
        //        dic.Add(kv[0].Trim(), kv[1].Trim());
        //    }
        //    return dic;
        //}
        //private static Dictionary<String, MailServerInfo> getMailServers()
        //{
        //    Dictionary<String, MailServerInfo> dic = new Dictionary<String, MailServerInfo>();
        //    String[] keyValues = Properties.Settings.Default.MailServers.Split(';');
        //    foreach (String keyValue in keyValues)
        //    {
        //        String setting = keyValue.Trim();
        //        if (!String.IsNullOrEmpty(setting))
        //        {
        //            MailServerInfo mailServer = new MailServerInfo(setting);
        //            //if (mailServer.Email == "qway.inc.f@hotmail.com")
        //                dic.Add(mailServer.Email, mailServer);
        //        }
        //    }
        //    return dic;
        //}
    }
}
