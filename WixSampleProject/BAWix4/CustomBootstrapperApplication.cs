using WixToolset.Bootstrapper;

namespace CustomBA
{
    using Threading = System.Windows.Threading;
    using CustomBA.Views;
    using CustomBA.ViewModels;
    using System;

    /// <summary>
    /// The WiX toolset user experience.
    /// </summary>
    public class WixBA : BootstrapperApplication
    {
        /// <summary>
        /// Gets the global model.
        /// </summary>
        static public Model Model { get; private set; }

        /// <summary>
        /// Gets the global view.
        /// </summary>
        static public RootView View { get; private set; }


        /// <summary>
        /// Gets the global view.
        /// </summary>
        static public MainWindow MainWindow { get; private set; }

        /// <summary>
        /// 安装/卸载完成是画面
        /// </summary>
        static public InstallCompleteView CompleteView { get; private set; }


        // TODO: We should refactor things so we dont have a global View.

        /// <summary>
        /// Gets the global dispatcher.
        /// </summary>
        static public Threading.Dispatcher Dispatcher { get; private set; }

        /// <summary>
        /// Starts planning the appropriate action.
        /// </summary>
        /// <param name="action">Action to plan.</param>
        public static void Plan(LaunchAction action)
        {
            WixBA.Model.PlannedAction = action;
            WixBA.Model.Engine.Plan(WixBA.Model.PlannedAction);
        }

        /// <summary>
        /// Thread entry point for WiX Toolset UX.
        /// </summary>
        protected override void Run()
        {
            this.Engine.Log(LogLevel.Verbose, "Running the WiX BA.");
            WixBA.Model = new Model(this);
            WixBA.Dispatcher = Threading.Dispatcher.CurrentDispatcher;
            RootViewModel viewModel = new RootViewModel();

            MainWindowViewModel mainViewModel = new MainWindowViewModel();

            // Kick off detect which will populate the view models.
            this.Engine.Detect();

            // Create a Window to show UI.
            this.Engine.Log(LogLevel.Verbose, "Creating a UI.");
            //WixBA.View = new RootView(viewModel);
            //WixBA.View.Show();

            try
            {
                WixBA.MainWindow = new MainWindow(mainViewModel);
                WixBA.MainWindow.Show();

                this.Engine.Log(LogLevel.Verbose, "Creating a UI sucessful");
            }
            catch(Exception ex)
            {
                this.Engine.Log(LogLevel.Verbose, "Creating a UI failed " + ex);
            }
            
            Threading.Dispatcher.Run();

            this.Engine.Quit(WixBA.Model.Result);
        }
    }
}
