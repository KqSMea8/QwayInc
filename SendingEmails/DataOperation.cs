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

namespace SendingEmails
{
    public class DataOperation
    {

        internal static List<EmailInfo> GetEamils(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.GetEmailHotel]", parameters: null);
            return dataTable.Serialize<EmailInfo>();
        }

        internal static bool UpdateEamil(string connString, EmailInfo email)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.UpdateEmailStatus]", parameters: email.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id>0;
        }
    }
}
