using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Threading.Tasks;
using System.IO;

namespace Temp_GUI_GroundTerminal
{
    public partial class MainWindow : Window
    {
        private HubConnection _connection;
        public ObservableCollection<TelemetryDataModel> TelemetryDataList { get; set; }
        public ObservableCollection<TelemetryDataModel> SearchResults { get; set; }
        private readonly TelemetryApiService _telemetryApiService;

        public MainWindow()
        {
            InitializeComponent();
            TelemetryDataList = new ObservableCollection<TelemetryDataModel>();
            SearchResults = new ObservableCollection<TelemetryDataModel>();
            this.DataContext = this;  // Set the DataContext to this Window for binding
            _telemetryApiService = new TelemetryApiService();
            // Initialize SignalR connection
            InitializeSignalRConnection();
        }

        private async void InitializeSignalRConnection()
        {
            // Initialize the HubConnection
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7224/telemetryHub")  // Replace with your actual SignalR URL
                .Build();

            // Handle receiving telemetry data
            _connection.On<string>("ReceiveTelemetry", (message) =>
            {
                // Use the TelemetryParser to parse the message
                var telemetryData = TelemetryParser.Parse(message);

                if (telemetryData != null)
                {
                    // Successfully parsed telemetry data
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TelemetryDataList.Add(telemetryData);

                        // Optionally, remove the oldest entry if you exceed 100 items
                        if (TelemetryDataList.Count > 100)
                        {
                            TelemetryDataList.RemoveAt(0);
                        }
                    });
                }
                else
                {
                    // Handle the error (e.g., log, show alert, etc.)
                    Console.WriteLine("Failed to parse telemetry data.");
                }
            });

            // Start the connection
            try
            {
                await _connection.StartAsync();
                lblStatus.Text = "Status: Connected";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Status: {ex.Message}";
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = searchTextBox.Text; // Get search term from TextBox
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var telemetryData = await _telemetryApiService.SearchTelemetryAsync(searchTerm);

                if (telemetryData != null)
                {
                    // Bind the data to the DataGrid
                    searchDataGrid.ItemsSource = telemetryData;
                }
                else
                {
                    MessageBox.Show("No data found or error fetching data.");
                }
            }
            else
            {
                MessageBox.Show("Please enter a search term.");
            }
        }


        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = searchTextBox.Text;
            string filePath = "telemetry_log_"+searchQuery+".txt"; // Define the log file path

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    // Log each search result to the file
                    foreach (var data in SearchResults)
                    {
                        writer.WriteLine($"{data.Timestamp} - {data.TailNumber} - {data.SequenceNumber} - {data.X},{data.Y},{data.Z},{data.Weight},{data.Altitude},{data.Pitch},{data.Bank},{data.Checksum}");
                    }
                }
                MessageBox.Show("Search results logged successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging to file: {ex.Message}");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Close the connection when the window is closed
            if (_connection.State == HubConnectionState.Connected)
            {
                _connection.StopAsync().Wait();
            }
        }
      

    }

}
