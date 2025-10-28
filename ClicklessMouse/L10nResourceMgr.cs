using System;
using ClicklessMouse.Properties;
using System.ComponentModel;
using System.Globalization;

namespace ClicklessMouse {
    public class L10nResourceMgr : INotifyPropertyChanged {
        private L10nResourceMgr() {
            Culture = CultureInfo.CurrentCulture;
        }

        public CultureInfo Culture
        {
            get => CultureInfo.CurrentUICulture;
            set
            {
                CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
        }

        public static L10nResourceMgr Instance { get; } = new();

        public object this[string resourceKey]
            => Resources.ResourceManager.GetObject(resourceKey, Culture) ?? Array.Empty<byte>();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}