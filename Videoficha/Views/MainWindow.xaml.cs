using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Videoficha.Core;

namespace Videoficha.Views
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;
        private List<string> systemInfo;
        private const string ConfigFolderName = "config"; // Nombre de la carpeta de configuración
        private string ConfigFolderPath; // Ruta completa de la carpeta de configuración
        private string currentVideoPath;
        
        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = false;
            this.KeyDown += Window_KeyDown;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            // Ruta completa de la carpeta config
            ConfigFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFolderName);

            // Crear la carpeta config si no existe
            CreateConfigFolder();
        
            // Suscribirse al evento PowerModeChanged
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Verifica si se está presionando Control y la flecha arriba
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.S)
                {
                    // Llama al método para mostrar la ventana de selección de archivos
                    ShowFileSelectionWindow();
                }
                else if (e.Key == Key.I)
                {
                    // Abre la ventana de edición de información del sistema
                    ShowSystemInfoEditWindow();
                }
            }
        }

        private void ShowSystemInfoEditWindow()
        {
            var systemInfoEditWindow = new SystemInfoEditWindow(systemInfo, Path.Combine(ConfigFolderPath, "systemInfo.txt"));
            systemInfoEditWindow.Owner = this;
            systemInfoEditWindow.ShowDialog();
        }

        private void ShowFileSelectionWindow()
        {
            FileSelectionWindow fileSelectionWindow = new FileSelectionWindow
            {
                Owner = this
            };

            if (fileSelectionWindow.ShowDialog() == true)
            {
                ProcessSelectedFiles(fileSelectionWindow);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)

        {
            LoadingLabel.Visibility = Visibility.Visible;

            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);

            systemInfo = LoadSystemInfo();

            if (systemInfo == null || !systemInfo.Any())
            {
                systemInfo = await Task.Run(() => GetSystemInfo());
                SaveSystemInfo(systemInfo);
            }

            currentVideoPath = LoadVideoSelection();

            if (!string.IsNullOrEmpty(currentVideoPath) && File.Exists(currentVideoPath))
            {
                PlaySelectedVideo(currentVideoPath);
            }
            else
            {
                SelectFiles();
            }

            LoadingLabel.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            this.KeyDown -= Window_KeyDown;
            this.Loaded -= MainWindow_Loaded;
            this.Closing -= MainWindow_Closing;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume && !string.IsNullOrEmpty(currentVideoPath))
            {
                // Asegúrate de detener el video antes de reiniciar
                videoPlayer.Stop();

                // Reconfigura la fuente y reproduce
                videoPlayer.Source = new Uri(currentVideoPath);
                videoPlayer.Play();
            }
        }

        
        private void SelectFiles()
        {
            FileSelectionWindow fileSelectionWindow = new FileSelectionWindow
            {
                Owner = this
            };

            if (fileSelectionWindow.ShowDialog() == true)
            {
                ProcessSelectedFiles(fileSelectionWindow);
            }
            else
            {
                PlayDefaultVideo();
                ShowDefaultPDF();
            }
        }

        private void ProcessSelectedFiles(FileSelectionWindow fileSelectionWindow)
        {
            string selectedVideoFile = fileSelectionWindow.VideoFilePath;
            string selectedPDFFile = fileSelectionWindow.OtherFilePath;

            if (!string.IsNullOrEmpty(selectedVideoFile))
            {
                SaveVideoSelection(selectedVideoFile);
                PlaySelectedVideo(selectedVideoFile);
            }

            if (!string.IsNullOrEmpty(selectedPDFFile))
            {
                SavePDFSelection(selectedPDFFile);
            }
        }


        private void PlayDefaultVideo()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string defaultVideoPath = Path.Combine(baseDirectory, "Assets", "Samples", "HP.wmv");

            if (File.Exists(defaultVideoPath))
            {
                videoPlayer.Source = new Uri(defaultVideoPath);
                videoPlayer.Play();
            }
        }

        private void ShowDefaultPDF()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string defaultPDFPath = Path.Combine(baseDirectory, "Assets", "Samples", "sample.pdf");

            if (File.Exists(defaultPDFPath))
            {
                LeerFicha leerFichaWindow = new LeerFicha(defaultPDFPath);
                leerFichaWindow.Owner = this;
                leerFichaWindow.ShowDialog();
            }
        }

        private void PlaySelectedVideo(string videoPath)
        {
            currentVideoPath = videoPath;

            if (videoPlayer != null && !string.IsNullOrEmpty(videoPath))
            {
                videoPlayer.Source = new Uri(videoPath);
                videoPlayer.Play();
            }
            else
            {
                MessageBox.Show("El reproductor de video no está inicializado o el archivo no es válido.");
            }
        }

        private void SaveVideoSelection(string videoPath)
        {
            currentVideoPath = videoPath;
            try
            {
                File.WriteAllText(Path.Combine(ConfigFolderPath, "videoSelection.txt"), videoPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la selección del video: {ex.Message}");
            }
        }
        private void SavePDFSelection(string pdfPath)
        {
            try
            {
                File.WriteAllText(Path.Combine(ConfigFolderPath, "pdfSelection.txt"), pdfPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la selección del PDF: {ex.Message}");
            }
        }

        
        private string LoadVideoSelection()
        {
            return File.Exists(Path.Combine(ConfigFolderPath, "videoSelection.txt"))
                ? File.ReadAllText(Path.Combine(ConfigFolderPath, "videoSelection.txt"))
                : string.Empty;
        }

        private string LoadPDFSelection()
        {
            return File.Exists(Path.Combine(ConfigFolderPath, "pdfSelection.txt")) 
                ? File.ReadAllText(Path.Combine(ConfigFolderPath, "pdfSelection.txt")) 
                : string.Empty;
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            videoPlayer.Position = TimeSpan.Zero;
            videoPlayer.Play();
        }

        private void ViewFichaButton_Click(object sender, RoutedEventArgs e)
        {
            string pdfPath = LoadPDFSelection();
            if (!string.IsNullOrEmpty(pdfPath))
            {
                LeerFicha leerFichaWindow = new LeerFicha(pdfPath);
                leerFichaWindow.Owner = this;
                leerFichaWindow.ShowDialog();
            }
            else
            {
                ShowDefaultPDF();
            }
        }


        private void ViewInfoButton_Click(object sender, RoutedEventArgs e)
        {
            SystemInfoWindow systemInfoWindow = new SystemInfoWindow();
            systemInfoWindow.Owner = this;
            systemInfoWindow.ShowDialog();
        }

        private List<string> GetSystemInfo()
        {
            var systemInfo = new List<string>();

            try
            {
                // Obtener el modelo de la computadora
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        systemInfo.Add(item["Model"].ToString());
                    }
                }

                // Obtener el sistema operativo
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        systemInfo.Add(item["Caption"].ToString());
                    }
                }

                // Obtener el procesador
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get())
                    {
                        systemInfo.Add(item["Name"].ToString());
                    }
                }

                // Obtener la memoria RAM y redondearla al múltiplo de 2 más cercano
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        var ramInGB = Math.Ceiling(Convert.ToDouble(item["TotalPhysicalMemory"]) / (1024 * 1024 * 1024)); // Convertir a GB
                        ramInGB = (int)(Math.Round(ramInGB / 2.0) * 2); // Redondear al múltiplo de 2 más cercano
                        systemInfo.Add(ramInGB + " GB");
                    }
                }

                // Obtener el almacenamiento y redondear al múltiplo de 256 GB más cercano
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'"))
                {
                    foreach (var item in searcher.Get())
                    {
                        var storageInGB = Math.Ceiling(Convert.ToDouble(item["Size"]) / (1024 * 1024 * 1024)); // Convertir a GB
                        storageInGB = (int)(Math.Round(storageInGB / 256.0) * 256); // Redondear al múltiplo de 256 GB más cercano
                        systemInfo.Add(storageInGB + " GB");
                    }
                }
                
                // Obtener la tarjeta gráfica principal (usando la primera tarjeta gráfica encontrada)
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var item in searcher.Get())
                    {
                        // Seleccionamos la primera tarjeta gráfica encontrada como la principal
                        systemInfo.Add(item["Name"].ToString());
                        break; // Solo tomamos la primera tarjeta gráfica
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la información del sistema: " + ex.Message);
            }

            return systemInfo;
        }

        private void SaveSystemInfo(List<string> systemInfo)
        {
            try
            {
                File.WriteAllLines(Path.Combine(ConfigFolderPath, "systemInfo.txt"), systemInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la información del sistema: {ex.Message}");
            }
        }

        private List<string> LoadSystemInfo()
        {
            string systemInfoPath = Path.Combine(ConfigFolderPath, "systemInfo.txt");
            return File.Exists(systemInfoPath) ? File.ReadLines(systemInfoPath).ToList() : null;
        }

        private void CreateConfigFolder()
        {
            if (!Directory.Exists(ConfigFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(ConfigFolderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al crear la carpeta config: {ex.Message}");
                }
            }
        }
        
    }
}
