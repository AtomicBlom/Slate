using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using ReactiveUI;

namespace SunriseLauncher.Models
{
    public class Server : INotifyPropertyChanged
    {
        [JsonPropertyName("manifest_url")]
        public string ManifestURL { get; set; }
        [JsonPropertyName("install_path")]
        public string InstallPath { get; set; }
        [JsonPropertyName("metadata")]
        public ManifestMetadata Metadata { get; set; }

        private string launch;
        [JsonPropertyName("launch")]
        public string Launch
        {
            get => launch;
            set 
            {
                launch = value;
                NotifyPropertyChanged();
            }
        }

        private State state;
        [JsonIgnore]
        public State State
        {
            get => state;
            set
            {
                state = value;
                NotifyPropertyChanged();
            }
        }

        private bool progressShow;
        [JsonIgnore]
        public bool ProgressShow
        {
            get => progressShow;
            set
            {
                progressShow = value;
                NotifyPropertyChanged();
            }
        }

        private ProgressState progressState;
        [JsonIgnore]
        public ProgressState ProgressState
        {
            get => progressState;
            set
            {
                progressState = value;
                NotifyPropertyChanged();
            }
        }

        [JsonIgnore]
        public CancellationTokenSource CancellationTokenSource { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Server()
        {
            ProgressState = new ProgressState();
            ProgressState.Desc = "waky";
        }
    }

    public enum State
    {
        Unchecked = 0,
        Ready = 1,
        Updating = 2,
        Error = 3
    }
}
