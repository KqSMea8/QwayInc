/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.09.22
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
using System.Diagnostics;
using System.Net.Mail;
using Utilities;

namespace SendingEmails
{
    public static partial class BusinessLogic
    {
        internal static void SendingEmail(string connString, string code)
        {
            Boolean success = false;
            Utilities.Utilities.Log(message: "Sending emails Start ... ", isWriteLine: false, addTime: true);
            MailMessage mailMessage = getMailMessage();
            SmtpClient smtpClient = getsmtpClient();
            String emailBodyOrg = getEmailBody(code);
            List<EmailInfo> emails = DataOperation.GetEamils(connString);
            Utilities.Utilities.Log(message: $"[{emails.Count}]", isWriteLine: true);
            Int32 count = (Int32)(emails.Count * 0.1);
            foreach (EmailInfo email in emails.Take(count))
            {
                Utilities.Utilities.Log(message: $"Sending [{email.Email}] ... ", isWriteLine: false, addTime: true);
                email.Code = code;
                email.CompanyName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(email.CompanyName.ToLower());
                String emailBody = emailBodyOrg.Replace("{Company}", email.CompanyName).Replace("\r\n", "<br />");
                string[] toEmails = email.Email.Split(';');
                mailMessage.To.Clear();
                foreach (String toEmail in toEmails)
                    try { mailMessage.To.Add(toEmail); }
                    catch (Exception) { }
                System.Net.Mail.AlternateView plainView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(System.Text.RegularExpressions.Regex.Replace(emailBody, @"<(.|\n)*?>", string.Empty), null, "text/plain");
                System.Net.Mail.AlternateView htmlView = System.Net.Mail.AlternateView.CreateAlternateViewFromString(emailBody, null, "text/html");
                mailMessage.AlternateViews.Add(plainView);
                mailMessage.AlternateViews.Add(htmlView);
                try
                {
                    smtpClient.Send(mailMessage);
                    success = DataOperation.UpdateEamil(connString, email);
                    Utilities.Utilities.Log(message: success ? $"[Done]" : "[X]", isWriteLine: true, addTime: false);
                }
                catch (Exception ex)
                {
                    Utilities.Utilities.Log(message: $"[XXX]\n{ex.Message}", isWriteLine: true, addTime: false);
                }
            }
            smtpClient.Dispose();
            mailMessage.Dispose();
        }

        private static MailMessage getMailMessage()
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("qway.inc@gmail.com");
            mailMessage.Subject = "Hotel Linen Products by QWay";
            mailMessage.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
            mailMessage.IsBodyHtml = true;
            return mailMessage;
        }

        private static SmtpClient getsmtpClient()
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Credentials = new System.Net.NetworkCredential("qway.inc@gmail.com", "Ah630615");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            return smtpClient;
        }

        private static string getEmailBody(string code)
        {
            switch (code)
            {
                case "ALHT":
                    return Properties.Resources.Script_HT;
                default:
                    return String.Empty;
            }
        }
    }
}
