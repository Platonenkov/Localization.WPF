using System;
using System.Linq;

namespace Localization.WPF.TestWPF
{
    public partial class App
    {
        public App()
        {
            SetCulture();
        }

        /// <summary>Set current culture settings</summary>
        private static void SetCulture()
        {
            LocalizationManager.CultureChanging += (s, e) => //Running when application culture is changed
            {
                var culture = e.NewCulture;
                TestWPF.Properties.Resources.Culture = culture;
                //Insert other projects or libraries here
            };
            LocalizationManager.CultureChanged += (s, e) =>
            {
                //Here we save the settings that will be applied when the application starts
                TestWPF.Properties.Settings.Default.Culture = e.NewCulture.Name;
                TestWPF.Properties.Settings.Default.Save();
            };

            //Here we load the settings that was saved at last time
            var settings = TestWPF.Properties.Settings.Default;
            var last_culture = settings.Culture;
            var new_culture = last_culture == string.Empty ? "ru-RU" : last_culture;

            //or was sent in command line arguments
            var args = Environment.GetCommandLineArgs();
            if (args.Contains("-en")) new_culture = "en-US";
            else if (args.Contains("-rus")) new_culture = "ru-RU";

            LocalizationManager.ChangeCulture(new_culture);
        }

    }

}
