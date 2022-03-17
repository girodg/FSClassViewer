using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace FSClassViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static AppSettings _appSettings = new AppSettings();
        public static string LastFilePath = string.Empty;
        public static string RosterRootPath = string.Empty;
        public static string GradingRootPath = string.Empty;
        public static string ContactMessage = string.Empty;
        public static string ContactList = string.Empty;
        public static ObservableCollection<RecentClass> RecentClasses = new ObservableCollection<RecentClass>();

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            LoadSettings();
            base.OnStartup(e);

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name)));

            //RunInReleaseMode();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SaveSettings();
            base.OnExit(e);
        }
        //static void Application_Exit(object sender, ExitEventArgs e)
        //{
        //    //if (_updateSettings)
        //    {
        //        SaveSettings();
        //    }
        //}

        private void LoadSettings()
        {
            try
            {
                _appSettings.Reload();
                if (_appSettings["LastFilePath"] != null)
                {
                    LastFilePath = _appSettings.LastFilePath;
                }
                if (_appSettings["RecentClasses"] != null)
                {
                    RecentClasses = _appSettings.RecentClasses;
                }
                if (_appSettings["RosterRootPath"] != null)
                {
                    RosterRootPath = _appSettings.RosterRootPath;
                }
                if (_appSettings["GradingRootPath"] != null)
                {
                    GradingRootPath = _appSettings.GradingRootPath;
                }
                if (_appSettings["ContactMessage"] != null)
                {
                    ContactMessage = _appSettings.ContactMessage;
                }
                if (_appSettings["ContactList"] != null)
                {
                    ContactList = _appSettings.ContactList;
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;
                File.Delete(filename);
                ////_appSettings = new AppSettings();
                ////_appSettings.Reload();
                ////force the app to restart so the config will load correctly
                //SingletonAppManager.ShouldRestartApp = true;
                //System.Windows.Forms.Application.Restart();
                //Process.GetCurrentProcess().Kill();
            }
        }

        private static void SaveSettings()
        {
            try
            {
                _appSettings.LastFilePath = LastFilePath;
                _appSettings.RecentClasses = RecentClasses;
                _appSettings.RosterRootPath = RosterRootPath;
                _appSettings.GradingRootPath = GradingRootPath;
                _appSettings.ContactMessage = ContactMessage;
                _appSettings.ContactList = ContactList;
                _appSettings.Save();//if the user.config file is hidden or read-only, this will throw an exception
            }
            catch (Exception)
            {
                //don't allow exceptions to prevent the app from closing
            }
        }
    }
}
