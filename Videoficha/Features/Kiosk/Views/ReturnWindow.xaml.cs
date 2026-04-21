using System.Windows;

namespace Videoficha.Features.Kiosk.Views
{
    public partial class ReturnWindow : Window
    {
        private readonly Window _mainWindow;

        public ReturnWindow(Window mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            this.DataContext = _mainWindow.DataContext;
            
            // Posicionar en la esquina inferior derecha
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            this.Left = screenWidth - this.Width - 20;
            this.Top = screenHeight - this.Height - 20;
        }

        private void Return_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.WindowState = WindowState.Maximized;
            _mainWindow.Activate();
            this.Close();
        }
    }
}
