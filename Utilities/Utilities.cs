/******************************************************************************
 * Copyright (c) Qway Inc. 2003 - 2018                                        *
 *                                                                            *
 *                All rights reserved.                                        *
 *                https://sites.google.com/view/qway-inc                      *
 * ========================================================================== *
 *        Author: Andrew Huang
 *   Create Date: 2018.07.29
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
using System.IO;
using System.Security.Permissions;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.Windows;
using System.Xml.Xsl;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Data;
using System.Configuration;

namespace Utilities
{

    public static class Utilities
    {
        #region Public Methods
        public static StreamWriter CloseConsole()
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            Console.SetOut(sw);
            return sw;
        }
        public static void OpenConsole(StreamWriter sw)
        {
            Console.Out.Close();
            sw.Close();
            StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }
        public static void Log(
            String message,
            Boolean isWriteLine = true,
            Boolean addTime = false,
            Boolean feedLine = false,
            Boolean isClear = false,
            Boolean isDebug = true,
            Point cursorPosition = new Point(),
            ConsoleColor color = ConsoleColor.White,
            String logFileName = "",
            Boolean isError=false)
        {
            if (isError && color == ConsoleColor.White)
                color = ConsoleColor.Red;
            if (isDebug)
            {
                if (cursorPosition != new Point())
                    Console.SetCursorPosition(cursorPosition.X, cursorPosition.Y);
                else
                {
                    if (feedLine)
                        message = $"\n{message}";
                    String time = String.Format("[{0:HH:mm:ss.fff}] ", DateTime.Now);
                    message = String.Format("{0}{1}{2}", addTime ? time : "", message, isWriteLine ? "\n" : "");
                }
                if (color == ConsoleColor.White)
                    Console.Write(message);
                else
                {
                    ConsoleColor foregroundColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.Write(message);
                    Console.ForegroundColor = foregroundColor;
                }
                if (!String.IsNullOrEmpty(logFileName))
                    System.IO.File.AppendAllText(logFileName, message);
            }
        }

        public static String GetCurrentMethodName(Int32 stackIndex = 1)
        {
            String methodName = String.Empty;
            System.Reflection.MethodBase method = GetMethodBase(stackIndex + 1);
            if (method != null)
            {
                if (method.MemberType == MemberTypes.Constructor)
                    methodName = method.DeclaringType.Name;
                else
                    methodName = method.Name;
            }
            return methodName;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static System.Reflection.MethodBase GetMethodBase(Int32 stackIndex)
        {
            StatusExtension.Initialize();
            System.Reflection.MethodBase method = null;
            try
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(stackIndex);
                if (stackFrame == null && stackIndex > 0)
                    stackFrame = stackTrace.GetFrame(stackIndex - 1);
                if (stackFrame != null)
                {
                    method = stackFrame.GetMethod();
                }
            }
            catch (Exception ex) { StatusExtension.ErrorMessage = ex.Message; }
            return method;
        }
        #endregion
    }
}
