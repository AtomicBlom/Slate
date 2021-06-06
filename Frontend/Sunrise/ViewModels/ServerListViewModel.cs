using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using SunriseLauncher.Models;
using SunriseLauncher.Views;
using ReactiveUI;
using SunriseLauncher.Services;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace SunriseLauncher.ViewModels
{
    public class ServerListViewModel : ViewModelBase
    {
        private ServerFile serverFile = new ServerFile();
        private FileUpdater fileUpdater = new FileUpdater();
        private Launcher launcher = new Launcher();

        public ServerListViewModel(Window window) : base(window)
        {
            window.Closed += Window_Closed;
            window.Opened += Window_Opened;

            var file = serverFile.Load();
            if (file != null)
            {
                Items = new ObservableCollection<Server>(file.Servers);
                SelectedItem = Items.FirstOrDefault(x => x.ManifestURL == file.Selected);
            } 
            else Items = new ObservableCollection<Server>();
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            if (Items.Count == 0)
                OpenAddDialog();
            else
                RefreshServers();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            serverFile.Save(Items, selectedItem?.ManifestURL);
        }

        public ObservableCollection<Server> Items { get; }

        private Server selectedItem;
        public Server SelectedItem
        {
            get => selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedItem, value);
            }
        }

        public async void OpenAddDialog()
        {
            var dialog = new EditServerView();
            dialog.Title = "Add New Server";
            var vm = new EditServerViewModel(dialog, null);
            dialog.DataContext = vm;
            var dialogResult = await dialog.ShowDialog<string>(Window);
            if (dialogResult == "save")
            {
                if (Items.Any(x => x.ManifestURL == vm.ManifestURL))
                {
                    var msgbox = new MessageBoxView(string.Format("Cannot add '{0}' because it is already on your server list.", vm.ManifestURL), null, false);
                    await msgbox.ShowDialog(Window);
                    return;
                }

                var server = new Server();
                server.InstallPath = vm.InstallPath;
                server.ManifestURL = vm.ManifestURL;
                server.Launch = vm.LaunchOption.Name;
                server.Metadata = vm.Metadata;
                server.Metadata.Version = null;
                Items.Add(server);
                SelectedItem = server;

                var updateResult = await fileUpdater.UpdateAsync(server, true);
                if (!updateResult.Success && !string.IsNullOrEmpty(updateResult.Message))
                {
                    var msgbox = new MessageBoxView(updateResult.Message, "See log.txt for details.", false);
                    await msgbox.ShowDialog(Window);
                }
            }
        }

        public async void OpenEditDialog(Server server)
        {
            var dialog = new EditServerView();
            dialog.Title = "Edit Server";
            var vm = new EditServerViewModel(dialog, server);
            dialog.DataContext = vm;
            var result = await dialog.ShowDialog<string>(Window);
            if (result == "save")
            {

                if (Items.Any(x => x.ManifestURL == vm.ManifestURL && x != server))
                {
                    var msgbox = new MessageBoxView(string.Format("Cannot change to '{0}' because it is already on your server list.", vm.ManifestURL), null, false);
                    await msgbox.ShowDialog(Window);
                    return;
                }

                //preserve version only if install path or manifesturl didn't change
                if (server.InstallPath != vm.InstallPath || server.ManifestURL != vm.ManifestURL)
                    vm.Metadata.Version = null;
                else
                    vm.Metadata.Version = server.Metadata.Version;

                server.InstallPath = vm.InstallPath;
                server.ManifestURL = vm.ManifestURL;
                server.Launch = vm.LaunchOption.Name;
                server.Metadata = vm.Metadata;

                var updateResult = await fileUpdater.UpdateAsync(server, false);
                if (!updateResult.Success && !string.IsNullOrEmpty(updateResult.Message))
                {
                    var msgbox = new MessageBoxView(updateResult.Message, "See log.txt for details.", false);
                    await msgbox.ShowDialog(Window);
                }
            }
        }

        public async void OpenRemoveDialog(Server server)
        {
            var msg = String.Format("Are you sure you wish to remove {0}?", server.Launch);
            var dialog = new MessageBoxView(msg, "This will not remove any installed files.", true);
            var result = await dialog.ShowDialog<string>(Window);
            if (result == "ok")
            {
                Items.Remove(server);
            }
        }

        public async void CancelUpdate()
        {
            if (SelectedItem == null)
                return;

            SelectedItem.CancellationTokenSource.Cancel();
        }

        public async void RefreshServers()
        {
            foreach (var server in Items.Where(x => x.State != State.Updating))
            {
                if (server.State != State.Updating) 
                    server.State = State.Unchecked;
            }

            foreach (var server in Items.Where(x => x.State == State.Unchecked))
            {
                var updateResult = await fileUpdater.UpdateAsync(server, false);
                if (!updateResult.Success && !string.IsNullOrEmpty(updateResult.Message))
                {
                    var msgbox = new MessageBoxView(updateResult.Message, "See log.txt for details.", false);
                    await msgbox.ShowDialog(Window);
                }
            }
        }

        public async void VerifyFiles()
        {
            if (SelectedItem == null)
                return;

            var updateResult = await fileUpdater.UpdateAsync(SelectedItem, true);
            if (!updateResult.Success && !string.IsNullOrEmpty(updateResult.Message))
            {
                var msgbox = new MessageBoxView(updateResult.Message, "See log.txt for details.", false);
                await msgbox.ShowDialog(Window);
            }
        }

        public async void Launch()
        {
            if (SelectedItem == null || SelectedItem.State != State.Ready)
                return;

            //check manifest has updated since app has opened
            //even if this step fails, launch anyway in case manifest server is down
            var updateResult = await fileUpdater.UpdateAsync(SelectedItem, false);
            if (!updateResult.Success && !string.IsNullOrEmpty(updateResult.Message))
            {
                var msgbox = new MessageBoxView(updateResult.Message, "See log.txt for details.", false);
                await msgbox.ShowDialog(Window);
            }

            var launch = SelectedItem.Metadata.LaunchOptions.FirstOrDefault(x => x.Name == SelectedItem.Launch);
            if (launch == null)
                return;

            launcher.Launch(SelectedItem, launch);

            Window.WindowState = WindowState.Minimized;
        }

    }
}