using System;
using System.IO;
using System.Windows;

namespace Videoficha
{
    public partial class FileSelectionWindow : Window
    {
        private const string VideoFilePathText = "config/videoSelection.txt";
        private const string PdfFilePathText = "config/pdfSelection.txt";

        public string VideoFilePath { get; private set; }
        public string OtherFilePath { get; private set; }

        // Rutas predeterminadas
        private readonly string DefaultVideoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample", "HP.wmv");
        private readonly string DefaultPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample", "sample.pdf");

        // Constructor que trata de leer los archivos actuales desde los archivos de texto
        public FileSelectionWindow()
        {
            InitializeComponent();

            // Asegurarse de que la carpeta 'config' existe
            string configFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            // Tratar de leer las rutas desde los archivos de texto
            VideoFilePath = ReadFilePath(VideoFilePathText) ?? DefaultVideoPath;
            OtherFilePath = ReadFilePath(PdfFilePathText) ?? DefaultPdfPath;

            // Actualizar las etiquetas con las rutas actuales o "Por defecto"
            VideoPathLabel.Content = (VideoFilePath == DefaultVideoPath) ? "Por defecto" : VideoFilePath;
            PdfPathLabel.Content = (OtherFilePath == DefaultPdfPath) ? "Por defecto" : OtherFilePath;
        }

        // Método para leer la ruta de un archivo desde un archivo de texto
        private string ReadFilePath(string fileName)
        {
            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName).Trim();
            }
            return null;
        }

        // Método para guardar la ruta en un archivo de texto
        private void SaveFilePath(string fileName, string filePath)
        {
            File.WriteAllText(fileName, filePath);
        }

        private void SelectVideoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Video Files|*.wmv;*.mp4;*.avi;*.mkv",
                Title = "Seleccionar Video"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                VideoFilePath = openFileDialog.FileName;
                VideoPathLabel.Content = VideoFilePath; // Actualiza la etiqueta
                SaveFilePath(VideoFilePathText, VideoFilePath); // Guarda la ruta en el archivo
            }
        }

        private void SelectPdfButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Seleccionar PDF"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                OtherFilePath = openFileDialog.FileName;
                PdfPathLabel.Content = OtherFilePath; // Actualiza la etiqueta
                SaveFilePath(PdfFilePathText, OtherFilePath); // Guarda la ruta en el archivo
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Establece el resultado del diálogo a verdadero y cierra la ventana
            DialogResult = true;
            Close();
        }
    }
}
