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

        // load currencies
        private async Task LoadCurrenciesAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"{API_URL}/currencies");
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        JObject currencies = JObject.Parse(json);

                        List<string> currencyKeys = new List<string>(currencies.Properties().Select(p => p.Name));

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

        // calculating
        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtAmount.Text) || !decimal.TryParse(txtAmount.Text, out decimal amount))
                {
                    MessageBox.Show("Please enter a valid amount.");
                    return;
                }

                if (cmbFromCurrency.SelectedItem == null || cmbToCurrency.SelectedItem == null)
                {
                    MessageBox.Show("Please select both currencies.");
                    return;
                }

                string fromCurrency = cmbFromCurrency.SelectedItem.ToString();
                string toCurrency = cmbToCurrency.SelectedItem.ToString();

                decimal result = await ConvertCurrency(fromCurrency, toCurrency, amount);
                txtResult.Text = $"{amount} {fromCurrency} = {result:F2} {toCurrency}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // api request
        private async Task<decimal> ConvertCurrency(string fromCurrency, string toCurrency, decimal amount)
        {
            string url = $"{API_URL}/latest?from={fromCurrency}&to={toCurrency}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(json);

                    decimal rate = (decimal)data["rates"][toCurrency]; // Nur der Umrechnungskurs wird genommen
                    return rate * amount; // Der Betrag wird dann auf Basis des Kurses multipliziert
                }
                else
                {
                    throw new Exception("Failed to fetch exchange rate.");
                }
            }
        }

        // clear button
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            cmbFromCurrency.SelectedItem = null;
            cmbToCurrency.SelectedItem = null;
            txtAmount.Text = string.Empty;
            txtResult.Text = "Conversion Result will appear here";
        }
    }
}
