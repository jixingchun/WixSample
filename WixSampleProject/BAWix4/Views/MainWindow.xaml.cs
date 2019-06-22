using CustomBA.ViewModels;
using System.Windows;
using System.Windows.Interop;

namespace CustomBA.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;

            this.Loaded += (sender, e) => WixBA.Model.Engine.CloseSplashScreen();
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();

            this.InitializeComponent();

            viewModel.ViewWindowHandle = new WindowInteropHelper(this).EnsureHandle();
        }
    }
}
