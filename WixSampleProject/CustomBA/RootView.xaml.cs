namespace CustomBA
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;

    /// <summary>
    /// RootView.xaml 的交互逻辑
    /// </summary>
    public partial class RootView : Window
    {
        /// <summary>
        /// RootView
        /// </summary>
        /// <param name="viewModel"></param>
        public RootView(RootViewModel viewModel)
        {
            this.DataContext = viewModel;

            this.Loaded += (sender, e) => WixBA.Model.Engine.CloseSplashScreen();
            this.Closed += (sender, e) => this.Dispatcher.InvokeShutdown(); 

            this.InitializeComponent();

            viewModel.ViewWindowHandle = new WindowInteropHelper(this).EnsureHandle();
        }
    }
}
