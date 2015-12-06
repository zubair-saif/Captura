using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Captura
{
    public partial class AppearanceSettings : UserControl, INotifyPropertyChanged
    {
        public AppearanceSettings()
        {
            InitializeComponent();

            AppearanceManager.AccentChanged += () => SelectedAccentColor = AppearanceManager.AccentColor;

            DataContext = this;
        }

        static readonly Color[] AccentColorCollection = new Color[]
        {
            Color.FromRgb(0x8c, 0xbf, 0x26),   // lime
            Color.FromRgb(0x33, 0x99, 0x33),   // green  
            Color.FromRgb(0x00, 0x8a, 0x00),   // emerald
            Color.FromRgb(0x33, 0x99, 0xff),   // blue
            Color.FromRgb(0x00, 0x50, 0xef),   // cobalt
            Color.FromRgb(0x6a, 0x00, 0xff),   // indigo
            Color.FromRgb(0xf4, 0x72, 0xd0),   // pink
            Color.FromRgb(0xd8, 0x00, 0x73),   // magenta
            Color.FromRgb(0xff, 0x00, 0x97),   // magenta
            Color.FromRgb(0xe3, 0xc8, 0x00),   // yellow
            Color.FromRgb(0xf0, 0xa3, 0x0a),   // amber
            Color.FromRgb(0xf0, 0x96, 0x09),   // orange
            Color.FromRgb(0xfa, 0x68, 0x00),   // orange
            Color.FromRgb(0xff, 0x45, 0x00),   // orange red
            Color.FromRgb(0xe5, 0x14, 0x00),   // red
            Color.FromRgb(0xa2, 0x00, 0x25),   // crimson
            Color.FromRgb(0x82, 0x5a, 0x2c),   // brown
            Color.FromRgb(0xa2, 0x00, 0xff),   // purple
        };

        public Color[] AccentColors { get { return AccentColorCollection; } }

        static Color _SelectedAccentColor = AppearanceManager.AccentColor;

        public Color SelectedAccentColor
        {
            get { return _SelectedAccentColor; }
            set
            {
                if (_SelectedAccentColor != value)
                {
                    _SelectedAccentColor = value;
                    OnPropertyChanged("SelectedAccentColor");

                    AppearanceManager.AccentColor = value;
                }
            }
        }

        #region INotifyPropertyChanged
        void OnPropertyChanged(string e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(e));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
