using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WixToolset.Bootstrapper;

namespace CustomBA
{
    public class CustomAction
    {
        #region 安装SQL

        /// <summary>
        /// 检测SQL版本是否满足需求：大于等于SQL SERVER 2008 R2
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private bool CheckSQLServerVersionIsBiggerThanSqlServer2008R2(string version)
        {
            if (string.IsNullOrEmpty(version)) return false;
            var versionArray = version.Split('.');
            if (versionArray.Length < 2) return false;
            try
            {
                var sqlVersion = int.Parse(versionArray[0]);
                if (sqlVersion < 10)
                {
                    return false;
                }
                if (sqlVersion > 10)
                {
                    return true;
                }
                var sqlChildVersion = int.Parse(versionArray[1]);
                if (sqlChildVersion >= 50)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检测是否需要安装SQL Server
        /// 机器安装sql server版本号大于SQL SERVER 2008 R2时，不许要安装，小于或者未安装时均需要安装
        /// </summary>
        /// <returns>true:需要、false:不需要</returns>
        public bool IsNeedInstallSqlServer()
        {
            var rk = Environment.Is64BitOperatingSystem
                ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);


            WriteLog($"RegistryKey is {rk.Name}");

            var rkNames = rk.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");
            if (rkNames != null)
            {
                bool isVersionBigger = false;
                var array = rkNames.GetValueNames();
                var list = (from name in array select rkNames.GetValue(name) into value where value != null select value.ToString()).ToList();

                int i = 0;
                foreach (var db in list)
                {
                    var path = string.Format(@"SOFTWARE\Microsoft\Microsoft SQL Server" + @"\" + db + @"\MSSQLServer\CurrentVersion");

                    var sqlRk = rk.OpenSubKey(path);
                    if (sqlRk != null)
                    {
                        var version = sqlRk.GetValue("CurrentVersion");

                        if (version != null)
                        {
                            WriteLog($"CurrentVersion is {version.ToString()}");

                            if (CheckSQLServerVersionIsBiggerThanSqlServer2008R2(version.ToString()))
                            {
                                isVersionBigger = true;
                                break;
                            }
                        }
                    }
                }

                if (isVersionBigger)//version is satisfy
                {
                    //如果SQL版本大于等于SQL server 2008 R2，则完成
                    WriteLog($"CurrentVersion is bigger than sql server 2008 R2 ");

                    return false;
                }
                else
                {
                    //如果SQL版本小于SQL server 2008 R2，需要安装SQL
                    WriteLog($"CurrentVersion is smaller than sql server 2008 R2 ");

                    return true;
                }
            }
            else
            {
                //当前机器未安装Sql server
                WriteLog($"RegistryKey is {rk.ToString()} is not found, ==>  Sqlserver is not installed on the machine ");

                return true;
            }
        }

        /// <summary>
        /// 启动SQL安装包
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool InstallSqlServer(string sqlSetupDir)
        {
            //string lan = session["ProductLanguage"];
            string lan = "1033";

            //2052：简体中文  0
            //1028：繁体中文  1
            //1033：英语      2
            int lanIndex = 2;
            if ("2052".Equals(lan)) lanIndex = 0;
            if ("1028".Equals(lan)) lanIndex = 1;

            try
            {
                string sqlSetupFileName = Environment.Is64BitOperatingSystem ? "SQLEXPR_x64_ENU.exe" : "SQLEXPR_x86_ENU.exe";

                string sqlFilePath = Path.Combine(sqlSetupDir, sqlSetupFileName);

                //string args = @"/QS /ACTION=Install /FEATURES=SQL,AS,RS,IS,Tools /INSTANCENAME=SQLEXPRESS /SQLSVCACCOUNT=""SYSTEM"" /SQLSYSADMINACCOUNTS=""NT AUTHORITY\SYSTEM"" /AGTSVCACCOUNT=""NT AUTHORITY\Network Service"" /IACCEPTSQLSERVERLICENSETERMS";

                string args = @"/ACTION=Install ";


                WriteLog("SqlFilePath: " + sqlFilePath);
                WriteLog("Setup Argument: " + args);

                if (File.Exists(sqlFilePath))
                {
                    Process process = Process.Start(sqlFilePath, args);
                }
                else
                {
                    string[] fileNotFoundList = new string[] {
                        "未能查找到SQL安装包,查找路径:",
                        "未能查找到SQL安裝包,查找路徑:",
                        "Failed to find the SQL installation package, find the path:"
                    };
                    System.Windows.Forms.MessageBox.Show(fileNotFoundList[lanIndex] + sqlFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteLog($"Error happen in installing sql server, {ex}");
                return false;
            }
        }
        #endregion

        private static void WriteLog( string log)
        {
            log = DateTime.Now.ToString("MMdd HH:mm:ss fff") + "CustomActions.WriteLog" + log;

            WixBA.Model.Engine.Log(LogLevel.Verbose, log);
        }
    }
}
