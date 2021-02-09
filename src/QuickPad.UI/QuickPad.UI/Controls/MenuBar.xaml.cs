using System;
using QuickPad.Mvvm.Commands;
using QuickPad.Mvvm.Models;
using QuickPad.Mvvm.ViewModels;
using QuickPad.UI.Helpers;
using QuickPad.UI.Theme;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace QuickPad.UI.Controls
{
    public sealed partial class MenuBar : UserControl
    {
        public IVisualThemeSelector VtSelector => VisualThemeSelector.Current;

        public WindowsSettingsViewModel Settings => App.Settings;

        public QuickPadCommands<StorageFile, IRandomAccessStream> Commands => App.Commands;

        public DocumentViewModel<StorageFile, IRandomAccessStream> ViewModel
        {
            get => DataContext as DocumentViewModel<StorageFile, IRandomAccessStream>;
            set
            {
                if (value == null || DataContext == value) return;

                DataContext = value;
            }
        }

        public DocumentModel<StorageFile, IRandomAccessStream> ViewModelDocument => ViewModel.Document;

        public MenuBar()
        {
            this.InitializeComponent();

            Settings.PropertyChanged += SettingsOnPropertyChanged;

            AddCachedItemsToMenu();
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WindowsSettingsViewModel.CurrentMode):
                    Bindings.Update();
                    break;
            }
        }

        private async void AddCachedItemsToMenu()
        {
            StorageFolder cacheFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("cachedfiles", CreationCollisionOption.OpenIfExists);

            IReadOnlyList<StorageFile> rawFiles = await cacheFolder.GetFilesAsync();
            List<StorageFile> items = new List<StorageFile>(rawFiles);

            items = await DeleteExcessiveFiles(items);

            foreach (var file in items)
            {
                var command = new SimpleCommand<DocumentViewModel<StorageFile, IRandomAccessStream>>
                {
                    // TODO: Is there any other way to access WindowsDocumentManager?
                    Executioner = (vm) => App.ServiceProvider.GetService<WindowsDocumentManager>().LoadFile(ViewModel, file)
                };

                CachedFilesSubMenu.Items.Add(new MenuFlyoutItem()
                {
                    Text = $"({System.IO.Path.GetExtension(file.Name).ToUpper()}) {System.IO.Path.GetFileNameWithoutExtension(file.Name)}",
                    Command = command,
                    CommandParameter = ViewModel
                });
            }
        }

        private async Task<List<StorageFile>> DeleteExcessiveFiles(List<StorageFile> items)
        {
            const int MAX_CACHEDITEMS_COUNT = 10;

            bool excessiveFiles = items.Count > MAX_CACHEDITEMS_COUNT;
            
            if (!excessiveFiles)
                return items; // No files above the limit

            items.Sort((i1, i2) => i1.DateCreated.CompareTo(i2.DateCreated));

            // Date is ascending: (oldest -> newest)
            
            for (int i = 0; i < items.Count; i++)
            {
                await items[i].DeleteAsync(StorageDeleteOption.PermanentDelete);
                items.RemoveAt(i);

                if (items.Count <= MAX_CACHEDITEMS_COUNT)
                {
                    break;
                }
            }

            return items;
        }
    }
}