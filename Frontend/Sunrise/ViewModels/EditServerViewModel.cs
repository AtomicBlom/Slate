using ReactiveUI;
using SunriseLauncher.Models;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Linq;
using SunriseLauncher.Services;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SunriseLauncher.Views;
using System.Threading.Tasks;

namespace SunriseLauncher.ViewModels
{
    public class EditServerViewModel : ViewModelBase
    {
        public EditServerViewModel(Window window, Server server) : base(window)
        {
            if (server == null)
            {
                LaunchOptions = new ObservableCollection<LaunchOption>();
                return;
            }

            LaunchOptions = new ObservableCollection<LaunchOption>(server.Metadata.LaunchOptions);
            LaunchOption = LaunchOptions.FirstOrDefault(x => x.Name == server.Launch);
            manifestURL = server.ManifestURL;
            InstallPath = server.InstallPath;
            Metadata = server.Metadata;
        }

        private LaunchOption launchOption;
        public LaunchOption LaunchOption
        {
            get => launchOption;
            set => this.RaiseAndSetIfChanged(ref launchOption, value);
        }

        public ObservableCollection<LaunchOption> LaunchOptions { get; }

        private string manifestURL;
        public string ManifestURL
        {
            get => manifestURL;
            set
            {
                LaunchOptions.Clear();
                LaunchOption = null;
                Metadata = null;
                this.RaiseAndSetIfChanged(ref manifestURL, value);
            }
        }

        private string installPath;
        public string InstallPath
        {
            get => installPath;
            set => this.RaiseAndSetIfChanged(ref installPath, value);
        }

    public ManifestMetadata Metadata { get; set; }

        public async void FindMetadata()
        {
            if (string.IsNullOrEmpty(ManifestURL))
            {
                var msgbox = new MessageBoxView("Please enter a manifest URL.", null, false);
                await msgbox.ShowDialog(Window);
                return;
            }

            var manifest = MainfestFactory.Get(ManifestURL.TrimEnd());
            if (manifest == null)
            {
                var msgbox = new MessageBoxView("Could not determine manifest schema from URL.", null, false);
                await msgbox.ShowDialog(Window);
                return;
            }

            Metadata = await manifest.GetMetadataAsync();
            if (Metadata == null)
            {
                var msgbox = new MessageBoxView("Could not get manifest data from that location.", null, false);
                await msgbox.ShowDialog(Window);
                return;
            }

            if (!Metadata.Verify())
            {
                var msgbox = new MessageBoxView("Manifest verification failed.", "See log.txt for details", false);
                await msgbox.ShowDialog(Window);
                return;
            }

            LaunchOptions.Clear();
            Metadata.LaunchOptions.ForEach(x => LaunchOptions.Add(x));
            if (LaunchOptions.Count > 0) 
                LaunchOption = LaunchOptions[0];
        }

        public async void BrowseInstallPath()
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Install Path";
            InstallPath = await dialog.ShowAsync(Window);
        }
    }
}
