using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OnewheroBayVisitorManagement.Models;
using OnewheroBayVisitorManagement.Services;

namespace OnewheroBayVisitorManagement.Views
{
    public partial class BookingManagementWindow : Window
    {
        private readonly MongoDBService _dbService;
        private string _selectedBookingId;
        private Event _selectedEvent;

        public BookingManagementWindow()
        {
            InitializeComponent();

            // Initialize DB service
            _dbService = new MongoDBService();

            // Ensure we load data only after the Window and its visual tree are ready
            Loaded += BookingManagementWindow_Loaded;
        }

        private async void BookingManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe to avoid multiple calls in some cases
            Loaded -= BookingManagementWindow_Loaded;

            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load visitors
                var visitors = await _dbService.GetAllVisitorsAsync();
                if (visitors != null)
                    cmbVisitors.ItemsSource = visitors.OrderBy(v => v.FirstName).ToList();

                // Load active events
                var events = await _dbService.GetActiveEventsAsync();
                if (events != null)
                {
                    // Only show upcoming or today events in the combobox
                    var upcoming = events.Where(ev => ev.EventDate.Date >= DateTime.Now.Date)
                                         .OrderBy(ev => ev.EventDate).ToList();
                    cmbEvents.ItemsSource = upcoming;
                }

                // Load bookings
                await LoadBookingsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadBookingsAsync()
        {
            try
            {
                var bookings = await _dbService.GetAllBookingsAsync();
                if (bookings != null)
                    dgBookings.ItemsSource = bookings.OrderByDescending(b => b.BookingDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bookings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbVisitors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // You can add logic to show visitor's booking history here
        }

        private void cmbEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Protect against this handler running while UI elements are still null
            if (cmbEvents == null || txtEventDetails == null || txtTicketPrice == null || eventDetailsPanel == null)
                return;

            if (cmbEvents.SelectedItem is Event selectedEvent)
            {
                _selectedEvent = selectedEvent;

                // Display event details
                txtEventDetails.Text = $"Date: {selectedEvent.EventDate:dd/MM/yyyy}\n" +
                                      $"Time: {selectedEvent.EventTime}\n" +
                                      $"Location: {selectedEvent.Location}\n" +
                                      $"Available Seats: {selectedEvent.AvailableSeats}/{selectedEvent.Capacity}\n" +
                                      $"Price per ticket: ${selectedEvent.TicketPrice:F2}";

                eventDetailsPanel.Visibility = Visibility.Visible;
                txtTicketPrice.Text = $"{selectedEvent.TicketPrice:F2}";
                CalculateTotal();
            }
            else
            {
                eventDetailsPanel.Visibility = Visibility.Collapsed;
                _selectedEvent = null;
                txtTicketPrice?.Clear();
                if (txtTotalAmount != null) txtTotalAmount.Text = "$0.00";
            }
        }

        private void txtNumberOfTickets_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateTotal();
        }

        private void CalculateTotal()
        {
            // Guard - ensure UI element exists
            if (txtTotalAmount == null || txtNumberOfTickets == null)
                return;

            // Default
            txtTotalAmount.Text = "$0.00";

            if (_selectedEvent == null)
                return;

            // Parse ticket count safely
            if (!int.TryParse(txtNumberOfTickets.Text?.Trim(), out int tickets) || tickets <= 0)
            {
                // if invalid number, leave as 0.00 (or you can show a validation)
                return;
            }

            decimal total = tickets * _selectedEvent.TicketPrice;
            txtTotalAmount.Text = $"${total:F2}";
        }

        private async void CreateBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (cmbVisitors.SelectedValue == null)
                {
                    MessageBox.Show("Please select a visitor", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbEvents.SelectedValue == null)
                {
                    MessageBox.Show("Please select an event", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtNumberOfTickets.Text, out int numberOfTickets) || numberOfTickets <= 0)
                {
                    MessageBox.Show("Please enter a valid number of tickets", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get fresh event from DB to ensure available seats are up-to-date
                var eventId = cmbEvents.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(eventId))
                {
                    MessageBox.Show("Invalid event selection", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedEvent = await _dbService.GetEventByIdAsync(eventId);
                if (selectedEvent == null)
                {
                    MessageBox.Show("Selected event not found", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (selectedEvent.AvailableSeats < numberOfTickets)
                {
                    MessageBox.Show($"Not enough seats available. Only {selectedEvent.AvailableSeats} seats left.",
                        "Insufficient Seats", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get visitor info (safely)
                var visitor = cmbVisitors.SelectedItem as Visitor;
                if (visitor == null)
                {
                    MessageBox.Show("Selected visitor not found", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get booking status safely
                string bookingStatus = "Confirmed";
                if (cmbBookingStatus?.SelectedItem is ComboBoxItem cboItem && cboItem.Content != null)
                    bookingStatus = cboItem.Content.ToString();

                // Create booking
                var booking = new Booking
                {
                    VisitorId = cmbVisitors.SelectedValue.ToString(),
                    VisitorName = visitor.FullName,
                    EventId = selectedEvent.Id,
                    EventName = selectedEvent.EventName,
                    BookingDate = DateTime.Now,
                    NumberOfTickets = numberOfTickets,
                    TotalAmount = numberOfTickets * selectedEvent.TicketPrice,
                    BookingStatus = bookingStatus
                };

                await _dbService.CreateBookingAsync(booking);

                // Update event available seats in DB
                selectedEvent.AvailableSeats -= numberOfTickets;
                await _dbService.UpdateEventAsync(selectedEvent.Id, selectedEvent);

                MessageBox.Show("Booking created successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating booking: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedBookingId))
                {
                    MessageBox.Show("Please select a booking to update", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get existing booking
                var allBookings = await _dbService.GetAllBookingsAsync();
                var existingBooking = allBookings?.FirstOrDefault(b => b.Id == _selectedBookingId);

                if (existingBooking == null)
                {
                    MessageBox.Show("Booking not found", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get the event (fresh)
                var eventItem = await _dbService.GetEventByIdAsync(existingBooking.EventId);
                if (eventItem == null)
                {
                    MessageBox.Show("Event not found", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Parse new ticket count
                if (!int.TryParse(txtNumberOfTickets.Text, out int newTicketCount) || newTicketCount <= 0)
                {
                    MessageBox.Show("Please enter a valid number of tickets", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Calculate seat difference
                int seatDifference = newTicketCount - existingBooking.NumberOfTickets;

                // Check available seats for increase
                if (seatDifference > 0 && eventItem.AvailableSeats < seatDifference)
                {
                    MessageBox.Show($"Not enough seats available. Only {eventItem.AvailableSeats} seats left.",
                        "Insufficient Seats", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update booking
                existingBooking.NumberOfTickets = newTicketCount;
                existingBooking.TotalAmount = newTicketCount * eventItem.TicketPrice;

                if (cmbBookingStatus?.SelectedItem is ComboBoxItem statusItem && statusItem.Content != null)
                    existingBooking.BookingStatus = statusItem.Content.ToString();

                await _dbService.UpdateBookingAsync(_selectedBookingId, existingBooking);

                // Update event seats
                eventItem.AvailableSeats -= seatDifference;
                await _dbService.UpdateEventAsync(eventItem.Id, eventItem);

                MessageBox.Show("Booking updated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating booking: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(sender is Button button) || button.Tag == null)
                    return;

                var bookingId = button.Tag.ToString();

                var result = MessageBox.Show("Are you sure you want to delete this booking?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                // Get booking details
                var bookings = await _dbService.GetAllBookingsAsync();
                var booking = bookings?.FirstOrDefault(b => b.Id == bookingId);

                if (booking != null)
                {
                    // Return seats to event (if event exists)
                    var eventItem = await _dbService.GetEventByIdAsync(booking.EventId);
                    if (eventItem != null)
                    {
                        eventItem.AvailableSeats += booking.NumberOfTickets;
                        await _dbService.UpdateEventAsync(eventItem.Id, eventItem);
                    }

                    await _dbService.DeleteBookingAsync(bookingId);

                    MessageBox.Show("Booking deleted successfully! Seats returned to event.", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadBookingsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting booking: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgBookings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgBookings.SelectedItem is Booking booking)
                {
                    _selectedBookingId = booking.Id;

                    // Set visitor safely
                    var visitorsList = (cmbVisitors.ItemsSource as System.Collections.Generic.List<Visitor>);
                    var visitor = visitorsList?.FirstOrDefault(v => v.Id == booking.VisitorId);
                    if (visitor != null)
                        cmbVisitors.SelectedItem = visitor;
                    else
                        cmbVisitors.SelectedIndex = -1;

                    // Set event safely
                    var eventsList = (cmbEvents.ItemsSource as System.Collections.Generic.List<Event>);
                    var eventItem = eventsList?.FirstOrDefault(ev => ev.Id == booking.EventId);
                    if (eventItem != null)
                    {
                        cmbEvents.SelectedItem = eventItem;
                        _selectedEvent = eventItem;
                    }
                    else
                    {
                        cmbEvents.SelectedIndex = -1;
                        _selectedEvent = null;
                    }

                    txtNumberOfTickets.Text = booking.NumberOfTickets.ToString();

                    // Set status
                    if (cmbBookingStatus != null)
                    {
                        foreach (ComboBoxItem item in cmbBookingStatus.Items)
                        {
                            if (item.Content?.ToString() == booking.BookingStatus)
                            {
                                cmbBookingStatus.SelectedItem = item;
                                break;
                            }
                        }
                    }

                    // Update shown totals
                    CalculateTotal();
                }
                else
                {
                    // No selection
                    _selectedBookingId = null;
                }
            }
            catch
            {
                // Silently ignore UI-binding transient errors
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedBookingId = null;
            _selectedEvent = null;

            if (cmbVisitors != null) cmbVisitors.SelectedIndex = -1;
            if (cmbEvents != null) cmbEvents.SelectedIndex = -1;
            if (txtNumberOfTickets != null) txtNumberOfTickets.Text = "1";
            if (txtTicketPrice != null) txtTicketPrice.Clear();
            if (txtTotalAmount != null) txtTotalAmount.Text = "$0.00";
            if (cmbBookingStatus != null) cmbBookingStatus.SelectedIndex = 0;
            if (eventDetailsPanel != null) eventDetailsPanel.Visibility = Visibility.Collapsed;
            if (txtSearch != null) txtSearch.Clear();
            if (dgBookings != null) dgBookings.SelectedItem = null;
        }

        private async void SearchBooking_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchText = txtSearch?.Text?.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    await LoadBookingsAsync();
                    return;
                }

                var allBookings = await _dbService.GetAllBookingsAsync();
                var filtered = allBookings?
                    .Where(b =>
                        (b.VisitorName ?? string.Empty).ToLower().Contains(searchText) ||
                        (b.EventName ?? string.Empty).ToLower().Contains(searchText) ||
                        (b.BookingStatus ?? string.Empty).ToLower().Contains(searchText))
                    .OrderByDescending(b => b.BookingDate)
                    .ToList();

                dgBookings.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            await LoadDataAsync();
        }
    }
}
