using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SunriseLauncher.Models
{
    public class ProgressState : INotifyPropertyChanged
    {

        public ProgressState()
        {
            Desc = "wack";
        }

        private object _lock = new object();

        private int index;

        private FileProgressState[] fileStates;

        public void SetFiles(int count)
        {
            index = 0;
            fileStates = new FileProgressState[count];
            Progress = 0;
            ProgressMax = count;
        }

        public void Update(int i, FileProgressState state)
        {
            fileStates[i] = state;
            if (i == index)
            {
                lock (_lock)
                {
                    while (fileStates[index].Complete)
                    {
                        Progress++;
                        index++;
                        if (index >= fileStates.Length)
                        {
                            Clear();
                            return;
                        }
                    }
                    Desc = fileStates[index].Desc;
                    FileProgress = fileStates[index].Progress;
                    FileProgressMax = fileStates[index].Max;
                }
            }
        }

        public void Clear()
        {
            Desc = null;
            Progress = 0;
            ProgressMax = 0;
            FileProgress = 0;
            FileProgressMax = 0;
        }

        public void Update(int i, long progress)
        {
            fileStates[i].Progress = progress;
            if (i == index)
            {
                FileProgress = progress;
            }
        }

        public void Update(int i, bool complete)
        {
            var state = fileStates[i];
            state.Complete = complete;
            Update(i, state);
        }

        private string desc;
        [JsonIgnore]
        public string Desc
        {
            get => desc;
            set
            {
                desc = value;
                NotifyPropertyChanged();
            }
        }

        private int progress;
        [JsonIgnore]
        public int Progress
        {
            get => progress;
            set
            {
                progress = value;
                NotifyPropertyChanged();
            }
        }

        private int progressMax;
        [JsonIgnore]
        public int ProgressMax
        {
            get => progressMax;
            set
            {
                progressMax = value;
                NotifyPropertyChanged();
            }
        }

        private long fileProgress;
        [JsonIgnore]
        public long FileProgress
        {
            get => fileProgress;
            set
            {
                fileProgress = value;
                NotifyPropertyChanged();
            }
        }

        private long fileProgressMax;
        [JsonIgnore]
        public long FileProgressMax
        {
            get => fileProgressMax;
            set
            {
                fileProgressMax = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public struct FileProgressState
    {
        public string Desc;
        public long Progress;
        public long Max;
        public bool Complete;
    }
}
