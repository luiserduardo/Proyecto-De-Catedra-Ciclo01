using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace PROYECTO_FINAL.vistas
{
    public partial class vistaConfiguracion : UserControl
    {
        // Estructura de datos:
        // Objeto de tipo SerialPort para manejar la comunicación serie con el dispositivo externo (Arduino)
        private SerialPort? serialPort;

        // Constructor de la clase vistaConfiguracion
        // Inicializa los componentes visuales del control de usuario
        public vistaConfiguracion()
        {
            InitializeComponent();
        }

        // Función: BtnConfigurarPuerto_Click
        // Estructura utilizada: 
        // - SerialPort para acceder y configurar el puerto serie.
        // - string para guardar el nombre del puerto (ejemplo: "COM3").
        // Descripción:
        // Se ejecuta cuando el usuario da clic en el botón "Configurar Puerto COM".
        // Solicita al usuario el nombre del puerto a usar, intenta abrirlo con una velocidad de 115200 baudios.
        // Si ya hay un puerto abierto, lo cierra antes de abrir el nuevo.
        // Muestra mensajes de éxito o error mediante MessageBox.
        private void BtnConfigurarPuerto_Click(object sender, RoutedEventArgs e)
        {
            // Solicita el puerto al usuario mediante un input dialog
            string? puerto = Microsoft.VisualBasic.Interaction.InputBox("Introduce el puerto COM (ejemplo: COM3):", "Configurar Puerto Serial", "COM3");
            if (!string.IsNullOrEmpty(puerto))
            {
                try
                {
                    // Si ya hay un puerto abierto, ciérralo antes de abrir otro
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    // Crea y abre el nuevo puerto
                    serialPort = new SerialPort(puerto, 115200);
                    serialPort.Open();
                    MessageBox.Show($"Conectado correctamente a {puerto}");
                }
                catch (Exception ex)
                {
                    // Si hay error al abrir el puerto, muestra el mensaje de error
                    MessageBox.Show($"Error al conectar: {ex.Message}");
                }
            }
        }

        // Función: BtnDeshabilitarPuerto_Click
        // Estructura utilizada:
        // - SerialPort para cerrar el puerto serie abierto
        // Descripción:
        // Se ejecuta cuando el usuario da clic en el botón "Deshabilitar Puerto".
        // Si hay un puerto abierto, lo cierra y muestra un mensaje de confirmación.
        // Si no hay puerto abierto, informa al usuario.
        private void BtnDeshabilitarPuerto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    MessageBox.Show("Puerto COM cerrado correctamente.");
                }
                else
                {
                    MessageBox.Show("No hay puerto abierto actualmente.");
                }
            }
            catch (Exception ex)
            {
                // Si hay error al cerrar el puerto, muestra el mensaje de error
                MessageBox.Show($"Error al cerrar el puerto: {ex.Message}");
            }
        }

        // Función: brillo_ValueChanged
        // Estructura utilizada:
        // - Arreglo int[] para mapear los valores del slider (bajo, medio, alto) a valores de brillo reales para la matriz
        // - SerialPort para enviar el comando de brillo al dispositivo externo
        // Descripción:
        // Se ejecuta cada vez que el usuario cambia el valor del slider de brillo.
        // Convierte el valor del slider (0,1,2) a un valor real de brillo (3,8,15) y lo envía al Arduino si el puerto está abierto.
        private void brillo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int[] valoresBrillo = { 3, 8, 15 }; // Arreglo de valores de brillo: Bajo, Medio, Alto
            int nivel = (int)brillo.Value;      // Convertir el valor del slider a entero
            int brilloMatriz = valoresBrillo[nivel]; // Obtener el valor de brillo real

            // Si el puerto está abierto, enviar el comando de brillo
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.WriteLine($"#BRILLO:{brilloMatriz}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al enviar brillo: {ex.Message}");
                }
            }
        }
    }
}