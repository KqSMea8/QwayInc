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
using System.Data;
using System.Drawing;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;

using Utilities;

using System.Text.RegularExpressions;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Google.Apis.Customsearch.v1.Data;

namespace Google
{
    public class EmailInfo
    {
        public Int32 Id { get; set; } = -1;
        public String Code { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public String CompanyName { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }

        public EmailInfo() { }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Code",this.Code),
                new KeyValuePair<String, Object>("@Comments",this.Comments)
            };
            return parameters;
        }
    }
    public class EmailSettingInfo
    {
        public Int32 Id { get; set; } = -1;
        public Int32 SendPercentage { get; set; } = 100;
        public String Code { get; set; } = String.Empty;
        public String Subject { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String FileName { get; set; } = String.Empty;
        public String FileFullName { get { return System.IO.Path.Combine(@"./Resources", $"{this.FileName}"); } }
        public EmailSettingInfo() { }
    }
    public class KeyWordDetailInfo
    {
        public static Dictionary<String, Boolean> ClickKeys = new Dictionary<String, Boolean>()
        {
            { "contact us",false },
            { "contact-us",false },
            { "contactus",false },
            { "contact",true },
            { "about us",false },
        };
        private const String PATTERN = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        private String _Domains = "hotmail.com;gmail.com;yahoo.com;outlook.com";
        private List<String> _EmailEnds = new List<String>() { ".com", ".net", ".ca", ".org" };
        private Int32 _EmailFound = 0;
        public Int32 Id { get; set; } = -1;
        public static Int32 TimeoutCount { get; set; } = 0;
        public String Website { get; set; } = String.Empty;
        public String SearchCode { get; set; } = String.Empty;
        public String SearchKey { get; set; } = String.Empty;
        public String Url { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String Email { get { return getEmail(); } }
        public String Description { get; set; } = String.Empty;
        public String VedCode { get; set; } = String.Empty;
        public Int32 Page { get; set; } = -1;
        public Boolean IsAds { get; set; } = false;
        public String Domain { get; set; } = String.Empty;
        public String Comments { get; set; } = String.Empty;
        public String UsedWebsite { get; set; } = String.Empty;
        public String CurrentWebsite { get; set; } = String.Empty;
        public String DataPreconnectUrls { get; set; } = String.Empty;

        public Dictionary<String, String> Emails { get; set; } = new Dictionary<String, String>();
        public Dictionary<String, String> Veds = new Dictionary<String, String>();
        public DataTable VedsDataTable { get { return GetVedsDataTable(this.Veds); } }
        public String ErrorMessage { get { return getErrorMessage(); } }
        public Boolean HasError { get { return !String.IsNullOrEmpty(this.ErrorMessage); } }
        public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public List<KeyValuePair<String, Object>> SQLParametersAfter { get { return getSQLParametersAfter(); } }
        public KeyWordDetailInfo() { }
        public KeyWordDetailInfo(Result item, KeyWordInfo keyWord, Boolean isAds, Int32 page) : this()
        {
            this.IsAds = isAds;
            this.SearchCode = keyWord.Code;
            this.SearchKey = keyWord.KeyWordToSearch;
            this.Page = page;
            this.Name = item.Title;
            this.Url = item.HtmlFormattedUrl;
            this.Website = item.Link;
            this.Description = item.Snippet;
            updateWebsite();
        }
        public KeyWordDetailInfo(ChromeDriver driver, IWebElement element, KeyWordInfo keyWord, Boolean isAds, Int32 page) : this()
        {
            this.IsAds = isAds;
            this.SearchCode = keyWord.Code;
            this.SearchKey = keyWord.KeyWordToSearch;
            this.Page = page;
            if (this.IsAds)
                setPropertiesAds(driver, element);
            else
                setProperties(driver, element);
            updateWebsite();
        }

        private void updateWebsite()
        {
            if (String.IsNullOrEmpty(this.Website))
                this.Website = String.IsNullOrEmpty(this.DataPreconnectUrls) ? this.Url : this.DataPreconnectUrls;
            updateWebsite2();
            if (String.IsNullOrEmpty(this.Website))
            {
                this.Website = String.IsNullOrEmpty(this.DataPreconnectUrls) ? this.Url : this.DataPreconnectUrls;
                updateWebsite2();
            }
            if (this.Website.Right(1) == "/")
            {
                String mm = "AAA";
            }
        }
        private void updateWebsite2()
        {
            this.Website = this.Website.Trim().Split(' ')[0].Trim();
            String scheme = this.DataPreconnectUrls.GetUrlScheme();
            if (String.IsNullOrEmpty(scheme))
                scheme = this.Url.GetUrlScheme();
            this.Website = this.Website.GetBaseUrl(scheme);
        }

        internal void Update(ChromeDriver driver)
        {
            String metadata = driver.PageSource;
            if (metadata.Contains("@"))
            {
                try
                {
                    //Regex regex = new Regex(PATTERN);
                    //MatchCollection matches = regex.Matches(metadata, RegexOptions.Compiled);
                    MatchCollection matches = Regex.Matches(metadata, PATTERN, RegexOptions.Compiled, new TimeSpan(0, 1, 0));
                    Int32 count = matches.Count;
                    foreach (Match match in matches)
                    {
                        String email = match.Value.ToLower();
                        if (isEmailValid(email))
                            this.Emails[email] = email.GetDomain();
                        if (this.Emails.Count > 10)
                            break;
                    }
                }
                catch (System.Text.RegularExpressions.RegexMatchTimeoutException)
                {
                    this.Comments = $"Can't resolve email";
                }
            }
            if (this.Emails.Count > this._EmailFound)
            {
                this._EmailFound = this.Emails.Count;
                this.CurrentWebsite = driver.Url;
                if (String.IsNullOrEmpty(this.Domain))
                    this.Domain = (new Uri(this.CurrentWebsite)).Host.ToLower().Replace("www.", "");
            }
        }

        private bool isEmailValid(String email)
        {
            Boolean success = false;
            foreach (String emailEnd in this._EmailEnds)
                if (email.ToLower().EndsWith(emailEnd))
                {
                    success = true;
                    break;
                }
            return success;
        }

        private String getEmail()
        {
            List<String> list = this.Emails.Where(kvp => kvp.Value == this.Domain).Select(kvp => kvp.Key).ToList<String>();
            if (list.Count == 0)
            {
                list = this.Emails.Where(kvp => this._Domains.Contains(kvp.Value)).Select(kvp => kvp.Key).ToList<String>();
                if (list.Count == 0)
                    list = this.Emails.Keys.ToList<String>();
            }
            return list.Count > 0 ? String.Join(";", list) : String.Empty;
        }

        private void setProperties(ChromeDriver driver, IWebElement element)
        {
            String returnXPath = String.Empty;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            this.VedCode = element.GetValue("", "data-ved");
            this.Name = getValue(element, new List<String>() { ".//div/a/h3", ".//div/h3/a[last()]" }, "", ref returnXPath);
            List<String> list = new List<String>() { ".//div/a/h3", ".//div/a[last()]", ".//div/a[last()]/h3", ".//div/h3/a[last()]" };
            this.Url = getValue(element, list, "href", ref returnXPath);
            this.DataPreconnectUrls = getValue(element, list, "data-preconnect-Urls", ref returnXPath);
            this.Website = element.GetValue(".//cite", "");
            this.Description = getValue(element, new List<String>() { ".//span[@class='st']" }, "", ref returnXPath);
            setVeds(driver, element, ".//div/div[last()]/div/div/div");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
        }
        private void setVeds(ChromeDriver driver, IWebElement element, String xPath)
        {
            ReadOnlyCollection<IWebElement> elements = null;
            if (driver.GetElements(xPath, ref elements))
                foreach (IWebElement ele in elements.Take(10))
                    this.Veds[ele.GetValue("", "data-ved")] = ele.GetValue("", "", driver);
        }

        private void setPropertiesAds(ChromeDriver driver, IWebElement element)
        {
            String returnXPath = String.Empty;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            this.Name = getValue(element, new List<String>() { ".//div/a/h3", ".//div/h3/a[last()]" }, "", ref returnXPath);
            List<String> list = new List<String>() { ".//div/a/h3", ".//div/a[last()]", ".//div/h3/a[last()]" };
            this.Url = getValue(element, list, "href", ref returnXPath);
            this.DataPreconnectUrls = getValue(element, list, "data-preconnect-urls", ref returnXPath);
            this.Website = element.GetValue(".//cite", "");
            this.Description = getValue(element, new List<String>() { "div[2]", "div[3]", "div[last()]" }, "", ref returnXPath);
            this.VedCode = element.GetValue(".//div[last()]/div", "data-ved");
            setVeds(driver, element, ".//div[last()]/div/div/div");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
        }

        private String getValue(IWebElement element, List<String> xPaths, String attribute, ref String returnXPath)
        {
            String value = String.Empty;
            foreach (String xPath in xPaths)
            {
                value = element.GetValue(xPath, attribute);
                if (!String.IsNullOrEmpty(value))
                {
                    returnXPath = xPath;
                    break;
                }
            }
            return value;
        }
        private void setValue(IWebElement element, List<String> xPaths)
        {
            String returnXPath = String.Empty;
            this.Name = getValue(element, xPaths, "", ref returnXPath);
            if (!String.IsNullOrEmpty(this.Name))
                this.Url = element.GetValue(returnXPath, "href");
        }
        private List<KeyValuePair<String, Object>> getSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@SearchCode",this.SearchCode),
                new KeyValuePair<String, Object>("@SearchKey",this.SearchKey),
                new KeyValuePair<String, Object>("@Url",this.Url),
                new KeyValuePair<String, Object>("@Website",this.Website),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@DataPreconnectUrls",this.DataPreconnectUrls),
                new KeyValuePair<String, Object>("@Description",this.Description),
                new KeyValuePair<String, Object>("@VedCode",this.VedCode),
                new KeyValuePair<String, Object>("@Page",this.Page),
                new KeyValuePair<String, Object>("@IsAds",this.IsAds),
                new KeyValuePair<String, Object>("@Veds",this.VedsDataTable),
            };
            return parameters;
        }
        private List<KeyValuePair<String, Object>> getSQLParametersAfter()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Website",this.Website),
                new KeyValuePair<String, Object>("@Email",this.Email),
                new KeyValuePair<String, Object>("@Domain",this.Domain),
                new KeyValuePair<String, Object>("@Comments",this.Comments),
            };
            return parameters;
        }
        public static DataTable GetVedsDataTable(Dictionary<String, String> veds)
        {
            DataTable dt = new DataTable("Ved");
            dt.Columns.Add("Key", typeof(String));
            dt.Columns.Add("Value", typeof(String));
            foreach (String key in veds.Keys)
            {
                if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(veds[key]))
                {
                    DataRow row = dt.NewRow();
                    row["Key"] = key;
                    row["Value"] = veds[key].Substring(0, Math.Min(veds[key].Length, 200));
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        private string getErrorMessage()
        {
            StringBuilder sb = new StringBuilder();
            if (String.IsNullOrEmpty(this.Website))
                sb.AppendLine($"Website is empty.");
            //if (String.IsNullOrEmpty(this.Url))
            //    sb.AppendLine($"Url is empty.");
            //if (String.IsNullOrEmpty(this.Name))
            //    sb.AppendLine($"Name is empty.");
            ////var list = this.Veds.Where(kvp => !String.IsNullOrEmpty(kvp.Key) && !String.IsNullOrEmpty(kvp.Value));
            ////if (list == null || list.Count() == 0)
            ////    sb.AppendLine($"Veds is empty.");
            //var list = this.Veds.Where(kvp => String.IsNullOrEmpty(kvp.Key) && !String.IsNullOrEmpty(kvp.Value));
            //if (list != null && list.Count() > 0)
            //    sb.AppendLine($"Check Veds.");
            return sb.ToString();
        }

    }
    public class KeyWordInfo
    {
        public Int32 CategoryId { get; set; } = -1;
        public Int32 KeyWordId { get; set; } = -1;
        public String Code { get; set; } = String.Empty;
        public String Category { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public Int32 Pages { get; set; } = -1;
        public String KeyWord { get; set; } = String.Empty;
        public String KeyWordToSearch { get; set; } = String.Empty;
        public Boolean Distributor { get; set; } = false;
        public Boolean IsCompleted { get; set; } = false;
        //public List<KeyValuePair<String, Object>> SQLParameters { get { return getSQLParameters(); } }
        public List<KeyValuePair<String, Object>> SQLParametersForHistory { get { return getSQLParametersForHistory(); } }
        public KeyWordInfo() { }
        private List<KeyValuePair<String, Object>> getSQLParametersForHistory()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",this.Code),
                new KeyValuePair<String, Object>("@KeyWordToSearch",this.KeyWordToSearch),
                new KeyValuePair<String, Object>("@Pages",this.Pages),
            };
            return parameters;
        }
    }
}
