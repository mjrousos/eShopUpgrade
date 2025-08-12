using eShopLegacyMVC.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace eShopLegacyMVC.Services
{
    /// <summary>
    /// Service for integrating with external weather API to provide location-based weather information.
    /// Enhances user experience by displaying current temperature data based on user's zip code.
    /// Uses WeatherAPI.com service for reliable weather data retrieval with fallback mechanisms.
    /// </summary>
    public class WeatherService
    {
        /// <summary>
        /// Default zip code used when user location is not available (Redmond, WA)
        /// </summary>
        const int DefaultZipCode = 98052;
        
        /// <summary>
        /// API request format string for WeatherAPI.com current weather endpoint
        /// </summary>
        const string RequestFormatString = "http://api.weatherapi.com/v1/current.json?key={0}&q={1}&aqi=no";
        
        /// <summary>
        /// API key for authenticating with weather service
        /// </summary>
        private readonly string _apiKey;

        /// <summary>
        /// Initializes the weather service with API key from configuration or environment.
        /// Falls back to environment variable WEATHER_API_KEY if no key is provided.
        /// </summary>
        /// <param name="apiKey">Weather API key for service authentication</param>
        public WeatherService(string apiKey)
        {
            _apiKey = apiKey ?? Environment.GetEnvironmentVariable("WEATHER_API_KEY");
        }

        /// <summary>
        /// Retrieves current temperature for a specific user based on their zip code.
        /// Uses the user's stored zip code or falls back to default location if not available.
        /// Supports both Celsius and Fahrenheit temperature units.
        /// </summary>
        /// <param name="user">Application user with optional zip code information</param>
        /// <param name="celsius">True for Celsius, false for Fahrenheit temperature</param>
        /// <returns>Current temperature in requested units, or null if service unavailable</returns>
        public int? GetUserCurrentTemperature(ApplicationUser user, bool celsius)
        {
            var zipCode = user.ZipCode ?? DefaultZipCode;

            return GetTemperature(zipCode, celsius);
        }

        /// <summary>
        /// Retrieves current temperature for a specific zip code from the weather API.
        /// Makes HTTP request to WeatherAPI.com and parses JSON response for temperature data.
        /// Includes error handling for network issues and API failures.
        /// </summary>
        /// <param name="zipCode">Zip code for weather location lookup</param>
        /// <param name="celsius">True for Celsius, false for Fahrenheit temperature</param>
        /// <returns>Current temperature in requested units, or null if request fails</returns>
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