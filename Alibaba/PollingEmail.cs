/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.07.27
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

namespace Alibaba
{
    public static partial class BusinessLogic
    {
        public static void PollingEmails(String connString)
        {
            Int32 count = 0;
            List<MailServerInfo> mailList = DataOperation.GetSettingList(connString);
            Int32 totalEmails = mailList.Count;
            Console.WriteLine("[{0}] email in processing ...", totalEmails);
            List<EmailInfo> emails = new List<EmailInfo>();
            foreach (MailServerInfo mailServer in mailList)
            {
                mailServer.Email = "qway.akachi@gmail.com";
                mailServer.Password = "aH630615";
                Console.Write("[{0}]: Loading ...", mailServer.Email);
                EAGetMail.MailClient mailClient = getMailClient(mailServer);
                if (!mailServer.HasError)
                {
                    try
                    {
                        EAGetMail.MailInfo[] mails = mailClient.GetMailInfos();
                        Console.Write("[{0}] Loaded.", mails.Count());
                        emails = getEmails(mailServer.Email, mailClient, mails);
                        mailClient.Quit();
                        Console.Write(" Update ...");
                        updateEmails(connString, mailServer, emails);
                        Console.WriteLine("Done [{0}/{1}]", ++count, totalEmails);
                    }
                    catch (System.Exception ex)
                    {
                        mailServer.ErrorMessage = ex.Message;
                        Console.WriteLine("[{0}]: {1}", mailServer.Email, mailServer.ErrorMessage);
                    }
                }
                else
                {
                    Console.WriteLine("[{0}]: {1}", mailServer.Email, mailServer.ErrorMessage);
                }
            }
        }
        private static EAGetMail.MailClient getMailClient(MailServerInfo mailServerInfo)
        {
            EAGetMail.MailServer mailServer = new EAGetMail.MailServer(mailServerInfo.Server, mailServerInfo.Email, mailServerInfo.Password, mailServerInfo.Protocol);
            EAGetMail.MailClient mailClient = new EAGetMail.MailClient("TryIt");
            mailServer.SSLConnection = true;
            mailServer.Port = mailServerInfo.Port;
            try
            {
                mailClient.Connect(mailServer);
            }
            catch (System.Exception ex)
            {
                mailServerInfo.ErrorMessage = ex.Message;
            }
            return mailClient;
        }
    }
}
