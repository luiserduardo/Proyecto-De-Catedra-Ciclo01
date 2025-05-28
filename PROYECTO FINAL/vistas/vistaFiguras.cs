using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace PROYECTO_FINAL.vistas
{
    public partial class vistaFiguras : UserControl
    {
        // Estructura de datos:
        // SerialPort: Para la comunicación con Arduino.
        // string: Para almacenar la figura seleccionada.
        private SerialPort? serialPort; // Permite null debido a '?'
        private string selectedFigure = null;

        // Constructor
        // Inicializa la vista y carga los puertos seriales disponibles.
        public vistaFiguras()
        {
            InitializeComponent();
            LoadSerialPorts();
        }

        // Función: LoadSerialPorts
        // Estructura utilizada: string[] para almacenar los nombres de los puertos.
        // Asigna los puertos seriales disponibles al ComboBox de la interfaz.
        private void LoadSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            ComboBoxPorts.ItemsSource = ports;
            if (ports.Length > 0)
                ComboBoxPorts.SelectedIndex = 0;
        }

        // Función: ConnectButton_Click
        // Estructura utilizada: SerialPort para abrir la conexión.
        // Se ejecuta al hacer clic en el botón "Conectar". Intenta abrir el puerto seleccionado.
        // Actualiza la interfaz según el estado de conexión.
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort == null || !serialPort.IsOpen)
                {
                    serialPort = new SerialPort(ComboBoxPorts.SelectedItem.ToString(), 115200);
                    serialPort.Open();
                    StatusLabel.Text = "Conectado a " + ComboBoxPorts.SelectedItem.ToString();
                    ConnectButton.IsEnabled = false;
                    DisconnectButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar: " + ex.Message);
            }
        }

        // Función: DisconnectButton_Click
        // Estructura utilizada: SerialPort para cerrar la conexión.
        // Se ejecuta al hacer clic en "Desconectar". Cierra el puerto y actualiza la interfaz.
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                StatusLabel.Text = "Desconectado";
                ConnectButton.IsEnabled = true;
                DisconnectButton.IsEnabled = false;
            }
        }

        // Función: SendCommand
        // Estructura utilizada: string para el comando a enviar, SerialPort para la transmisión.
        // Envía comandos al Arduino con el prefijo '$'. Informa el estado a la interfaz.
        private void SendCommand(string command)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    serialPort.WriteLine("$" + command); // Enviar comando con prefijo $ seguido de \n
                    StatusLabel.Text = "Comando enviado: $" + command;
                    selectedFigure = command; // Actualizar figura seleccionada
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al enviar: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No hay conexión serial activa.");
            }
        }

        // Funciones manejadoras para cada botón de figura.
        // Cada una llama a SendCommand con el comando correspondiente.
        private void TriangleButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("TRIANGLE");
        }

        private void HeartButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("HEART");
        }

        private void CatButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("CAT");
        }

        private void HiButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("HI");
        }

        private void SquareButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("SQUARE");
        }
    }
}