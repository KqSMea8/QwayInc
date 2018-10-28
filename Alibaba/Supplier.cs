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
using System.Drawing;

namespace Alibaba
{
    public class Supplier : IDisposable
    {
        String _FolderName;

        public String Id { get; set; } = String.Empty;
        public String Id2 { get; set; } = String.Empty;
        public String Html { get; set; } = String.Empty;
        public String Name { get; set; } = String.Empty;
        public String CompanyType { get; set; } = String.Empty;
        public String Location { get; set; } = String.Empty;
        public String Tel { get; set; } = String.Empty;
        public String Mobile { get; set; } = String.Empty;
        public String Address { get; set; } = String.Empty;
        public String Zip { get; set; } = String.Empty;
        public String Province { get; set; } = String.Empty;
        public String City { get; set; } = String.Empty;
        public String Website { get; set; } = String.Empty;
        public String CreditLevel { get; set; } = String.Empty;
        public String ProfileUrl { get; set; } = String.Empty;
        public String ContactUrl { get; set; } = String.Empty;
        public String CompanyUrl { get; set; } = String.Empty;
        public String CountryCode { get; set; } = String.Empty;
        public String Country { get; set; } = String.Empty;
        public String ContactName { get; set; } = String.Empty;
        public String ContactImageFileName { get; set; } = String.Empty;
        public Image ContactImage { get; set; } = null;
        public String ContactTitle { get; set; } = String.Empty;
        public String FileName { get; set; } = String.Empty;
        public String ContactDepartment { get; set; } = String.Empty;
        public String TotalEmployees { get; set; } = String.Empty;
        public String TotalAnnualRevenue { get; set; } = String.Empty;
        public String YearEstablished { get; set; } = String.Empty;
        public String Account { get; set; } = String.Empty;
        public Boolean SupplierExists { get; set; } = false;
        public Dictionary<String, String> Errors { get; set; } = new Dictionary<String, String>();
        public String ErrorMessage { get { return getErrorMessage(); } }
        public Boolean IsWebSiteAvailable
        {
            get
            {
                Boolean success = Utilities.WebExtension.IsWebSiteAvailable(this.ProfileUrl);
                if (!success) this.Errors["WebSiteAvailable"] = Utilities.StatusExtension.ErrorMessage;
                return success;
            }
        }
        public Supplier() { }
        public Supplier(String folderName, String fileName, HtmlNode nodeCard, Boolean checkExists) : this()
        {
            this._FolderName = folderName;
            this.FileName = fileName;
            setCompanyId(nodeCard, "");
            this.SupplierExists = supplierExists();
            if (checkExists || !this.SupplierExists)
            {
                setCompany(nodeCard, ".//h2[@class='title ellipsis']/a");
                setCompanyContact(nodeCard, ".//div[@class='company']/a[@class='cd']");
                setCompanyProfile();
                setContact();
                if (String.IsNullOrEmpty(this.Id))
                    this.Id = this.Id2;
            }
        }

        public Supplier(DataRow row, String account)
        {
            this.Id = row["Id"].ToString();
            this.Name = row["Name"].ToString();
            this.ContactName = row["ContactName"].ToString();
            this.ProfileUrl = row["ProfileUrl"].ToString();
            this.ContactUrl = row["ContactUrl"].ToString();
            this.Account = account;
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
            return String.Format("[{0}], [{1}]", this.Id, this.Name);
        }

        private bool supplierExists()
        {
            Boolean success = false;
            if (!String.IsNullOrEmpty(this.Id))
            {
                List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                    new KeyValuePair<String, Object>("@CompanyId",this.Id),
                };
                String connString = System.Configuration.ConfigurationManager.ConnectionStrings["Alibaba"].ConnectionString;
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    try
                    {
                        SqlCommand cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "[qwi].[AB.SupplierExists]";
                        cmd.CommandTimeout = 1200;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddRange(getSqlParameters(parameters));
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        success = Convert.ToBoolean(dt.Rows[0][0]);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return success;
        }
        private SqlParameter[] getSqlParameters(List<KeyValuePair<String, Object>> parameters)
        {
            List<SqlParameter> paraList = new List<SqlParameter>();
            parameters.ForEach(p =>
            {
                paraList.Add(new SqlParameter(p.Key, p.Value));
            });
            return paraList.ToArray<SqlParameter>();
        }

        public List<KeyValuePair<String, Object>> GetSQLParameters()
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",this.Id),
                new KeyValuePair<String, Object>("@Name",this.Name),
                new KeyValuePair<String, Object>("@CompanyType",this.CompanyType),
                new KeyValuePair<String, Object>("@Location",this.Location),
                new KeyValuePair<String, Object>("@Tel",this.Tel),
                new KeyValuePair<String, Object>("@Mobile",this.Mobile),
                new KeyValuePair<String, Object>("@Address",this.Address),
                new KeyValuePair<String, Object>("@Zip",this.Zip),
                new KeyValuePair<String, Object>("@Province",this.Province),
                new KeyValuePair<String, Object>("@City",this.City),
                new KeyValuePair<String, Object>("@Website",this.Website),
                new KeyValuePair<String, Object>("@CreditLevel",this.CreditLevel),
                new KeyValuePair<String, Object>("@ProfileUrl",this.ProfileUrl),
                new KeyValuePair<String, Object>("@ContactUrl",this.ContactUrl),
                new KeyValuePair<String, Object>("@CompanyUrl",this.CompanyUrl),
                new KeyValuePair<String, Object>("@CountryCode",this.CountryCode),
                new KeyValuePair<String, Object>("@Country",this.Country),
                new KeyValuePair<String, Object>("@ContactName",this.ContactName),
                new KeyValuePair<String, Object>("@ContactTitle",this.ContactTitle),
                new KeyValuePair<String, Object>("@ContactDepartment",this.ContactDepartment),
                new KeyValuePair<String, Object>("@ErrorMessage",this.ErrorMessage),
                new KeyValuePair<String, Object>("@FileName",this.FileName),
                new KeyValuePair<String, Object>("@Html",this.Html)
            };
            return parameters;
        }
        private void setContact()
        {
            try
            {
                WebClient webClient = new WebClient();
                if (this.ContactUrl.StartsWith(@"//"))
                    this.ContactUrl = String.Format(@"https:{0}", this.ContactUrl);
                this.Html = webClient.DownloadString(this.ContactUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(this.Html);
                if (!setContactFormat1(doc))
                    if (!setContactFormat2(doc))
                        if (!setContactFormat3(doc))
                        {
                        }
            }
            catch (Exception ex)
            {
                this.Errors["Contact"] = ex.Message;
            }
        }
        private Boolean setContactFormat3(HtmlDocument doc)
        {
            HtmlNode nodeParent = doc.DocumentNode.SelectSingleNode("//div[@module-name='intl-icbu-smod-contactPerson']");
            Boolean success = nodeParent != null;
            if (success)
            {
                String jsonString = nodeParent.GetAttributeValue("module-data", "");
                jsonString = System.Web.HttpUtility.UrlDecode(jsonString);
                JObject json = (JObject)JsonConvert.DeserializeObject(jsonString);
                this.City = getValueForJson2(json, "accountCity");
                this.ContactName = getValueForJson2(json, "accountDisplayName");
                this.Zip = getValueForJson2(json, "accountCountry");
                this.Country = getValueForJson2(json, "accountCountry");
                this.ContactTitle = getValueForJson2(json, "accountJobTitle");
                this.Address = getValueForJson2(json, "accountAddress");
                this.Mobile = getValueForJson2(json, "accountMobileNo");
                this.Website = getValueForJson2(json, "esiteSubDomain");
                this.Province = getValueForJson2(json, "accountProvince");
                this.Tel = getValueForJson2(json, "accountPhone");
            }
            return success;
        }
        private Boolean setContactFormat2(HtmlDocument doc)
        {
            HtmlNode nodeParent = doc.DocumentNode.SelectSingleNode("//div[@id='contact-person']");
            Boolean success = nodeParent != null;
            if (success)
            {
                HtmlNode nodeInner = nodeParent.SelectSingleNode("//div[@class='contact-picture']/img");
                //String imgFileName = nodeInner == null ? String.Empty : nodeInner.GetAttributeValue("src", "");
                //this.ContactImage = String.IsNullOrEmpty(imgFileName) ? null : Image.FromFile(imgFileName);

                this.ContactName = getInnerText(nodeParent, "..//div[@class='contact-info']/h1[@class='name']");
                HtmlNodeCollection nodes = nodeParent.SelectNodes("..//div[@class='contact-info']/dl[@class='dl-horizontal']/dd");
                if (nodes != null && nodes.Count >= 2)
                {
                    this.ContactDepartment = nodes[0].InnerText.Trim();
                    this.ContactTitle = nodes[1].InnerText.Trim();

                }
                nodes = nodeParent.SelectNodes("..//div[@class='contact-detail']/div[@class='sensitive-info hide-sensitive']/dl[@class='dl-horizontal']/dd");
                if (nodes != null && nodes.Count >= 2)
                {
                    this.Tel = nodes[0].InnerText.Trim();
                    this.Mobile = nodes[1].InnerText.Trim();

                }
                nodes = nodeParent.SelectNodes("..//div[@class='contact-detail']/div[@class='public-info']/dl[@class='dl-horizontal']/dd");
                if (nodes != null && nodes.Count >= 5)
                {
                    this.Address = nodes[0].InnerText.Trim();
                    this.Zip = nodes[1].InnerText.Trim();
                    this.Country = nodes[2].InnerText.Trim();
                    this.Province = nodes[3].InnerText.Trim();
                    this.City = nodes[4].InnerText.Trim();

                }
                nodeParent = doc.DocumentNode.SelectSingleNode("//div[@class='company-contact-information']");
                if (nodeParent != null)
                {
                    nodes = nodeParent.SelectNodes("..//table[@class='company-info-data table']/tr/td");
                    if (nodes != null && nodes.Count > 6)
                    {
                        if (String.IsNullOrEmpty(this.Name))
                            this.Name = Utilities.WebExtension.DecodeHtml(nodes[1].InnerText.Trim());
                        if (String.IsNullOrEmpty(this.Address))
                            this.Address = nodes[3].InnerText.Trim();
                        this.Website = nodes[5].InnerText.Trim();
                    }
                }
            }
            return success;
        }
        private Boolean setContactFormat1(HtmlDocument doc)
        {
            HtmlNode nodeParent = doc.DocumentNode.SelectSingleNode("//div[@class='person-info']");
            Boolean success = nodeParent != null;
            if (success)
            {
                HtmlNode nodeInner = nodeParent.SelectSingleNode("//div[@class='contact-image']/img");
                this.ContactImageFileName = nodeInner == null ? String.Empty : nodeInner.GetAttributeValue("src", "");
                //this.ContactImage = String.IsNullOrEmpty(this.ContactImageFileName) ? null : Image.FromFile(this.ContactImageFileName);
                this.ContactName = getInnerText(nodeParent, "//div[@class='contact-name']");
                this.ContactDepartment = getInnerText(nodeParent, "//div[@class='contact-department']");
                this.ContactTitle = getInnerText(nodeParent, "//div[@class='contact-job']");
                HtmlNodeCollection nodes = nodeParent.SelectNodes("//table[@class='info-table']/tr");
                foreach (HtmlNode node in nodes)
                {
                    nodeInner = node.SelectSingleNode("th");
                    String value = nodeInner == null ? String.Empty : nodeInner.InnerText.Trim();
                    if (!String.IsNullOrEmpty(value))
                    {
                        nodeInner = node.SelectSingleNode("td");
                        if (nodeInner != null)
                        {
                            if (value.StartsWith("Telephone:"))
                            {
                                this.Tel = nodeInner.InnerText.Trim().Replace("View details", "");
                            }
                            else if (value.StartsWith("Mobile Phone:"))
                            {
                                this.Mobile = nodeInner.InnerText.Trim();
                            }
                            else if (value.StartsWith("Address:"))
                            {
                                this.Address = nodeInner.InnerText.Trim();
                            }
                            else if (value.StartsWith("Zip:"))
                            {
                                this.Zip = nodeInner.InnerText.Trim();
                            }
                            else if (value.StartsWith("Country/Region:"))
                            {
                                this.Country = nodeInner.InnerText.Trim();
                            }
                            else if (value.StartsWith("Province/State:"))
                            {
                                this.Province = nodeInner.InnerText.Trim();
                            }
                            else if (value.StartsWith("City:"))
                            {
                                this.City = nodeInner.InnerText.Trim();
                            }
                        }
                    }
                }
                nodeParent = doc.DocumentNode.SelectSingleNode("//table[@class='contact-table']/tr[@class='info-item']/td[@class='item-value']/div");
                if (nodeParent != null)
                    this.Website = nodeParent.InnerText.Trim();
            }
            return success;
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
        private void setCompanyProfile()
        {
            String jsonString = String.Empty;
            HtmlNode nodeParent = null;
            JObject json = null;
            try
            {
                WebClient webClient = new WebClient();
                if (this.ProfileUrl.StartsWith(@"//"))
                    this.ProfileUrl = String.Format(@"https:{0}", this.ProfileUrl);
                this.Html = webClient.DownloadString(this.ProfileUrl);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(this.Html);
                nodeParent = doc.DocumentNode.SelectSingleNode("//div[@id='bp-shopsign']/script[@id='bp-shopsign-data']");
                if (nodeParent != null)
                {
                    jsonString = nodeParent.InnerText.Trim();
                    json = (JObject)JsonConvert.DeserializeObject(jsonString);
                    if (String.IsNullOrEmpty(this.Name))
                        this.Name = Utilities.WebExtension.DecodeHtml(getValueFroJson(json, "companyName"));
                    this.CountryCode = getValueFroJson(json, "companyRegisterCountry");
                    this.Country = getValueFroJson(json, "companyRegisterFullCountry");
                    this.CompanyType = getValueFroJson(json, "companyType");
                    this.CompanyUrl = getValueFroJson(json, "wapSubDomain");
                    this.Location = getValueFroJson(json, "companyLocation");
                    this.Id2 = getValueFroJson(json, "companyId");
                    this.CreditLevel = getValueFroJson(json, "baoAccountCreditLevel");
                }
                else
                {
                    nodeParent = doc.DocumentNode.SelectSingleNode("//div[@class='content-wrap']/div[@id='content']");
                    jsonString = nodeParent.GetAttributeValue("data-user-context", "");
                    json = (JObject)JsonConvert.DeserializeObject(jsonString);
                    this.Id2 = getValueForJson(json, "uid");

                    if (String.IsNullOrEmpty(this.Name))
                    {
                        nodeParent = doc.DocumentNode.SelectSingleNode("//div[@class='m-body']/div[@class='m-header']/h3");
                        this.Name = Utilities.WebExtension.DecodeHtml(nodeParent.InnerText.Trim());
                    }

                    HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='information-content util-clearfix']/table[@class='content-table']/tr/td[@class='col-value']");

                    if (nodes != null && nodes.Count > 5)
                    {
                        this.CompanyType = nodes[0].InnerText.Trim();
                        this.Location = nodes[1].InnerText.Trim();

                        this.TotalEmployees = nodes[3].InnerText.Trim();
                        this.TotalAnnualRevenue = nodes[4].InnerText.Trim();
                        nodeParent = nodes[5].SelectSingleNode("a");
                        this.YearEstablished = nodeParent == null ? "confidential" : nodeParent.InnerText.Trim();
                    }

                    //this.CountryCode = getValueFroJson(json, "companyRegisterCountry");
                    //this.Country = getValueFroJson(json, "companyRegisterFullCountry");
                    //this.CompanyUrl = getValueFroJson(json, "wapSubDomain");
                    //this.CreditLevel = getValueFroJson(json, "baoAccountCreditLevel");

                }
            }
            catch (Exception ex)
            {
                this.Errors["CompanyProfile"] = ex.Message;
            }
        }
        private String getValueForJson2(JObject json, String key)
        {
            String value = String.Empty;
            try
            {
                value = json["mds"]["moduleData"]["data"][key].ToString().Trim();

            }
            catch (Exception)
            {
            }
            return value;
        }
        private String getValueForJson(JObject json, String key, String key2 = "")
        {
            String value = String.Empty;
            try
            {
                if (String.IsNullOrEmpty(key2))
                    value = json[key].ToString().Trim();
                else
                    value = json[key][key2].ToString().Trim();

            }
            catch (Exception)
            {
            }
            return value;
        }
        private String getValueFroJson(JObject json, String key)
        {
            return getValueForJson(json, "data", key);
        }
        private void setCompanyContact(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode node = nodeCard.SelectSingleNode(xPath);
                this.ContactUrl = node.GetAttributeValue("href", "");
            }
            catch (Exception ex)
            {
                this.Errors["CompanyContact"] = ex.Message;
            }
        }

        private void setCompany(HtmlNode nodeCard, string xPath)
        {
            try
            {
                HtmlNode node = nodeCard.SelectSingleNode(xPath);
                this.ProfileUrl = node.GetAttributeValue("href", "");
                this.Name = Utilities.WebExtension.DecodeHtml(node.InnerText);
            }
            catch (Exception ex)
            {
                this.Errors["Company"] = ex.Message;
            }
        }
        private String removeXmlChars(String str)
        {
            return str.Trim()
                .Replace("&amp;", "&")
                .Replace("&lt;", "")
                .Replace("&gt;", "")
                .Replace("&quot;", "")
                .Replace("&apos;", "")
                .Replace("&nbsp;", "")
                .Replace("  ", "");
        }
        private void setCompanyId(HtmlNode nodeCard, string xPath)
        {
            try
            {
                this.Id = nodeCard.GetAttributeValue("data-ctrdot", "");
                if (String.IsNullOrEmpty(this.Id))
                    this.Errors["Id"] = "Empty";
            }
            catch (Exception ex)
            {
                this.Errors["Id"] = ex.Message;
            }
        }

        public void Dispose()
        {

        }
    }
}
