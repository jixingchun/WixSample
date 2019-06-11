using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CustomAction1(Session session)
        {
            session.Log("Begin CustomAction1");

            return ActionResult.Success;
        }

        /// <summary>
        /// 检测SQL版本是否满足需求：大于等于SQL SERVER 2008 R2
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static bool CheckSQLServerVersion(string version)
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
        /// 检测SQL版本
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult checkSQLInstallAndVersionAction(Session session)
        {

            WriteLog(session, $"HAS_SQLSERVER_DATASOURCE = {session["HAS_SQLSERVER_DATASOURCE"]}");
            WriteLog(session, $"WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT = {session["WIXUI_EXITDIALOGOPTIONALCHECKBOXTEXT"]}");



            WriteLog(session, "DIAView:The version information for the SQL is being detected.");

            var rk = Environment.Is64BitOperatingSystem
                ? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                : RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            var rkNames = rk.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL");
            if (rkNames != null)
            {
                bool isVersionBigger = false;
                var array = rkNames.GetValueNames();
                var list = (from name in array select rkNames.GetValue(name) into value where value != null select value.ToString()).ToList();
                WriteLog(session, @"DIAView:KeyValue Count : " + list.Count);
                int i = 0;
                foreach (var db in list)
                {

                    var path = string.Format(@"SOFTWARE\Microsoft\Microsoft SQL Server" + @"\" + db + @"\MSSQLServer\CurrentVersion");
                    WriteLog(session, "DIAView:KEYPATH #" + (i++) + "# " + path);
                    var sqlRk = rk.OpenSubKey(path);
                    if (sqlRk != null)
                    {
                        var version = sqlRk.GetValue("CurrentVersion");

                        WriteLog(session, "SQL CurrentVersion = " + version);

                        if (version != null)
                        {
                            if (CheckSQLServerVersion(version.ToString()))
                            {
                                WriteLog(session, "DIAView: The Version is OK by" + version.ToString());
                                isVersionBigger = true;
                                break;
                            }
                        }
                    }
                }

                if (isVersionBigger)//version is satisfy
                {
                    //如果SQL版本大于等于SQL server 2008 R2，则完成
                    WriteLog(session, "DIAView:The SQL version meets the requirements.");
                    WriteLog(session, "DIAView:#IS_INSTALL_SQL#:" + session["IS_INSTALL_SQL"]);
                    WriteLog(session, "DIAView:#INSTALL_SQL_REASON#:" + session["INSTALL_SQL_REASON"]);
                    session["IS_INSTALL_SQL"] = "0";
                    session["INSTALL_SQL_REASON"] = "NONE";
                    WriteLog(session, "DIAView:Set Property Value");
                    WriteLog(session, "DIAView:#IS_INSTALL_SQL#:" + session["IS_INSTALL_SQL"]);
                    WriteLog(session, "DIAView:#INSTALL_SQL_REASON#:" + session["INSTALL_SQL_REASON"]);
                }
                else
                {
                    //如果SQL版本小于SQL server 2008 R2，需要安装SQL
                    WriteLog(session, "DIAView:The SQL version is below the minimum version requirement and enters the installation SQL step.");
                    WriteLog(session, "DIAView:#IS_INSTALL_SQL#:" + session["IS_INSTALL_SQL"]);
                    WriteLog(session, "DIAView:#INSTALL_SQL_REASON#:" + session["INSTALL_SQL_REASON"]);
                    session["IS_INSTALL_SQL"] = "1";
                    session["INSTALL_SQL_REASON"] = "LowVerison";
                    WriteLog(session, "DIAView:Set Property Value");
                    WriteLog(session, "DIAView:#IS_INSTALL_SQL#:" + session["IS_INSTALL_SQL"]);
                    WriteLog(session, "DIAView:#INSTALL_SQL_REASON#:" + session["INSTALL_SQL_REASON"]);
                }
            }
            else
            {
                WriteLog(session, "DIAView:The machine was detected not installed with SQL.");
                WriteLog(session, "DIAView:#IS_INSTALL_SQL#:" + session["IS_INSTALL_SQL"]);
                WriteLog(session, "DIAView:#INSTALL_SQL_REASON#:" + session["INSTALL_SQL_REASON"]);

                session["IS_INSTALL_SQL"] = "1";
                session["INSTALL_SQL_REASON"] = "NotInstall";

                WriteLog(session, "DIAView:Set Property Value");
                WriteLog(session, "DIAView:#IS_INSTALL_SQL#:" + session["IS_INSTALL_SQL"]);
                WriteLog(session, "DIAView:#INSTALL_SQL_REASON#:" + session["INSTALL_SQL_REASON"]);
            }
            return ActionResult.Success;
        }

        /// <summary>
        /// 启动SQL安装包
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        [CustomAction]
        public static ActionResult installSQLaction(Session session)
        {
            //WIXUI_EXITDIALOGOPTIONALCHECKBOX_INSTALL = 1 and NOT Installed
            WriteLog(session, "WIXUI_EXITDIALOGOPTIONALCHECKBOX_INSTALL = " + session["WIXUI_EXITDIALOGOPTIONALCHECKBOX_INSTALL"]);

            WriteLog(session, "IS_INSTALL_SQL =" + session["IS_INSTALL_SQL"]);
            WriteLog(session, "HAS_SQLSERVER_DATASOURCE =" + session["HAS_SQLSERVER_DATASOURCE"]);
            WriteLog(session, "WIXUI_EXITDIALOGOPTIONALCHECKBOX_SQL =" + session["WIXUI_EXITDIALOGOPTIONALCHECKBOX_SQL"]);


            //2052：简体中文  0
            //1028：繁体中文  1
            //1033：英语      2
            int lanIndex = 2;
            //if (session["ProductLanguage"].Equals("2052")) lanIndex = 0;
            //if (session["ProductLanguage"].Equals("1028")) lanIndex = 1;

            if ("2052".Equals(session["ProductLanguage"])) lanIndex = 0;
            if ("1028".Equals(session["ProductLanguage"])) lanIndex = 1;

            try
            {
                string SourceDir = session["SourceDir"];  //msi文件路径
                string upDir = SourceDir.Substring(0, SourceDir.LastIndexOf(@"\", SourceDir.Length - 2) + 1);
                string SQLdirpath = Path.Combine(upDir, "SQL");

                string SQLfilePath = Path.Combine(SQLdirpath, Environment.Is64BitOperatingSystem ? "SQLEXPR_x64_ENU.exe" : "SQLEXPR_x86_ENU.exe");


                string args = @"/QS /ACTION=Install /FEATURES=SQL,AS,RS,IS,Tools /INSTANCENAME=SQLEXPRESS /SQLSVCACCOUNT=""SYSTEM"" /SQLSYSADMINACCOUNTS=""NT AUTHORITY\SYSTEM"" /AGTSVCACCOUNT=""NT AUTHORITY\Network Service"" /IACCEPTSQLSERVERLICENSETERMS";
                string installCmd = SQLfilePath + " " + args;

                WriteLog(session, "DIAView:SourceDir:" + SourceDir);
                WriteLog(session, "DIAView:SQLdirpath:" + SQLdirpath);
                WriteLog(session, "DIAView:SQLfilePath:" + SQLfilePath);
                WriteLog(session, "DIAView:SQLfilePath:Argument" + installCmd);

                if (File.Exists(SQLfilePath))
                {
                    Process.Start(installCmd);
                }
                else
                {
                    string[] fileNotFoundList = new string[] {
                        "未能查找到SQL安装包,查找路径:",
                        "未能查找到SQL安裝包,查找路徑:",
                        "Failed to find the SQL installation package, find the path:"
                    };
                    MessageBox.Show(fileNotFoundList[lanIndex] + SQLfilePath);
                }

                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                WriteLog(session, $"Error happen in installSQLaction , {ex}");
                return ActionResult.Failure;
            }
        }


        private static void WriteLog(Session session, string log)
        {
            log = DateTime.Now.ToString("MMdd HH:mm:ss fff") + "CustomActions.WriteLog" + log;
            session.Log(log);
            try
            {
                File.AppendAllText(@"d:\log.txt", log + System.Environment.NewLine);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
