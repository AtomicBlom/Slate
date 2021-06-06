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

        private int _index;

        private FileProgressState[] _fileStates;

        public void SetFiles(int count)
        {
            _index = 0;
            _fileStates = new FileProgressState[count];
            Progress = 0;
            ProgressMax = count;
        }

        public void Update(int i, FileProgressState state)
        {
            _fileStates[i] = state;
            if (i == _index)
            {
                lock (_lock)
                {
                    while (_fileStates[_index].Complete)
                    {
                        Progress++;
                        _index++;
                        if (_index >= _fileStates.Length)
                        {
                            Clear();
                            return;
                        }
                    }
                    Desc = _fileStates[_index].Desc;
                    FileProgress = _fileStates[_index].Progress;
                    FileProgressMax = _fileStates[_index].Max;
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
            _fileStates[i].Progress = progress;
            if (i == _index)
            {
                FileProgress = progress;
            }
        }

        public void Update(int i, bool complete)
        {
            var state = _fileStates[i];
            state.Complete = complete;
            Update(i, state);
        }

        private string _desc;
        [JsonIgnore]
        public string Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                NotifyPropertyChanged();
            }
        }

        private int _progress;
        [JsonIgnore]
        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                NotifyPropertyChanged();
            }
        }

        private int _progressMax;
        [JsonIgnore]
        public int ProgressMax
        {
            get => _progressMax;
            set
            {
                _progressMax = value;
                NotifyPropertyChanged();
            }
        }

        private long _fileProgress;
        [JsonIgnore]
        public long FileProgress
        {
            get => _fileProgress;
            set
            {
                _fileProgress = value;
                NotifyPropertyChanged();
            }
        }

        private long _fileProgressMax;
        [JsonIgnore]
        public long FileProgressMax
        {
            get => _fileProgressMax;
            set
            {
                _fileProgressMax = value;
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
