using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace CurrencyConverter
{
    public partial class MainWindow : Window
    {
        private const string API_URL = "https://api.frankfurter.app";

        public MainWindow()
        {
            InitializeComponent();
            LoadCurrenciesAsync();
        }

        // Load available currencies from the API
        private async Task LoadCurrenciesAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{API_URL}/currencies");
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the JSON response
                        string json = await response.Content.ReadAsStringAsync();
                        JObject currencies = JObject.Parse(json);

                        // Extract currency codes into a list
                        List<string> currencyKeys = new List<string>(currencies.Properties().Select(p => p.Name));

                        // Populate the ComboBox with the currency codes
                        cmbFromCurrency.ItemsSource = currencyKeys;
                        cmbToCurrency.ItemsSource = currencyKeys;
                    }
                    else
                    {
                        MessageBox.Show("Failed to load currencies.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Event handler for the Convert button click
        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate if the amount entered is a valid decimal number
                if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out decimal amount))
                {
                    MessageBox.Show("Please enter a valid amount.");
                    return;
                }

                // Check if both currencies are selected
                if (cmbFromCurrency.SelectedItem == null || cmbToCurrency.SelectedItem == null)
                {
                    MessageBox.Show("Please select both currencies.");
                    return;
                }

                string fromCurrency = cmbFromCurrency.SelectedItem.ToString();
                string toCurrency = cmbToCurrency.SelectedItem.ToString();

                // Perform the conversion and display the result
                decimal result = await ConvertCurrency(fromCurrency, toCurrency, amount);
                txtResult.Text = $"{amount} {fromCurrency} = {result:F2} {toCurrency}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Fetch the exchange rate and perform the currency conversion
        private async Task<decimal> ConvertCurrency(string fromCurrency, string toCurrency, decimal amount)
        {
            string url = $"{API_URL}/latest?from={fromCurrency}&to={toCurrency}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response to get the exchange rate
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    // Extract the exchange rate and calculate the result
                    decimal rate = (decimal)data["rates"][toCurrency];
                    return rate * amount;
                }
                else
                {
                    throw new Exception("Failed to fetch exchange rate.");
                }
            }
        }

        // Clear the input fields and reset the result text
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cmbFromCurrency.SelectedItem = null;
            cmbToCurrency.SelectedItem = null;
            txtAmount.Text = string.Empty;
            txtResult.Text = "Conversion Result will appear here";
        }
    }
}
