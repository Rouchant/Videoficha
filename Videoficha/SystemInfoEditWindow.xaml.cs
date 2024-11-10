using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Videoficha
{
    public partial class SystemInfoEditWindow : Window
    {
        private List<string> _systemInfo;
        private readonly string _filePath;
        private DispatcherTimer inactivityTimer; // Temporizador para detectar inactividad
        private const int InactivityThreshold = 300000; // 5 minutos en milisegundos

        public SystemInfoEditWindow(List<string> systemInfo, string filePath)
        {
            InitializeComponent();
            _systemInfo = systemInfo;
            _filePath = filePath;

            // Cargar los valores actuales en los TextBox
            ModelTextBox.Text = _systemInfo[0];
            OSTextBox.Text = _systemInfo[1];
            ProcessorTextBox.Text = _systemInfo[2];
            RAMTextBox.Text = _systemInfo[3];
            StorageTextBox.Text = _systemInfo[4];
            GraphicsTextBox.Text = _systemInfo[5];

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Actualizar la lista de información del sistema con los valores editados
            _systemInfo[0] = ModelTextBox.Text;
            _systemInfo[1] = OSTextBox.Text;
            _systemInfo[2] = ProcessorTextBox.Text;
            _systemInfo[3] = RAMTextBox.Text;
            _systemInfo[4] = StorageTextBox.Text;
            _systemInfo[5] = GraphicsTextBox.Text;

            // Guardar los valores actualizados en el archivo de información del sistema
            File.WriteAllLines(_filePath, _systemInfo);
            Close();
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
