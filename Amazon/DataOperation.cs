/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.09.15
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

namespace Amazon
{
    public class DataOperation
    {

        internal static bool UpdateProductToSearchStatus(string connString, ProductToSearchInfo productToSearch)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.UpdateProductToSearchStatus]", parameters: productToSearch.SQLParametersStatus);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id > 0;
        }

        internal static Boolean UpdateProduct(string connString, ProductInfo product)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.UpdateProduct]", parameters: product.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id>0;
        }

        internal static List<ProductToSearchInfo> GetProductToSearch(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.GetProductToSearch]", parameters: null);
            return dataTable.Serialize<ProductToSearchInfo>();
        }

        internal static List<SellerInfo> GetSeller(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.GetSeller]", parameters: null);
            return dataTable.Serialize<SellerInfo>();
        }

        internal static int UpdateSeller(string connString, SellerInfo seller)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.UpdateSeller]", parameters: seller.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id;
        }

        internal static List<CategoryInfo> GetCategory(string connString)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.GetCategory]", parameters: null);
            return dataTable.Serialize<CategoryInfo>();
        }

        internal static Int32 UpdateCategory(string connString, CategoryInfo category)
        {
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[AZ.UpdateCategory]", parameters: category.SQLParameters);
            SQLResultStatusInfo sqlResultStatus = dataTable.SerializeFirst<SQLResultStatusInfo>();
            return sqlResultStatus.Id;
        }
        internal static WebsiteInfo GetWebsite(String connString, String code)
        {
            List<KeyValuePair<String, Object>> parameters = new List<KeyValuePair<String, Object>>() {
                new KeyValuePair<String, Object>("@Code",code)
            };
            DataTable dataTable = SQLExtension.GetDataTableFromStoredProcedure(connString: connString, storedProcedureName: "[qwi].[GetWebsite]", parameters: parameters);
            return dataTable.SerializeFirst<WebsiteInfo>();
        }
    }
}
