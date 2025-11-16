using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OnewheroBayVisitorManagement.Services;

namespace OnewheroBayVisitorManagement.Views
{
    public partial class AnalyticsWindow : Window
    {
        private readonly MongoDBService _dbService;

        public AnalyticsWindow()
        {
            InitializeComponent();
            _dbService = new MongoDBService();
            LoadAnalytics();
        }

        private async void LoadAnalytics()
        {
            try
            {
                // Get all data
                var visitors = await _dbService.GetAllVisitorsAsync();
                var bookings = await _dbService.GetAllBookingsAsync();
                var events = await _dbService.GetAllEventsAsync();

                // Calculate statistics
                txtTotalVisitors.Text = visitors.Count.ToString();
                txtTotalBookings.Text = bookings.Count.ToString();
                txtActiveEvents.Text = events.Count(e => e.IsActive && e.EventDate >= DateTime.Now).ToString();

                decimal totalRevenue = bookings.Sum(b => b.TotalAmount);
                txtTotalRevenue.Text = $"${totalRevenue:F2}";

                // Popular Events (by booking count)
                var popularEvents = bookings
                    .GroupBy(b => new { b.EventId, b.EventName })
                    .Select(g => new
                    {
                        EventName = g.Key.EventName,
                        BookingCount = g.Count(),
                        Revenue = g.Sum(b => b.TotalAmount)
                    })
                    .OrderByDescending(x => x.BookingCount)
                    .Take(10)
                    .ToList();

                dgPopularEvents.ItemsSource = popularEvents;

                // Top Cities
                var topCities = visitors
                    .Where(v => !string.IsNullOrEmpty(v.City))
                    .GroupBy(v => v.City)
                    .Select(g => new
                    {
                        City = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                dgCities.ItemsSource = topCities;

                // Popular Interests
                var allInterests = visitors
                    .SelectMany(v => v.Interests)
                    .Where(i => !string.IsNullOrEmpty(i))
                    .GroupBy(i => i)
                    .Select(g => new
                    {
                        Interest = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                dgInterests.ItemsSource = allInterests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analytics: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadAnalytics();
        }
    }
}