using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Comuns
{
    public class EodhdApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? Endpoint { get; }
        public string? RawResponse { get; }

        public EodhdApiException(
            string message,
            HttpStatusCode statusCode,
            string? endpoint = null,
            string? rawResponse = null,
            Exception? inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
            Endpoint = endpoint;
            RawResponse = rawResponse;
        }

        public override string ToString()
        {
            return
                $"EODHD API Error\n" +
                $"Status: {(int)StatusCode} ({StatusCode})\n" +
                (Endpoint != null ? $"Endpoint: {Endpoint}\n" : "") +
                (RawResponse != null ? $"Response: {RawResponse}\n" : "") +
                $"Message: {Message}\n";
        }
    }

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
            string endpoint = $"user?api_token={ApiKeyEODHD}";
            try
            {
                var searchResponsex = await http.GetStringAsync(endpoint);

                string[] array = searchResponsex
                    .Replace("\"", "")   // elimina totes les cometes
                    .Replace("{", "")    // elimina {
                    .Replace("}", "")    // elimina }
                    .Replace(":", " : ") // separa : amb espais
                    .Split(',');

                return array;
            }
            catch (Exception ex)
            {
                throw new EodhdApiException(ex.Message, 0, endpoint, null, ex);
            }
        }

        public static async Task<string> GetTicker(string isin)
        {
            string endpoint = $"search/{isin}?api_token={ApiKeyEODHD}&fmt=json";

            try
            {
                // 1️⃣ Buscar ticker
                var searchResponse = await http.GetStringAsync(endpoint);

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
            catch (Exception ex)
            {
                throw new EodhdApiException(ex.Message, 0, endpoint, null, ex);
            }
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
            string endpoint = $"eod/{ticker}?api_token={ApiKeyEODHD}&fmt=json&filter=last_close";

            try
            { 
            // 2️⃣ Obtenir last_close
            var closeResponse = await http.GetStringAsync(endpoint);

            if (decimal.TryParse(closeResponse, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lastClose))
            {
                return lastClose;
            }

            return null;
            }
            catch (Exception ex)
            {
                throw new EodhdApiException(ex.Message, 0, endpoint, null, ex);
            }
        }
    }
}