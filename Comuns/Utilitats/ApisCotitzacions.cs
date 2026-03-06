using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Comuns
{
    public class EodhdUserService
    {
        private static readonly string ApiKeyEODHD;

        static EodhdUserService()
        {
            ApiKeyEODHD = Environment.GetEnvironmentVariable("EODHD_API_KEY");
        }

        private static readonly HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("https://eodhd.com/api/")
        };

        public static async Task<string[]> EstatEodHd()
        {
            var searchResponsex = await http.GetStringAsync($"user?api_token={ApiKeyEODHD}");

            string[] array = searchResponsex
                .Replace("\"", "")   // elimina totes les cometes
                .Replace("{", "")    // elimina {
                .Replace("}", "")    // elimina }
                .Replace(":", " : ") // separa : amb espais
                .Split(',');

            return array;
        }

        public static async Task<string> GetTicker(string isin)
        {
            // 1️⃣ Buscar ticker
            var searchResponse = await http.GetStringAsync($"search/{isin}?api_token={ApiKeyEODHD}&fmt=json");

            var searchJson = JsonDocument.Parse(searchResponse);

            if (searchJson.RootElement.GetArrayLength() == 0)
            {
                Console.WriteLine("ISIN no trobat a EODHD.");
                return null;
            }

            var firstResult = searchJson.RootElement[0];
            string code = firstResult.GetProperty("Code").GetString();
            string exchange = firstResult.GetProperty("Exchange").GetString();

            string ticker = $"{code}.{exchange}";
        
            return ticker;
        }

        /// <summary>
        /// Recupera asíncronament el preu de tancament més recent per a l'ISIN especificat. Opcionalment, converteix el preu 
        /// a euros utilitzant el tipus de canvi EUR/USD actual.
        /// </summary>
        /// <remarks>
        /// Si es sol·licita la conversió a euros i el tipus de canvi EUR/USD no està ja disponible, 
        /// el mètode recupera el tipus de canvi més recent abans de realitzar la conversió.
        /// </remarks>
        /// <param name="ticker">El número d'identificació internacional de valors (ISIN) de l'actiu per al qual s'ha d'obtenir l'últim tancament
        /// price.
        /// </param>
        /// <returns>A <see cref="decimal"/> representing the last closing price of the specified asset, or <see
        /// langword="null"/> if the ticker is not found or the price cannot be retrieved.</returns>
        public static async Task<decimal?> GetLastCloseFromIsinAsync(string ticker)
        {
            // 2️⃣ Obtenir last_close
            var closeResponse = await http.GetStringAsync($"eod/{ticker}?api_token={ApiKeyEODHD}&fmt=json&filter=last_close");

            if (decimal.TryParse(closeResponse, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lastClose))
            {
                return lastClose;
            }

            return null;
        }
    }
}