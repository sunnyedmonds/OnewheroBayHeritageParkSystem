using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OnewheroBayVisitorManagement.Models;
using OnewheroBayVisitorManagement.Services;

namespace OnewheroBayVisitorManagement.Views
{
    public partial class AttractionWindow : Window
    {
        private readonly MongoDBService _dbService;
        private string _selectedAttractionId;

        public AttractionWindow()
        {
            InitializeComponent();
            _dbService = new MongoDBService();
            LoadAttractions();
        }

        private async void LoadAttractions()
        {
            try
            {
                var attractions = await _dbService.GetAllAttractionsAsync();
                attractionsPanel.ItemsSource = attractions.OrderBy(a => a.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attractions: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddAttraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(txtAttractionName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text) ||
                    cmbCategory.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtOpeningHours.Text))
                {
                    MessageBox.Show("Please fill in all required fields (*)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new attraction
                var attraction = new Attraction
                {
                    Name = txtAttractionName.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    Category = (cmbCategory.SelectedItem as ComboBoxItem).Content.ToString(),
                    OpeningHours = txtOpeningHours.Text.Trim(),
                    ImageUrl = txtImageUrl.Text.Trim(),
                    IsActive = chkIsActive.IsChecked == true
                };

                await _dbService.CreateAttractionAsync(attraction);

                MessageBox.Show("Attraction added successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
                LoadAttractions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding attraction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UpdateAttraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedAttractionId))
                {
                    MessageBox.Show("Please select an attraction to update", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validation
                if (string.IsNullOrWhiteSpace(txtAttractionName.Text) ||
                    string.IsNullOrWhiteSpace(txtDescription.Text) ||
                    cmbCategory.SelectedItem == null ||
                    string.IsNullOrWhiteSpace(txtOpeningHours.Text))
                {
                    MessageBox.Show("Please fill in all required fields (*)", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get existing attraction
                var attractions = await _dbService.GetAllAttractionsAsync();
                var attraction = attractions.FirstOrDefault(a => a.Id == _selectedAttractionId);

                if (attraction != null)
                {
                    attraction.Name = txtAttractionName.Text.Trim();
                    attraction.Description = txtDescription.Text.Trim();
                    attraction.Category = (cmbCategory.SelectedItem as ComboBoxItem).Content.ToString();
                    attraction.OpeningHours = txtOpeningHours.Text.Trim();
                    attraction.ImageUrl = txtImageUrl.Text.Trim();
                    attraction.IsActive = chkIsActive.IsChecked == true;

                    await _dbService.UpdateAttractionAsync(_selectedAttractionId, attraction);

                    MessageBox.Show("Attraction updated successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    LoadAttractions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating attraction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditAttraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var attractionId = button.Tag.ToString();

                var attractions = await _dbService.GetAllAttractionsAsync();
                var attraction = attractions.FirstOrDefault(a => a.Id == attractionId);

                if (attraction != null)
                {
                    _selectedAttractionId = attraction.Id;
                    txtAttractionName.Text = attraction.Name;
                    txtDescription.Text = attraction.Description;
                    txtOpeningHours.Text = attraction.OpeningHours;
                    txtImageUrl.Text = attraction.ImageUrl;
                    chkIsActive.IsChecked = attraction.IsActive;

                    // Set category
                    foreach (ComboBoxItem item in cmbCategory.Items)
                    {
                        if (item.Content.ToString() == attraction.Category)
                        {
                            cmbCategory.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attraction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteAttraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var attractionId = button.Tag.ToString();

                var result = MessageBox.Show("Are you sure you want to delete this attraction?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _dbService.DeleteAttractionAsync(attractionId);
                    MessageBox.Show("Attraction deleted successfully!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAttractions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting attraction: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedAttractionId = null;
            txtAttractionName.Clear();
            txtDescription.Clear();
            cmbCategory.SelectedIndex = -1;
            txtOpeningHours.Text = "9:00 AM - 5:00 PM";
            txtImageUrl.Clear();
            chkIsActive.IsChecked = true;
            txtSearch.Clear();
        }

        private async void SearchAttraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var searchText = txtSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(searchText))
                {
                    LoadAttractions();
                    return;
                }

                var allAttractions = await _dbService.GetAllAttractionsAsync();
                var filtered = allAttractions.Where(a =>
                    a.Name.ToLower().Contains(searchText) ||
                    a.Category.ToLower().Contains(searchText) ||
                    a.Description.ToLower().Contains(searchText)
                ).OrderBy(a => a.Name).ToList();

                attractionsPanel.ItemsSource = filtered;
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
            LoadAttractions();
        }
    }
}