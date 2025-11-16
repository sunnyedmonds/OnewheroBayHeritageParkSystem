using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnewheroBayVisitorManagement.Models;
using OnewheroBayVisitorManagement.Services;

namespace OnewheroBayVisitorManagement.Views
{
    public partial class VisitorManagementWindow : Window
    {
        private readonly MongoDBService _dbService;
        private string _selectedVisitorId;

        public VisitorManagementWindow()
        {
            InitializeComponent();
            _dbService = new MongoDBService();
            LoadVisitors();
        }

        private async void LoadVisitors()
        {
            try
            {
                var visitors = await _dbService.GetAllVisitorsAsync();
                dgVisitors.ItemsSource = visitors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddVisitor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                    string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    MessageBox.Show("Please fill in all required fields (*)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if email already exists
                var existingVisitor = await _dbService.GetVisitorByEmailAsync(txtEmail.Text.Trim());
                if (existingVisitor != null)
                {
                    MessageBox.Show("A visitor with this email already exists!", "Duplicate Email",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get selected interests
                var interests = new List<string>();
                foreach (CheckBox checkbox in InterestsPanel.Children.OfType<CheckBox>())
                {
                    if (checkbox.IsChecked == true)
                    {
                        interests.Add(checkbox.Content.ToString());
                    }
                }

                // Create new visitor
                var visitor = new Visitor
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    City = txtCity.Text.Trim(),
                    Country = txtCountry.Text.Trim(),
                    Interests = interests,
                    RegistrationDate = DateTime.Now
                };

                await _dbService.CreateVisitorAsync(visitor);

                MessageBox.Show("Visitor registered successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
                LoadVisitors();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding visitor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateVisitor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedVisitorId))
                {
                    MessageBox.Show("Please select a visitor to update", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation
                if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                    string.IsNullOrWhiteSpace(txtLastName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPhone.Text))
                {
                    MessageBox.Show("Please fill in all required fields (*)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get selected interests
                var interests = new List<string>();
                foreach (CheckBox checkbox in InterestsPanel.Children.OfType<CheckBox>())
                {
                    if (checkbox.IsChecked == true)
                    {
                        interests.Add(checkbox.Content.ToString());
                    }
                }

                // Get existing visitor
                var visitor = await _dbService.GetVisitorByIdAsync(_selectedVisitorId);
                if (visitor != null)
                {
                    visitor.FirstName = txtFirstName.Text.Trim();
                    visitor.LastName = txtLastName.Text.Trim();
                    visitor.Email = txtEmail.Text.Trim();
                    visitor.Phone = txtPhone.Text.Trim();
                    visitor.Address = txtAddress.Text.Trim();
                    visitor.City = txtCity.Text.Trim();
                    visitor.Country = txtCountry.Text.Trim();
                    visitor.Interests = interests;

                    await _dbService.UpdateVisitorAsync(_selectedVisitorId, visitor);

                    MessageBox.Show("Visitor updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    LoadVisitors();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating visitor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteVisitor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var visitorId = button.Tag.ToString();

                var result = MessageBox.Show("Are you sure you want to delete this visitor?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _dbService.DeleteVisitorAsync(visitorId);
                    MessageBox.Show("Visitor deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadVisitors();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting visitor: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgVisitors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgVisitors.SelectedItem is Visitor visitor)
            {
                _selectedVisitorId = visitor.Id;
                txtFirstName.Text = visitor.FirstName;
                txtLastName.Text = visitor.LastName;
                txtEmail.Text = visitor.Email;
                txtPhone.Text = visitor.Phone;
                txtAddress.Text = visitor.Address;
                txtCity.Text = visitor.City;
                txtCountry.Text = visitor.Country;

                // Set checkboxes
                foreach (CheckBox checkbox in InterestsPanel.Children.OfType<CheckBox>())
                {
                    checkbox.IsChecked = visitor.Interests.Contains(checkbox.Content.ToString());
                }
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedVisitorId = null;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtCity.Clear();
            txtCountry.Clear();
            txtSearch.Clear();

            foreach (CheckBox checkbox in InterestsPanel.Children.OfType<CheckBox>())
            {
                checkbox.IsChecked = false;
            }

            dgVisitors.SelectedItem = null;
        }

        private async void SearchVisitor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchText = txtSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    LoadVisitors();
                    return;
                }

                var allVisitors = await _dbService.GetAllVisitorsAsync();
                var filtered = allVisitors.Where(v =>
                    v.FirstName.ToLower().Contains(searchText) ||
                    v.LastName.ToLower().Contains(searchText) ||
                    v.Email.ToLower().Contains(searchText) ||
                    v.Phone.Contains(searchText) ||
                    (v.City != null && v.City.ToLower().Contains(searchText))
                ).ToList();

                dgVisitors.ItemsSource = filtered;
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
            LoadVisitors();
        }
    }
}