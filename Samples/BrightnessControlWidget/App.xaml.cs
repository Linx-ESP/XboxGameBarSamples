using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Gaming.XboxGameBar;

namespace BrightnessControlWidget
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private XboxGameBarWidget widget1 = null;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            XboxGameBarWidgetActivatedEventArgs widgetArgs = null;
            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolArgs = args as IProtocolActivatedEventArgs;
                string scheme = protocolArgs.Uri.Scheme;
                if (scheme.Equals("ms-gamebarwidget"))
                {
                    widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
                }
            }
            if (widgetArgs != null)
            {
                if (widgetArgs.IsLaunchActivation)
                {
                    var rootFrame = new Frame();
                    rootFrame.NavigationFailed += OnNavigationFailed;
                    Window.Current.Content = rootFrame;

                    // Create Game Bar widget object which bootstraps the connection with Game Bar
                    widget1 = new XboxGameBarWidget(
                        widgetArgs,
                        Window.Current.CoreWindow,
                        rootFrame);
                    rootFrame.Navigate(typeof(Widget1));

                    Window.Current.Closed += Widget1Window_Closed;

                    Window.Current.Activate();
                }
            }
        }

        private void Widget1Window_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            widget1 = null;
            Window.Current.Closed -= Widget1Window_Closed;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.
        /// </summary>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            widget1 = null;
            deferral.Complete();
        }
    }
}
