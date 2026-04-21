# Videoficha

**Videoficha** es una aplicación de escritorio premium desarrollada en .NET 8 con WPF y LibVLC, diseñada para funcionar como un kiosko multimedia autónomo y expositor de especificaciones técnicas de hardware en puntos de venta.

## ✨ Características Principales

- **Motor de Video LibVLC**: Reproducción ultra-estable mediante el motor de VLC, superando las limitaciones del MediaElement nativo.
- **Bucle de Video Interactivo**: Transición fluida entre contenido multimedia de ficha técnica y video de inactividad (Attract Loop).
- **Diseño Premium Moderno**: Interfaz con estilo **Glassmorphism**, layout optimizado para diferentes formatos de pantalla y esquinas cuadradas minimalistas.
- **Detección de Hardware Robusta**: Obtención precisa de CPU, RAM (compatible con VMs), GPU y Almacenamiento con redondeo comercial.
- **Modo Kiosko Total**: 
    - Prioridad de proceso **Alta** asignada automáticamente para máxima fluidez.
    - Prevención de suspensión del sistema.
    - Ejecución en pantalla completa (Topmost) con recuperación ante inactividad.
- **Panel de Administración Integrado**: Configuración visual de precios, SKU, temas de color y selección de archivos.

## 🚀 Arquitectura y Optimización

El proyecto está diseñado para funcionar 24/7 sin degradación de rendimiento:

- **Decodificación por Hardware**: Uso de `:hwdec=auto` para minimizar el uso de CPU.
- **Gestión de Memoria**: Liberación explícita de recursos de video en cada ciclo para evitar fugas de memoria (Anti-Leaks).
- **Ligereza**: Eliminación de dependencias pesadas como WebView2 para reducir el consumo de RAM base.
- **Compatibilidad VM**: Optimizado con renderizado por software y detección de hardware específica para entornos virtuales.

## 🛠️ Tecnologías

- **Framework**: .NET 8 (WPF)
- **Motor Multimedia**: LibVLCSharp + VideoLAN.LibVLC.Windows
- **Detección**: System.Management (WMI)
- **Recursos**: Iconografía embebida como recursos de ensamblado.

## 📋 Requisitos

- Windows 10/11
- .NET 8 Runtime

## 🔐 Acceso a Administración (Admin)

Para configurar el equipo:

1.  **Gesto de acceso**: Realizar **4 toques rápidos** en la esquina superior derecha de la pantalla principal.
2.  **Panel**: Permite editar precios, SKU, cambiar videos de loop y refrescar la detección de hardware.

---
Desarrollado con ❤️ para la gestión eficiente de puntos de venta y exposición técnica profesional.
