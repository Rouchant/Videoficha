using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class ReturnWindow : Window
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        private readonly Window _mainWindow;

        public ReturnWindow(Window mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            
            if (this.Content is FrameworkElement root)
            {
                root.DataContext = _mainWindow.Content is FrameworkElement mainRoot ? mainRoot.DataContext : null;
            }
            
            // Configurar ventana flotante sin bordes en la esquina inferior derecha
            ConfigureWindow();
        }

        private void ConfigureWindow()
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Hacer que sea sin bordes, no redimensionable y siempre al frente
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(false, false);
                presenter.IsAlwaysOnTop = true;
                presenter.IsResizable = false;
            }

            int width = 300;
            int height = 120;

            // Obtener dimensiones de pantalla y posicionar
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            int x = screenWidth - width - 20;
            int y = screenHeight - height - 20;

            appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, width, height));
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            var mainHwnd = WinRT.Interop.WindowNative.GetWindowHandle(_mainWindow);
            ShowWindow(mainHwnd, SW_RESTORE);
            _mainWindow.Activate();
            this.Close();
        }
    }
}
