using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace QuickPad
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
#if !DEBUG
            AppCenter.Start("64a87afd-a838-4cd0-a46d-b3ea528dd53d", typeof(Analytics), typeof(Crashes)); //send analytics to app center
#endif
        }

        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            var rootFrame = new Frame();
            rootFrame.Navigate(typeof(MainPage), args);
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    if (!string.IsNullOrEmpty(e.Arguments))
                    {
                        rootFrame.Navigate(typeof(MainPage), GetParameterURI(e.Arguments));
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                }
                Window.Current.Activate();
            }
        }

#region OnActivated

        protected override void OnActivated(IActivatedEventArgs args)
        {
            string additionParameter = "";
            switch (args.Kind)
            {
                case ActivationKind.CommandLineLaunch:
                    CommandLineActivatedEventArgs cmdLineArgs =
                        args as CommandLineActivatedEventArgs;
                    CommandLineActivationOperation operation = cmdLineArgs.Operation;
                    string cmdLineString = operation.Arguments;
                    string activationPath = operation.CurrentDirectoryPath;
                    break;
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs uriArg = args as ProtocolActivatedEventArgs;
                    additionParameter = GetParameterURI(uriArg.Uri.OriginalString);
                    break;
            }
            //Navigate
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }
            rootFrame.Navigate(typeof(MainPage), additionParameter);

            Window.Current.Activate();

        }

#endregion

#region Extract URI

        public string GetParameterURI(string fullURI)
        {
            string output = "";
            if (fullURI.Contains("//"))
            {
                output = fullURI.Substring(fullURI.IndexOf("//"));
                if (output.Contains("/"))
                {
                    output = output.Replace("/", string.Empty);
                }
            }
            return output;
        }

#endregion

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

    }
}
