using System.Linq;

namespace DIAViewSetup.Runtime.Preprocessing
{
    using Microsoft.Deployment.Compression;
    using Microsoft.Deployment.WindowsInstaller;
    using ProjectPack.Common;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class MSIEditor
    {
        /// <summary>
        /// 把运行时嵌入到MSI文件中
        /// </summary>
        /// <param name="diaviewDir"></param>
        /// <returns></returns>
        public static bool EmbedRuntimeFile(string runtimeMsiFile, string diaviewDir)
        {
            // 嵌入RuntimeFiles
            string runtimeFileName = "RuntimeFileList.txt";

            List<string> files = File.ReadAllLines(runtimeFileName,Encoding.UTF8).ToList();
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            foreach(string file in files)
            {
                string scrFile = Path.Combine(diaviewDir, file);
                string desFile = Path.Combine(tempDir, file);

                FileUtils.CopyFile(scrFile, desFile, false);
            }

            string  tempFile = Path.GetTempFileName();
            ArchiveInfo runtimeFileAchiveInfo = new Microsoft.Deployment.Compression.Zip.ZipInfo(tempFile);
            runtimeFileAchiveInfo.Pack(tempDir, true, CompressionLevel.Max, null);
            FileUtils.DeleteDirectory(tempDir);

            using (Database db = new Database(runtimeMsiFile, DatabaseOpenMode.Direct))
            {
                using (View view = db.OpenView("INSERT INTO `Binary` (`Name`, `Data`) VALUES(?, ?)"))
                {
                    using (Record record = new Record(2))
                    {
                        record[1] = "RuntimeFiles";
                        record.SetStream(2, tempFile);
                        view.Execute(record);
                    }
                }
                db.Commit();
            }

            return true;
        }

        /// <summary>
        /// 把用户工程嵌入到MSI文件中
        /// </summary>
        /// <param name="projectFile"></param>
        /// <param name="lan"></param>
        /// <returns></returns>
        public static bool EmbedProjectFile(string msiFile, string projectFile, string lan)
        {
            // 生成ZIP包
            string tempFile = Path.GetTempFileName();
            string projectFileDir = Path.GetDirectoryName(projectFile);
            ArchiveInfo archiveInfo = new Microsoft.Deployment.Compression.Zip.ZipInfo(tempFile);
            archiveInfo.Pack(projectFileDir, true, CompressionLevel.Max, null);

            // 把ZIP文件写入 MSI文件中
            using (Database db = new Database(msiFile, DatabaseOpenMode.Direct))
            {
                using (View view = db.OpenView("INSERT INTO `Binary` (`Name`, `Data`) VALUES(?, ?)"))
                {
                    using (Record record = new Record(2))
                    {
                        record[1] = "UserProject";
                        record.SetStream(2, tempFile);
                        view.Execute(record);
                    }
                }

                using (View view2 = db.OpenView("INSERT INTO `Property` (`Property`, `Value`) VALUES (?, ?)"))
                {
                    using (Record record = new Record(2))
                    {
                        record[1] = "SetupLanguage";
                        record[2] = lan;
                        view2.Execute(record);
                    }
                }

                db.Commit();
            }
            return true;
        }
    }
}
