using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace DIAViewSetup.Runtime.Preprocessing
{
    class Program
    {
        static void Main(string[] args)
        {

            string msiBaseDir = @"D:\work\Source\Common\Trial\Xingchun.ji\ProjectPack.V3\DIAViewSetup.Runtime.V3\bin\Debug";

            string msi_en_US = Path.Combine(msiBaseDir, @"en-us\DIAViewSetup.Runtime.msi");
            string msi_en = Path.Combine(msiBaseDir, @"en\DIAViewSetup.Runtime.msi");
            string msi_zh_CN = Path.Combine(msiBaseDir, @"zh-cn\DIAViewSetup.Runtime.msi");
            string msi_zh_CHT = Path.Combine(msiBaseDir, @"zh-tw\DIAViewSetup.Runtime.msi");

            Dictionary<string, string> dicLanFiles = new Dictionary<string, string>()
            {
                { "en-US",msi_en_US},
                { "en",msi_en},
                { "zh-CN",msi_zh_CN},
                { "zh-CHT",msi_zh_CHT},
            };

            LanguageTransService service = new LanguageTransService();
            bool ret = service.Excute(dicLanFiles);

            if (!ret)
            {
                MessageBox.Show("处理失败！");
            }

            //service.SetupTest(dicLanFiles);

            //ForTest();
        }


        private static void ForTest()
        {
            string msiBaseDir = @"D:\work\Source\Common\Trial\Xingchun.ji\ProjectPack.V3\DIAViewSetup.Runtime.V3\bin\Debug";

            string msiFile = Path.Combine(msiBaseDir, @"en-us\DIAViewSetup.Runtime.msi");
            string msiFile2 = Path.Combine(msiBaseDir, @"en-us\DIAViewSetup.Runtime.copy.msi");
            File.Copy(msiFile, msiFile2, true);

            string diaViwe = @"C:\Program Files (x86)\DIAView";

            string projectFile = @"D:\work\DiaViewProject\DemoProject\DemoProject260.project";
            string lan = "zh-CN";

            MSIEditor.EmbedRuntimeFile(msiFile2, diaViwe);
            MSIEditor.EmbedProjectFile(msiFile2, projectFile, lan);
        }

        private void SetShortCutForTest()
        {
            string msiBaseDir = @"D:\work\Source\Common\Trial\Xingchun.ji\ProjectPack.V3\DIAViewSetup.Runtime.V3\bin\Debug";

            string msi_en_US = Path.Combine(msiBaseDir, @"en-us\DIAViewSetup.Runtime.msi");
            string msi_en = Path.Combine(msiBaseDir, @"en\DIAViewSetup.Runtime.msi");
            string msi_zh_CN = Path.Combine(msiBaseDir, @"zh-cn\DIAViewSetup.Runtime.msi");
            string msi_zh_CHT = Path.Combine(msiBaseDir, @"zh-tw\DIAViewSetup.Runtime.msi");

            string msiFile2 = Path.Combine(msiBaseDir, @"en-us\DIAViewSetup.Runtime.copy.msi");
            using (Database db = new Database(msiFile2, DatabaseOpenMode.Direct))
            {
                string sql = db.Tables["Shortcut"].SqlSelectString + " WHERE `Shortcut` = 'DesktopShortcut'";
                string updateSql = "UPDATE `Shortcut` SET `Description` = 'xxxyyy' WHERE `Shortcut` = 'DesktopShortcut'";

                db.Execute(updateSql);


                using (View view3 = db.OpenView(sql))
                {
                    IList<TableInfo> viewTables = view3.Tables;
                    string sql2 = view3.Tables[0].SqlSelectString + " WHERE `Shortcut` = 'DesktopShortcut'";


                }

                db.Commit();
                return;
            }
        }



        public class LanguageTransService
        {
            private string WixToolPath = @"C:\Program Files (x86)\WiX Toolset v3.11\bin";

            public bool Excute(Dictionary<string, string> dicFiles)
            {
                if (LanguageTransfor(dicFiles))
                {
                    //return EmbedLanguageResource(dicFiles);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 生成变形文件
            /// </summary>
            private bool LanguageTransfor(Dictionary<string, string> dicFiles)
            {
                if (dicFiles == null || dicFiles.Count <= 1) return false;

                // 准备torch命令
                string torchExe = Path.Combine(WixToolPath, "torch.exe");
                string baseName = dicFiles.ElementAt(0).Key;
                string baseMsiFile = dicFiles.ElementAt(0).Value;

                for(int i=1; i< dicFiles.Count; i++)
                {
                    string toName = dicFiles.ElementAt(i).Key;
                    string toMsiFile = dicFiles.ElementAt(i).Value;

                    string mstFile = $"transforms\\{toName}.mst";

                    string torchCmd = $"-t language {baseMsiFile} {toMsiFile} -out {mstFile}";

                    bool ret = ProcessUtils.RunWixCommand(torchExe, torchCmd, Directory.GetCurrentDirectory());
                    if (!ret) return false;
                }
                return true;
            }

            /// <summary>
            /// 嵌入资源文件到MSI包
            /// </summary>
            private bool EmbedLanguageResource(Dictionary<string, string> dicFiles)
            {
                string baseName = dicFiles.ElementAt(0).Key;
                string baseMsiFile = dicFiles.ElementAt(0).Value;

                for (int i = 1; i < dicFiles.Count; i++)
                {
                    string toName = dicFiles.ElementAt(i).Key;
                    string toMsiFile = dicFiles.ElementAt(i).Value;

                    string mstFile = $"transforms\\{toName}.mst";

                    bool ret = EmberdFile(baseMsiFile, mstFile);
                    if (!ret) return false;
                }
                return true;
            }

            private bool EmberdFile(string msiFile, string transformFile)
            {
                using (Database db = new Database(msiFile, DatabaseOpenMode.Direct))
                {
                    using (View view = db.OpenView("INSERT INTO `_Storages` (`Name`, `Data`) VALUES(?, ?)"))
                    {
                        using (Record record = new Record(2))
                        {
                            record[1] = Path.GetFileName(transformFile);
                            record.SetStream(2, transformFile);
                            view.Execute(record);
                        }
                    }
                    db.Commit();
                }
                return true;
            }

            /// <summary>
            /// 测试安装包
            /// </summary>
            public void SetupTest(Dictionary<string, string> dicFiles)
            {
                string baseName = dicFiles.ElementAt(0).Key;
                string baseMsiFile = dicFiles.ElementAt(0).Value;

                for (int i = 0; i < dicFiles.Count; i++)
                {
                    string toName = dicFiles.ElementAt(i).Key;
                    string args = $"/i  {baseMsiFile} TRANSFORMS=:{toName}.mst";

                    ProcessUtils.RunWixCommand("msiexec", args, Directory.GetCurrentDirectory());
                    Console.ReadLine();
                }
            }
        }
    }


    public class ProcessUtils
    {
        private static string DefaultWorkingDir = Path.Combine(Path.GetTempPath(), "DiaviewPack");

        /// <summary>
        /// 执行Wix相关命令
        /// </summary>
        /// <param name="wixExe"></param>
        /// <param name="para"></param>
        /// <param name="defaultWorkingDir"></param>
        /// <returns></returns>
        public static bool RunWixCommand(string wixExe, string para, string workingDir = "")
        {
            if (string.IsNullOrEmpty(workingDir))
            {
                workingDir = DefaultWorkingDir;
            }

            var wixProcess = new Process
            {
                StartInfo =
                {
                    FileName = wixExe,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDir,
                    Arguments=para
                },
            };

            Stopwatch sw = new Stopwatch();

            try
            {
                string cmd = $"{wixExe} {para}";
                
                sw.Start();

                wixProcess.Start();
                wixProcess.WaitForExit();
                int exitCode = wixProcess.ExitCode;
                if (exitCode != 0)
                {
                    string output = wixProcess.StandardOutput.ReadToEnd();
                    throw new Exception(output);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (wixProcess != null)
                {
                    wixProcess.Close();
                }
                sw.Stop();

                sw = null;
            }
        }
    }
}
