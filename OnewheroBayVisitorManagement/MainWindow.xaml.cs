using System.Windows;
using OnewheroBayVisitorManagement.Views;

namespace OnewheroBayVisitorManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ManageVisitors_Click(object sender, RoutedEventArgs e)
        {
            var visitorWindow = new VisitorManagementWindow();
            visitorWindow.Show();
        }

        private void ManageEvents_Click(object sender, RoutedEventArgs e)
        {
            var eventWindow = new EventManagementWindow();
            eventWindow.Show();
        }

        private void ManageBookings_Click(object sender, RoutedEventArgs e)
        {
            var bookingWindow = new BookingManagementWindow();
            bookingWindow.Show();
        }

        private void ViewAttractions_Click(object sender, RoutedEventArgs e)
        {
            var attractionWindow = new AttractionWindow();
            attractionWindow.Show();
        }

        private void ViewAnalytics_Click(object sender, RoutedEventArgs e)
        {
            var analyticsWindow = new AnalyticsWindow();
            analyticsWindow.Show();
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = new AdminLoginWindow();
            adminWindow.Show();
        }
    }
}
