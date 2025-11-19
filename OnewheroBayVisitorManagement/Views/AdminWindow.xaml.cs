using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OnewheroBayVisitorManagement.Models;
using OnewheroBayVisitorManagement.Services;

namespace OnewheroBayVisitorManagement.Views
{
    public partial class AdminWindow : Window
    {
        private readonly MongoDBService _dbService;
        private string _selectedBookingId;
        private string _selectedEventId;
        private Event _selectedEvent;

        public AdminWindow()
        {
            InitializeComponent();
            _dbService = new MongoDBService();
            Loaded += AdminWindow_Loaded;
        }

        private async void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AdminWindow_Loaded;
            await LoadBookingsAsync();
            await LoadEventsAsync();
        }

        #region Bookings

        private async Task LoadBookingsAsync()
        {
            try
            {
                var bookings = await _dbService.GetAllBookingsAsync();
                dgBookings.ItemsSource = bookings.OrderByDescending(b => b.BookingDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgBookings.SelectedItem is Booking booking)
            {
                _selectedBookingId = booking.Id;
                txtVisitorName.Text = booking.VisitorName;
                txtEventName.Text = booking.EventName;
                txtNumberOfTickets.Text = booking.NumberOfTickets.ToString();
                txtTotalAmount.Text = $"${booking.TotalAmount:F2}";
            }
            else
            {
                _selectedBookingId = null;
                ClearBookingForm();
            }
        }

        private async void btnDeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedBookingId)) return;

            var result = MessageBox.Show("Are you sure you want to delete this booking?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _dbService.DeleteBookingAsync(_selectedBookingId);
                MessageBox.Show("Booking deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _selectedBookingId = null;
                await LoadBookingsAsync();
                ClearBookingForm();
            }
        }

        private void ClearBookingForm()
        {
            txtVisitorName.Clear();
            txtEventName.Clear();
            txtNumberOfTickets.Text = "1";
            txtTotalAmount.Text = "$0.00";
        }

        private async void txtSearchBooking_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var searchText = txtSearchBooking.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    await LoadBookingsAsync();
                    return;
                }

                var allBookings = await _dbService.GetAllBookingsAsync();
                var filtered = allBookings.Where(b =>
                    (b.VisitorName ?? string.Empty).ToLower().Contains(searchText) ||
                    (b.EventName ?? string.Empty).ToLower().Contains(searchText) ||
                    (b.BookingStatus ?? string.Empty).ToLower().Contains(searchText))
                    .OrderByDescending(b => b.BookingDate)
                    .ToList();

                dgBookings.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Events

        private async Task LoadEventsAsync()
        {
            try
            {
                var events = await _dbService.GetAllEventsAsync();
                dgEvents.ItemsSource = events.OrderBy(e => e.EventDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgEvents.SelectedItem is Event eventItem)
            {
                _selectedEventId = eventItem.Id;
                txtEventNameEvent.Text = eventItem.EventName;
                txtLocationEvent.Text = eventItem.Location;
                dpEventDate.SelectedDate = eventItem.EventDate;
                txtCapacityEvent.Text = eventItem.Capacity.ToString();
                txtTicketPriceEvent.Text = eventItem.TicketPrice.ToString("F2");
                chkIsActive.IsChecked = eventItem.IsActive;
            }
            else
            {
                _selectedEventId = null;
                ClearEventForm();
            }
        }

        private async void btnDeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedEventId)) return;

            var result = MessageBox.Show("Are you sure you want to delete this event?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                await _dbService.DeleteEventAsync(_selectedEventId);
                MessageBox.Show("Event deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _selectedEventId = null;
                await LoadEventsAsync();
                ClearEventForm();
            }
        }

        private void ClearEventForm()
        {
            txtEventNameEvent.Clear();
            txtLocationEvent.Clear();
            dpEventDate.SelectedDate = DateTime.Now.AddDays(7);
            txtCapacityEvent.Text = "50";
            txtTicketPriceEvent.Text = "25.00";
            chkIsActive.IsChecked = true;
        }

        private async void txtSearchEvent_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var searchText = txtSearchEvent.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    await LoadEventsAsync();
                    return;
                }

                var allEvents = await _dbService.GetAllEventsAsync();
                var filtered = allEvents.Where(ev =>
                    (ev.EventName ?? string.Empty).ToLower().Contains(searchText) ||
                    (ev.Location ?? string.Empty).ToLower().Contains(searchText) ||
                    (ev.Category ?? string.Empty).ToLower().Contains(searchText))
                    .OrderBy(ev => ev.EventDate)
                    .ToList();

                dgEvents.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching events: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
