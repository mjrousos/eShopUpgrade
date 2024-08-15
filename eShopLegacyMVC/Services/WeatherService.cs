﻿using eShopLegacyMVC.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace eShopLegacyMVC.Services
{
    public class WeatherService
    {
        const int DefaultZipCode = 98052;
        const string RequestFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public WeatherService(string apiKey, HttpClient httpClient)
        {
            _apiKey = apiKey ?? Environment.GetEnvironmentVariable("WEATHER_API_KEY");
            _httpClient = httpClient;
        }

        public async Task<int?> GetUserCurrentTemperature(ApplicationUser user, bool celsius)
        {
            var zipCode = (await user.GetZipCode(_httpClient)) ?? DefaultZipCode;

            return GetTemperature(zipCode, celsius);
        }

        private int? GetTemperature(int zipCode, bool celsius)
        {
            using (var client = new WebClient())
            {
                var data = client.DownloadData(string.Format(RequestFormatString, _apiKey, zipCode));

                if (data != null && data.Length > 0)
                {
                    var weatherData = JsonConvert.DeserializeAnonymousType(Encoding.UTF8.GetString(data), new { Current = new { Temp_C = 0, Temp_F = 0 } });
                    if (weatherData != null)
                    {
                        return celsius ? weatherData.Current.Temp_C : weatherData.Current.Temp_F;
                    }
                }
            }

            return null;
        }
    }
}