/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.05.31
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
using System.Data;
using System.Drawing;
using System.Net;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Utilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Office.Interop.Outlook;
using EAGetMail;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Alibaba
{
    public class HotelInfo
    {
        public Int32 Id { get; set; } = -1;
        public String Telephone { get; set; } = String.Empty;
        public String Address { get; set; } = String.Empty;
        public String HotelId { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String LinkedMedata { get; set; } = String.Empty;
        public String Medata { get; set; } = String.Empty;
        public String StarRating { get; set; } = String.Empty;
        public String Region { get; set; } = String.Empty;
        public String Domain { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String BaseUrl { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public Dictionary<String, String> Emails { get; set; } = new Dictionary<String, String>();
        public String Email { get { return getEmail(); } }
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public HotelInfo() { }
        public HotelInfo(ChromeDriver driver, IWebElement element, String region) : this()
        {
            this.Id = 0;
            this.Region = region;
            try
            {
                //this.HotelId = element.GetAttribute("data-hotel-id");
                this.HotelId = element.GetValue("data-hotel-id");
                this.Name = element.GetElementValue(".//h3/a");
                this.Address = element.GetElementValue(".//div[@class='contact']/p[@class='p-adr']");
                this.Telephone = element.GetElementValue(".//div[@class='contact']/p[@class='p-tel']");
                this.StarRating = element.GetElementValue(".//div[@class='star-rating-text']");
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                if (js != null)
                {
                    this.LinkedMedata = (string)js.ExecuteScript("return arguments[0].outerHTML;", element);
                }
            }
            catch (System.Exception)
            {
            }

        }
        public HotelInfo(HtmlNode node, String region) : this()
        {
            try
            {
                this.Id = 0;
                this.Region = region;
                this.HotelId = node.GetAttributeValue("data-hotel-id", "*****");
                this.HotelId = node.GetValue(".", "data-hotel-id", "*****");
                this.Name = node.GetValue("//h3/a");
                this.Address = node.GetValue("//div[@class='contact']/p[@class='p-adr']", removeReturn: true);
                this.Telephone = node.GetValue("//div[@class='contact']/p[@class='p-tel']");
                this.StarRating = node.GetValue("//div[@class='star-rating-text']");
                this.LinkedMedata = node.OuterHtml;
            }
            catch (System.Exception)
            {
            }
        }

        internal void Update(ChromeDriver driver)
        {
            this.Url = driver.Url;
            this.BaseUrl = this.Url.GetBaseUrl();
            driver.Navigate().GoToUrl(this.BaseUrl);
            this.Medata = driver.PageSource;
            this.Domain = (new Uri(this.Url)).Host.ToLower().Replace("www.", "");
            String pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(this.Medata);
            foreach (Match match in matches)
            {
                String email = match.Value.ToLower();
                this.Emails[email] = getDomain(email);
            }
        }

        private String getEmail()
        {
            List<String> list = this.Emails.Where(kvp => kvp.Value == this.Domain).Select(kvp => kvp.Key).ToList<String>();
            if (list.Count == 0)
                list = this.Emails.Keys.ToList<String>();
            return list.Count > 0 ? String.Join(";", list) : String.Empty;
        }
        private String getDomain(String email)
        {
            String domain = String.Empty;
            String[] items = email.Split('@');
            if (items.Length > 1)
                domain = items[1];
            return domain;
        }

        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@HotelId",this.HotelId),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Region",this.Region),
                new KeyValuePair<String, Object>("@Address",this.Address),
                new KeyValuePair<String, Object>("@Telephone",this.Telephone),
                new KeyValuePair<String, Object>("@StarRating",this.StarRating),
                new KeyValuePair<String, Object>("@LinkedMedata",this.LinkedMedata),
                new KeyValuePair<String, Object>("@Domain",this.Domain),
                new KeyValuePair<String, Object>("@Email",this.Email),
                new KeyValuePair<String, Object>("@Comments",this.Comments),
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@BaseUrl",this.BaseUrl),
                new KeyValuePair<String, Object>("@Medata",this.Medata),
            };
            return parameters;
        }
    }

    public class SupplierUrlInfo
    {
        public Int32 Id { get; set; } = -1;
        public String ProfileUrl { get; set; } = String.Empty;
        public String Status { get; set; } = String.Empty;
        public Int32 Tries { get; set; } = -1;
        public DateTime AddedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime UpdateDate { get; set; } = new DateTime(1753, 1, 1);
        public String ErrorMessage { get; set; } = String.Empty;
        public Boolean Active { get; set; } = false;
        public Boolean HasError { get { return !String.IsNullOrEmpty(this.ErrorMessage); } }
        public Int32 CategoryId { get; set; } = -1;
        public String CategoryName { get; set; } = String.Empty;
        public String CategoryCode { get; set; } = String.Empty;
        public Int32 CategoryLevel { get; set; } = -1;
        public String CompanyMetadata { get; set; } = String.Empty;
        public String ContactMetadata { get; set; } = String.Empty;
        public String CompanyProfileUrl { get; set; } = String.Empty;
        public String ContactProfileUrl { get; set; } = String.Empty;
        public String CompanyUrl { get { return "/company_profile.html".GetAbsoluteUrl(this.ProfileUrl, false); } }
        public String ContactUrl { get { return "/contactinfo.html".GetAbsoluteUrl(this.ProfileUrl, false); } }
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Status",this.Status),
                new KeyValuePair<String, Object>("@ErrorMessage",this.ErrorMessage),
                new KeyValuePair<String, Object>("@CompanyMetadata",this.CompanyMetadata),
                new KeyValuePair<String, Object>("@ContactMetadata",this.ContactMetadata),
                new KeyValuePair<String, Object>("@CompanyProfileUrl",this.CompanyUrl),
                new KeyValuePair<String, Object>("@ContactProfileUrl",this.ContactUrl)
            };
            return parameters;
        }

        internal void Update()
        {
            this.Status = "Metadata";
            using (WebClient client = new WebClient())
            {
                this.CompanyMetadata = getMetadata(client, this.CompanyUrl);
                this.ContactMetadata = getMetadata(client, this.ContactUrl);
            }
        }
        private String getMetadata(WebClient client, String url)
        {
            String meta = String.Empty;
            if (url.IsWebSiteAvailable())
                meta = client.DownloadString(url);
            else
                this.ErrorMessage = $"Not Available: {url}";
            return meta;
        }
    }
    public class WebsiteInfo
    {
        private static List<String> _IgnoredList = getIgnoredList();

        public String Code { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String ParentUrl { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String Metadata { get; set; } = String.Empty;
        public String WebsiteUrl { get { return getWebsiteUrl(); } }
        public Boolean Active { get; set; } = true;
        public Boolean IsIgnored { get { return getIsIgnored(); } }
        public Int32 Id { get; set; } = -1;
        public Int32 DumpLevel { get; set; } = -1;
        public Int32 ParentId { get; set; } = -1;
        public DateTime AddedDate { get; set; } = new DateTime(1800, 1, 1);
        public DateTime UpdatedDate { get; set; } = new DateTime(1800, 1, 1);
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public WebsiteInfo() { }
        public WebsiteInfo(String[] args)
        {
            if (args != null && args.Length >= 3)
            {
                this.Code = args[0];
                this.Url = args[1];
                this.DumpLevel = Convert.ToInt32(args[2]);
                this.Name = "Root";
            }
        }
        public WebsiteInfo(WebsiteInfo website)
        {
            if (website != null)
            {
                this.Code = website.Code;
                this.ParentUrl = website.WebsiteUrl;
                this.DumpLevel = website.DumpLevel - 1;
            }
        }

        private static List<string> getIgnoredList()
        {
            return new List<String>() {
                ".google.com",
                ".googleapis.com",
                ".googleblog.com",
                ".googletagmanager.com",
                ".googleblog.com"
            };
        }

        private bool getIsIgnored()
        {
            Boolean success = true;
            if (!String.IsNullOrEmpty(this.Url) && !this.Url.StartsWith("/"))
            {
                if (!this.Url.Contains(".google"))
                    success = _IgnoredList.Where(item => this.Url.Contains(item)).Count() > 0;
            }
            return success;
        }

        private String getWebsiteUrl()
        {
            String url = String.Empty;
            try
            {
                String orgUrl = this.Url.Replace(@"\", "").Replace("\"", "").Replace("'", "");
                if (!orgUrl.ToLower().StartsWith("http://")
                    && !orgUrl.ToLower().StartsWith("https://")
                    && !String.IsNullOrEmpty(this.ParentUrl))
                {
                    String[] list = this.ParentUrl.Split(':');
                    if (list.Length > 1)
                        orgUrl = String.Format("{0}://{1}", list[0], orgUrl.Replace("//", ""));
                }
                System.Uri uri = new Uri(orgUrl);
                url = uri.AbsoluteUri;
            }
            catch (System.Exception)
            {
            }

            return url;
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",this.Code),
                new KeyValuePair<String, Object>("@Active",this.Active),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@WebsiteUrl",this.WebsiteUrl),
                new KeyValuePair<String, Object>("@ParentUrl",this.ParentUrl),
                new KeyValuePair<String, Object>("@ParentId",this.ParentId),
                new KeyValuePair<String, Object>("@Metadata",this.Metadata)
            };
            return parameters;
        }
    }
    public class EmailInfo : IDisposable
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
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }

        public EmailInfo() { }
        public EmailInfo(String mapiFolderName, String senderEmailAddress, String mapiFolderStoreName, String folderName) : this()
        {
            this.MAPIFolderName = mapiFolderName;
            this.SenderEmailAddress = senderEmailAddress;
            this.MAPIFolderStoreName = mapiFolderStoreName;
            this.FolderName = folderName;
        }
        public EmailInfo(String email, EAGetMail.MailInfo mailInfo, EAGetMail.Mail mail) : this()
        {
            try
            {

                this.MAPIFolderName = email;
                this.SenderEmailAddress = mail.From.Address;
                //this.MAPIFolderStoreName = mailInfo.Categories.ToString();
                //this.FolderName = mailInfo.EWSChangeKey;
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
                char[] ccc = this.Body.ToCharArray();
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


        private List<KeyValuePair<String, Object>> getSQLParameters()
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
    public class MailServerInfo : SettingsInfo
    {
        public String Provider { get { return getProvider(); } }
        public String Server { get { return getServer(); } }
        public Int32 Port { get { return getPort(); } }
        public EAGetMail.ServerProtocol Protocol { get { return getProtocol(); } }

        public String ErrorMessage { get; set; }
        public Boolean HasError { get { return !String.IsNullOrEmpty(this.ErrorMessage); } }
        public MailServerInfo() { }

        private int getPort()
        {
            switch (this.Provider.ToLower())
            {
                case "hotmail.com":
                case "gmail.com":
                    return 993;
                default:
                    return 993;
            }
        }
        private ServerProtocol getProtocol()
        {
            switch (this.Provider.ToLower())
            {
                case "hotmail.com":
                case "gmail.com":
                    return EAGetMail.ServerProtocol.Imap4;
                default:
                    return EAGetMail.ServerProtocol.Imap4;
            }
        }

        private string getServer()
        {
            switch (this.Provider.ToLower())
            {
                case "hotmail.com":
                    return "imap-mail.outlook.com";
                case "gmail.com":
                    return "imap.gmail.com";
                default:
                    return String.Empty;
            }
        }

        private string getProvider()
        {
            String[] mails = this.Email.Split('@');
            if (mails.Length == 2)
                return mails[1];
            return String.Empty;
        }
    }
    public class SupplierInfo
    {
        #region Private Fields
        private ChromeDriver _Driver;
        private WebDriverWait _Wait;
        private String _Url;
        private SupplierCategoryInfo _Category;
        private SupplierUrlInfo _SupplierUrl;
        private HtmlDocument _HtmlDocument;
        private KeyValuePair<String, String> _XPathToCheck = new KeyValuePair<String, String>("//section[@class='error-404']", "//div[@class='banner-copy']");
        private String _CompanyName;

        #endregion
        #region Public Properties
        public Int32 Id { get; set; } = -1;
        public String CompanyUrl { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public String Account { get; set; } = String.Empty;
        public String ContactPhotoUrl { get; set; } = String.Empty;
        public String ContactName { get; set; } = String.Empty;
        public String Department { get; set; } = String.Empty;
        public String JobTitle { get; set; } = String.Empty;
        public String ContactId { get; set; } = String.Empty;
        public String ContactSid { get; set; } = String.Empty;
        public String ContactPid { get; set; } = String.Empty;
        public String Telephone { get; set; } = String.Empty;
        public String MobilePhone { get; set; } = String.Empty;
        public String Fax { get; set; } = String.Empty;
        public String Address { get; set; } = String.Empty;
        public String Zip { get; set; } = String.Empty;
        public String Country { get; set; } = String.Empty;
        public String Province { get; set; } = String.Empty;
        public String City { get; set; } = String.Empty;
        public String CompanyName { get; set; } = String.Empty;
        public String OperationalAddress { get; set; } = String.Empty;
        public String Website { get; set; } = String.Empty;
        public String WebsiteAlibaba { get; set; } = String.Empty;
        public Int32 CategoryId { get; set; } = -1;
        public String CategoryName { get; set; } = String.Empty;
        public String CategoryCode { get; set; } = String.Empty;
        public String CompanyBusinessType { get; set; } = String.Empty;
        public String CompanyLocation { get; set; } = String.Empty;
        public String CompanyCreditLevel { get; set; } = String.Empty;
        public String SupplierMainProducts { get; set; } = String.Empty;
        public String CompanyNumberOfEmployees { get; set; } = String.Empty;
        public String SupplierTotalAnnualSalesVolume { get; set; } = String.Empty;
        public String CompanyEstablishedYear { get; set; } = String.Empty;
        public String CompanyMainMarket { get; set; } = String.Empty;
        public String CountryCode { get; set; } = String.Empty;
        public String ModuleData { get; set; } = String.Empty;
        public String MetadataContact { get; set; } = String.Empty;
        public String MetadataCompany { get; set; } = String.Empty;
        public String CompanyDescription { get; set; } = String.Empty;
        public String ProductCertification { get; set; } = String.Empty;
        public SQLResultStatusInfo Result { get; set; } = new SQLResultStatusInfo();
        public String ContactProfileUrl { get; set; } = String.Empty;
        public String CompanyProfileUrl { get; set; } = String.Empty;
        public String CompanyId { get; set; } = String.Empty;
        public String Status { get; set; } = String.Empty;
        public String ScriptName { get; set; } = String.Empty;
        public String ScriptFileName { get { return System.IO.Path.Combine(@"./Resources", $"MessageScript-{this.ScriptName}.txt"); } }
        public String ErrorMessage { get { return this.Errors.ToString2(); } }
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public Dictionary<String, String> Errors { get; set; } = new Dictionary<String, String>();
        public Boolean HasError { get { return this.Errors.Count > 0; } }
        public Boolean FatalError { get; set; } = false;
        #endregion
        #region Constructors
        public SupplierInfo() { }
        public SupplierInfo(SupplierUrlInfo supplierUrl, ChromeDriver driver = null) : this()
        {
            this._Driver = driver;
            this.Status = "SupplierInfo - I";
            this._SupplierUrl = supplierUrl;
            this.CategoryId = this._SupplierUrl.CategoryId;
            this.CategoryName = this._SupplierUrl.CategoryName;
            this.CategoryCode = this._SupplierUrl.CategoryCode;
            this.CompanyProfileUrl = this._SupplierUrl.CompanyUrl;
            this.ContactProfileUrl = this._SupplierUrl.ContactUrl;
            this._HtmlDocument = new HtmlDocument();

            if (setPropertiesForCompanyMetadata())
                if (setpropertiesForContactMetadata())
                    updateProperties();
        }
        private Boolean setpropertiesForContactMetadata()
        {
            this.Status = "setpropertiesForContact";
            try
            {
                this.MetadataContact = this.ContactProfileUrl.DownloadString(this._Driver, xPathToCheck: this._XPathToCheck);
                this.ContactProfileUrl = this._Driver == null ? this.ContactProfileUrl : this._Driver.Url;
                if (StatusExtension.HasError || !String.IsNullOrEmpty(StatusExtension.ErrorMessage))
                {
                    this.ContactProfileUrl = this._HtmlDocument.GetValue("//ul[@class='navigation-list']/li[4]/a", "href");
                    this.MetadataContact = this.ContactProfileUrl.DownloadString(this._Driver);
                }
                if (StatusExtension.HasError || !String.IsNullOrEmpty(StatusExtension.ErrorMessage))
                {
                    this.Errors[this.Status] = StatusExtension.ErrorMessage;
                }
                else
                {
                    //System.IO.File.WriteAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Contact.html", this.MetadataContact);
                    this._HtmlDocument.LoadHtml(this.MetadataContact);
                    if (this._HtmlDocument.DocumentNode.SelectSingleNode("//div[@class='main-wrap region region-type-big']") != null) //Format A
                        setPropertiesForContactMetadataA();
                    else if (this._HtmlDocument.DocumentNode.SelectSingleNode("//body[@class='icbu-shop view contacts']") != null)    //Format B
                        setPropertiesForContactMetadataB();
                    else if (this._HtmlDocument.DocumentNode.SelectSingleNode(this._XPathToCheck.Key) != null)    //404 error
                        this.Errors[this.Status] = this._HtmlDocument.GetValue(this._XPathToCheck.Value);
                    else
                        this.Errors[this.Status] = "New content format";
                }
            }
            catch (System.Exception ex)
            {
                this.Errors[this.Status] = ex.Message;
            }
            return !this.HasError;
        }


        private void setPropertiesForContactMetadataA()
        {
            this.Status = "setPropertiesForContactMetadataA";
            try
            {
                HtmlNode node = this._HtmlDocument.DocumentNode.SelectSingleNode("//div[@id='contact-person']");
                if (node != null)
                {
                    this.ContactPhotoUrl = node.GetValue(".//div[@class='contact-picture']/img", "data-big-src");
                    this.ContactName = node.GetValue(".//div[@class='contact-info']/h1");
                    setPropertiesIds(node.GetValue(".//div[@class='supplier-atm']/a", "data-domdot"));
                }
                else
                {
                    this.Errors[this.Status] = "//div[@id='contact-person'] not found";
                }
                Dictionary<String, String> dic = this._HtmlDocument.GetValues2(
                    xPath: "//div[@class='contact-info']/dl",
                    xPathKey: ".//dt",
                    xPathValue: ".//dd");
                this.JobTitle = dic.GetValue("Job Title:");
                this.Department = dic.GetValue("Department:");
                dic = this._HtmlDocument.GetValues2(
                    xPath: "//div[@class='public-info']/dl",
                    xPathKey: ".//dt",
                    xPathValue: ".//dd");
                this.Address = dic.GetValue("Address:");
                this.Zip = dic.GetValue("Zip:");
                this.Country = dic.GetValue("Country/Region:");
                this.Province = dic.GetValue("Province/State:");
                this.City = dic.GetValue("City:");

                dic = this._HtmlDocument.GetValues(
                    xPath: "//table[@class='company-info-data table']/tbody/tr",
                    xPathKey: ".//th",
                    xPathValue: ".//td[2]");
                this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? dic.GetValue("Company Name:") : this.CompanyName;
                this.OperationalAddress = dic.GetValue("Operational Address:");
                this.Website = dic.GetValue("Website:");
                this.WebsiteAlibaba = dic.GetValue("Website on alibaba.com:");
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors[this.Status] = ex.Message;
            }
        }
        private void setPropertiesForContactMetadataB()
        {
            this.Status = "setPropertiesForContactMetadataB";
            try
            {
                //Left
                HtmlNode node = this._HtmlDocument.DocumentNode.SelectSingleNode("//div[@class='person-info']");
                if (node != null)
                {
                    this.ContactPhotoUrl = node.GetValue(".//div[@class='contact-image']/img", "src");
                    this.ContactName = node.GetValue(".//div[@class='contact-name']");
                    this.Department = node.GetValue(".//div[@class='contact-department']");
                    this.JobTitle = node.GetValue(".//div[@class='contact-job']");
                }
                else
                {
                    this.Errors[this.Status] = "//div[@class='person-info'] not found";
                }
                setPropertiesJson(this._HtmlDocument.GetValue("//div[@module-title='contactPerson']", "module-data"));

                //this._Driver.ClickElement(this._Wait, "//div[@class='sens-mask']/a[@class='icbu-link-default']");
                //Right
                Dictionary<String, String> dic = this._HtmlDocument.GetValues(
                    xPath: "//table[@class='info-table']/tr",
                    xPathKey: ".//th",
                    xPathValue: ".//td");
                this.Telephone = dic.GetValue("Telephone:");
                this.MobilePhone = dic.GetValue("Mobile Phone:");

                this.Address = dic.GetValue("Address:");
                this.Zip = dic.GetValue("Zip:");
                this.Country = dic.GetValue("Country/Region:");
                this.Province = dic.GetValue(@"Province/State:");
                this.City = dic.GetValue("City:");

                dic = this._HtmlDocument.GetValues(
                    xPath: "//table[@class='contact-table']/tr",
                    xPathKey: ".//th",
                    xPathValue: ".//td");

                this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? dic.GetValue("Company Name:") : this.CompanyName;
                this.OperationalAddress = dic.GetValue("Operational Address:");
                this.Website = dic.GetValue("Website:");
                this.WebsiteAlibaba = dic.GetValue("Website on alibaba.com:");
                this.WebsiteAlibaba = this.WebsiteAlibaba.StartsWith("https://") ? this.WebsiteAlibaba : String.Format("https://{0}", this.WebsiteAlibaba);
            }
            catch (System.Exception ex)
            {
                this.Errors[this.Status] = ex.Message;
            }
        }

        private Boolean setPropertiesForCompanyMetadata()
        {
            this.Status = "setPropertiesForCompanyMetadata";
            try
            {
                this.MetadataCompany = this.CompanyProfileUrl.DownloadString(this._Driver, xPathToCheck: this._XPathToCheck);
                this.CompanyProfileUrl = this._Driver == null ? this.CompanyProfileUrl : this._Driver.Url;
                if (StatusExtension.HasError || !String.IsNullOrEmpty(StatusExtension.ErrorMessage))
                {
                    this.Errors[this.Status] = StatusExtension.ErrorMessage;
                }
                else
                {
                    this._HtmlDocument.LoadHtml(this.MetadataCompany);
                    HtmlNode node = this._HtmlDocument.DocumentNode.SelectSingleNode("//div[@class='detail-verified']");
                    //this._HtmlDocument.Save(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\HTMLPageUV.html");
                    if (this._HtmlDocument.DocumentNode.SelectSingleNode("//div[@class='detail-verified']") != null) //verified
                        setPropertiesForCompanyMetadataVerified();
                    else if (this._HtmlDocument.DocumentNode.SelectSingleNode("//div[@class='information-content util-clearfix']") != null)    //Unverified
                        setpropertiesForCompanyMetadataUnverifiedA();
                    else if (this._HtmlDocument.DocumentNode.SelectSingleNode(this._XPathToCheck.Key) != null)    //404 error
                        this.Errors[this.Status] = this._HtmlDocument.GetValue(this._XPathToCheck.Value);
                    else
                        this.Errors[this.Status] = "New content format";
                }
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors[this.Status] = ex.Message;
            }
            return !this.HasError;
        }
        //private String downloadString(ref String url)
        //{
        //    this.Status = "downloadString";
        //    String metadata = url.DownloadString(this._Driver, this._XPathToCheck);
        //    if (StatusExtension.HasError || !String.IsNullOrEmpty(StatusExtension.ErrorMessage))
        //    {
        //        url = url.GetBaseUrl();
        //        metadata = url.DownloadString(this._Driver);
        //        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
        //        htmlDocument.LoadHtml(metadata);
        //        if (htmlDocument.DocumentNode.SelectSingleNode(xPathToCheck.Key) != null)
        //        {
        //            StatusExtension.ErrorMessage = String.IsNullOrEmpty(xPathToCheck.Value) ? "Not Available" : htmlDocument.GetValue(xPathToCheck.Value);
        //            html = String.Empty;
        //        }

        //    }
        //    else if (this._Driver != null)
        //        url = this._Driver.Url;
        //    if (StatusExtension.HasError || !String.IsNullOrEmpty(StatusExtension.ErrorMessage))
        //        this.Errors[this.Status] = StatusExtension.ErrorMessage;
        //    return metadata;
        //}
        private void setpropertiesForCompanyMetadataUnverifiedA()
        {
            this.CompanyName = this._HtmlDocument.GetValue("//div[@class='m-header']/h3");
            this.CompanyDescription = this._HtmlDocument.GetValue("//div[@class='company-description company-description-full']");
            setPropertiesIds(this._HtmlDocument.GetValue(".//div[@class='supplier-atm']/a", "data-domdot"));
            Dictionary<String, String> dic = this._HtmlDocument.GetValues(
                xPath: "//div[@class='information-content util-clearfix']/table[@class='content-table']/tr",
                xPathKey: ".//th",
                xPathValue: ".//td[1]");

            this.CompanyBusinessType = dic.GetValue("Business Type:");
            this.CompanyLocation = dic.GetValue("Location:");
            this.SupplierMainProducts = dic.GetValue("Main Products:");
            this.CompanyEstablishedYear = dic.GetValue("Year Established:");
            this.SupplierTotalAnnualSalesVolume = dic.GetValue("Total Annual Revenue:");
            this.CompanyNumberOfEmployees = dic.GetValue("Total Employees:");
            this.CompanyMainMarket = dic.GetValue("Top 3 Markets:");

            //this.ProductCertification = element.GetElementValue(".//table[@class='content-table']/tbody/tr[8]/td[1]/a");
        }
        private void setPropertiesForCompanyMetadataVerified()
        {
            setCompanyName();
            this.CompanyDescription = this._HtmlDocument.GetValue("//div[@class='desc-content icbu-clearfix']");
            Dictionary<String, String> dicVerified = this._HtmlDocument.GetValues(
                xPath: "//div[@class='detail-verified']/div",
                xPathKey: ".//div[@class='item-title']",
                xPathValue: ".//div[@class='item-value']");
            Dictionary<String, String> dicUnverified = this._HtmlDocument.GetValues(
                xPath: "//div[@class='detail-unverified']/div",
                xPathKey: ".//div[@class='item-title']",
                xPathValue: ".//div[@class='item-value']");
            setPropertiesForCompanyVerified(dicVerified, dicUnverified);
        }
        private void setCompanyName()
        {
            if (String.IsNullOrEmpty(this.CompanyName))
            {
                String companyName = this._HtmlDocument.GetValue("//div[@class='com-name']");
                if (String.IsNullOrEmpty(companyName))
                    companyName = this._HtmlDocument.GetValue("//span[@class='cp-name']");
                this.CompanyName = companyName;
            }
        }
        private void setPropertiesForCompanyVerified(Dictionary<String, String> dicVerified, Dictionary<String, String> dicUnverified)
        {
            Dictionary<String, String> dic = dicVerified.Concat(dicUnverified).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

            this.CompanyLocation = dic.GetValue("Location");
            this.SupplierMainProducts = dic.GetValue("Main Products");
            this.CompanyEstablishedYear = dic.GetValue("Year Established");
            this.SupplierTotalAnnualSalesVolume = dic.GetValue("Total Annual Revenue");
            this.ProductCertification = dic.GetValue("Certifications");
            this.CompanyBusinessType = dic.GetValue("Business Type");
            this.CompanyNumberOfEmployees = dic.GetValue("Total Employees");
            this.CompanyMainMarket = dic.GetValue("Top 3 Markets");
            //this.Ownership = dic.GetValue("Ownership");
            //this.ProductCertifications = dic.GetValue("Product Certifications(10)");
            //this.Patents = dic.GetValue("Patents(1)");
            //this.Trademarks = dic.GetValue("Trademarks(1)");
        }

        public SupplierInfo(ChromeDriver driver, WebDriverWait wait, String url, SupplierCategoryInfo category) : this()
        {
            this.Status = "SupplierInfo - I";
            this._Driver = driver;
            this._Wait = wait;
            this._Url = url;
            this._Category = category;
            this.CategoryId = this._Category.Id;
            this.CategoryName = this._Category.Name;
            this._Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(4);
            if (url.Contains("company_profile.html"))
            {
                setpropertiesForCompany(this._Url);
                if (checkContact())
                    setpropertiesForContact(this.ContactProfileUrl);
            }
            else if (url.Contains("contactinfo.html"))
            {
                setpropertiesForContact(this._Url);
                if (checkCompany())
                    setpropertiesForCompany(this.CompanyProfileUrl);
            }
            else
                this.Errors[this.Status] = "URL can be identified. ";
            this._Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            updateProperties();
        }
        #endregion
        #region Public Methods
        #endregion
        #region Private Methods
        private void setpropertiesForCompany(String url)
        {
            this.Status = "setpropertiesForCompany";
            if (url.IsWebSiteAvailable())
            {
                try
                {
                    this.CompanyProfileUrl = url;
                    using (WebClient client = new WebClient())
                    {
                        this.MetadataCompany = client.DownloadString(this.CompanyProfileUrl);
                    }
                    this._Driver.Navigate().GoToUrl(this.CompanyProfileUrl);
                    if (this._Driver.PageSource.Contains("detail-verified"))
                        setpropertiesForCompanyVerified();
                    else if (this._Driver.PageSource.Contains("segment layout layout-s5m0"))
                        setpropertiesForCompanyUnverifiedA();
                    else
                        this.Errors[this.Status] = "New content format";
                }
                //catch (TimeoutException ex) { throw ex; }
                catch (System.Exception ex)
                {
                    this.Errors[this.Status] = ex.Message;
                }
            }
            else
                this.Errors[this.Status] = "URL not available. ";
        }
        private Boolean checkCompany()
        {
            Boolean success = false;
            this.Status = "checkCompany";
            this.CompanyProfileUrl = "/company_profile.html".GetAbsoluteUrl(this.ContactProfileUrl, false);
            if (this.CompanyProfileUrl.IsWebSiteAvailable())
                success = true;
            else
                this.Errors[this.Status] = "Company URL not available. ";
            return success;
        }
        private void setpropertiesForContact(string url)
        {
            this.Status = "setpropertiesForContact";
            if (url.IsWebSiteAvailable())
            {
                try
                {
                    this.ContactProfileUrl = url;
                    using (WebClient client = new WebClient())
                    {
                        this.MetadataContact = client.DownloadString(this.ContactProfileUrl);
                    }
                    this._Driver.Navigate().GoToUrl(this.ContactProfileUrl);
                    if (this._Driver.PageSource.Contains("main-wrap region region-type-big"))
                        setpropertiesForContactA();
                    else if (this._Driver.PageSource.Contains("icbu-shop view contacts"))
                        setpropertiesForContactB();
                    else
                        this.Errors[this.Status] = "New content format";
                }
                catch (System.Exception ex)
                {
                    this.Errors[this.Status] = ex.Message;
                }
            }
            else
                this.Errors[this.Status] = "URL not available.";
        }
        private void setpropertiesForContactA()
        {
            this.Status = "setpropertiesForContactA";
            try
            {
                IWebElement element = this._Driver.FindElement(By.XPath("//div[@id='contact-person']"));
                this.ContactPhotoUrl = element.GetElementValue(".//div[@class='contact-picture']/img", "data-big-src");
                this.ContactName = element.GetElementValue(".//div[@class='contact-info']/h1");
                Dictionary<String, String> dic = this._Driver.GetElementValues2(
                    xPath: "//div[@class='contact-info']/dl",
                    xPathKey: ".//dt",
                    xPathValue: ".//dd");
                this.JobTitle = dic.GetValue("Job Title:");
                this.Department = dic.GetValue("Department:");
                setPropertiesIds(element.GetElementValue(".//div[@class='supplier-atm']/a", "data-domdot"));
                dic = this._Driver.GetElementValues2(
                    xPath: "//div[@class='public-info']/dl",
                    xPathKey: ".//dt",
                    xPathValue: ".//dd");
                this.Address = dic.GetValue("Address:");
                this.Zip = dic.GetValue("Zip:");
                this.Country = dic.GetValue("Country/Region:");
                this.Province = dic.GetValue("Province/State:");
                this.City = dic.GetValue("City:");

                dic = this._Driver.GetElementValues(
                    xPath: "//table[@class='company-info-data table']/tbody/tr",
                    xPathKey: ".//th",
                    xPathValue: ".//td[2]");
                this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? dic.GetValue("Company Name:") : this.CompanyName;
                this.OperationalAddress = dic.GetValue("Operational Address:");
                this.Website = dic.GetValue("Website:");
                this.WebsiteAlibaba = dic.GetValue("Website on alibaba.com:");
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors[this.Status] = ex.Message;
            }
        }
        private void setpropertiesForContactB()
        {
            this.Status = "setpropertiesForContactB";
            try
            {
                //Left
                IWebElement element = this._Driver.FindElement(By.XPath("//div[@class='person-info']"));
                this.ContactPhotoUrl = element.GetElementValue(".//div[@class='contact-image']/img", "src");
                this.ContactName = element.GetElementValue(".//div[@class='contact-name']");
                this.Department = element.GetElementValue(".//div[@class='contact-department']");
                this.JobTitle = element.GetElementValue(".//div[@class='contact-job']");
                setPropertiesJson(this._Driver.GetElementValue(this._Wait, "//div[@module-title='contactPerson']", "module-data"));

                //this._Driver.ClickElement(this._Wait, "//div[@class='sens-mask']/a[@class='icbu-link-default']");
                //Right
                Dictionary<String, String> dic = this._Driver.GetElementValues(
                    xPath: "//table[@class='info-table']/tr",
                    xPathKey: ".//th",
                    xPathValue: ".//td");
                this.Telephone = dic.GetValue("Telephone:");
                this.MobilePhone = dic.GetValue("Mobile Phone:");

                this.Address = dic.GetValue("Address:");
                this.Zip = dic.GetValue("Zip:");
                this.Country = dic.GetValue("Country/Region:");
                this.Province = dic.GetValue(@"Province/State:");
                this.City = dic.GetValue("City:");

                dic = this._Driver.GetElementValues(
                    xPath: "//table[@class='contact-table']/tr",
                    xPathKey: ".//th",
                    xPathValue: ".//td");

                this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? dic.GetValue("Company Name:") : this.CompanyName;
                this.OperationalAddress = dic.GetValue("Operational Address:");
                this.Website = dic.GetValue("Website:");
                this.WebsiteAlibaba = dic.GetValue("Website on alibaba.com:");
                this.WebsiteAlibaba = this.WebsiteAlibaba.StartsWith("https://") ? this.WebsiteAlibaba : String.Format("https://{0}", this.WebsiteAlibaba);
            }
            catch (System.Exception ex)
            {
                this.Errors[this.Status] = ex.Message;
            }
        }
        private void updateProperties()
        {
            this.Status = "updateProperties";
            if (String.IsNullOrEmpty(this.CompanyName))
                this.CompanyName = this._CompanyName;
            if (String.IsNullOrEmpty(this.WebsiteAlibaba))
                this.WebsiteAlibaba = this._Url.GetBaseUrl();
            if (String.IsNullOrEmpty(this.CompanyProfileUrl))
                this.CompanyProfileUrl = "/company_profile.html".GetAbsoluteUrl(this._Url.GetBaseUrl());
            if (String.IsNullOrEmpty(this.ContactProfileUrl))
                this.ContactProfileUrl = "/contactinfo.html".GetAbsoluteUrl(this._Url.GetBaseUrl());
            this.CompanyId = this.ContactPid;
            this.CompanyUrl = this.ContactProfileUrl.GetBaseUrl();

            if (String.IsNullOrEmpty(this.CompanyId))
                this.Errors[this.Status] = "Company Id not found.";
        }
        private void setpropertiesForCompanyUnverifiedA()
        {
            Dictionary<String, String> dic = this._Driver.GetElementValues(
                xPath: "//div[@class='information-content util-clearfix']/table[@class='content-table']/tbody/tr",
                xPathKey: ".//th",
                xPathValue: ".//td[1]");

            this.CompanyBusinessType = dic.GetValue("Business Type:");
            this.CompanyLocation = dic.GetValue("Location:");
            this.SupplierMainProducts = dic.GetValue("Main Products:");
            this.CompanyEstablishedYear = dic.GetValue("Year Established:");
            this.SupplierTotalAnnualSalesVolume = dic.GetValue("Total Annual Revenue:");
            this.CompanyNumberOfEmployees = dic.GetValue("Total Employees:");
            this.CompanyMainMarket = dic.GetValue("Top 3 Markets:");

            this.CompanyDescription = this._Driver.GetElementHiddenValue(".//div[@class='company-description company-description-full']");
            setPropertiesIds(this._Driver.GetElementValue(this._Wait, ".//div[@class='supplier-atm']/a", "data-domdot"));
            //this.ProductCertification = element.GetElementValue(".//table[@class='content-table']/tbody/tr[8]/td[1]/a");
        }
        private void setPropertiesIds(String ids)
        {
            Dictionary<String, String> dic = ids.GetValuesFromJson();
            this.ContactId = dic.ContainsKey("id") ? dic["id"] : String.Empty;
            this.ContactSid = dic.ContainsKey("sid") ? dic["sid"] : String.Empty;
            this.ContactPid = dic.ContainsKey("pid") ? dic["pid"] : String.Empty;
            this._CompanyName = dic.ContainsKey("mn") && dic["mn"].ToLower() != "contact information" ? dic["mn"] : this._CompanyName;
        }
        #endregion





        private void setpropertiesForCompanyVerified()
        {
            Dictionary<String, String> dicVerified = this._Driver.GetElementValues(
                xPath: "//div[@class='detail-verified']/div",
                xPathKey: ".//div[@class='item-title']",
                xPathValue: ".//div[@class='item-value']");
            Dictionary<String, String> dicUnverified = this._Driver.GetElementValues(
                xPath: "//div[@class='detail-unverified']/div",
                xPathKey: ".//div[@class='item-title']",
                xPathValue: ".//div[@class='item-value']");
            Dictionary<String, String> dic = dicVerified.Concat(dicUnverified).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

            this.CompanyLocation = dic.GetValue("Location");
            this.SupplierMainProducts = dic.GetValue("Main Products");
            this.CompanyEstablishedYear = dic.GetValue("Year Established");
            this.SupplierTotalAnnualSalesVolume = dic.GetValue("Total Annual Revenue");
            this.ProductCertification = dic.GetValue("Certifications");
            this.CompanyBusinessType = dic.GetValue("Business Type");
            this.CompanyNumberOfEmployees = dic.GetValue("Total Employees");
            this.CompanyMainMarket = dic.GetValue("Top 3 Markets");
            this.CompanyDescription = this._Driver.GetElementValue(this._Wait, "//div[@class='desc-content icbu-clearfix']");
            //this.Ownership = dic.GetValue("Ownership");
            //this.ProductCertifications = dic.GetValue("Product Certifications(10)");
            //this.Patents = dic.GetValue("Patents(1)");
            //this.Trademarks = dic.GetValue("Trademarks(1)");
            this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? this._Driver.GetElementValue(this._Wait, "//div[@class='com-name']") : this.CompanyName;
        }
        private Boolean checkContact()
        {
            Boolean success = false;
            this.Status = "checkContact";
            this.ContactProfileUrl = "/contactinfo.html".GetAbsoluteUrl(this.CompanyProfileUrl, false);
            if (this.ContactProfileUrl.IsWebSiteAvailable())
                success = true;
            else
                this.Errors[this.Status] = "Contact URL not available. ";
            return success;
        }
        public SupplierInfo(ChromeDriver driver, IWebElement element, SupplierCategoryInfo category, String classType) : this()
        {
            this.CategoryId = category.Id;
            this.CategoryName = category.Name;
            this.CompanyUrl = driver.Url;
            try
            {
                switch (classType)
                {
                    case "A":
                        setPropertiesContactOverviewA(element);
                        setPropertiesContactDetailA(element);
                        setPropertiesCompanyA(element);
                        break;
                    case "B":
                        setPropertiesContactOverviewB(element);
                        setPropertiesContactDetailB(element);
                        setPropertiesCompanyB(element);
                        break;
                }
                this.ContactProfileUrl = getWebsite(this.WebsiteAlibaba, "contactinfo.html");
                this.CompanyProfileUrl = getWebsite(this.WebsiteAlibaba, "company_profile.html");
                this.CompanyId = this.ContactPid;

                if (String.IsNullOrEmpty(this.CompanyId))
                    this.Errors.Add("L22 - SupplierInfo", "Company Id not found.");
                else if (String.IsNullOrEmpty(this.CompanyProfileUrl))
                    this.Errors.Add("L24 - SupplierInfo", "CompanyProfileUrl is empty");
                else if (this.CompanyProfileUrl.IsWebSiteAvailable())
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            this.MetadataCompany = client.DownloadString(this.CompanyProfileUrl);
                        }
                        driver.Navigate().GoToUrl(this.CompanyProfileUrl);
                        element = driver.FindElement(By.XPath("//div[@class='page-body']"));
                        Update(driver, element);
                    }
                    //catch (TimeoutException ex) { throw ex; }
                    catch (System.Exception ex)
                    {
                    }
                }
                //else
                //    this.Errors.Add("L23 - SupplierInfo", String.Format("CompanyProfileUrl not Available [{0}]", this.CompanyProfileUrl));
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L21 - SupplierInfo", ex.Message);
            }
        }

        private void setPropertiesCompanyB(IWebElement element)
        {
            try
            {
                Dictionary<String, String> dic = element.GetElementDictionaryValues(
                    xPathKey: ".//div[@class='mod-content']/div[@class='content']/table[@class='contact-table']/tr[{0}]/th/span[@class='title-text']",
                    xPathValue: ".//div[@class='mod-content']/div[@class='content']/table[@class='contact-table']/tr[{0}]/td",
                    count: 4);
                foreach (String key in dic.Keys)
                {
                    switch (key)
                    {
                        case "Company Name:":
                            this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? dic[key] : this.CompanyName;
                            break;
                        case "Operational Address:":
                            this.OperationalAddress = dic[key];
                            break;
                        case "Website on alibaba.com:":
                            this.WebsiteAlibaba = dic[key];
                            this.WebsiteAlibaba = this.WebsiteAlibaba.StartsWith("https://") ? this.WebsiteAlibaba : String.Format("https://{0}", this.WebsiteAlibaba);
                            break;
                        case "Website:":
                            this.Website = dic[key];
                            break;
                    }
                }
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L3 - setPropertiesCompanyB", ex.Message);
            }
        }

        private void setPropertiesContactDetailB(IWebElement element)
        {
            try
            {
                //this.Telephone = element.GetElementValue(".//table[@class='info-table']/tr/td", 0);
                //this.MobilePhone = element.GetElementValue(".//table[@class='info-table']/tr/td", 0);
                this.Address = element.GetElementValue(".//table[@class='info-table']/tr[3]/td");
                this.Zip = element.GetElementValue(".//table[@class='info-table']/tr[4]/td");
                this.Country = element.GetElementValue(".//table[@class='info-table']/tr[5]/td");
                this.Province = element.GetElementValue(".//table[@class='info-table']/tr[6]/td");
                this.City = element.GetElementValue(".//table[@class='info-table']/tr[7]/td");

            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L3 - setPropertiesContactDetailB", ex.Message);
            }
        }

        private void setPropertiesContactOverviewB(IWebElement element)
        {
            try
            {
                this.ContactPhotoUrl = element.GetElementValue(".//div[@class='contact-image']/img", "src");
                this.ContactName = element.GetElementValue(".//div[@class='contact-name']");
                this.JobTitle = element.GetElementValue(".//div[@class='contact-job']");
                setPropertiesJson(element.GetElementValue(".//div[@class='J_module  ']", "module-data"));

            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L3 - setPropertiesContactOverviewB", ex.Message);
            }
        }

        private void setPropertiesJson(String moduleData)
        {
            try
            {

                this.ModuleData = System.Net.WebUtility.UrlDecode(moduleData);
                JObject json = (JObject)JsonConvert.DeserializeObject(this.ModuleData);
                string str = json.ToString();
                if (json != null)
                {
                    this.ContactPid = json["mds"]["moduleData"]["data"]["companyId"].ToString().Trim();
                    this.ContactSid = json["gdc"]["siteId"].ToString().Trim();
                    this.ContactId = json["gdc"]["bizId"].ToString().Trim();
                }
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L4 - setPropertiesJson", ex.Message);
            }
        }
        private String getWebsite(String master, String sub)
        {
            return String.Format("{0}{1}{2}", master, master.EndsWith("/") ? "" : "/", sub);
        }

        internal void Update(ChromeDriver driver, IWebElement element)
        {
            try
            {
                this.CompanyBusinessType = element.GetElementValue(".//table[@class='content-table']/tbody/tr[1]/td[1]");
                this.CompanyLocation = element.GetElementValue(".//table[@class='content-table']/tbody/tr[2]/td[1]");
                this.SupplierMainProducts = element.GetElementValue(".//table[@class='content-table']/tbody/tr[3]/td[1]/a");
                this.CompanyNumberOfEmployees = element.GetElementValue(".//table[@class='content-table']/tbody/tr[4]/td[1]");
                this.SupplierTotalAnnualSalesVolume = element.GetElementValue(".//table[@class='content-table']/tbody/tr[5]/td[1]");
                this.CompanyEstablishedYear = element.GetElementValue(".//table[@class='content-table']/tbody/tr[6]/td[1]");
                this.CompanyMainMarket = element.GetElementValue(".//table[@class='content-table']/tbody/tr[7]/td[1]");
                this.ProductCertification = element.GetElementValue(".//table[@class='content-table']/tbody/tr[8]/td[1]/a");
                this.CompanyDescription = element.GetElementHiddenValue(driver, ".//table[@class='content-table']/tbody/tr[9]/td[1]/div[2]");

            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L2 - Update", ex.Message);
            }
        }

        private void setPropertiesCompanyA(IWebElement element)
        {
            try
            {
                Dictionary<String, String> dic = element.GetElementDictionaryValues(
                    xPathKey: ".//table[@class='company-info-data table']/tbody/tr[{0}]/th",
                    xPathValue: ".//table[@class='company-info-data table']/tbody/tr[{0}]/td[2]",
                    count: 4);
                foreach (String key in dic.Keys)
                {
                    switch (key)
                    {
                        case "Company Name:":
                            this.CompanyName = String.IsNullOrEmpty(this.CompanyName) ? dic[key] : this.CompanyName;
                            break;
                        case "Operational Address:":
                            this.OperationalAddress = dic[key];
                            break;
                        case "Website on alibaba.com:":
                            this.WebsiteAlibaba = dic[key];
                            this.WebsiteAlibaba = this.WebsiteAlibaba.StartsWith("https://") ? this.WebsiteAlibaba : String.Format("https://{0}", this.WebsiteAlibaba);
                            break;
                        case "Website:":
                            this.Website = dic[key];
                            break;

                    }

                }
            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L3 - setPropertiesCompanyA", ex.Message);
            }
        }

        private void setPropertiesContactDetailA(IWebElement element)
        {
            try
            {
                this.Telephone = element.GetElementValue(".//div[@class='sensitive-info hide-sensitive']/dl/dd[1]");
                this.MobilePhone = element.GetElementValue(".//div[@class='sensitive-info hide-sensitive']/dl/dd[2]");
                this.Fax = element.GetElementValue(".//div[@class='sensitive-info hide-sensitive']/dl/dd[3]");
                this.Address = element.GetElementValue(".//div[@class='public-info']/dl/dd[1]");
                this.Zip = element.GetElementValue(".//div[@class='public-info']/dl/dd[2]");
                this.Country = element.GetElementValue(".//div[@class='public-info']/dl/dd[3]");
                this.Province = element.GetElementValue(".//div[@class='public-info']/dl/dd[4]");
                this.City = element.GetElementValue(".//div[@class='public-info']/dl/dd[5]");

            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L3 - setPropertiesContactDetailA", ex.Message);
            }
        }

        private void setPropertiesContactOverviewA(IWebElement element)
        {
            try
            {
                this.ContactPhotoUrl = element.GetElementValue(".//div[@class='contact-picture']/img", "data-big-src");
                this.ContactName = element.GetElementValue(".//div[@class='contact-info']/h1");
                String first = element.GetElementValue(".//div[@class='contact-info']/dl[1]/dt");
                String second = element.GetElementValue(".//div[@class='contact-info']/dl[2]/dt");
                if (first.StartsWith("Job"))
                {
                    this.JobTitle = element.GetElementValue(".//div[@class='contact-info']/dl[1]/dd");
                    this.Department = element.GetElementValue(".//div[@class='contact-info']/dl[2]/dd");
                }
                else
                {
                    this.JobTitle = element.GetElementValue(".//div[@class='contact-info']/dl[2]/dd");
                    this.Department = element.GetElementValue(".//div[@class='contact-info']/dl[1]/dd");
                }
                setPropertiesIds(element.GetElementValue(".//div[@class='supplier-atm']/a", "data-domdot"));

            }
            //catch (TimeoutException ex) { throw ex; }
            catch (System.Exception ex)
            {
                this.Errors.Add("L3 - setPropertiesContactOverviewA", ex.Message);
            }
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@CompanyId",this.CompanyId),
                new KeyValuePair<String, Object>("@CompanyName",this.CompanyName),
                new KeyValuePair<String, Object>("@CompanyUrl",this.CompanyUrl),
                new KeyValuePair<String, Object>("@Website",this.Website),
                new KeyValuePair<String, Object>("@WebsiteAlibaba",this.WebsiteAlibaba),
                new KeyValuePair<String, Object>("@CompanyProfileUrl",this.CompanyProfileUrl),
                new KeyValuePair<String, Object>("@CompanyBusinessType",this.CompanyBusinessType),
                new KeyValuePair<String, Object>("@CompanyCreditLevel",this.CompanyCreditLevel),
                new KeyValuePair<String, Object>("@CompanyEstablishedYear",this.CompanyEstablishedYear),
                new KeyValuePair<String, Object>("@OperationalAddress",this.OperationalAddress),
                new KeyValuePair<String, Object>("@CompanyLocation",this.CompanyLocation),
                new KeyValuePair<String, Object>("@CompanyDescription",this.CompanyDescription),
                new KeyValuePair<String, Object>("@ProductCertification",this.ProductCertification),
                new KeyValuePair<String, Object>("@CategoryId",this.CategoryId),
                new KeyValuePair<String, Object>("@CategoryName",this.CategoryName),
                new KeyValuePair<String, Object>("@CategoryCode",this.CategoryCode),
                new KeyValuePair<String, Object>("@Address",this.Address),
                new KeyValuePair<String, Object>("@City",this.City),
                new KeyValuePair<String, Object>("@Province",this.Province),
                new KeyValuePair<String, Object>("@Zip",this.Zip),
                new KeyValuePair<String, Object>("@Country",this.Country),
                new KeyValuePair<String, Object>("@CountryCode",this.CountryCode),
                new KeyValuePair<String, Object>("@SupplierMainProducts",this.SupplierMainProducts),
                new KeyValuePair<String, Object>("@CompanyNumberOfEmployees",this.CompanyNumberOfEmployees),
                new KeyValuePair<String, Object>("@SupplierTotalAnnualSalesVolume",this.SupplierTotalAnnualSalesVolume),
                new KeyValuePair<String, Object>("@CompanyMainMarket",this.CompanyMainMarket),
                new KeyValuePair<String, Object>("@ContactName",this.ContactName),
                new KeyValuePair<String, Object>("@ContactProfileUrl",this.ContactProfileUrl),
                new KeyValuePair<String, Object>("@ContactPhotoUrl",this.ContactPhotoUrl),
                new KeyValuePair<String, Object>("@Department",this.Department),
                new KeyValuePair<String, Object>("@JobTitle",this.JobTitle),
                new KeyValuePair<String, Object>("@ContactId",this.ContactId),
                new KeyValuePair<String, Object>("@ContactSid",this.ContactSid),
                new KeyValuePair<String, Object>("@ContactPid",this.ContactPid),
                new KeyValuePair<String, Object>("@Telephone",this.Telephone),
                new KeyValuePair<String, Object>("@MobilePhone",this.MobilePhone),
                new KeyValuePair<String, Object>("@Fax",this.Fax),
                new KeyValuePair<String, Object>("@MetadataContact",this.MetadataContact),
                new KeyValuePair<String, Object>("@MetadataCompany",this.MetadataCompany)
            };
            return parameters;
        }

    }
    public class SupplierCategoryInfo
    {
        public Int32 Id { get; set; } = -1;
        public Int32 CountAdded { get; set; } = 0;
        public Int32 Count { get; set; } = -1;
        public Int32 Parenent { get; set; } = -1;
        public Int32 Level { get; set; } = -1;
        public Boolean Active { get; set; } = false;
        public String Name { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String UrlSupplier { get; set; } = String.Empty;
        public String Code { get; set; } = String.Empty;
        public String FullCode { get; set; } = String.Empty;
        public String PostCode { get; set; } = String.Empty;
        public String SID { get; set; } = String.Empty;
        public Dictionary<String, String> SubCategories { get; set; } = new Dictionary<String, String>();
        public DateTime AddedDate { get; set; } = new DateTime(1753, 1, 1);
        public DateTime PostedDate { get; set; } = new DateTime(1753, 1, 1);
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public Dictionary<String, String> Errors { get; set; } = new Dictionary<String, String>();
        public Boolean HasError { get { return this.Errors.Count > 0; } }
        public SupplierCategoryInfo() { }
        public SupplierCategoryInfo(Int32 id, String name, Int32 level) : this()
        {
            this.Id = id;
            this.Name = name;
            this.Level = level;
        }
        public SupplierCategoryInfo(SupplierCategoryInfo category, String name, String url, Boolean active) : this()
        {
            this.Name = name;
            this.Url = url;
            this.Level = category.Level + 1;
            this.Parenent = category.Id;
            this.Active = active;
            setSID();
        }
        public SupplierCategoryInfo(HtmlNode node, Int32 level, Int32 parent, String suuplierBaseUrl) : this()
        {
            this.Level = level;
            this.Parenent = parent;
            this.Active = true;
            switch (level)
            {
                case 1:
                    setLevel1(node);
                    break;
                case 2:
                    setLevel2(node);
                    this.UrlSupplier = $"{suuplierBaseUrl}{this.SID.Replace("p", "")}";
                    break;
                case 3:
                    setLevel3(node);
                    this.UrlSupplier = $"{suuplierBaseUrl}{this.SID.Replace("pid", "")}";
                    break;
            }
        }

        private void setLevel1(HtmlNode node)
        {
            this.Code = "P";
            this.SID = node.GetValue("", "data-spm");
            this.Name = node.GetValue(".//h3");
            this.Url = $"P[{this.SID}]{this.Name}";
        }

        private void setLevel2(HtmlNode node)
        {
            this.Code = "ID";
            this.Name = node.GetValue(".//h4/a");
            this.Url = node.GetValue(".//h4/a", "href");
            this.SID = getSID(node, ".//h4/a");
        }
        private String getSID(HtmlNode node, String xPath)
        {
            String sid = node.GetValue(xPath, "data-spm-anchor-id");
            if (String.IsNullOrEmpty(sid))
            {
                String[] items = this.Url.Split('_');
                sid = items[items.Length - 1];
            }
            return sid;
        }
        private void setLevel3(HtmlNode node)
        {
            this.Code = "PID";
            this.Name = node.GetValue(".//a");
            this.Url = node.GetValue(".//a", "href");
            this.SID = getSID(node, ".//a");
        }

        public SupplierCategoryInfo(ChromeDriver driver, WebDriverWait wait, IWebElement elementParent, SupplierCategoryInfo supplierCategoryParent, Boolean isDetail) : this()
        {
            if (elementParent != null)
            {
                this.Level = supplierCategoryParent.Level + 1;
                this.Parenent = supplierCategoryParent.Id;
                try
                {
                    if (isDetail)
                    {
                        this.Active = true;
                        this.Name = elementParent.Text.DecodeHtml();
                        this.Url = elementParent.GetAttribute("href").DecodeHtml();
                    }
                    else
                    {
                        this.Active = false;
                        this.Name = elementParent.GetElementValue(".//a");
                        this.Url = elementParent.GetElementValue(".//a", "href");
                    }
                    if (String.IsNullOrEmpty(this.Url))
                        this.Errors.Add("URL", "Empty");
                    else
                        setSID();
                }
                catch (System.Exception ex)
                {
                    this.Errors.Add("Exception", ex.Message);
                }
            }
        }
        //public SupplierCategoryInfo(ChromeDriver driver, WebDriverWait wait, IReadOnlyCollection<IWebElement> elements, SupplierCategoryInfo supplierCategoryParent) : this()
        //{
        //    if (elements == null)
        //    {
        //        this.Level = supplierCategoryParent.Level + 1;
        //        this.Parenent = supplierCategoryParent.Id;
        //        this.Active = false;
        //        try
        //        {
        //            foreach (IWebElement element in elements)
        //            {
        //                String url = element.GetAttribute("href").DecodeHtml();
        //                String name = element.Text.DecodeHtml();
        //                if (name != "..." && !String.IsNullOrEmpty(url) && !String.IsNullOrEmpty(name))
        //                    SubCategories[url] = name;
        //            }

        //        }
        //        catch (System.Exception ex)
        //        {
        //            this.Errors.Add("Exception", ex.Message);
        //        }
        //    }
        //}
        public SupplierCategoryInfo(ChromeDriver driver, WebDriverWait wait, IWebElement elementParent) : this()
        {
            this.Level = 1;
            this.Parenent = 0;
            this.Active = false;
            try
            {
                this.Name = elementParent.GetElementValue(".//dt/a");
                this.Url = elementParent.GetElementValue(".//dt/a", "href");
                if (String.IsNullOrEmpty(this.Url))
                    this.Errors.Add("URL", "Empty");
                else
                {
                    setSID();
                    IReadOnlyCollection<IWebElement> elements = elementParent.FindElements(By.XPath(".//dd/a"));
                    if (elements != null)
                    {
                        foreach (IWebElement element in elements)
                        {
                            String url = element.GetAttribute("href").DecodeHtml();
                            String name = element.Text.DecodeHtml();
                            if (name != "..." && !String.IsNullOrEmpty(url) && !String.IsNullOrEmpty(name))
                                SubCategories[url] = name;
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                this.Errors.Add("Exception", ex.Message);
            }
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@UrlSupplier",this.UrlSupplier),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@Level",this.Level),
                new KeyValuePair<String, Object>("@Parenent",this.Parenent),
                new KeyValuePair<String, Object>("@SID",this.SID),
                new KeyValuePair<String, Object>("@Code",this.Code),
                new KeyValuePair<String, Object>("@Active",this.Active)
            };
            return parameters;
        }
        private void setSID()
        {
            String[] arr = System.Text.RegularExpressions.Regex.Split(this.Url, "_sid");
            if (arr.Length == 2)
                this.Code = "SID";
            else
            {
                arr = System.Text.RegularExpressions.Regex.Split(this.Url, "_s");
                if (arr.Length == 2)
                    this.Code = "S";
                else
                {
                    arr = System.Text.RegularExpressions.Regex.Split(this.Url, "_pid");
                    if (arr.Length == 2)
                        this.Code = "PID";
                    else
                        this.Code = String.Empty;
                }
            }
            this.SID = arr.Length == 2 ? arr[1] : String.Empty;
        }
    }
    public class SQLResultStatusInfo
    {
        public String Code { get; set; } = String.Empty;
        public Int32 Id { get; set; } = -1;
        public String Status { get; set; } = String.Empty;
    }
    public class BusinessCardInfo
    {
        #region Public Properties
        public Int32 Id { get; set; } = 0;
        public String Name { get; set; } = String.Empty;
        public String Title { get; set; } = String.Empty;
        public String ContactUrl { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public String Tel { get; set; } = String.Empty;
        public String Fax { get; set; } = String.Empty;
        public String Mobile { get; set; } = String.Empty;
        public Image Photo { get; set; } = null;
        public String PhotoFileName { get; set; } = String.Empty;
        public String Website { get; set; } = String.Empty;
        public String SupplierId { get; set; } = String.Empty;
        public String Company { get; set; } = String.Empty;
        public String CompanyUrl { get; set; } = String.Empty;
        public String CompanyType { get; set; } = String.Empty;
        public String CountryCode { get; set; } = String.Empty;
        public String CountryName { get; set; } = String.Empty;
        public String Country { get; set; } = String.Empty;
        public String InteractionStatus { get; set; } = String.Empty;
        public DateTime ConnectionDate { get; set; } = new DateTime(1753, 1, 1);
        public String ConnectionType { get; set; } = String.Empty;
        public String ConnectStatus { get; set; } = String.Empty;
        public DateTime SentDate { get; set; } = new DateTime(1753, 1, 1);
        public Boolean IsRead { get; set; } = false;
        public String Account { get; set; } = String.Empty;
        public String ErrorMessage { get; set; } = String.Empty;
        public DateTime InputDateTime { get; set; } = new DateTime(1753, 1, 1);
        public Boolean Ignored { get; set; } = false;
        public String MemberId { get; set; } = String.Empty;
        public String Metadata { get; set; } = String.Empty;
        public String MetadataSent { get; set; } = String.Empty;
        public DateTime UpdatedDate { get; set; } = new DateTime(1753, 1, 1);
        public Boolean Checked { get; set; } = false;
        public Boolean IsSent { get; set; } = false;
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        #endregion


        public String CompanyName { get; set; } = String.Empty;
        public String CompanyProfileUrl { get; set; } = String.Empty;
        public String ContactName { get; set; } = String.Empty;
        public String AccountEmail { get; set; } = String.Empty;
        public DateTime ConnectedDate { get; set; } = new DateTime(1753, 1, 1);
        public SQLResultStatusInfo Result { get; set; } = new SQLResultStatusInfo();

        public String ProfileUrl { get; set; } = String.Empty;
        public String PhotoUrl { get; set; } = String.Empty;
        public String CountryDescription { get; set; } = String.Empty;

        public BusinessCardInfo() { }
        public BusinessCardInfo(ChromeDriver driver, IWebElement element, String account, String accountEmail, Boolean isSent)
        {
            this.Account = account;
            this.AccountEmail = accountEmail;
            this.IsSent = isSent;
            if (this.IsSent)
                setPropertiesForSent(driver, element);
            else
                setPropertiesForRecieved(driver, element);
        }

        private void setPropertiesForRecieved(ChromeDriver driver, IWebElement element)
        {
            this.Metadata = (String)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", element);
            this.IsRead = String.IsNullOrEmpty(element.GetAttribute("class").Replace("ui2-list-item list-item", "").Trim());
            this.MemberId = element.GetElementValue(".//div[@class='J-item-data f-hide']", "data-memberid");
            setConnection(element, ".//div[@class='time']");
            this.InteractionStatus = getInteractionStatus(element);
            setPropertiesForRecievedCardFront(element.FindElement(By.XPath(".//div[@class='card-front']")));
            setPropertiesForRecievedCardBack(element.FindElement(By.XPath(".//div[@class='card-back']/div/div[@class='desc']")));
        }

        private void setPropertiesForRecievedCardBack(IWebElement element)
        {
            this.ContactName = String.IsNullOrEmpty(this.ContactName) ? element.GetElementValue(".//div[@class='name']") : this.ContactName;
            this.Email = String.IsNullOrEmpty(this.Email) ? element.GetElementValue(".//div[2]/span[@class='val']", "title") : this.Email;
            this.Tel = String.IsNullOrEmpty(this.Tel) ? element.GetElementValue(".//div[3]/span[@class='val']", "title") : this.Tel;
            this.Fax = String.IsNullOrEmpty(this.Fax) ? element.GetElementValue(".//div[4]/span[@class='val']", "title") : this.Fax;
            this.Mobile = String.IsNullOrEmpty(this.Mobile) ? element.GetElementValue(".//div[5]/span[@class='val']", "title") : this.Mobile;
            this.Website = String.IsNullOrEmpty(this.Website) ? element.GetElementValue(".//div[6]/span[@class='val']", "title") : this.Website;
        }

        private void setPropertiesForRecievedCardFront(IWebElement element)
        {
            this.ProfileUrl = element.GetElementValue(".//a[@class='imgbox']", "href");
            this.PhotoUrl = element.GetElementValue(".//a[@class='imgbox']/img", "src");
            if (!String.IsNullOrEmpty(this.PhotoUrl))
                this.Photo = this.PhotoUrl.GetImageFromUrl();
            this.ContactName = element.GetElementValue(".//div[@class='desc']/a[@class='name']");
            this.Title = element.GetElementValue(".//div[@class='desc']/div[@class='title']", "title");
            this.Email = element.GetElementValue(".//div[@class='desc']/div[@class='email']", "title");
            this.CompanyName = element.GetElementValue(".//div[@class='desc']/a[@class='company']", "title");
            this.CompanyProfileUrl = element.GetElementValue(".//div[@class='desc']/a[@class='company']", "href");
            this.Country = element.GetElementValue(".//div[@class='bi-icon']/span[2]", "data-country");
            this.CountryDescription = element.GetElementValue(".//div[@class='bi-icon']/span[2]", "title");
            this.CountryCode = element.GetElementValue(".//div[@class='bi-icon']/span[2]", "data-country-sp");
            this.CompanyType = element.GetElementValue(".//div[@class='bi-icon']/span[@class='type']", "title");
        }

        private void setPropertiesForSent(ChromeDriver driver, IWebElement element)
        {
            this.MetadataSent = (String)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].outerHTML;", element);
            this.MemberId = element.GetElementValue(".//div[@class='J-item-data f-hide']", "data-memberid");
            this.ProfileUrl = element.GetElementValue(".//a[@class='avatar']", "href");
            this.PhotoUrl = element.GetElementValue(".//div[@class='imgbox']/img", "src");
            if (!String.IsNullOrEmpty(this.PhotoUrl))
                this.Photo = this.PhotoUrl.GetImageFromUrl();
            this.ContactName = element.GetElementValue(".//a[@class='name']");
            this.Title = element.GetElementValue(".//div[@class='desc']/div[@class='title']", "title");
            this.CompanyName = element.GetElementValue(".//a[@class='company']");
            this.CompanyProfileUrl = element.GetElementValue(".//a[@class='company']", "href");
            this.ConnectStatus = element.GetElementValue(".//div[@data-role='history']", "class").GetLastWord();
            this.InteractionStatus = getInteractionStatus(element);
            setConnection(element, ".//div[@class='time']");
        }
        private void setConnection(IWebElement element, String xPath)
        {
            String time = element.FindElement(By.XPath(xPath)).Text;
            String[] times = time.Split(new string[] { "  " }, StringSplitOptions.None);
            if (this.IsSent)
                this.SentDate = times[0].DecodeHtml().ConvertToDate();
            else
                this.ConnectedDate = times[0].DecodeHtml().ConvertToDate();
            if (times.Length > 1)
                this.ConnectionType = times[1].DecodeHtml();
        }
        private String getInteractionStatus(IWebElement elementParaent)
        {
            String interactionStatus = String.Empty;
            try
            {
                IWebElement element = elementParaent.FindElement(By.XPath(".//div[@data-role='history']/div[@class='connected-before']/a"));
                if (element.Displayed)
                    interactionStatus = element.Text;
                else
                {
                    element = elementParaent.FindElement(By.XPath(".//div[@data-role='history']/div[@class='unconnected-before']"));
                    if (element.Displayed)
                        interactionStatus = element.Text;
                }
                interactionStatus = interactionStatus.DecodeHtml();
            }
            catch (System.Exception)
            {
            }
            return interactionStatus;
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@MemberId",this.MemberId),
                new KeyValuePair<String, Object>("@ContactName",this.ContactName),
                new KeyValuePair<String, Object>("@Title",this.Title),
                new KeyValuePair<String, Object>("@ProfileUrl",this.ProfileUrl),
                new KeyValuePair<String, Object>("@Email",this.Email),
                new KeyValuePair<String, Object>("@Tel",this.Tel),
                new KeyValuePair<String, Object>("@Fax",this.Fax),
                new KeyValuePair<String, Object>("@Mobile",this.Mobile),
                //new KeyValuePair<String, Object>("@Photo",this.Photo),
                new KeyValuePair<String, Object>("@PhotoUrl",this.PhotoUrl),
                new KeyValuePair<String, Object>("@Website",this.Website),
                new KeyValuePair<String, Object>("@CompanyName",this.CompanyName),
                new KeyValuePair<String, Object>("@CompanyProfileUrl",this.CompanyProfileUrl),
                new KeyValuePair<String, Object>("@CompanyType",this.CompanyType),
                new KeyValuePair<String, Object>("@CountryCode",this.CountryCode),
                new KeyValuePair<String, Object>("@Country",this.Country),
                new KeyValuePair<String, Object>("@CountryDescription",this.CountryDescription),
                new KeyValuePair<String, Object>("@ConnectedDate",this.ConnectedDate),
                new KeyValuePair<String, Object>("@ConnectionType",this.ConnectionType),
                new KeyValuePair<String, Object>("@Account",this.Account),
                new KeyValuePair<String, Object>("@AccountEmail",this.AccountEmail),
                new KeyValuePair<String, Object>("@InteractionStatus",this.InteractionStatus),
                new KeyValuePair<String, Object>("@ConnectStatus",this.ConnectStatus),
                new KeyValuePair<String, Object>("@SentDate",this.SentDate),
                new KeyValuePair<String, Object>("@IsRead",this.IsRead),
                new KeyValuePair<String, Object>("@Metadata",this.Metadata),
                new KeyValuePair<String, Object>("@MetadataSent",this.MetadataSent),
                new KeyValuePair<String, Object>("@IsSent",this.IsSent)
            };
            return parameters;
        }
    }
    public class SettingsInfo
    {
        public Int32 Id { get; set; } = -1;
        public Int32 OpenPages { get; set; } = -1;
        public String Email { get; set; } = String.Empty;
        public String Password { get; set; } = String.Empty;
        public String FilterEmail { get; set; } = String.Empty;
        public String Flag { get; set; } = String.Empty;
        public String UrlMessage { get; set; } = String.Empty;
        public String UrlAlibaba { get; set; } = String.Empty;
        public String UrlSent { get; set; } = String.Empty;
        public String UrlReceivied { get; set; } = String.Empty;
        public DateTime LastRunTime { get; set; } = new DateTime(1800, 1, 1);
        public DateTime LastRunTimeEmail { get; set; } = new DateTime(1800, 1, 1);
        public DateTime LastEmailRetrived { get; set; } = new DateTime(1800, 1, 1);
        public DateTime AddedDate { get; set; } = new DateTime(1800, 1, 1);
        public String Account { get; set; } = String.Empty;
        public Boolean Closed { get; set; } = false;
        public Boolean CheckAllPages { get; set; } = false;
        public SettingsInfo() { }
    }

    public class WebDriverSettingInfo
    {
        public Boolean HideError { get; set; } = true;
        public Boolean HideBrowser { get; set; } = false;
        public Boolean HideCommand { get; set; } = false;
        public WebDriverSettingInfo() { }
        public WebDriverSettingInfo(Boolean hideCommand, Boolean hideBrowser, Boolean hideError = true) : this()
        {
            this.HideBrowser = hideBrowser;
            this.HideCommand = hideCommand;
            this.HideError = hideError;
        }
    }
}
