using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WinForm = System.Windows.Forms;

namespace PackageGenerator
{
    class PackageViewModel: PropertyNotifyBase
    {
        private string _projectFile;
        public string ProjectFile
        {
            get
            {
                return _projectFile;
            }
            set
            {
                if (this._projectFile != value)
                {
                    this._projectFile = value;
                    base.OnPropertyChanged("ProjectFile");
                }
            }
        }

        private string _outputDir;
        public string OutputDir
        {
            get
            {
                return _outputDir;
            }
            set
            {
                if (this._outputDir != value)
                {
                    this._outputDir = value;
                    base.OnPropertyChanged("OutputDir");
                }
            }
        }


        string diaviewSetupDir;

        public PackageViewModel()
        {

        }

        public void Init()
        {
            diaviewSetupDir = @"D:\soft\DIAViewSetup_PreRelease";

            //ProjectFile = @"D:\work\DiaViewProject\DemoProject\DemoProject260.project";
            OutputDir = @"D:\MSI\DIAViewSetup";
        }

        /// <summary>
        /// 选择用户工程
        /// </summary>
        public void SelectUserProject()
        {
            WinForm.OpenFileDialog openFileDialog = new WinForm.OpenFileDialog();
            openFileDialog.Filter = " *.project|*.project";
            openFileDialog.CheckFileExists = true;
            if (openFileDialog.ShowDialog() == WinForm.DialogResult.OK)
            {
                this.ProjectFile = openFileDialog.FileName;
            }
            SetSaveButtonEnable();
        }

        /// <summary>
        /// 选择输出目录
        /// </summary>
        public void SelectOutPutDir()
        {
            WinForm.FolderBrowserDialog folderBrowserDialog = new WinForm.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == WinForm.DialogResult.OK)
            {
                this.OutputDir = folderBrowserDialog.SelectedPath;
            }
            SetSaveButtonEnable();
        }

        // 设置打包按钮是否可用
        private void SetSaveButtonEnable()
        {
            if (string.IsNullOrEmpty(OutputDir) || string.IsNullOrEmpty(ProjectFile))
            {
                CanSave = false;
            }
            else
            {
                CanSave = true;
            }
        }

        /// <summary>
        /// 工程打包
        /// </summary>
        public void Pack()
        {
            if (!InputCheck())
            {
                return;
            }
            // 从安装目录，复用相关安装文件
            CopyOtherSetupFile();

            // 生成用户工程压缩包，并复制到输出目录
            SetupModel setupModel = new SetupModel();
            setupModel.CreateProjectRuntimeMsi(_outputDir);

            string toZipFileName = Path.Combine(_outputDir, "DIAView", ProjectPackConst.RuntimeZipFileName);
            setupModel.ZipUserProjectFile(_projectFile, toZipFileName);
        }

        /// <summary>
        /// 画面必须输入检查
        /// </summary>
        /// <returns></returns>
        private bool InputCheck()
        {
            try
            {
                if (string.IsNullOrEmpty(_outputDir))
                {
                    throw new Exception("输出目录不能为空");
                }

                if (!Directory.Exists(_outputDir))
                {
                    try
                    {
                        Directory.CreateDirectory(_outputDir);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.Log.Info("Failed in create directory " + _outputDir + ex);
                        throw new Exception("Failed in create directory " + _outputDir);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 从安装目录，复用相关安装文件
        /// </summary>
        private void CopyOtherSetupFile()
        {
            string targetDir = Path.Combine(_outputDir);

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            FileUtils.CopyDirectory(Path.Combine(diaviewSetupDir, "DotNet"), Path.Combine(targetDir, "DotNet"), true);
            FileUtils.CopyDirectory(Path.Combine(diaviewSetupDir, "SenseLockDrivers"), Path.Combine(targetDir, "SenseLockDrivers"), true);
            FileUtils.CopyDirectory(Path.Combine(diaviewSetupDir, "Patch"), Path.Combine(targetDir, "Patch"), true);
            FileUtils.CopyDirectory(Path.Combine(diaviewSetupDir, "OPC"), Path.Combine(targetDir, "OPC"), true);
            FileUtils.CopyDirectory(Path.Combine(diaviewSetupDir, "SQL"), Path.Combine(targetDir, "SQL"), true);

            // Key:资源ID， Value：输出目录
            Dictionary<string, string> dicResouceOutput = new Dictionary<string, string>();
            dicResouceOutput.Add("ProjectPack.SetupResource.DIAViewSetup.dat", Path.Combine(_outputDir, "DIAViewSetup.exe"));

            // 把资源从程序集中释放到指定目录下
            Assembly assm = Assembly.GetExecutingAssembly();
            foreach (KeyValuePair<string, string> keyValuePair in dicResouceOutput)
            {
                using (Stream istr = assm.GetManifestResourceStream(keyValuePair.Key))
                {
                    using (FileStream fs = new FileStream(keyValuePair.Value, FileMode.Create))
                    {
                        byte[] bytes = new byte[istr.Length];
                        istr.Read(bytes, 0, bytes.Length);

                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 删除编译临时文件
        /// </summary>
        private void DeleteCompileTempFile()
        {
            string tempDir = System.Environment.GetEnvironmentVariable("TEMP");
            tempDir = Path.Combine(tempDir, "DiaviewPack");

            if (Directory.Exists(tempDir))
            {
                FileUtils.DeleteDirectory(tempDir);
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {

        }
    }
}
