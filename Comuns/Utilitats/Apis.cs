using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Comuns
{
    public class ExceptionApi : System.Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? Endpoint { get; }
        public string? RawResponse { get; }

        public ExceptionApi(
            string message,
            HttpStatusCode statusCode,
            string? endpoint = null,
            string? rawResponse = null,
            System.Exception? inner = null)
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

    public class FestiusCatalunya
    {
        #region *** Definicio de les classes per mapejar registre JSON ***

        public class EstatalsAutonomicsResult
        {
            [JsonPropertyName("date")]
            public DateTime Data { get; set; }

            [JsonPropertyName("localName")]
            public string NomLocal { get; set; }

            [JsonPropertyName("name")]
            public string Nom { get; set; }

            [JsonPropertyName("counties")]
            public List<string> Counties { get; set; } // On surt "ES-CT"

            [JsonPropertyName("types")]
            public string[] Types { get; set; }
        }

        public class LocalsResult
        {
            public string any_calendari { get; set; }
            public DateTime data { get; set; }
            public string ajuntament_o_nucli_municipal { get; set; }
            public string codi_municipal { get; set; }
            public string codi_municipi_ine { get; set; }
            public string pedania { get; set; }
            public string festiu { get; set; }
            public string codiidescat { get; set; }
        }

        public class TotsResult
        {
            public DateTime Data { get; set; }
            public bool? EsAutonomica { get; set; }
            public string Ambit { get; set; }
        }

        #endregion

        public static async Task<List<TotsResult>> Tots(int any, string municipi = null)
        {
            List<TotsResult> festius = new List<TotsResult>();

            foreach (var festiu in EstatalsAutonomics(any).Result)
            {
                festius.Add(new TotsResult
                {
                    Data = festiu.Data,
                    EsAutonomica = festiu.Counties != null,
                    Ambit = festiu.Counties != null ? "Autonòmic" : "Estatal",
                });
            }

            foreach (var festiu in Locals(any, municipi).Result)
            {
                festius.Add(new TotsResult
                {
                    Data = festiu.data,
                    EsAutonomica = null,
                    Ambit = festiu.ajuntament_o_nucli_municipal
                });
            }

            return festius;
        }

        public static async Task<List<EstatalsAutonomicsResult>> EstatalsAutonomics(int any)
        {
            using var client = new HttpClient();
            string url = $"https://date.nager.at/api/v3/PublicHolidays/{any}/ES";

            try
            {
                var festiusDelAny = await client.GetStringAsync(url);

                var festes = JsonSerializer.Deserialize<List<EstatalsAutonomicsResult>>(festiusDelAny);

                if (festiusDelAny != null)
                {
                    // Filtrem: 
                    // 1. Festius estatals (Counties és null)
                    // 2. Festius de Catalunya (Counties conté "ES-CT")
                    var festiusPropis = festes
                        .Where(h => h.Counties == null || h.Counties.Contains("ES-CT"))
                        .OrderBy(h => h.Data)
                        .ToList();

                    return festiusPropis;
                }
                else
                {
                    return new List<EstatalsAutonomicsResult>();
                }
            }
            catch (Exception ex)
            {
                throw new ExceptionApi(ex.Message, 0, url, null, ex);
            }
        }

        public static async Task<List<LocalsResult>> Locals(int any, string municipi = null)
        {
            using var client = new HttpClient();

            string url = "https://analisi.transparenciacatalunya.cat/resource/b4eh-r8up.json" +
            $"?any_calendari={any}";

            if (municipi != null)
                url += $"&ajuntament_o_nucli_municipal={municipi}"; // Filtra per municipi (ex: "Barcelona")

            try
            {
                var totsElsFestius = await client.GetStringAsync(url);

                var festes = JsonSerializer.Deserialize<List<LocalsResult>>(totsElsFestius);

                return festes;
            }
            catch (Exception ex)
            {
                throw new ExceptionApi(ex.Message, 0, url, null, ex);
            }
        }
    }

    public class FinancialModelingPrep
    {
        public class MarketHolidayResult
        {
            public string Exchange { get; set; }

            [JsonPropertyName("Name")]
            public string Nom { get; set; }

            [JsonPropertyName("Date")]
            public DateTime Data { get; set; }
            public JsonElement IsClosed { get; set; }
            public bool EstaTancat
            {
                get
                {
                    return IsClosed.ValueKind == JsonValueKind.True;
                }
            }

            public string AdjCloseTime { get; set; }
            public TimeSpan? HoraTancament
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(AdjCloseTime))
                        return null;
                    else
                        return TimeSpan.ParseExact(AdjCloseTime, "hh\\:mm", null);
                }
            }
        }

        public static async Task<List<MarketHolidayResult>> FestiusBorses(string exchange = "NASDAQ", int any = 2026)
        {
            // https://site.financialmodelingprep.com/developer/docs

            var ApiKeyFMP = "FbuLo7QMuoYakBR4y3tGgmN1vUlHDkqH";  // 👉 posa aquí la teva clau de FMP

            var from = new DateTime(any, 1, 1).AddDays(-1).ToString("yyyy-MM-dd");
            var to = new DateTime(any, 12, 31).ToString("yyyy-MM-dd");

            string url = $"https://financialmodelingprep.com/stable/holidays-by-exchange" +
            $"?exchange={exchange}&from={from}&to={to}&apikey={ApiKeyFMP}";

            try
            {
                HttpClient client = new HttpClient();
                string json = await client.GetStringAsync(url);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var holidays = JsonSerializer.Deserialize<List<MarketHolidayResult>>(json, options);

                return holidays;
            }
            catch (Exception ex)
            {
                throw new ExceptionApi(ex.Message, 0, url, null, ex);
            }
        }
    }

    public class EodhdUserService
    {
        private static readonly string ApiKeyEODHD = "6999904864f052.20133225";  // 👉 posa aquí la teva clau de FMP

        private static readonly HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("https://eodhd.com/api/")
        };

        /// <summary>
        /// Retrieves user information from the EODHD API and returns the parsed response as an array of strings.
        /// </summary>
        /// <remarks>The returned array contains key-value pairs from the API response, with formatting
        /// applied to remove quotes and braces and to add spaces around colons. The method performs an HTTP GET request
        /// and may throw exceptions if the request fails or the response cannot be parsed.</remarks>
        /// <returns>An array of strings containing the parsed user information from the EODHD API response. The array may be
        /// empty if the response contains no data.</returns>
        /// <exception cref="ExceptionEODHd">Thrown when an error occurs while retrieving or parsing the user information from the EODHD API.</exception>
        
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
                throw new ExceptionApi(ex.Message, 0, endpoint, null, ex);
            }
        }

        public static async Task<string> TrobaTickerEODHd(string isin)
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
                throw new ExceptionApi(ex.Message, 0, endpoint, null, ex);
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
        public static async Task<decimal?> UltimTancamentEODHd(string ticker)
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
                throw new ExceptionApi(ex.Message, 0, endpoint, null, ex);
            }
        }
    }
}