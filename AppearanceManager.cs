using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Captura
{
    /// <summary>
    /// Manages the theme, font size and accent colors for a Modern UI application.
    /// </summary>
    public static class AppearanceManager
    {
        const string KeyAccentColor = "AccentColor",
            KeyAccent = "Accent";

        static ResourceDictionary GetThemeDictionary()
        {
            // determine the current theme by looking at the app resources and return the first dictionary having the resource key 'WindowBackground' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("WindowBackground")
                    select dict).FirstOrDefault();
        }

        static Uri GetThemeSource()
        {
            var dict = GetThemeDictionary();
            if (dict != null) return dict.Source;

            // could not determine the theme dictionary
            return null;
        }

        static void SetThemeSource(Uri source)
        {
            if (source == null) throw new ArgumentNullException("source");

            var oldThemeDict = GetThemeDictionary();
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDict = new ResourceDictionary { Source = source };

            // if theme defines an accent color, use it
            var accentColor = themeDict[KeyAccentColor] as Color?;
            if (accentColor.HasValue)
                // remove from the theme dictionary and apply globally if useThemeAccentColor is true
                themeDict.Remove(KeyAccentColor);

            // add new before removing old theme to avoid dynamicresource not found warnings
            dictionaries.Add(themeDict);

            // remove old theme
            if (oldThemeDict != null) dictionaries.Remove(oldThemeDict);
        }
        
        /// <summary>
        /// Gets or sets the accent color.
        /// </summary>
        public static Color AccentColor
        {
            get
            {
                var accentColor = Application.Current.Resources[KeyAccentColor] as Color?;

                if (accentColor.HasValue)
                    return accentColor.Value;

                // default color: teal
                return Color.FromArgb(0xff, 0x1b, 0xa1, 0xe2);
            }
            set
            {
                // set accent color and brush resources
                Application.Current.Resources[KeyAccentColor] = value;
                Application.Current.Resources[KeyAccent] = new SolidColorBrush(value);

                // re-apply theme to ensure brushes referencing AccentColor are updated
                var themeSource = GetThemeSource();
                if (themeSource != null) SetThemeSource(themeSource);

                if (AccentChanged != null) AccentChanged();
            }
        }

        public static event Action AccentChanged;
    }
}
