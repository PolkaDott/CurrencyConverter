using System;
using System.Collections.Generic;
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
            SelectedDate = DateTime.Parse(Preferences.Get("date", DateTime.Now.ToString()));
            CurrencyList = GetCurrencyList();
            FirstValue = Preferences.Get("firstValue", "");
            if (!Preferences.ContainsKey("firstCurr"))
                FirstCurr = CurrencyList[new Random().Next(CurrencyList.Count)];
            else
            {
                string curr = Preferences.Get("firstCurr", "");
                FirstCurr = CurrencyList.First(x => x.CharCode == curr);
            }
            if (!Preferences.ContainsKey("secondCurr"))
                SecondCurr = CurrencyList[new Random().Next(CurrencyList.Count)];
            else
            {
                string curr = Preferences.Get("secondCurr", "");
                SecondCurr = CurrencyList.First(x => x.CharCode == curr);
            }
            
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

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                CurrencyList = GetCurrencyList();
                Preferences.Set("date", _selectedDate.ToString());
                SecondValue = "";
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        private void RecountCurrencies()
        {
            double a;
            if (Double.TryParse(FirstValue, out a) && SecondCurr != null && FirstCurr != null)
            {
                SecondValue = $"{a * (FirstCurr.Value / FirstCurr.Nominal) / (SecondCurr.Value / SecondCurr.Nominal):0.00}";
            }
        }

        private Currency _firstCurr;
        public Currency FirstCurr
        {
            get => _firstCurr;
            set
            {
                _firstCurr = value;
                
                RecountCurrencies();
                Preferences.Set("firstCurr", value?.CharCode);
                OnPropertyChanged(nameof(FirstCurr));
            }
        }

        private Currency _secondCurr;
        public Currency SecondCurr
        {
            get => _secondCurr;
            set
            {
                _secondCurr = value;
                RecountCurrencies();
                Preferences.Set("secondCurr", value?.CharCode);
                OnPropertyChanged(nameof(SecondCurr));
            }
        }

        private string _firstValue;
        public string FirstValue
        {
            get => _firstValue;
            set
            {
                _firstValue = value;
                RecountCurrencies();
                Preferences.Set("firstValue", _firstValue);
                OnPropertyChanged(nameof(FirstValue));
            }
        }

        private string _secondValue;

        public string SecondValue
        {
            get => _secondValue;
            set
            {
                _secondValue = value;
                OnPropertyChanged(nameof(SecondValue));
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
                return allValutesDictionary.Select(x => x.Value).ToList();
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
