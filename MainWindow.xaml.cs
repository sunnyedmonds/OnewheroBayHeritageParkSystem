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

namespace OnewheroBayVMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClickGoEvents(object sender, RoutedEventArgs e)
        {
            var EventsWindow = new Events();
            EventsWindow.Show();
            this.Close();
        }

        private void OnClickGoParkInfo(object sender, RoutedEventArgs e)
        {
            var ParkInfoWindow = new ParkInformaition();
            ParkInfoWindow.Show();
            this.Close();
        }

        private void OnClickGoAccount(object sender, RoutedEventArgs e)
        {
            var LoginWindow = new Login();
            LoginWindow.Show();
            this.Close();
        }

        private void OnClickGoContact(object sender, RoutedEventArgs e)
        {
            var ContactWindow = new ContactUs();
            ContactWindow.Show();
            this.Close();
        }
    }
}