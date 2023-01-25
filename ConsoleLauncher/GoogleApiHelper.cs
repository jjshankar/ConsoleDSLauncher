using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ConsoleLauncher
{
    class GoogleApiHelper
    {
        internal struct Coordinates
        {
            public string lat;
            public string lng;
        }

        public static void Run(string[] args)
        {
            // e.g: 1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA
            Console.Write("Enter address to geocode as a single line (leave blank to exit):");
            string address = Console.ReadLine();

            if (!string.IsNullOrEmpty(address))
            {
                Coordinates coords = Geocode(address).Result;
                Console.WriteLine("Address was geocoded as - Lat: {0}, Lng: {1}", coords.lat, coords.lng);
            }
            return;
        }

        public async static Task<Coordinates> Geocode(string address)
        {
            string _serviceBaseUri = "https://maps.googleapis.com/maps/api/geocode/";
            string _method = "json";
            string _serviceAPIKey = "AIzaSyC9FCXHgUM3_X6yiBo81p9jED_0omyHmes";

            if (!string.IsNullOrEmpty(address))
            {
                HttpClient client = new HttpClient();

                // Get client
                if (client != null)
                {
                    client.BaseAddress = new Uri(_serviceBaseUri);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }

                string path = _method;
                path += "?address=" + address.Replace(" ", "+");
                path += "&key=" + _serviceAPIKey;

                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    string resp = await response.Content.ReadAsStringAsync();
                    // Debug
                    Console.WriteLine("----- DBG -----\n {0} \n----------", resp);
                    // Debug
                    JObject results = JObject.Parse(resp);

                    return new Coordinates
                    {
                        lat = (string)results["results"][0]["geometry"]["location"]["lat"],
                        lng = (string)results["results"][0]["geometry"]["location"]["lng"]
                    };
                }
            }

            // Return empty
            return new Coordinates { lat = "n/a", lng = "n/a" };
        }
    }
}
