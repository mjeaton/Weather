using System;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace weather
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Mike's Weather App");

            IWeatherFetcher wf = new WeatherFetcher();
            var currentWeather = wf.GetCurrentWeather("49036");

            Console.WriteLine($"The temp in {currentWeather.name} is {currentWeather.main.temp}.");
        }
    }

    public interface IWeatherFetcher
    {
        CurrentWeather GetCurrentWeather(string zipCode);
    }

    public class WeatherFetcher : IWeatherFetcher
    {
        public CurrentWeather GetCurrentWeather(string zipCode) 
        {
            var json = RunAsync(getApiKey(), zipCode).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<CurrentWeather>(json);
        }

        private string getApiKey()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Startup>();

            IConfigurationRoot configuration = builder.Build();
            return configuration["SecretValue:key"];
        }

        private HttpClient client = new HttpClient();
        private async Task<string> RunAsync(string key, string zipCode) 
        {
            client.BaseAddress = new Uri("http://api.openweathermap.org");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var result = "";
            try 
            {
                result = await GetWeatherAsync(key, zipCode);
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }

        private async Task<string> GetWeatherAsync(string key, string zipCode) 
        {
            var result = "";
            string url = $"/data/2.5/weather?q={zipCode}&units=imperial&appid={key}";

            HttpResponseMessage response = await client.GetAsync(url);
            if(response.IsSuccessStatusCode) 
            {
                result = await response.Content.ReadAsStringAsync();
            } else {
                // dump any errors to the screen
                Console.WriteLine(response.ToString());
            }
            return result;
        }

    }

    public class CurrentWeather 
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Coord
    {
        public int lon { get; set; }
        public float lat { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public float deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public float message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
    }
}
