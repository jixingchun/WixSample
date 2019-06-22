using CustomBA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CustomBA.Views
{
    /// <summary>
    /// InstallOtherComponentView.xaml 的交互逻辑
    /// </summary>
    public partial class InstallOptionsView : Window
    {
        public InstallOptionsView(InstallOptionsViewModel viewModel)
        {
            this.DataContext = viewModel;

            this.Loaded += (sender, e) => WixBA.Model.Engine.CloseSplashScreen();
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();

            this.InitializeComponent();
        }
    }
}
