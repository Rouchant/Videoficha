using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using System.Windows.Threading;

namespace Videoficha
{
    public partial class LeerFicha : Window
    {
        private DispatcherTimer inactivityTimer; // Temporizador para detectar inactividad
        private const int InactivityThreshold = 300000; // 5 minutos en milisegundos

        public LeerFicha(string pdfPath)
        {
            InitializeComponent();
            InitializeWebView();
            LoadPDF(pdfPath);
            SetWindowSize();
            
            // Inicializar el temporizador de inactividad
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;

            // Suscribir los eventos para detectar actividad
            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;

            // Iniciar el temporizador de inactividad
            inactivityTimer.Start();
        }

        private async void InitializeWebView()
        {
            await pdfWebView.EnsureCoreWebView2Async();
        }

        private void SetWindowSize()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Width = screenWidth * 0.8;
            this.Height = screenHeight * 0.8;

            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
        }

        private void LoadPDF(string pdfPath)
        {
            // Cargar el PDF en el WebView2
            pdfWebView.Source = new Uri($"file:///{pdfPath}");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Método que reinicia el temporizador de inactividad
        private void ResetInactivityTimer(object sender, EventArgs e)
        {
            inactivityTimer.Stop();  // Detener el temporizador
            inactivityTimer.Start(); // Reiniciar el temporizador
        }

        // Evento que se ejecuta cuando el temporizador alcanza el umbral de inactividad
        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            // Cerrar la ventana después de 5 minutos de inactividad
            this.Close();
        }
    }
}
