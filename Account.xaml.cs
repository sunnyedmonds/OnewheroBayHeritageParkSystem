using System;
using System.Collections.Generic;
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
    /// Interaction logic for Account.xaml
    /// </summary>
    public partial class Account : Window
    {
        public Account()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Account(User user) : this()
        {
            UserName = user.Username;
            UserEmail = user.Email;
            UserPhone = user.Phone;
            DataContext = this;
        }

        public string UserName { get; set; } = "admin";
        public string UserEmail {  get; set; } = "admin@vms.com";
        public string UserPhone { get; set; } = "+1-234-567-8901";


        private void OnClickLogout(object sender, RoutedEventArgs e)
        {
            var Logout = new MainWindow();
            Logout.Show();
            this.Close();
        }


    }


}
