/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.04.27
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

using HtmlAgilityPack;
using System.Drawing;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using System.Reflection;
using System.Diagnostics;
using EAGetMail;

namespace Alibaba
{
    public class Email : IDisposable
    {
        String _FolderName;

        public String EmailId { get; set; } = String.Empty;
        public String FeedbackId { get; set; } = String.Empty;
        public String MAPIFolderName { get; set; } = String.Empty;
        public String FolderName { get; set; } = String.Empty;
        public String MAPIFolderStoreName { get; set; } = String.Empty;
        public String SenderEmailAddress { get; set; } = String.Empty;
        public String SentOnBehalfOfName { get; set; } = String.Empty;
        public DateTime SentOn { get; set; } = new DateTime();
        public String SentTo { get; set; } = String.Empty;
        public String SentToOriginal { get; set; } = String.Empty;
        public String Subject { get; set; } = String.Empty;
        public String Body { get; set; } = String.Empty;
        public String HtmlBody { get; set; } = String.Empty;
        public String ContactName { get; set; } = String.Empty;
        public String CompanyName { get; set; } = String.Empty;
        public String UIDL { get; set; } = String.Empty;
        public String Version { get; set; } = String.Empty;
        public String SenderIP { get; set; } = String.Empty;
        public Dictionary<String, String> Errors { get; set; } = new Dictionary<String, String>();
        public String ErrorMessage { get { return getErrorMessage(); } }

        public Email() { }
        public Email(String mapiFolderName, String senderEmailAddress, String mapiFolderStoreName, String folderName) : this()
        {
            this.MAPIFolderName = mapiFolderName;
            this.SenderEmailAddress = senderEmailAddress;
            this.MAPIFolderStoreName = mapiFolderStoreName;
            this.FolderName = folderName;
        }
        public Email(String email,EAGetMail.MailInfo mailInfo, EAGetMail.Mail mail) : this()
        {
            try
            {

                this.MAPIFolderName = email;
                this.SenderEmailAddress = mail.From.Address;
                this.MAPIFolderStoreName = mailInfo.Categories.ToString();
                this.FolderName = mailInfo.EWSChangeKey;
                this.UIDL = mailInfo.UIDL;
                this.HtmlBody = mail.HtmlBody;
                this.SentOn = mail.SentDate;
                this.Subject = mail.Subject;
                this.SentTo = mail.To != null && mail.To.Count() > 0 ? String.Join(";", mail.To.Select(m => m.Address).ToArray<String>()) : String.Empty;
                this.Version = mail.Version;
                this.SenderIP = getSenderIP(mail.Headers);
                if (this.HtmlBody.Contains("feedbackid"))
                //if (this.SenderIP == "198.11.134.49" || this.SenderIP == "115.124.25.42")
                    SetProperties();
                //foreach (HeaderItem header in mail.Headers)
                //{
                //    Console.WriteLine("{0}: {1}", header.HeaderKey, header.HeaderValue);
                //}
            }
            catch (System.Exception ex)
            {
                this.Errors["SetProperties"] = ex.Message;
            }
        }
        private String getSenderIP(HeaderCollection headers)
        {
            String ip = String.Empty;
            if (headers != null && headers.Count > 0)
            {
                Int32 index = headers.SearchKey("X-Sender-IP");
                if (index >= 0)
                    ip = ((HeaderItem)headers[index]).HeaderValue;
            }
            return ip;
        }
        public void Save()
        {

        }
        public void SetProperties(MailItem mailItem)
        {
            this.SenderEmailAddress = mailItem.SenderEmailAddress;
            this.SentOn = mailItem.SentOn;
            this.SentTo = mailItem.To;
            this.EmailId = mailItem.EntryID;
            this.HtmlBody = mailItem.HTMLBody;
            this.Subject = mailItem.Subject;
            this.SentOnBehalfOfName = mailItem.SentOnBehalfOfName;
            SetProperties();
        }
        public void SetProperties()
        {
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(this.HtmlBody);
                HtmlNodeCollection nodesParent = doc.DocumentNode.SelectNodes("//body/table/tbody/tr/td/table/tbody/tr/td/table/tbody/tr/td/table");
                HtmlNodeCollection nodes = nodesParent[3].SelectNodes(".//tbody/tr/td");
                this.ContactName = Utilities.WebExtension.DecodeHtml(nodes[1].InnerText.Trim());
                this.CompanyName = Utilities.WebExtension.DecodeHtml(nodes[2].InnerText.Trim());
                HtmlNode node = nodesParent[5].SelectSingleNode(".//tbody/tr/td");
                this.Body = Utilities.WebExtension.DecodeHtml(node.InnerText.Trim());
                nodesParent = doc.DocumentNode.SelectNodes("//body/i");
                String feedbackid = String.Empty;
                if (nodesParent != null)
                {
                    node = nodesParent.Last<HtmlNode>();
                    feedbackid = node == null ? String.Empty : node.InnerText.Trim().Replace("==", "").Replace("&quot;", "\"");
                    JObject json = (JObject)JsonConvert.DeserializeObject(feedbackid);
                    if (json != null)
                        this.FeedbackId = json["feedbackid"].ToString().Trim();
                }
                nodes = doc.DocumentNode.SelectNodes("//body/img");
                if (nodes != null)
                {
                    String value = nodes.Last<HtmlNode>().GetAttributeValue("src", "").Replace("&amp;", "&");
                    String[] values = value.Split('?');
                    if (values.Length > 1)
                    {
                        values = values[1].Split('&');
                        foreach (String item in values)
                        {
                            String[] pair = item.Split('=');
                            if (pair.Length == 2)
                            {
                                value = pair[1];
                                switch (pair[0])
                                {
                                    case "crm_mtn_tracelog_log_id":
                                        break;
                                    case "crm_mtn_tracelog_task_id":
                                        break;
                                    case "from":
                                        break;
                                    case "to":
                                        this.SentToOriginal = value;
                                        break;
                                    case "from_sys":
                                        break;
                                    case "biz_type":
                                        break;
                                    case "template":
                                        break;
                                }
                            }
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                this.Errors["HtmlBody"] = ex.Message;
            }
        }
        private string getErrorMessage()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String key in this.Errors.Keys)
                sb.AppendLine(String.Format("[{0}] - {1}", key, this.Errors[key]));
            return sb.ToString();
        }

        public override string ToString()
        {
            return String.Format("[{0}], [{1}]", this.ContactName, this.CompanyName);
        }


        public List<KeyValuePair<String, Object>> GetSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@EmailId",this.EmailId),
                new KeyValuePair<String, Object>("@FeedbackId",this.FeedbackId),
                new KeyValuePair<String, Object>("@MAPIFolderName",this.MAPIFolderName),
                new KeyValuePair<String, Object>("@MAPIFolderStoreName",this.MAPIFolderStoreName),
                new KeyValuePair<String, Object>("@FolderName",this.FolderName),
                new KeyValuePair<String, Object>("@SentOn",this.SentOn),
                new KeyValuePair<String, Object>("@SentFrom",this.SenderEmailAddress),
                new KeyValuePair<String, Object>("@SentOnBehalfOfName",this.SentOnBehalfOfName),
                new KeyValuePair<String, Object>("@SentTo",this.SentTo),
                new KeyValuePair<String, Object>("@SentToOriginal",this.SentToOriginal),
                new KeyValuePair<String, Object>("@SenderIP",this.SenderIP),
                new KeyValuePair<String, Object>("@Subject",this.Subject),
                new KeyValuePair<String, Object>("@Body",this.Body),
                new KeyValuePair<String, Object>("@HtmlBody",this.HtmlBody),
                new KeyValuePair<String, Object>("@ContactName",this.ContactName),
                new KeyValuePair<String, Object>("@CompanyName",this.CompanyName)
            };
            return parameters;
        }
        private String getInnerText(HtmlNode nodeParent, String xPath)
        {
            String value = String.Empty;
            if (nodeParent != null)
            {
                HtmlNode nodeInner = nodeParent.SelectSingleNode(xPath);
                value = nodeInner == null ? String.Empty : nodeInner.InnerText.Trim();
            }
            return value;
        }
        private String getValue(HtmlNode node, String keyValue, String key)
        {
            String value = String.Empty;
            if (keyValue.StartsWith(key))
            {
                HtmlNode nodeInner = node.SelectSingleNode("td");
                value = nodeInner == null ? String.Empty : nodeInner.InnerText.Trim();
            }
            return value;
        }

        public void Dispose()
        {

        }
    }

}
