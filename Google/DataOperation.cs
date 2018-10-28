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

//using HtmlAgilityPack;
//using System.Drawing;
//using System.Net;
//using System.Web.Script.Serialization;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using System.Data;
//using System.Data.SqlClient;
//using System.Runtime.InteropServices;
//using Microsoft.Office.Interop.Outlook;
//using System.Reflection;
//using System.IO;

//using OpenQA.Selenium;
//using OpenQA.Selenium.Chrome;

using Utilities;
//using EAGetMail;

namespace Google
{
    public class DataOperation
    {

        internal static bool UpdateSearchHistory(string connString, KeyWordInfo keyWord)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateSearchHistory]", parameters: keyWord.SQLParametersForHistory);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id > 0;
        }

        internal static Boolean UpdateSearchResult(string connString, KeyWordDetailInfo keyWordDetail)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateSearchResult]", parameters: keyWordDetail.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            keyWordDetail.Id = sqlResultStatus.Id;
            return sqlResultStatus.Id > 0;
        }

        internal static bool UpdateVedCodeCrossLink(string connString, KeyWordInfo keyWord, Dictionary<String, String> veds)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@KeyWord",keyWord.KeyWordToSearch),
                new KeyValuePair<String, Object>("@DetailId",-1),
                new KeyValuePair<String, Object>("@Veds",KeyWordDetailInfo.GetVedsDataTable(veds))
            };
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateVedCodeCrossLink]", parameters: parameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id > 0;
        }

        internal static List<EmailSettingInfo> GetEmailSetting(string connString, String code)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.GetEmailSetting]", parameters: getParameters(code));
            return dataTable.Serialize<EmailSettingInfo>();
        }
        internal static List<EmailInfo> GetEmails(string connString, String code)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.GetEmail]", parameters: getParameters(code));
            return dataTable.Serialize<EmailInfo>();
        }

        private static List<KeyValuePair<String, Object>> getParameters(String code)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",code)
            };
            return parameters;
        }

        //internal static bool UpdateKeyWord(string connString, KeyWordInfo keyWord)
        //{
        //    DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateKeyWord]", parameters: keyWord.SQLParameters);
        //    SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
        //    return sqlResultStatus.Id > 0;
        //}

        internal static List<KeyWordDetailInfo> GetKeyWordDetails(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.GetKeyWordDetail]", parameters: null);
            return dataTable.Serialize<KeyWordDetailInfo>();
        }

        internal static List<KeyWordInfo> GetKeyWords(string connString, String code)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.GetKeyWord]", parameters: getParameters(code));
            return dataTable.Serialize<KeyWordInfo>();
        }

        internal static Boolean UpdateKeyWordDetail(string connString, KeyWordDetailInfo keyWordDetail)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateKeyWordDetail]", parameters: keyWordDetail.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id > 0;
        }

        internal static bool UpdateEmail(string connString, EmailInfo email)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateEmailSent]", parameters: email.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id > 0;
        }

        internal static bool UpdateKeyWordDetailAfter(string connString, KeyWordDetailInfo keyWordDetail)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GG.UpdateKeyWordDetailAfter]", parameters: keyWordDetail.SQLParametersAfter);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id > 0;
        }
    }
}
