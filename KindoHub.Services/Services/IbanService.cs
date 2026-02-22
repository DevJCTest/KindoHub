using KindoHub.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Services.Services
{
    public class IbanService : IIbanService
    {
        // Diccionario con longitudes de IBAN por código de país (según ISO 13616)
        private static readonly Dictionary<string, int> CountryLengths = new Dictionary<string, int>
        {
            { "AD", 24 }, { "AE", 23 }, { "AL", 28 }, { "AT", 20 }, { "AZ", 28 },
            { "BA", 20 }, { "BE", 16 }, { "BG", 22 }, { "BH", 22 }, { "BR", 29 },
            { "BY", 28 }, { "CH", 21 }, { "CR", 22 }, { "CY", 28 }, { "CZ", 24 },
            { "DE", 22 }, { "DK", 18 }, { "DO", 28 }, { "EE", 20 }, { "ES", 24 },
            { "FI", 18 }, { "FO", 18 }, { "FR", 27 }, { "GB", 22 }, { "GE", 22 },
            { "GI", 23 }, { "GL", 18 }, { "GR", 27 }, { "GT", 28 }, { "HR", 21 },
            { "HU", 28 }, { "IE", 22 }, { "IL", 23 }, { "IQ", 23 }, { "IS", 26 },
            { "IT", 27 }, { "JO", 30 }, { "KW", 30 }, { "KZ", 20 }, { "LB", 28 },
            { "LC", 32 }, { "LI", 21 }, { "LT", 20 }, { "LU", 20 }, { "LV", 21 },
            { "MC", 27 }, { "MD", 24 }, { "ME", 22 }, { "MK", 19 }, { "MR", 27 },
            { "MT", 31 }, { "MU", 30 }, { "NL", 18 }, { "NO", 15 }, { "PK", 24 },
            { "PL", 28 }, { "PS", 29 }, { "PT", 25 }, { "QA", 29 }, { "RO", 24 },
            { "RS", 22 }, { "SA", 24 }, { "SC", 31 }, { "SE", 24 }, { "SI", 19 },
            { "SK", 24 }, { "SM", 27 }, { "ST", 25 }, { "SV", 28 }, { "TL", 23 },
            { "TN", 24 }, { "TR", 26 }, { "UA", 29 }, { "VA", 22 }, { "VG", 24 },
            { "XK", 20 }
        };

        public async Task<bool> IsValid(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return false;

            string normalized = Normalize(iban);
            if (!CheckLength(normalized))
                return false;

            string rearranged = Rearrange(normalized);
            string converted = ConvertToNumber(rearranged);
            return ValidateModulo(converted);
        }

        /// <summary>
        /// Normaliza el IBAN: elimina espacios y convierte a mayúsculas.
        /// </summary>
        private static string Normalize(string iban)
        {
            return iban.Replace(" ", "").ToUpper();
        }

        /// <summary>
        /// Verifica si la longitud del IBAN coincide con la del país.
        /// </summary>
        private static bool CheckLength(string iban)
        {
            if (iban.Length < 4)
                return false;

            string countryCode = iban.Substring(0, 2);
            return CountryLengths.TryGetValue(countryCode, out int expectedLength) && iban.Length == expectedLength;
        }

        /// <summary>
        /// Reordena el IBAN: mueve los primeros 4 caracteres al final.
        /// </summary>
        private static string Rearrange(string iban)
        {
            return iban.Substring(4) + iban.Substring(0, 4);
        }

        /// <summary>
        /// Convierte letras a números (A=10, B=11, ..., Z=35).
        /// </summary>
        private static string ConvertToNumber(string iban)
        {
            var result = new System.Text.StringBuilder();
            foreach (char c in iban)
            {
                if (char.IsLetter(c))
                {
                    result.Append((c - 'A' + 10).ToString());
                }
                else if (char.IsDigit(c))
                {
                    result.Append(c);
                }
                else
                {
                    // Carácter inválido, aunque en teoría no debería llegar aquí
                    return string.Empty;
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Valida el módulo 97: el número debe ser divisible por 97 con resto 1.
        /// </summary>
        private static bool ValidateModulo(string numberString)
        {
            if (string.IsNullOrEmpty(numberString))
                return false;

            try
            {
                BigInteger number = BigInteger.Parse(numberString);
                return number % 97 == 1;
            }
            catch
            {
                return false;
            }
        }

    }
}
