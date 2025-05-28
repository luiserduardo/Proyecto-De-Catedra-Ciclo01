using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PROYECTO_FINAL.vistas;

namespace PROYECTO_FINAL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ContenidoDinamico.Content = new vistaBienvenida();

        }

        //Este metodo se activa cada vez que se hace clic en un menuItem
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
 
              //Se pasa de un objeto generico sender a un evento de MenuItem para poder manejar adecuadamente el dato
              MenuItem item
                 = (MenuItem)sender as MenuItem;

            //Comparamos el texto para saber que opcion se esta ejecutando
            //tomamos el dato que esta dentro de item del menu para verificar el caso
            switch (item.Header.ToString())
            {

                case "INICIO":

                    //Creamos una instancia dentro del controlador de contenido para manejar nuestro elemento
                    try
                    {
                        ContenidoDinamico.Content = new vistaBienvenida();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());   

                    }
                        break;

                case "FIGURAS":

                    ContenidoDinamico.Content = new vistaFiguras();

                    break;

                case "JUEGO PING PONG":

                    ContenidoDinamico.Content = new vistaJuegoPong();

                    break;


                case "CONFIGURACIÓN":
                    ContenidoDinamico.Content = new vistaConfiguracion();


                    break;
    
                case "SALIR":

                    //cerrarmos
                    MessageBox.Show("GRACIAS POR EJECUTAR EL PROGRAMA");

                    Application.Current.Shutdown();
                    break;

            }

            
        }
    }
}