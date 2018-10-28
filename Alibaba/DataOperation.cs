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
using System.Diagnostics;

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
using System.IO;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using Utilities;
using EAGetMail;

namespace Alibaba
{
    public class DataOperation
    {
        public static SQLResultStatusInfo SQLResultStatus = new SQLResultStatusInfo();
        internal static List<HotelInfo> GetHotels(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[DL.GetHotels]", parameters: null);
            return dataTable.Serialize<HotelInfo>();
        }

        internal static Boolean AddHotel(String connString, HotelInfo hotel)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[DL.UpdateHotel]", parameters: hotel.SQLParameters);
            SQLResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return SQLResultStatus.Id > 0;
        }

        internal static Int32 UpdateProductCategory(String connString, SupplierCategoryInfo category)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateProductCategory]", parameters: category.SQLParameters);
            SQLResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return SQLResultStatus.Id;
        }

        internal static DataTable GetSupplierMetadata(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSupplierMetadata]");
            return dataTable;
        }
        internal static SQLResultStatusInfo UpdateSupplierURL(string connString, SupplierUrlInfo supplierUrl)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierURLMetadata]", parameters: supplierUrl.SQLParameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>();
        }


        internal static SQLResultStatusInfo UpdateWebsite(String connString, WebsiteInfo website)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[UpdateWebsite]", parameters: website.SQLParameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>();
        }

        internal static List<SettingsInfo> GetSettings(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromCommand(connString: connString, command: "SELECT * FROM [qwi].[AB.vSettings] ORDER BY [Email]");
            return dataTable.Serialize<SettingsInfo>();
        }

        internal static List<BusinessCardInfo> GetSupplierUrlFromBC(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSupplierUrlFromBusinessCard]");
            return dataTable.Serialize<BusinessCardInfo>();
        }

        internal static Boolean UpdateSettingStatus(String connString, SettingsInfo setting, String errorMessage)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(setting, errorMessage);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSettingStatus]", parameters: parameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>().Code == "S";
        }

        private static List<KeyValuePair<String, Object>> getSQLParameters(SettingsInfo setting, String errorMessage)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",setting.Id),
                new KeyValuePair<String, Object>("@Message",errorMessage)
            };
            return parameters;
        }

        internal static Boolean UpdateSettingPostStatus(string connString, SettingsInfo setting)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParametersPostStatus(setting);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSettingPostStatus]", parameters: parameters);
            return !StatusExtension.HasError;
        }
        private static List<KeyValuePair<String, Object>> getSQLParametersPostStatus(SettingsInfo setting)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Email",setting.Email)
            };
            return parameters;
        }
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="supplier"></param>
        /// <returns></returns>
        internal static SQLResultStatusInfo UpdateSupplierPostStatus(String connString, SupplierInfo supplier)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParametersPostStatus(supplier);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierPostStatus]", parameters: parameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>();
        }
        private static List<KeyValuePair<String, Object>> getSQLParametersPostStatus(SupplierInfo supplier)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id", supplier.Id),
                new KeyValuePair<String, Object>("@Account", supplier.Account),
                new KeyValuePair<String, Object>("@Comments", supplier.ErrorMessage)
            };
            return parameters;
        }

        internal static SettingsInfo GetSetting(String connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSetting]");
            return dataTable.SerializeFirst<SettingsInfo>();
        }

        internal static bool UpdateSupplierUrlStatus(string connString, SupplierUrlInfo supplierUrl, SupplierInfo supplier)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(supplierUrl, supplier);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierUrlStatus]", parameters: parameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>().Code == "S";
        }

        private static List<KeyValuePair<String, Object>> getSQLParameters(SupplierUrlInfo supplierUrl, SupplierInfo supplier)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Id",supplierUrl.Id),
                new KeyValuePair<String, Object>("@ErrorMessage",supplier.ErrorMessage),
                new KeyValuePair<String, Object>("@Status",supplier.Status)
            };
            return parameters;
        }

        internal static List<SupplierUrlInfo> GetSupplierUrls(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSupplierUrls]");
            return dataTable.Serialize<SupplierUrlInfo>();
        }
        internal static Int32 UpdateSupplierCategory(String connString, SupplierCategoryInfo supplierCategory)
        {
            if (String.IsNullOrEmpty(supplierCategory.Url))
                SQLResultStatus.Status = "***";
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierCategory]", parameters: supplierCategory.SQLParameters);
            SQLResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return SQLResultStatus.Id;
        }

        internal static WebsiteInfo GetWebsite(String connString, String code)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(code);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetWebsite]", parameters: parameters);
            return dataTable.SerializeFirst<WebsiteInfo>();
        }

        private static List<KeyValuePair<String, Object>> getSQLParameters(String code)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",code)
            };
            return parameters;
        }

        internal static SQLResultStatusInfo UpdateEmail(String connString, String email, DateTime lastEmailRetrived)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(email, lastEmailRetrived);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSettingLastEmailRetrived]", parameters: parameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>();
        }

        private static List<KeyValuePair<String, Object>> getSQLParameters(String email, DateTime lastEmailRetrived)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Email",email),
                new KeyValuePair<String, Object>("@LastEmailRetrived",lastEmailRetrived),
            };
            return parameters;
        }
        internal static SQLResultStatusInfo UpdateEmail(String connString, EmailInfo email)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateEmail]", parameters: email.SQLParameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>();
        }

        internal static List<MailServerInfo> GetSettingList(String connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSettingForEmailPolling]");
            return dataTable.Serialize<MailServerInfo>();
        }

        internal static SQLResultStatusInfo UpdateSupplierURL(String connString, String url, String status, String errorMessage, SupplierCategoryInfo category)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParametersStatus(url, status, errorMessage, category);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierURL]", parameters: parameters);
            return dataTable.SerializeFirst<SQLResultStatusInfo>();
        }
        private static List<KeyValuePair<String, Object>> getSQLParametersStatus(String url, String status, String errorMessage, SupplierCategoryInfo category)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@ProfileUrl",url),
                new KeyValuePair<String, Object>("@Status",status),
                new KeyValuePair<String, Object>("@ErrorMessage",errorMessage),
                new KeyValuePair<String, Object>("@CategoryId",category.Id),
                new KeyValuePair<String, Object>("@CategoryCode",category.FullCode)
            };
            return parameters;
        }

        internal static bool UpdateSupplier(String connString, SupplierInfo supplier)
        {
            //List<KeyValuePair<String, Object>> parameters = getSQLParameters(supplier);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplier]", parameters: supplier.SQLParameters);
            supplier.Result = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return !StatusExtension.HasError;
        }

        internal static bool UpdateSupplierCategoryStatus(string connString, SupplierCategoryInfo category)
        {
            Boolean success = false;
            List<KeyValuePair<String, Object>> parameters = getSQLParametersStatus(category);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateCategoryStatus]", parameters: parameters);
            SQLResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            success = !StatusExtension.HasError;
            return success;
        }
        private static List<KeyValuePair<String, Object>> getSQLParametersStatus(SupplierCategoryInfo category)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@PostCode",category.PostCode),
                new KeyValuePair<String, Object>("@Id",category.Id),
                new KeyValuePair<String, Object>("@Comments",category.HasError? category.Errors.ToString2():String.Empty)
            };
            return parameters;
        }

        internal static bool UpdateSuppliers(string connString, List<SupplierInfo> suppliers)
        {
            Boolean success = false;
            foreach (SupplierInfo supplier in suppliers)
            {
                DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplier]", parameters: supplier.SQLParameters);
                supplier.Result = dataTable.SerializeFirst<SQLResultStatusInfo>();
                success = !StatusExtension.HasError;
            }
            return success;
        }
        //private static List<KeyValuePair<String, Object>> getSQLParameters(SupplierInfo supplier)
        //{
        //    List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
        //        new KeyValuePair<String, Object>("@CompanyId",supplier.CompanyId),
        //        new KeyValuePair<String, Object>("@CompanyName",supplier.CompanyName),
        //        new KeyValuePair<String, Object>("@CompanyUrl",supplier.CompanyUrl),
        //        new KeyValuePair<String, Object>("@Website",supplier.Website),
        //        new KeyValuePair<String, Object>("@WebsiteAlibaba",supplier.WebsiteAlibaba),
        //        new KeyValuePair<String, Object>("@CompanyProfileUrl",supplier.CompanyProfileUrl),
        //        new KeyValuePair<String, Object>("@CompanyBusinessType",supplier.CompanyBusinessType),
        //        new KeyValuePair<String, Object>("@CompanyCreditLevel",supplier.CompanyCreditLevel),
        //        new KeyValuePair<String, Object>("@CompanyEstablishedYear",supplier.CompanyEstablishedYear),
        //        new KeyValuePair<String, Object>("@OperationalAddress",supplier.OperationalAddress),
        //        new KeyValuePair<String, Object>("@CompanyLocation",supplier.CompanyLocation),
        //        new KeyValuePair<String, Object>("@CompanyDescription",supplier.CompanyDescription),
        //        new KeyValuePair<String, Object>("@ProductCertification",supplier.ProductCertification),
        //        new KeyValuePair<String, Object>("@CategoryId",supplier.CategoryId),
        //        new KeyValuePair<String, Object>("@CategoryName",supplier.CategoryName),
        //        new KeyValuePair<String, Object>("@Address",supplier.Address),
        //        new KeyValuePair<String, Object>("@City",supplier.City),
        //        new KeyValuePair<String, Object>("@Province",supplier.Province),
        //        new KeyValuePair<String, Object>("@Zip",supplier.Zip),
        //        new KeyValuePair<String, Object>("@Country",supplier.Country),
        //        new KeyValuePair<String, Object>("@CountryCode",supplier.CountryCode),
        //        new KeyValuePair<String, Object>("@SupplierMainProducts",supplier.SupplierMainProducts),
        //        new KeyValuePair<String, Object>("@CompanyNumberOfEmployees",supplier.CompanyNumberOfEmployees),
        //        new KeyValuePair<String, Object>("@SupplierTotalAnnualSalesVolume",supplier.SupplierTotalAnnualSalesVolume),
        //        new KeyValuePair<String, Object>("@CompanyMainMarket",supplier.CompanyMainMarket),
        //        new KeyValuePair<String, Object>("@ContactName",supplier.ContactName),
        //        new KeyValuePair<String, Object>("@ContactProfileUrl",supplier.ContactProfileUrl),
        //        new KeyValuePair<String, Object>("@ContactPhotoUrl",supplier.ContactPhotoUrl),
        //        new KeyValuePair<String, Object>("@Department",supplier.Department),
        //        new KeyValuePair<String, Object>("@JobTitle",supplier.JobTitle),
        //        new KeyValuePair<String, Object>("@ContactId",supplier.ContactId),
        //        new KeyValuePair<String, Object>("@ContactSid",supplier.ContactSid),
        //        new KeyValuePair<String, Object>("@ContactPid",supplier.ContactPid),
        //        new KeyValuePair<String, Object>("@Telephone",supplier.Telephone),
        //        new KeyValuePair<String, Object>("@MobilePhone",supplier.MobilePhone),
        //        new KeyValuePair<String, Object>("@Fax",supplier.Fax)
        //    };
        //    return parameters;
        //}

        //internal static DataTable UpdateSupplierCategory(String connString, SupplierCategoryInfo category)
        //{
        //    List<KeyValuePair<String, Object>> parameters = getSQLParameters(category);
        //    DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSupplierCategory]", parameters: parameters);
        //    return dataTable;
        //}
        private static List<KeyValuePair<String, Object>> getSQLParametersForCategoryList(String code)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",code)
            };
            return parameters;
        }
        internal static List<SupplierCategoryInfo> GetCategoryList(String connString, String code)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParametersForCategoryList(code);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetCategory]", parameters: parameters);
            return Utilities.DataExtension.Serialize<SupplierCategoryInfo>(dataTable);
        }
        internal static bool UpdateBusinessCards(string connString, List<BusinessCardInfo> cards)
        {
            Boolean success = false;
            foreach (BusinessCardInfo card in cards)
            {
                DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateBusinessCard]", parameters: card.SQLParameters);
                card.Result = dataTable.SerializeFirst<SQLResultStatusInfo>();
                success = !StatusExtension.HasError;
            }
            return success;
        }

        private static List<KeyValuePair<String, Object>> getSQLParameters(BusinessCardInfo card, Boolean isSent)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@MemberId",card.MemberId),
                new KeyValuePair<String, Object>("@ContactName",card.ContactName),
                new KeyValuePair<String, Object>("@Title",card.Title),
                new KeyValuePair<String, Object>("@ProfileUrl",card.ProfileUrl),
                new KeyValuePair<String, Object>("@Email",card.Email),
                new KeyValuePair<String, Object>("@Tel",card.Tel),
                new KeyValuePair<String, Object>("@Fax",card.Fax),
                new KeyValuePair<String, Object>("@Mobile",card.Mobile),
                //new KeyValuePair<String, Object>("@Photo",card.Photo),
                new KeyValuePair<String, Object>("@PhotoUrl",card.PhotoUrl),
                new KeyValuePair<String, Object>("@Website",card.Website),
                new KeyValuePair<String, Object>("@CompanyName",card.CompanyName),
                new KeyValuePair<String, Object>("@CompanyProfileUrl",card.CompanyProfileUrl),
                new KeyValuePair<String, Object>("@CompanyType",card.CompanyType),
                new KeyValuePair<String, Object>("@CountryCode",card.CountryCode),
                new KeyValuePair<String, Object>("@Country",card.Country),
                new KeyValuePair<String, Object>("@CountryDescription",card.CountryDescription),
                new KeyValuePair<String, Object>("@ConnectedDate",card.ConnectedDate),
                new KeyValuePair<String, Object>("@ConnectionType",card.ConnectionType),
                new KeyValuePair<String, Object>("@Account",card.Account),
                new KeyValuePair<String, Object>("@AccountEmail",card.AccountEmail),
                new KeyValuePair<String, Object>("@InteractionStatus",card.InteractionStatus),
                new KeyValuePair<String, Object>("@ConnectStatus",card.ConnectStatus),
                new KeyValuePair<String, Object>("@SentDate",card.SentDate),
                new KeyValuePair<String, Object>("@IsRead",card.IsRead),
                new KeyValuePair<String, Object>("@IsSent",isSent)
            };
            return parameters;
        }

        //internal static Boolean UpdateSupplierByBusinessCardsSent(String connString, List<BusinessCardInfo> cards)
        //{
        //    Boolean success = false;
        //    foreach (BusinessCardInfo card in cards)
        //    {
        //        List<KeyValuePair<String, Object>> parameters = getSQLParameters(card, true);
        //        DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateBusinessCard]", parameters: parameters);
        //        card.Result = dataTable.SerializeFirst<SQLResultStatusInfo>();
        //        success = !StatusExtension.HasError;
        //    }
        //    return success;
        //}
        //private static List<KeyValuePair<String, Object>> getSQLParametersSent(BusinessCardInfo card)
        //{
        //    List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
        //        new KeyValuePair<String, Object>("@CompanyName",card.CompanyName),
        //        new KeyValuePair<String, Object>("@ConnectDate",card.ConnectedDate),
        //        new KeyValuePair<String, Object>("@ContactName",card.ContactName),
        //        new KeyValuePair<String, Object>("@Account",card.Account),
        //        new KeyValuePair<String, Object>("@ConnectStatus",card.ConnectStatus),
        //        new KeyValuePair<String, Object>("@InteractionStatus",card.InteractionStatus)
        //    };
        //    return parameters;
        //}

        internal static DataTable UpdateSetting(string connString, SettingsInfo setting)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(setting);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.UpdateSetting]", parameters: parameters);
            return dataTable;
        }
        private static List<KeyValuePair<String, Object>> getSQLParameters(SettingsInfo setting)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Email",setting.Email)
            };
            return parameters;
        }
        /// <summary>
        /// OK
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal static List<SupplierInfo> GetSuppliersToPost(String connString, Int32 count)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParametersSuppliersToPost(count);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetSuppliersToPost]", parameters: parameters);
            return Utilities.DataExtension.Serialize<SupplierInfo>(dataTable);
        }

        private static List<KeyValuePair<String, Object>> getSQLParametersSuppliersToPost(Int32 count)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Count",count)
            };
            return parameters;
        }
        internal static DataTable GetSupplierList(String connString, Int32 count)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(count);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetWebPageLinks]", parameters: parameters);
            return dataTable;
        }

        private static List<KeyValuePair<String, Object>> getSQLParameters(Int32 pages)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Pages",pages)
            };
            return parameters;
        }
        internal static DataTable GetSettingList(String connString, Int32 count, String flag)
        {
            List<KeyValuePair<String, Object>> parameters = getSQLParameters(flag, count);
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AB.GetEmailToPost]", parameters: parameters);
            return dataTable;
        }
        private static List<KeyValuePair<String, Object>> getSQLParameters(String flag, Int32 count)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Flag",flag),
                new KeyValuePair<String, Object>("@Count",count)
            };
            return parameters;
        }
    }
}
