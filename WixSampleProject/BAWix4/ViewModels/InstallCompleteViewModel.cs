using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace CustomBA.ViewModels
{
    public class InstallCompleteViewModel : PropertyNotifyBase
    {
        public IntPtr ViewWindowHandle { get; set; }

        ICommand installSqlServerCommand;

        private bool isNeedInstallSqlServer = false;
        public InstallCompleteViewModel()
        {
            CustomAction customAction = new CustomAction();
            isNeedInstallSqlServer = customAction.IsNeedInstallSqlServer();
        }

        public ICommand InstallSqlServerCommand
        {
            get
            {
                if (this.installSqlServerCommand == null)
                {
                    this.installSqlServerCommand = new RelayCommand(InstallSqlServer, CanInstallSqlServer);
                }

                return this.installSqlServerCommand;
            }
        }

        void InstallSqlServer(object param)
        {
            CustomAction customAction = new CustomAction();
            string sqlSetupDir = Environment.CurrentDirectory;
            isNeedInstallSqlServer = customAction.InstallSqlServer(sqlSetupDir);
        }

        bool CanInstallSqlServer(object param)
        {
            return isNeedInstallSqlServer;
        }
    }
}
