using System;
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Videoficha.Features.Kiosk.ViewModels;
using Videoficha.Infrastructure.Services;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class FileSelectionWindow : ContentDialog
    {
        private readonly FileSelectionViewModel _viewModel;
        private readonly IntPtr _parentHwnd;

        public string? VideoFilePath { get; private set; }

        public FileSelectionWindow() : this(IntPtr.Zero) { }

        public FileSelectionWindow(IntPtr parentHwnd)
        {
            InitializeComponent();
            _parentHwnd = parentHwnd;
            _viewModel = new FileSelectionViewModel(new ConfigService(), new SystemProvider());
            this.DataContext = _viewModel;
        }

        private async void SelectVideoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                picker.FileTypeFilter.Add(".mp4");
                picker.FileTypeFilter.Add(".wmv");
                picker.FileTypeFilter.Add(".avi");

                var hwnd = _parentHwnd != IntPtr.Zero ? _parentHwnd : Process.GetCurrentProcess().MainWindowHandle;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    _viewModel.VideoPath = file.Path;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir el selector de video: {ex.Message}");
            }
        }

        private async void SelectLogoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".svg");

                var hwnd = _parentHwnd != IntPtr.Zero ? _parentHwnd : Process.GetCurrentProcess().MainWindowHandle;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    _viewModel.DistributorLogoPath = file.Path;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir el selector de logo: {ex.Message}");
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string fieldName)
            {
                btn.IsEnabled = false;
                await _viewModel.RestoreFieldAsync(fieldName);
                btn.IsEnabled = true;
            }
        }

        private async void SelectInactivityVideoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                picker.FileTypeFilter.Add(".mp4");
                picker.FileTypeFilter.Add(".wmv");
                picker.FileTypeFilter.Add(".avi");

                var hwnd = _parentHwnd != IntPtr.Zero ? _parentHwnd : Process.GetCurrentProcess().MainWindowHandle;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    _viewModel.InactivityVideoPath = file.Path;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al abrir el selector de video de inactividad: {ex.Message}");
            }
        }

        private void RestoreMainVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RestoreDefaultMainVideo();
        }

        private void RestoreInactivityVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RestoreDefaultInactivityVideo();
        }

        private void RestoreLogoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RestoreDefaultLogo();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _viewModel.Save();
            VideoFilePath = _viewModel.VideoPath;
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Close is handled automatically
        }
    }
}
