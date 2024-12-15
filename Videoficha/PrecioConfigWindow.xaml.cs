using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Videoficha
{
    public partial class PrecioConfigWindow : Window
    {
        public string PrecioNormal { get; private set; }
        public string PrecioOferta { get; private set; }
        public string PrecioExclusivoTarjeta { get; private set; }
        public bool MostrarPrecio { get; private set; }
        public int PrecioDisplayMode { get; private set; }

        private DispatcherTimer inactivityTimer;
        private const int InactivityThreshold = 300000; // 5 minutes in milliseconds

        public PrecioConfigWindow(string precioNormal, string precioOferta, string precioExclusivoTarjeta, bool mostrarPrecio, int precioDisplayMode)
        {
            InitializeComponent();
            PrecioNormalTextBox.Text = precioNormal;
            PrecioOfertaTextBox.Text = precioOferta;
            PrecioExclusivoTarjetaTextBox.Text = precioExclusivoTarjeta;
            MostrarPrecioToggleButton.IsChecked = mostrarPrecio;
            PrecioDisplayModeComboBox.SelectedIndex = precioDisplayMode;

            // Initialize the inactivity timer
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;

            // Subscribe to events to detect activity
            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;

            // Start the inactivity timer
            inactivityTimer.Start();
        }

        private void MostrarPrecioToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            MostrarPrecioToggleButton.Background = System.Windows.Media.Brushes.Green;
            MostrarPrecioToggleButton.Foreground = System.Windows.Media.Brushes.White;
        }

        private void MostrarPrecioToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            MostrarPrecioToggleButton.Background = System.Windows.Media.Brushes.LightGray;
            MostrarPrecioToggleButton.Foreground = System.Windows.Media.Brushes.Black;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            PrecioNormal = PrecioNormalTextBox.Text;
            PrecioOferta = PrecioOfertaTextBox.Text;
            PrecioExclusivoTarjeta = PrecioExclusivoTarjetaTextBox.Text;
            MostrarPrecio = MostrarPrecioToggleButton.IsChecked ?? false;
            PrecioDisplayMode = PrecioDisplayModeComboBox.SelectedIndex;

            DialogResult = true;
            Close();
        }

        // Method to reset the inactivity timer
        private void ResetInactivityTimer(object sender, EventArgs e)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        // Event that triggers when the inactivity timer reaches the threshold
        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}