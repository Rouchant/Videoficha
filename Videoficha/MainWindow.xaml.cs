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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Videoficha
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        private List<string> systemInfo;
        private const string ConfigFolderName = "config";
        private string ConfigFolderPath;
        private string currentVideoPath;
        private string precioNormal;
        private string precioOferta;
        private string precioExclusivoTarjeta;
        private bool mostrarPrecio;
        private int precioDisplayMode;

        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = false;
            this.KeyDown += Window_KeyDown;
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            ConfigFolderPath = Path.Combine(Environment.CurrentDirectory, ConfigFolderName);
            CreateConfigFolder();
            LoadPrecioConfig();
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.S) ShowFileSelectionWindow();
                else if (e.Key == Key.I) ShowSystemInfoEditWindow();
                else if (e.Key == Key.P) ShowPrecioConfigWindow();
            }
        }

        private void ShowSystemInfoEditWindow()
        {
            var systemInfoEditWindow = new SystemInfoEditWindow(systemInfo, Path.Combine(ConfigFolderPath, "systemInfo.txt"))
            {
                Owner = this
            };
            systemInfoEditWindow.ShowDialog();
        }

        private void ShowFileSelectionWindow()
        {
            var fileSelectionWindow = new FileSelectionWindow { Owner = this };
            if (fileSelectionWindow.ShowDialog() == true) ProcessSelectedFiles(fileSelectionWindow);
        }

        private void ShowPrecioConfigWindow()
        {
            try
            {
                var precioConfigWindow = new PrecioConfigWindow(precioNormal, precioOferta, precioExclusivoTarjeta, mostrarPrecio, precioDisplayMode)
                {
                    Owner = this
                };

                if (precioConfigWindow.ShowDialog() == true)
                {
                    precioNormal = precioConfigWindow.PrecioNormal;
                    precioOferta = precioConfigWindow.PrecioOferta;
                    precioExclusivoTarjeta = precioConfigWindow.PrecioExclusivoTarjeta;
                    mostrarPrecio = precioConfigWindow.MostrarPrecio;
                    precioDisplayMode = precioConfigWindow.PrecioDisplayMode;

                    UpdatePrecioDisplay();
                    SavePrecioConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar la ventana de configuración de precios: {ex.Message}");
            }
        }

        private void LoadPrecioConfig()
        {
            try
            {
                string configFilePath = Path.Combine(ConfigFolderPath, "precios.txt");
                if (File.Exists(configFilePath))
                {
                    string[] configValues = File.ReadAllText(configFilePath).Split('|');
                    if (configValues.Length == 5)
                    {
                        precioNormal = configValues[0];
                        precioOferta = configValues[1];
                        precioExclusivoTarjeta = configValues[2];
                        precioDisplayMode = int.Parse(configValues[3]);
                        mostrarPrecio = configValues[4].ToLower() == "true";
                    }
                    else
                    {
                        MessageBox.Show("Error: El archivo de configuración está dañado o tiene un formato incorrecto.");
                    }
                }
                else
                {
                    string defaultConfig = "$999.990|$999.990|$999.990|0|false";
                    File.WriteAllText(configFilePath, defaultConfig);
                    precioNormal = precioOferta = precioExclusivoTarjeta = "$999.990";
                    precioDisplayMode = 0;
                    mostrarPrecio = false;
                }
                UpdatePrecioDisplay();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la configuración de precios: {ex.Message}");
            }
        }

        private void UpdatePrecioDisplay()
{
    try
    {
        if (!mostrarPrecio)
        {
            PrecioPanelNormalOferta.Visibility = Visibility.Collapsed;
            PrecioPanelNormalOfertaExclusivo.Visibility = Visibility.Collapsed;
            return;
        }

        if (!string.IsNullOrWhiteSpace(precioNormal) && !string.IsNullOrWhiteSpace(precioOferta))
        {
            PrecioNormalLabel.Text = $"Precio Normal: {precioNormal}";
            PrecioOfertaLabel.Content = $"Precio Oferta: {precioOferta}";
            PrecioNormalLabelExclusivo.Text = $"Precio Normal: {precioNormal}";
            PrecioOfertaLabelExclusivo.Content = $"Precio Oferta: {precioOferta}";
        }
        else
        {
            MessageBox.Show("Error: Los valores de los precios no son válidos.");
            return;
        }

        PrecioPanelNormalOferta.Visibility = Visibility.Collapsed;
        PrecioPanelNormalOfertaExclusivo.Visibility = Visibility.Collapsed;

        if (precioDisplayMode == 1 && !string.IsNullOrWhiteSpace(precioExclusivoTarjeta))
        {
            PrecioExclusivoTarjetaLabel.Content = $"Precio Exclusivo Tarjeta: {precioExclusivoTarjeta}";
            PrecioPanelNormalOfertaExclusivo.Visibility = Visibility.Visible;
            PrecioNormalLabelExclusivo.TextDecorations = TextDecorations.Strikethrough;
        }
        else
        {
            PrecioPanelNormalOferta.Visibility = Visibility.Visible;
            PrecioNormalLabel.TextDecorations = null;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error al actualizar los precios: {ex.Message}");
    }
}

        private void SavePrecioConfig()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(precioNormal) && !string.IsNullOrWhiteSpace(precioOferta) && !string.IsNullOrWhiteSpace(precioExclusivoTarjeta))
                {
                    string precioConfigData = $"{precioNormal}|{precioOferta}|{precioExclusivoTarjeta}|{precioDisplayMode}|{mostrarPrecio}";
                    File.WriteAllText(Path.Combine(ConfigFolderPath, "precios.txt"), precioConfigData);
                }
                else
                {
                    MessageBox.Show("Error: Los precios no pueden estar vacíos.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Error: No tienes permisos para guardar la configuración. {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la configuración de precios: {ex.Message}");
            }
        }

        private void LogError(Exception ex)
        {
            File.AppendAllText("errorLog.txt", $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\n");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadingLabel.Visibility = Visibility.Visible;
            UpdatePrecioDisplay();
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
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume && !string.IsNullOrEmpty(currentVideoPath))
            {
                videoPlayer.Stop();
                videoPlayer.Source = new Uri(currentVideoPath);
                videoPlayer.Play();
            }
        }

        private void SelectFiles()
        {
            var fileSelectionWindow = new FileSelectionWindow { Owner = this };
            if (fileSelectionWindow.ShowDialog() == true) ProcessSelectedFiles(fileSelectionWindow);
            else
            {
                PlayDefaultVideo();
                ShowDefaultPDF();
            }
        }

        private void ProcessSelectedFiles(FileSelectionWindow fileSelectionWindow)
        {
            if (!string.IsNullOrEmpty(fileSelectionWindow.VideoFilePath))
            {
                SaveVideoSelection(fileSelectionWindow.VideoFilePath);
                PlaySelectedVideo(fileSelectionWindow.VideoFilePath);
            }

            if (!string.IsNullOrEmpty(fileSelectionWindow.OtherFilePath))
            {
                SavePDFSelection(fileSelectionWindow.OtherFilePath);
            }
        }

        private void PlayDefaultVideo()
        {
            string defaultVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample", "HP.wmv");
            if (File.Exists(defaultVideoPath))
            {
                videoPlayer.Source = new Uri(defaultVideoPath);
                videoPlayer.Play();
            }
        }

        private void ShowDefaultPDF()
        {
            string defaultPDFPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample", "sample.pdf");
            if (File.Exists(defaultPDFPath))
            {
                var leerFichaWindow = new LeerFicha(defaultPDFPath) { Owner = this };
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
                var leerFichaWindow = new LeerFicha(pdfPath) { Owner = this };
                leerFichaWindow.ShowDialog();
            }
            else
            {
                ShowDefaultPDF();
            }
        }

        private void ViewInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var systemInfoWindow = new SystemInfoWindow { Owner = this };
            systemInfoWindow.ShowDialog();
        }

        private List<string> GetSystemInfo()
        {
            var systemInfo = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get()) systemInfo.Add(item["Model"].ToString());
                }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (var item in searcher.Get()) systemInfo.Add(item["Caption"].ToString());
                }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (var item in searcher.Get()) systemInfo.Add(item["Name"].ToString());
                }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                {
                    foreach (var item in searcher.Get())
                    {
                        var ramInGB = Math.Ceiling(Convert.ToDouble(item["TotalPhysicalMemory"]) / (1024 * 1024 * 1024));
                        ramInGB = (int)(Math.Round(ramInGB / 2.0) * 2);
                        systemInfo.Add(ramInGB + " GB");
                    }
                }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='C:'"))
                {
                    foreach (var item in searcher.Get())
                    {
                        var storageInGB = Math.Ceiling(Convert.ToDouble(item["Size"]) / (1024 * 1024 * 1024));
                        storageInGB = (int)(Math.Round(storageInGB / 256.0) * 256);
                        systemInfo.Add(storageInGB + " GB");
                    }
                }

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                {
                    foreach (var item in searcher.Get())
                    {
                        systemInfo.Add(item["Name"].ToString());
                        break;
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
    }
}