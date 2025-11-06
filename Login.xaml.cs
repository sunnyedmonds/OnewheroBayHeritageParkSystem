using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnewheroBayVMS
{
    /// <summary>  
    /// Interaction logic for Login.xaml  
    /// </summary>  
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void OnClickLogin(object sender, RoutedEventArgs e)
        {
            var mongoService = new MongoService();
            var user = mongoService.Authenticate(UsernameTextBox.Text, PasswordBox.Password);

            if (user != null)
            {
                var accountWindow = new Account(user);
                accountWindow.Show();
                this.Close();
            }
            else if (mongoService == null) // Fixed syntax error here  
            {
                MessageBox.Show("Database is not available. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
