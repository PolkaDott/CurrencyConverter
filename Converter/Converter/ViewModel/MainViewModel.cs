using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using Converter.Model;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Converter.ViewModel
{
    public class MainViewModel : BindableObject
    {
        private HttpClient httpClient;

        public MainViewModel()
        {
            httpClient = new HttpClient();
            CurrencyList = new List<Currency>();
            CurrencyList = GetCurrencyList();
        }

        private List<Currency> _currencyList;
        public List<Currency> CurrencyList
        {
            get => _currencyList;
            set
            {
                _currencyList = value;
                OnPropertyChanged(nameof(CurrencyList));
            }
        }

        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                CurrencyList = GetCurrencyList();
                //Preferences.Set("Date", _selectedDate.ToString());
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        public List<Currency> GetCurrencyList()
        {
            try
            {
                string date = SelectedDate.ToString("yyyy/MM/dd").Replace('.', '/');
                string uri = $"https://www.cbr-xml-daily.ru/archive/{date}/daily_json.js";
                string result = httpClient.GetStringAsync(uri).Result;
                string allValutesString = JObject.Parse(result)["Valute"]?.ToString();
                var allValutesDictionary = JsonConvert.DeserializeObject<Dictionary<string, Currency>>(allValutesString);
                return allValutesDictionary?.Select(x => x.Value)?.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SelectedDate = SelectedDate.AddDays(-1);
                return GetCurrencyList();
            }
        }
    }
}
