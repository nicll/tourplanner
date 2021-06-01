using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace TourPlanner.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // stops the application from shutting down while still saving to the db
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // corrects culture for all bindings application-wide
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }
    }
}
