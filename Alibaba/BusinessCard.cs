/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.04.26
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
using Utilities;
using System.IO;
using System.Data;

namespace Alibaba
{
    public class BusinessCard : IDisposable
    {
        String _FolderName;
        public String MemberId { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String FileName { get; set; } = String.Empty;
        public String Title { get; set; } = String.Empty;
        public String ContactUrl { get; set; } = String.Empty;
        public String Email { get; set; } = String.Empty;
        public String Tel { get; set; } = String.Empty;
        public String Fax { get; set; } = String.Empty;
        public String Mobile { get; set; } = String.Empty;
        public Image Photo { get; set; } = null;
        public String PhotoFileName { get; set; } = null;
        public String Website { get; set; } = String.Empty;
        public String Company { get; set; } = String.Empty;
        public String CompanyUrl { get; set; } = String.Empty;
        public String CompanyType { get; set; } = String.Empty;
        public String CountryCode { get; set; } = String.Empty;
        public String CountryName { get; set; } = String.Empty;
        public String Country { get; set; } = String.Empty;
        public DateTime ConnectionDate { get; set; } = new DateTime();
        public String ConnectionType { get; set; } = String.Empty;
        public String Html { get; set; } = String.Empty;
        public Boolean BusinessCardExists { get; set; } = false;
        public Dictionary<String, String> Errors { get; set; } = new Dictionary<String, String>();
        public String ErrorMessage { get { return getErrorMessage(); } }

        private string getErrorMessage()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String key in this.Errors.Keys)
            {
                sb.AppendLine(String.Format("[{0}]: {1}", key, this.Errors[key]));
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return String.Format("[{0}], [{1}]", this.Name, this.Company);
        }
        public BusinessCard() { }
        public BusinessCard(String connString, String fileName, HtmlNode nodeCard) : this()
        {
            this.FileName = fileName;
            this._FolderName = System.IO.Path.GetDirectoryName(this.FileName);
            this.Html = nodeCard.OuterHtml;
            setMemberId(nodeCard, ".//div[@class='J-item-data f-hide']");
            this.BusinessCardExists = businessCardExists(connString);
            if (!this.BusinessCardExists)
            {
                setPhoto(nodeCard, ".//div[@class='card-front']/div[@class='card-main ui2-box-wrap']/div[@class='avatar']/a[@class='imgbox']/img");
                setContactFront(nodeCard, ".//div[@class='card-front']/div[@class='card-main ui2-box-wrap']/div[@class='desc']");
                setContactBack(nodeCard, ".//div[@class='card-back']/div[@class='card-main ui2-box-wrap']/div[@class='desc']");
                setConnection(nodeCard, ".//div[@class='time']");
            }
        }
        public List<KeyValuePair<String, Object>> GetSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@MemberId", this.MemberId),
                new KeyValuePair<String, Object>("@Name", this.Name),
                new KeyValuePair<String, Object>("@Title", this.Title),
                new KeyValuePair<String, Object>("@ContactUrl", this.ContactUrl),
                new KeyValuePair<String, Object>("@Email", this.Email),
                new KeyValuePair<String, Object>("@Tel", this.Tel),
                new KeyValuePair<String, Object>("@Fax", this.Fax),
                new KeyValuePair<String, Object>("@Mobile", this.Mobile),
                new KeyValuePair<String, Object>("@Photo", this.Photo),
                new KeyValuePair<String, Object>("@PhotoFileName", this.PhotoFileName),
                new KeyValuePair<String, Object>("@Website", this.Website),
                new KeyValuePair<String, Object>("@Company", this.Company),
                new KeyValuePair<String, Object>("@CompanyUrl", this.CompanyUrl),
                new KeyValuePair<String, Object>("@CompanyType", this.CompanyType),
                new KeyValuePair<String, Object>("@CountryCode", this.CountryCode),
                new KeyValuePair<String, Object>("@CountryName", this.CountryName),
                new KeyValuePair<String, Object>("@Country", this.Country),
                new KeyValuePair<String, Object>("@ConnectionDate", this.ConnectionDate),
                new KeyValuePair<String, Object>("@ConnectionType", this.ConnectionType),
                new KeyValuePair<String, Object>("@ErrorMessage", this.ErrorMessage),
                new KeyValuePair<String, Object>("@FileName", this.FileName),
                new KeyValuePair<String, Object>("@Html", this.Html)
            };
            return parameters;
        }
        private bool businessCardExists(String connString)
        {
            Boolean success = false;
            if (!String.IsNullOrEmpty(this.MemberId))
            {
                List<KeyValuePair<String, Object>> parameters = getSQLParametersForBusinessCardExists();
                DataTable dt = Utilities.SQLExtension.GetDataTableFromStoredProcedure(connString, "[qwi].[AB.ReceivedCardExists]", "", parameters);
                success = dt != null && dt.Rows.Count > 0 ? Convert.ToBoolean(dt.Rows[0]["code"]) : false;
            }
            return success;
        }

        private List<KeyValuePair<String, Object>> getSQLParametersForBusinessCardExists()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@MemberId", this.MemberId)
            };
            return parameters;
        }

        private void setMemberId(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode node = nodeCard.SelectSingleNode(xPath);
                this.MemberId = node.GetAttributeValue("data-memberid", "");

            }
            catch (Exception ex)
            {
                this.Errors.Add("MemberId", ex.Message);
            }
        }

        private void setConnection(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode nodeParent = nodeCard.SelectSingleNode(xPath);
                String text = nodeParent.InnerText.Trim();
                string[] arr = text.Split('\n');
                this.ConnectionDate = Convert.ToDateTime(arr[0].Trim());
                this.ConnectionType = arr[1].Trim().Replace("&nbsp;", "");

            }
            catch (Exception ex)
            {
                this.Errors.Add("Connection", ex.Message);
            }
        }

        private void setContactFront(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode nodeParent = nodeCard.SelectSingleNode(xPath);
                HtmlNode node = nodeParent.SelectSingleNode("a[@class='name']");
                this.Name = Utilities.HtmlAgilityPackExtension.GetInnerText(nodeParent, "a[@class='name']");
                this.ContactUrl = Utilities.HtmlAgilityPackExtension.GetAttributeValue(nodeParent, "a[@class='name']", "href", "");

                this.Title = Utilities.HtmlAgilityPackExtension.GetInnerText(nodeParent, "div[@class='title']");

                this.Email = Utilities.HtmlAgilityPackExtension.GetInnerText(nodeParent, "div[@class='email']").Replace("Email:", "");

                this.Company = Utilities.HtmlAgilityPackExtension.GetInnerText(nodeParent, "a[@class='company']");
                this.Website = Utilities.HtmlAgilityPackExtension.GetAttributeValue(nodeParent, "a[@class='company']", "href", "");

                this.CompanyType = Utilities.HtmlAgilityPackExtension.GetInnerText(nodeParent, "//span[@class='type']");

                this.CountryName = Utilities.HtmlAgilityPackExtension.GetAttributeValue(nodeParent, "div[@class='bi-icon']/span[attribute::*[contains(local-name(), 'data-country')]]", "data-country", "");
                this.CountryCode = Utilities.HtmlAgilityPackExtension.GetAttributeValue(nodeParent, "div[@class='bi-icon']/span[attribute::*[contains(local-name(), 'data-country')]]", "data-country-sp", "");
                this.Country = Utilities.HtmlAgilityPackExtension.GetAttributeValue(nodeParent, "div[@class='bi-icon']/span[attribute::*[contains(local-name(), 'data-country')]]", "title", "");
            }
            catch (Exception ex)
            {
                this.Errors.Add("ContactFront", ex.Message);
            }
        }

        private void setContactBack(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode nodeParent = nodeCard.SelectSingleNode(xPath);
                this.Name = Utilities.HtmlAgilityPackExtension.GetInnerText(nodeParent, "div[@class='name']");
                HtmlNodeCollection nodes = nodeParent.SelectNodes("div[@class='line']");
                if (nodes.Count > 4)
                {
                    this.Email = Utilities.HtmlAgilityPackExtension.GetInnerText(nodes[0], "span[@class='val']");
                    this.Tel = Utilities.HtmlAgilityPackExtension.GetInnerText(nodes[1], "span[@class='val']");
                    this.Fax = Utilities.HtmlAgilityPackExtension.GetInnerText(nodes[2], "span[@class='val']");
                    this.Mobile = Utilities.HtmlAgilityPackExtension.GetInnerText(nodes[3], "span[@class='val']");
                    this.Website = Utilities.HtmlAgilityPackExtension.GetInnerText(nodes[4], "span[@class='val']");

                }
            }
            catch (Exception ex)
            {
                this.Errors.Add("ContactBack", ex.Message);
            }
        }

        private void setCompany(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode node = nodeCard.SelectSingleNode(xPath);
                this.Company = node.GetAttributeValue("title", "");
                this.CompanyUrl = node.GetAttributeValue("href", "");
            }
            catch (Exception ex)
            {
                this.Errors.Add("Company", ex.Message);
            }
        }

        private void setContact(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode node = nodeCard.SelectSingleNode(xPath);
                this.Name = node.GetAttributeValue("title", "");
                this.ContactUrl = node.GetAttributeValue("href", "");
            }
            catch (Exception ex)
            {
                this.Errors.Add("Contact", ex.Message);
            }
        }

        private void setPhoto(HtmlNode nodeCard, String xPath)
        {
            try
            {
                //HtmlNode node = nodeCard.SelectSingleNode(xPath);
                //this.PhotoFileName = node.GetAttributeValue("src", "").Replace(@"/", @"\");
                this.PhotoFileName = Utilities.HtmlAgilityPackExtension.GetAttributeValue(nodeCard, xPath, "src", "");
                if (!String.IsNullOrEmpty(this.PhotoFileName))
                {
                    this.PhotoFileName = PhotoFileName.Replace(@"/", @"\");
                    this.PhotoFileName = System.IO.Path.Combine(this._FolderName, this.PhotoFileName.Remove(0, 2));
                    this.Photo = Image.FromFile(this.PhotoFileName);
                }
            }
            catch (Exception ex)
            {
                this.Errors.Add("Photo", ex.Message);
                this.Photo = null;
            }
        }

        public void Dispose()
        {

        }
    }
}
