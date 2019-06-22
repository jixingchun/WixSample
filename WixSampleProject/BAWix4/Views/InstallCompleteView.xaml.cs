namespace CustomBA.Views
{
    using CustomBA.ViewModels;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// InstallCompleteView.xaml 的交互逻辑
    /// </summary>
    public partial class InstallCompleteView : Window
    {
        public InstallCompleteView(InstallCompleteViewModel viewModel)
        {
            this.DataContext = viewModel;

            this.Loaded += (sender, e) => WixBA.Model.Engine.CloseSplashScreen();
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown();

            this.InitializeComponent();

            viewModel.ViewWindowHandle = new WindowInteropHelper(this).EnsureHandle();
        }
    }
}
