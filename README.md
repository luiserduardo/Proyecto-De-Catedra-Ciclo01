# Proyecto Final: Interfaz de Comunicación Serial con Arduino

Este repositorio contiene una aplicación desarrollada en C# (WPF) enfocada en la comunicación serial entre una PC y un dispositivo Arduino, permitiendo el control dinámico de figuras, configuración de parámetros y la integración de un juego Pong para matriz de LEDs.

## Características principales

- **Comunicación Serial:** Utiliza la clase `SerialPort` de .NET para abrir, configurar y cerrar puertos COM, facilitando el envío de comandos y la retroalimentación visual desde la interfaz.
- **Control de Figuras:** Permite seleccionar y enviar comandos de diferentes figuras al Arduino para ser visualizadas en una matriz de LEDs.
- **Configuración Dinámica:** Ofrece una pantalla de configuración que permite al usuario seleccionar y configurar el puerto serial, así como ajustar el brillo de la matriz en tiempo real.
- **Juego Pong:** Incorpora una versión interactiva del clásico juego Pong, comunicando los estados y puntuaciones al Arduino a través del puerto serial.
- **Interfaz Modular:** Cada funcionalidad (figuras, configuración, juego) está aislada en vistas (`UserControl`s) y es gestionada desde una ventana principal con menú dinámico.

## Estructura del proyecto

- `/PROYECTO FINAL/MainWindow.xaml.cs`: Ventana principal que gestiona la navegación entre las diferentes vistas (Inicio, Figuras, Juego Pong, Configuración, Salida).
- `/PROYECTO FINAL/vistas/vistaFiguras.cs`: Lógica para el manejo de la selección de figuras y la comunicación de comandos seriales relacionados.
- `/PROYECTO FINAL/vistas/vistaConfiguracion.xaml.cs`: Permite al usuario seleccionar/configurar el puerto COM y ajustar el brillo de la matriz LED.
- `/PROYECTO FINAL/vistas/vistaJuegoPong.xaml.cs`: Implementación del juego Pong y la lógica para enviar los estados del juego a Arduino.
- Otros archivos asociados a la interfaz gráfica y la inicialización de la aplicación.

## Instalación y uso

1. **Requisitos:**
   - .NET Framework compatible con WPF.
   - Arduino con firmware esperando comandos seriales para controlar una matriz LED.
   - Cable USB para conexión serial.

2. **Ejecución:**
   - Clona el repositorio.
   - Abre la solución en Visual Studio.
   - Selecciona el proyecto principal y compílalo.
   - Ejecuta la aplicación y usa el menú para navegar entre las diferentes vistas.

3. **Interacción:**
   - **Configuración:** Selecciona y configura el puerto serial antes de usar otras funciones.
   - **Figuras:** Elige una figura y envíala al Arduino para visualizarla.
   - **Pong:** Juega y observa cómo la matriz de LEDs responde en tiempo real.

## Notas técnicas

- Todo el manejo serial está centralizado en las vistas, permitiendo modularidad.
- El brillo se ajusta mediante un slider que convierte valores a comandos específicos para Arduino.
- El juego Pong y el control de figuras usan eventos y actualizaciones en tiempo real para una experiencia fluida.

## Créditos

Desarrollado por [CesarAPK](https://github.com/CesarAPK) como parte de un proyecto final académico.

---
¡Contribuciones, sugerencias y mejoras son bienvenidas!
