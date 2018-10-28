/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.05.03
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

using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Utilities
{
    public static class SQLExtension
    {
        #region Private Constants
        private const Int32 SQL_TIMEOUT = 0;
        #endregion Private Constants
        public static DataTable GetDataTableFromStoredProcedure(String connString, String storedProcedureName, String tableName = "", List<KeyValuePair<String, Object>> parameters = null)
        {
            return GetDataTable(connString, storedProcedureName, "", tableName, parameters);
        }
        public static DataTable GetDataTableFromCommand(String connString, String command, String tableName = "")
        {
            return GetDataTable(connString, "", command, tableName, null);
        }
        public static DataTable GetDataTable(String connString, String storedProcedureName, String command, String tableName = "", List<KeyValuePair<String, Object>> parameters = null)
        {
            StatusExtension.Initialize();
            String commandText = String.IsNullOrEmpty(storedProcedureName) ? command : storedProcedureName;
            CommandType commandType = String.IsNullOrEmpty(storedProcedureName) ? CommandType.Text : CommandType.StoredProcedure;
            tableName = tableName ?? "Data";
            DataTable dt = new DataTable(tableName);
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandType = commandType;
                    cmd.CommandText = commandText;
                    cmd.CommandTimeout = SQL_TIMEOUT;
                    cmd.Parameters.Clear();
                    if (parameters != null)
                        cmd.Parameters.AddRange(GetSqlParameters(parameters));
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
                catch (Exception ex)
                {
                    StatusExtension.ErrorMessage = String.Format("[{0}]: {1}\n{2}", commandType, commandText, ex.Message);
                    if (StatusExtension.IsErrorDisplayed)
                        Console.WriteLine("\n[*SQL*] {0}", StatusExtension.ErrorMessage);
                }
            }
            return dt;
        }
        public static SqlParameter[] GetSqlParameters(List<KeyValuePair<String, Object>> parameters)
        {
            SqlParameter param = null;
            List<SqlParameter> paraList = new List<SqlParameter>();
            foreach (KeyValuePair<String, Object> parameter in parameters)
            {
                if (parameter.Value != null)
                {
                    Type type = parameter.Value.GetType();
                    switch (type.Name)
                    {
                        case "Bitmap":
                            System.Drawing.Image image = parameter.Value as System.Drawing.Image;
                            byte[] buffer;
                            MemoryStream stream = new MemoryStream();
                            image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            buffer = stream.ToArray(); // converted to byte array
                            param = new SqlParameter(parameter.Key, SqlDbType.Binary, buffer.Length);
                            param.Value = buffer;
                            break;
                        default:
                            param = new SqlParameter(parameter.Key, parameter.Value);
                            break;
                    }
                }
                else
                    param = new SqlParameter(parameter.Key, null);
                paraList.Add(param);
            }
            return paraList.ToArray<SqlParameter>();
        }

    }
}
