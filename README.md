# Videoficha

**Videoficha** es una aplicación de escritorio moderna desarrollada en .NET 8 con WPF, diseñada para funcionar como un kiosko multimedia y expositor de especificaciones técnicas de hardware.

## 🚀 Arquitectura: Screaming Architecture + MVVM

Este proyecto ha sido refactorizado siguiendo los principios de **Screaming Architecture**, donde la estructura de carpetas refleja las funcionalidades del negocio en lugar del framework.

### Estructura de Carpetas
- **`Features/`**: Módulos funcionales de la aplicación.
  - `DisplayKiosk/`: Control de reproducción de video en bucle y visor de PDF.
  - `SystemDiagnostics/`: Gestión y visualización de especificaciones técnicas del equipo.
- **`Infrastructure/`**: Implementaciones técnicas y servicios.
  - `Services/`: Lógica de obtención de hardware (WMI) y gestión de configuración.
- **`Domain/Models/`**: Entidades de datos puras (SystemSpec, KioskSettings).
- **`Shared/`**: Utilidades y extensiones comunes.

## ✨ Características Principales

- **Bucle de Video**: Reproducción continua de contenido multimedia.
- **Visor de PDF**: Integración con WebView2 para mostrar fichas técnicas.
- **Detección de Hardware**: Obtención automática de CPU, RAM, GPU y Almacenamiento mediante WMI.
- **Modo Kiosko**: Prevención de suspensión del sistema y shortcuts de administración.
- **Configuración Persistente**: Guarda las selecciones de archivos en una carpeta `config/` local.

## 🛠️ Tecnologías

- **Framework**: .NET 8 (WPF)
- **Lenguaje**: C#
- **Componentes**: 
  - Microsoft.Web.WebView2 (Visor PDF)
  - System.Management (Consultas WMI)
- **Patrón**: MVVM (Model-View-ViewModel)

## 📋 Requisitos

- Windows 10/11
- .NET 8 Runtime
- Microsoft Edge WebView2 Runtime

## ⌨️ Atajos de Teclado (Admin)

- `Ctrl + S`: Abrir ventana de selección de archivos (Video/PDF).
- `Ctrl + I`: Abrir ventana de edición manual de especificaciones.

---
Desarrollado con ❤️ para la gestión eficiente de puntos de venta y exposición.
