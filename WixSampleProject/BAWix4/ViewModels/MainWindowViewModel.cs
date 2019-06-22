using CustomBA.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using WixToolset.Bootstrapper;

namespace CustomBA.ViewModels
{
    public class MainWindowViewModel: RootViewModel
    {
        private object _currentView;
        private object _view1;
        private object _view2;

        /// <summary>
        /// Creates a new model of the root view.
        /// </summary>
        public MainWindowViewModel()
        {
            _view1 = new InstallPage();
            _view2 = new ProcessPage(base.InstallationViewModel);

            WixBA.Model.Engine.Log(LogLevel.Verbose, "MainWindowViewModel DetectState = " + base.DetectState);

            if( DetectionState.Absent.Equals(base.DetectState))
            {
                CurrentView = _view1;
            }
        }

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }

        private ICommand installCommand;
        public ICommand InstallCommand
        {
            get
            {
                if (this.installCommand == null)
                {
                    this.installCommand = new RelayCommand(param =>
                    {
                        CurrentView = _view2;

                        base.InstallationViewModel.InstallCommand.Execute(null);
                    }
                    ,
                    (x) => true
                    );
                }

                return this.installCommand;
            }
        }

        private ICommand uninstallCommand;
        public ICommand UninstallCommand
        {
            get
            {
                if (this.uninstallCommand == null)
                {
                    this.uninstallCommand = new RelayCommand(param =>
                    {
                        base.InstallationViewModel.UninstallCommand.Execute(null);
                    }
                    ,
                    (x) => true
                    );
                }

                return this.uninstallCommand;
            }
        }



    }
}
