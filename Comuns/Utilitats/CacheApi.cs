using Comuns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Comuns
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;

    public static class CacheApi
    {
        // Aquest fitxer es crearà automàticament a la carpeta bin/Debug
        private const string RUTA_FITXER = "cache_eodhd.json";

        // El nostre diccionari en memòria: La clau serà "TICKER_DATA" i el valor l'import
        private static Dictionary<string, decimal> _cache = new Dictionary<string, decimal>();

        // Aquest constructor estàtic s'executa automàticament el primer cop que fas servir la classe
        static CacheApi()
        {
            if (File.Exists(RUTA_FITXER))
            {
                string json = File.ReadAllText(RUTA_FITXER);
                _cache = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json) ?? new Dictionary<string, decimal>();
            }
        }

        public static decimal? ObtenirValor(string ticker, DateTime data)
        {
            string clau = GenerarClau(ticker, data);
            if (_cache.TryGetValue(clau, out decimal valor))
            {
                return valor; // Visca! Estava a la memòria
            }
            return null; // No hi és, tocarà cridar a l'API
        }

        public static void DesarValor(string ticker, DateTime data, decimal valor)
        {
            string clau = GenerarClau(ticker, data);

            // Afegim o actualitzem el valor al diccionari
            _cache[clau] = valor;

            // Guardem immediatament el diccionari al disc dur
            string json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(RUTA_FITXER, json);
        }

        private static string GenerarClau(string ticker, DateTime data)
        {
            // Genera una clau única tipus: "BRYN.XETRA_20260420"
            return $"{ticker}_{data:yyyyMMdd}";
        }
    }
}