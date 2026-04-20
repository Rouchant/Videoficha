using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Videoficha.Views
{
    public partial class SystemInfoWindow : Window
    {
        private const string ConfigFolderName = "config"; // Nombre de la carpeta de configuración
        private string ConfigFolderPath; // Ruta completa de la carpeta de configuración
        private string SystemInfoFilePath; // Ruta completa del archivo de información del sistema
        private DispatcherTimer inactivityTimer; // Temporizador para detectar inactividad
        private const int InactivityThreshold = 300000; // 5 minutos en milisegundos

        public SystemInfoWindow() // Constructor sin parámetros
        {
            InitializeComponent();

            // Ruta completa de la carpeta config
            ConfigFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolderName);

            // Ruta completa del archivo systemInfo.txt
            SystemInfoFilePath = Path.Combine(ConfigFolderPath, "systemInfo.txt");

            // Inicializar el temporizador de inactividad
            inactivityTimer = new DispatcherTimer();
            inactivityTimer.Interval = TimeSpan.FromMilliseconds(InactivityThreshold);
            inactivityTimer.Tick += InactivityTimer_Tick;

            // Suscribir los eventos para detectar actividad
            this.MouseMove += ResetInactivityTimer;
            this.KeyDown += ResetInactivityTimer;

            // Cargar información del sistema
            LoadSystemInfoFromFile();

            // Iniciar el temporizador de inactividad
            inactivityTimer.Start(); // <-- Asegura que el temporizador comience a contar desde la creación de la ventana.
        }

        private void LoadSystemInfoFromFile()
        {
            if (File.Exists(SystemInfoFilePath))
            {
                List<string> systemInfo = new List<string>(File.ReadAllLines(SystemInfoFilePath));

                if (systemInfo.Count >= 6) // Asegúrate de que haya suficientes líneas en el archivo
                {
                    PopulateSystemInfo(systemInfo);
                }
                else
                {
                    MessageBox.Show("El archivo de información del sistema no contiene suficientes datos.");
                }
            }
            else
            {
                MessageBox.Show("No se encontró el archivo de información del sistema.");
            }
        }

        private void PopulateSystemInfo(List<string> systemInfo)
        {
            ModelTextBlock.Text = systemInfo[0]; // Modelo
            OsTextBlock.Text = systemInfo[1]; // Sistema Operativo
            ProcessorTextBlock.Text = systemInfo[2]; // Procesador
            RamTextBlock.Text = systemInfo[3]; // RAM
            StorageTextBlock.Text = systemInfo[4]; // Almacenamiento
            GpuTextBlock.Text = systemInfo[5]; // Tarjeta Gráfica
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
