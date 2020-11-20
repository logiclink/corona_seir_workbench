using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogicLink.Corona {

    /// <summary>
    /// Gets population data from the World Bank REST-API
    /// </summary>
    public class WPPopulation {
        private static Dictionary<string, int> _dic = new Dictionary<string, int>();      // Memory Cache

        private const string WB_POPULATION_URL = "https://api.worldbank.org/v2/country/{0}/indicator/SP.POP.TOTL?date={1}&format=json";

        /// <summary>
        /// Gets the population for a country an year from the World Bank REST-API
        /// </summary>
        /// <param name="sCountryISO3">ISO3 code of the country</param>
        /// <param name="iYear">Year of the population</param>
        /// <returns>awaitable Population</returns>
        private async Task<int> GetPopulationAsync(string sCountryISO3, int iYear) {
            using(HttpClient cli = new HttpClient()) {
                HttpResponseMessage rm = await cli.GetAsync(new Uri(string.Format(WB_POPULATION_URL, sCountryISO3, iYear)));
                if(rm.IsSuccessStatusCode) {
                    JsonElement j = await JsonSerializer.DeserializeAsync<JsonElement>(await rm.Content.ReadAsStreamAsync());
                    return j[1][0].EnumerateObject().FirstOrDefault(p => p.Name == "value").Value.TryGetInt32(out int i) ? i : 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the population for a country an year
        /// </summary>
        /// <param name="sCountry">Name of the Country</param>
        /// <param name="iYear">Year of the population</param>
        /// <returns>awaitable Population</returns>
        /// <remarks>
        /// The function caches previous values and returns population values for two ships also
        /// </remarks>
        public async Task<int> GetDataAsync(string sCountry, int iYear) {
            if(_dic.TryGetValue(sCountry + iYear.ToString(), out int iPopulation))
                return iPopulation;

            string sCountryISO3 = Settings.Default.CountriesISO3[Settings.Default.Countries.IndexOf(sCountry)];
            switch(sCountryISO3) {
                case "MSD":                         // https://de.wikipedia.org/wiki/Diamond_Princess
                    iPopulation = 2670;
                    break;
                case "MSZ":                         // https://www.seereisedienst.de/kreuzfahrtschiffe/ms-zaandam/
                    iPopulation = 1432 + 615;
                    break;
                default:
                    using(HttpClient cli = new HttpClient()) {
                        HttpResponseMessage rm = await cli.GetAsync(new Uri(string.Format(WB_POPULATION_URL, sCountryISO3, iYear)));
                        if(rm.IsSuccessStatusCode) {
                            JsonElement j = await JsonSerializer.DeserializeAsync<JsonElement>(await rm.Content.ReadAsStreamAsync());
                            j[1][0].EnumerateObject().FirstOrDefault(p => p.Name == "value").Value.TryGetInt32(out iPopulation);
                        }
                    }
                    break;
            };

            _dic.Add(sCountry + iYear.ToString(), iPopulation);
            return iPopulation;
        }
    }
}
