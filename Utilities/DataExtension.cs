/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.06.01
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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Data;
using System.Reflection;

namespace Utilities
{
    public static class DataExtension
    {

        public static List<T> Serialize<T>(this DataTable dataTable)
        {
            StatusExtension.Initialize();
            List<T> list = new List<T>();
            foreach (DataRow row in dataTable.Rows)
                list.Add(row.Serialize<T>(true));
            return list;
        }
        public static T SerializeFirst<T>(this DataTable dataTable)
        {
            if (dataTable != null && dataTable.Rows.Count > 0)
                return dataTable.Rows[0].Serialize<T>();
            else
                return (T)Activator.CreateInstance(typeof(T), new object[] { });
        }
        public static T Serialize<T>(this DataRow row, Boolean isFromDataTable = false)
        {
            if (!isFromDataTable)
                StatusExtension.Initialize();
            Type type = typeof(T);
            PropertyInfo[] properties = type.GetProperties();
            T item = (T)Activator.CreateInstance(typeof(T), new object[] { });
            foreach (DataColumn col in row.Table.Columns)
            {
                PropertyInfo propertyInfo = properties.Where(p => p.Name.ToLower() == col.ColumnName.ToLower()).FirstOrDefault();
                if (propertyInfo != null)
                    setProperty<T>(ref item, propertyInfo, row[col]);
            }
            return item;
        }
        private static void setProperty<T>(ref T item, PropertyInfo propertyInfo, object value)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyInfo.Name);
            if (DBNull.Value.Equals(value))
                property.SetValue(item, null, null);
            else
                try
                {
                    switch (propertyInfo.PropertyType.FullName)
                    {
                        case "System.String":
                            property.SetValue(item, Convert.ToString(value), null);
                            break;
                        case "System.Int32":
                        case "System.Nullable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                        case "System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                            property.SetValue(item, Convert.ToInt32(value), null);
                            break;
                        case "System.Int64":
                        case "System.Nullable`1[[System.Int64, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                        case "System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                            property.SetValue(item, Convert.ToInt64(value), null);
                            break;
                        case "System.Single":
                            property.SetValue(item, Convert.ToSingle(value), null);
                            break;
                        case "System.Double":
                            property.SetValue(item, Convert.ToDouble(value), null);
                            break;
                        case "System.Decimal":
                        case "System.Nullable`1[[System.Decimal, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                        case "System.Nullable`1[[System.Decimal, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                            property.SetValue(item, Convert.ToDecimal(value), null);
                            break;
                        case "System.Char":
                            property.SetValue(item, Convert.ToChar(value), null);
                            break;
                        case "System.Boolean":
                            property.SetValue(item, Convert.ToBoolean(value), null);
                            break;
                        case "System.DateTime":
                        case "System.Nullable`1[[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                        case "System.Nullable`1[[System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]":
                            property.SetValue(item, Convert.ToDateTime(value), null);
                            break;
                        default:
                            property.SetValue(item, null, null);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    StatusExtension.ErrorMessages.Add(ex.Message);
                }
        }
    }
}
