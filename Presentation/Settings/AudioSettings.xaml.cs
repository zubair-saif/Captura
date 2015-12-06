﻿using ScreenWorks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

namespace Captura
{
    public partial class AudioSettings : UserControl, INotifyPropertyChanged
    {
        public static AudioSettings Instance;

        #region Init
        static AudioSettings()
        {
            if (!App.IsLamePresent) EncodeAudio = false;
            else
            {
                MaxAudioQuality = SharpAviEncoder.LameSupportedBitRatesCount - 1;
                AudioQuality = SharpAviEncoder.LameSupportedBitRatesCount / 2;
            }

            AvailableAudioSources = new ObservableCollection<KeyValuePair<string, string>>();
        }

        public AudioSettings()
        {
            _AvailableAudioSources = AvailableAudioSources;

            InitializeComponent();

            DataContext = this;

            Instance = this;

            if (App.IsLamePresent) AudioQualitySlider.Maximum = MaxAudioQuality;
            else
            {
                AudioQualitySlider.IsEnabled = false;
                EncodeMp3Box.IsEnabled = false;
            }
                        
            RefreshAudioSources();

            AudioSourcesBox.SelectedIndex = 0;
        }
        #endregion

        void AudioVideoSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VideoSettings.Instance.CheckFunctionalityAvailability();
        }

        public static ObservableCollection<KeyValuePair<string, string>> AvailableAudioSources { get; private set; }

        public ObservableCollection<KeyValuePair<string, string>> _AvailableAudioSources { get; private set; }

        public static string SelectedAudioSourceId = "-1";

        public static void RefreshAudioSources()
        {
            AvailableAudioSources.Clear();

            AvailableAudioSources.Add(new KeyValuePair<string, string>("-1", "[No Sound]"));
            
            foreach (var Dev in AudioProvider.EnumerateAudioDevices())
                AvailableAudioSources.Add(Dev);

            if (Instance != null) Instance.AudioSourcesBox.SelectedIndex = 0;
        }

        public string _SelectedAudioSourceId
        {
            get { return SelectedAudioSourceId; }
            set
            {
                if (SelectedAudioSourceId != value)
                {
                    SelectedAudioSourceId = value;
                    OnPropertyChanged("_SelectedAudioSourceId");
                }
            }
        }

        #region Audio
        static readonly int MaxAudioQuality = 0;

        public static int AudioQuality = 0;

        public double _AudioQuality
        {
            get { return AudioQuality; }
            set
            {
                if (AudioQuality != (int)value)
                {
                    AudioQuality = (int)value;
                    OnPropertyChanged("_AudioQuality");
                }
            }
        }

        public static bool EncodeAudio = true;

        public bool _EncodeAudio
        {
            get { return EncodeAudio; }
            set
            {
                if (EncodeAudio != value)
                {
                    EncodeAudio = value;
                    OnPropertyChanged("_EncodeAudio");
                }
            }
        }

        public static bool Stereo = false;

        public bool _Stereo
        {
            get { return Stereo; }
            set
            {
                if (Stereo != value)
                {
                    Stereo = value;
                    OnPropertyChanged("_Stereo");
                }
            }
        }
        #endregion

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
