using System.Windows;
using System.Windows.Input;

namespace CustomBA.ViewModels
{
    public  class InstallOptionsViewModel: PropertyNotifyBase
    {
        private bool _installSqlServer;
        public bool InstallSqlServer
        {
            get => _installSqlServer;
            set
            {
                if(_installSqlServer != value)
                {
                    _installSqlServer = value;
                    base.OnPropertyChanged("InstallSqlServer");
                }
            }
        }


        private bool installOpcServer;
        public bool InstallOpcServer
        {
            get => installOpcServer;
            set
            {
                installOpcServer = value;
                base.OnPropertyChanged("InstallOpcServer");
            }
        }

        public InstallOptionsViewModel()
        {
            Init();
            //WixBA.Model.InstallSqlServer = false.ToString();
            WixBA.Model.InstallSqlServer = "";
        }

        private void Init()
        {
            InstallSqlServer = true;
            installOpcServer = false;
            InstallSenseLock = false;
        }

        public bool InstallSenseLock { get; set; }


        ICommand _confirmCommand;
        public ICommand ConfirmCommand
        {
            get
            {
                if (this._confirmCommand == null)
                {
                    this._confirmCommand = new RelayCommand(Confirm, (x)=> true);
                }

                return this._confirmCommand;
            }
        }

        private void Confirm(object para)
        {
            WixBA.Model.InstallSqlServer = this.InstallSqlServer.ToString();
            Window win = para as Window;
            if (win != null)
            {
                win.Hide();
            }
        }
    }
}
