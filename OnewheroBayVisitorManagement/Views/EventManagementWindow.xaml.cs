using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnewheroBayVisitorManagement.Models;
using OnewheroBayVisitorManagement.Services;

namespace OnewheroBayVisitorManagement.Views
{
    public partial class EventManagementWindow : Window
    {
        private readonly MongoDBService _dbService;
        private string _selectedEventId;

        public EventManagementWindow()
        {
            InitializeComponent();
            _dbService = new MongoDBService();
            dpEventDate.SelectedDate = DateTime.Now.AddDays(7);
            LoadEvents();
        }

        private async void LoadEvents()
        {
            try
            {
                var events = await _dbService.GetAllEventsAsync();
                dgEvents.ItemsSource = events.OrderBy(e => e.EventDate).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtEventName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text) ||
                    dpEventDate.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(txtEventTime.Text) ||
                    string.IsNullOrWhiteSpace(txtLocation.Text) ||
                    cmbCategory.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtCapacity.Text) ||
                    string.IsNullOrWhiteSpace(txtTicketPrice.Text))
                {
                    MessageBox.Show("Please fill in all required fields (*)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Parse numeric values
                if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity <= 0)
                {
                    MessageBox.Show("Please enter a valid capacity (positive number)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtTicketPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid ticket price", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new event
                var newEvent = new Event
                {
                    EventName = txtEventName.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    EventDate = dpEventDate.SelectedDate.Value,
                    EventTime = txtEventTime.Text.Trim(),
                    Location = txtLocation.Text.Trim(),
                    Category = (cmbCategory.SelectedItem as ComboBoxItem).Content.ToString(),
                    Capacity = capacity,
                    AvailableSeats = capacity,
                    TicketPrice = price,
                    ImageUrl = txtImageUrl.Text.Trim(),
                    IsActive = chkIsActive.IsChecked == true
                };

                await _dbService.CreateEventAsync(newEvent);

                MessageBox.Show("Event created successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
                LoadEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding event: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedEventId))
                {
                    MessageBox.Show("Please select an event to update", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation
                if (string.IsNullOrWhiteSpace(txtEventName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text) ||
                    dpEventDate.SelectedDate == null ||
                    string.IsNullOrWhiteSpace(txtEventTime.Text) ||
                    string.IsNullOrWhiteSpace(txtLocation.Text) ||
                    cmbCategory.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtCapacity.Text) ||
                    string.IsNullOrWhiteSpace(txtTicketPrice.Text))
                {
                    MessageBox.Show("Please fill in all required fields (*)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Parse numeric values
                if (!int.TryParse(txtCapacity.Text, out int capacity) || capacity <= 0)
                {
                    MessageBox.Show("Please enter a valid capacity", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtTicketPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid ticket price", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get existing event
                var eventItem = await _dbService.GetEventByIdAsync(_selectedEventId);
                if (eventItem != null)
                {
                    // Calculate available seats based on capacity change
                    int bookedSeats = eventItem.Capacity - eventItem.AvailableSeats;
                    int newAvailableSeats = capacity - bookedSeats;

                    eventItem.EventName = txtEventName.Text.Trim();
                    eventItem.Description = txtDescription.Text.Trim();
                    eventItem.EventDate = dpEventDate.SelectedDate.Value;
                    eventItem.EventTime = txtEventTime.Text.Trim();
                    eventItem.Location = txtLocation.Text.Trim();
                    eventItem.Category = (cmbCategory.SelectedItem as ComboBoxItem).Content.ToString();
                    eventItem.Capacity = capacity;
                    eventItem.AvailableSeats = newAvailableSeats >= 0 ? newAvailableSeats : 0;
                    eventItem.TicketPrice = price;
                    eventItem.ImageUrl = txtImageUrl.Text.Trim();
                    eventItem.IsActive = chkIsActive.IsChecked == true;

                    await _dbService.UpdateEventAsync(_selectedEventId, eventItem);

                    MessageBox.Show("Event updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    LoadEvents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating event: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var eventId = button.Tag.ToString();

                var result = MessageBox.Show("Are you sure you want to delete this event?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _dbService.DeleteEventAsync(eventId);
                    MessageBox.Show("Event deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadEvents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting event: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgEvents.SelectedItem is Event eventItem)
            {
                _selectedEventId = eventItem.Id;
                txtEventName.Text = eventItem.EventName;
                txtDescription.Text = eventItem.Description;
                dpEventDate.SelectedDate = eventItem.EventDate;
                txtEventTime.Text = eventItem.EventTime;
                txtLocation.Text = eventItem.Location;
                txtCapacity.Text = eventItem.Capacity.ToString();
                txtTicketPrice.Text = eventItem.TicketPrice.ToString("F2");
                txtImageUrl.Text = eventItem.ImageUrl;
                chkIsActive.IsChecked = eventItem.IsActive;

                // Set category
                foreach (ComboBoxItem item in cmbCategory.Items)
                {
                    if (item.Content.ToString() == eventItem.Category)
                    {
                        cmbCategory.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedEventId = null;
            txtEventName.Clear();
            txtDescription.Clear();
            dpEventDate.SelectedDate = DateTime.Now.AddDays(7);
            txtEventTime.Text = "10:00 AM";
            txtLocation.Clear();
            cmbCategory.SelectedIndex = -1;
            txtCapacity.Text = "50";
            txtTicketPrice.Text = "25.00";
            txtImageUrl.Clear();
            chkIsActive.IsChecked = true;
            txtSearch.Clear();
            dgEvents.SelectedItem = null;
        }

        private async void SearchEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchText = txtSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    LoadEvents();
                    return;
                }

                var allEvents = await _dbService.GetAllEventsAsync();
                var filtered = allEvents.Where(ev =>
                    ev.EventName.ToLower().Contains(searchText) ||
                    ev.Category.ToLower().Contains(searchText) ||
                    ev.Location.ToLower().Contains(searchText) ||
                    ev.Description.ToLower().Contains(searchText)
                ).OrderBy(e => e.EventDate).ToList();

                dgEvents.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            LoadEvents();
        }
    }
}