/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.05.14
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
using System.Net;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using HtmlAgilityPack;

namespace Utilities
{
    public static class WebExtension
    {
        public static String GetBaseUrl(this String url,String scheme)
        {
            if (!url.StartsWith("http"))
                url = $"{scheme}://{url}";
            String newUrl = String.Empty;
            try
            {
                Uri uri = new Uri(url);
                newUrl = $"{uri.Scheme}://{uri.Host}";
            }
            catch (Exception) { }
            return newUrl;
        }
        public static String GetUrlScheme(this String url)
        {
            String scheme = String.Empty;
            try
            {
                Uri uri = new Uri(url);
                scheme = uri.Scheme;
            }
            catch (Exception) { }
            return scheme;
        }
        public static String GetValidUrl(this String url, Boolean isSimpleScheme)
        {
            url = url.Split(' ')[0];
            if (!url.IsValidUrl())
                if (!url.Contains("http://") && !url.Contains("https://"))
                {
                    url = isSimpleScheme ? $"http://{url}" : $"https://{url}";
                }
            return url;
        }
        public static Boolean IsValidUrl(this String url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return false;
            Uri tmp;
            if (!Uri.TryCreate(url, UriKind.Absolute, out tmp))
                return false;
            return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
        }

        public static String DownloadString(this String url, ChromeDriver driver = null, KeyValuePair<String, String> xPathToCheck = new KeyValuePair<String, String>(), Int32 secondsWait = 2)
        {
            StatusExtension.Initialize();
            String html = "";
            if (url.IsValidUrl())
                try
                {
                    if (driver == null)
                        using (WebClient client = new WebClient())
                            html = client.DownloadString(url);
                    else if (url.IsWebSiteAvailable())
                    {
                        driver.Navigate().GoToUrl(url);
                        IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                        js.ExecuteScript("window.scrollTo(0, 100)");
                        System.Threading.Thread.Sleep(secondsWait * 1000);
                        html = driver.PageSource;
                    }
                    else StatusExtension.ErrorMessage = $"Url not available.\n{StatusExtension.ErrorMessage}";
                    if (!String.IsNullOrEmpty(html) && !String.IsNullOrEmpty(xPathToCheck.Key))
                    {
                        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                        htmlDocument.LoadHtml(html);
                        if (htmlDocument.DocumentNode.SelectSingleNode(xPathToCheck.Key) != null)
                        {
                            StatusExtension.ErrorMessage = String.IsNullOrEmpty(xPathToCheck.Value) ? "Not Available" : htmlDocument.GetValue(xPathToCheck.Value);
                            html = String.Empty;
                        }
                    }
                }
                catch (Exception ex) { StatusExtension.ErrorMessage = ex.Message; }
            else StatusExtension.ErrorMessage = $"Url not valid.";
            return html;
        }
        public static String GetBaseUrl(this String url)
        {
            Boolean success = url.StartsWith("http");
            String newURL = String.Empty;
            try
            {
                if (!success)
                    url = $"http://{url}";
                Uri uri = new Uri(url);
                newURL = $"{uri.Scheme}://{uri.Host}";
                if (!success)
                    newURL = newURL.Replace("http://","");
            }
            catch (Exception) { }
            return newURL;
        }
        public static String GetAbsoluteUrl(this String url, String baseUrl, Boolean isBaseUrl = true)
        {
            String newURL = String.Empty;
            try
            {
                if (isBaseUrl)
                {
                    Uri uri = new Uri(new Uri(baseUrl), url);
                    newURL = uri.AbsoluteUri;
                }
                else
                {
                    string str = baseUrl.Substring(0, baseUrl.Length - baseUrl.Split('/').Last().Length - 1);
                    newURL = $"{str}{url}";
                }
            }
            catch (Exception) { }
            return newURL;
        }
        public static Image GetImageFromUrl(this String url)
        {
            Image image = null;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    byte[] data = webClient.DownloadData(url);

                    using (MemoryStream memoryStream = new MemoryStream(data))
                    {
                        image = Image.FromStream(memoryStream);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return image;
        }
        public static String DecodeHtml(this String html, Boolean removeDoubleSpace = true, Boolean removeReturn = true)
        {
            html = System.Web.HttpUtility.HtmlDecode(html.Trim());
            html = Regex.Replace(html, "\x00A0", Convert.ToChar(13).ToString());
            if (removeReturn)
            {
                html = html.Replace("\r", "");
                html = html.Replace("\n", "");
            }
            //html = Regex.Replace(html, @"\p{Zs}", "");
            if (removeDoubleSpace)
                html = html.Replace("  ", "").Replace("??", "").Replace("  ", "");
            return html;
        }
        public static String DecodeUrl(this String url)
        {
            url = System.Web.HttpUtility.UrlDecode(url.Trim());
            return url;
        }

        public static Boolean IsWebSiteAvailable(this String url)
        {
            StatusExtension.Initialize();
            Boolean success = false;
            if (url.IsValidUrl())
                try
                {

                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                    request.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    request.Method = "GET";
                    request.Timeout = 30 * 1000;
                    HttpWebResponse response = null;
                    HttpStatusCode code;
                    try
                    {
                        using (response = (HttpWebResponse)request.GetResponse())
                        {
                            code = response.StatusCode;
                            success = true;
                            //var stream = response.GetResponseStream();
                            //var sr = new StreamReader(stream);
                            //var content = sr.ReadToEnd();
                            //System.IO.File.WriteAllText(@"C:\AndrewHuang\Development\Visual Studio 2015\Projects\QwayInc\TestConsoleApplication\TestFiles\Contact.html", content);
                        }
                    }
                    catch (WebException ex)
                    {
                        if (ex.Response != null)
                        {
                            response = ex.Response as HttpWebResponse;
                            code = response.StatusCode;
                            StatusExtension.ErrorMessage = String.Format("[{0}] {1}", code, ex.Message);
                            success = code == HttpStatusCode.OK;
                        }
                        else
                            StatusExtension.ErrorMessage = ex.Message;
                    }
                }
                catch (Exception ex)
                {
                    StatusExtension.ErrorMessage = ex.Message;
                }
            else
                StatusExtension.ErrorMessage = $"Url is not valid.";
            return success;
        }
    }
}
