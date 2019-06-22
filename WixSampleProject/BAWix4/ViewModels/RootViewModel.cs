

namespace CustomBA
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using CustomBA.ViewModels;
    using CustomBA.Views;
    using WinForm = System.Windows.Forms;

    /// <summary>
    /// The errors returned from the engine
    /// </summary>
    public enum Error
    {
        UserCancelled = 1223,
    }

    /// <summary>
    /// The model of the root view in WixBA.
    /// </summary>
    public class RootViewModel : PropertyNotifyBase
    {
        private ICommand cancelCommand;
        private ICommand closeCommand;
        private ICommand selectInstallDirectoryCommand;

        private bool canceled;
        private InstallationState installState;
        private DetectionState detectState;

        /// <summary>
        /// Creates a new model of the root view.
        /// </summary>
        public RootViewModel()
        {
            this.InstallationViewModel = new InstallationViewModel(this);
            this.ProgressViewModel = new ProgressViewModel(this);
            //this.UpdateViewModel = new UpdateViewModel(this);

            // 默认安装目录为 “C:\Program Files (x86)”
            this.InstallDirectory = WixBA.Model.Engine.StringVariables["ProgramFilesFolder"];

            CustomAction customAction = new CustomAction();
            bool isNeedInstallSqlServer = customAction.IsNeedInstallSqlServer();
            this.InstallSqlServerFlag = isNeedInstallSqlServer;
        }

        public InstallationViewModel InstallationViewModel { get; private set; }
        public ProgressViewModel ProgressViewModel { get; private set; }
        public UpdateViewModel UpdateViewModel { get; private set; }
        public IntPtr ViewWindowHandle { get; set; }

        public ICommand CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new RelayCommand(param => WixBA.View.Close());
                }

                return this.closeCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (this.cancelCommand == null)
                {
                    this.cancelCommand = new RelayCommand(param =>
                    {
                        lock (this)
                        {
                            this.Canceled = (MessageBoxResult.Yes == MessageBox.Show(WixBA.View, "Are you sure you want to cancel?", "WiX Toolset", MessageBoxButton.YesNo, MessageBoxImage.Error));
                        }
                    },
                    param => this.InstallState == InstallationState.Applying);
                }

                return this.cancelCommand;
            }
        }

        public ICommand SelectInstallDirectoryCommand
        {
            get
            {
                if (this.selectInstallDirectoryCommand == null)
                {
                    this.selectInstallDirectoryCommand = new RelayCommand(param =>
                    {
                        WinForm.FolderBrowserDialog folderBrowserDialog = new WinForm.FolderBrowserDialog();
                        if (folderBrowserDialog.ShowDialog() == WinForm.DialogResult.OK)
                        {
                            this.InstallDirectory = folderBrowserDialog.SelectedPath;
                        }
                    },
                    param =>
                    {
                        return this.InstallState != InstallationState.Applying;
                    }

                    );
                }

                return this.selectInstallDirectoryCommand;
            }
        }

        public bool SelectInstallDirectoryEnabled
        {
            get { return this.SelectInstallDirectoryCommand.CanExecute(this); }
        }


        public bool CancelEnabled
        {
            get { return this.CancelCommand.CanExecute(this); }
        }

        public bool Canceled
        {
            get
            {
                return this.canceled;
            }

            set
            {
                if (this.canceled != value)
                {
                    this.canceled = value;
                    base.OnPropertyChanged("Canceled");
                }
            }
        }

        /// <summary>
        /// Gets and sets the detect state of the view's model.
        /// </summary>
        public DetectionState DetectState
        {
            get
            {
                return this.detectState;
            }

            set
            {
                if (this.detectState != value)
                {
                    this.detectState = value;

                    // Notify all the properties derived from the state that the state changed.
                    base.OnPropertyChanged("DetectState");
                    base.OnPropertyChanged("CancelEnabled");
                }
            }
        }

        /// <summary>
        /// Gets and sets the installation state of the view's model.
        /// </summary>
        public InstallationState InstallState
        {
            get
            {
                return this.installState;
            }

            set
            {
                if (this.installState != value)
                {
                    this.installState = value;

                    // Notify all the properties derived from the state that the state changed.
                    base.OnPropertyChanged("InstallState");
                    base.OnPropertyChanged("CancelEnabled");

                    WixBA.Model.Engine.Log(WixToolset.Bootstrapper.LogLevel.Verbose, $"InstallState SET={value.ToString()}");
                }
            }
        }

        /// <summary>
        /// Gets and sets the state of the view's model before apply begins 
        /// in order to return to that state if cancel or rollback occurs.
        /// </summary>
        public InstallationState PreApplyState { get; set; }

        /// <summary>
        /// Gets and sets the path where the bundle is currently installed or will be installed.
        /// </summary>
        public string InstallDirectory
        {
            get
            {
                return WixBA.Model.InstallDirectory;
            }

            set
            {
                if (WixBA.Model.InstallDirectory != value)
                {
                    WixBA.Model.InstallDirectory = value;
                    base.OnPropertyChanged("InstallDirectory");
                }
            }
        }

        bool installSqlServerFlag = false;

        /// <summary>
        /// 是否需要安装Sql server
        /// 1: 安装，0：不安装
        /// </summary>
        public bool InstallSqlServerFlag
        {
            get
            {
                string str = WixBA.Model.InstallSqlServerFlag;
                if ("1".Equals(str) || "true".Equals(str, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (this.installSqlServerFlag != value)
                {
                    installSqlServerFlag = value;
                    if (installSqlServerFlag)
                    {
                        WixBA.Model.InstallSqlServerFlag = "1";
                    }
                    else
                    {
                        WixBA.Model.InstallSqlServerFlag = "0";
                    }

                    base.OnPropertyChanged("InstallSqlServerFlag");
                }
            }
        }

        ICommand _openOptionCommand;
        public ICommand OpenOptionCommand
        {
            get
            {
                if (this._openOptionCommand == null)
                {
                    this._openOptionCommand = new RelayCommand(OpenOptionWindow, (x) => true);
                }

                return this._openOptionCommand;
            }
        }

        private void OpenOptionWindow(object para)
        {
            WixBA.Dispatcher.Invoke(new Action(() =>
            {
                InstallOptionsViewModel optionsViewModel = new InstallOptionsViewModel();
                InstallOptionsView optionsView = new InstallOptionsView(optionsViewModel);
                optionsView.Show();

            }
            ));
            
        }
    }
}
