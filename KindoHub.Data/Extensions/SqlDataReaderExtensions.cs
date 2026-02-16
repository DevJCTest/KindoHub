using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KindoHub.Data.Extensions
{
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Obtiene un string del reader, devolviendo string.Empty si es null.
        /// </summary>
        public static string GetStringOrEmpty(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal).Trim();
        }

        /// <summary>
        /// Obtiene un string nullable del reader.
        /// </summary>
        public static string? GetNullableString(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal).Trim();
        }

        /// <summary>
        /// Obtiene un DateTime nullable del reader.
        /// </summary>
        public static DateTime? GetNullableDateTime(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
        }

        /// <summary>
        /// Obtiene un int nullable del reader.
        /// </summary>
        public static int? GetNullableInt(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        }

        /// <summary>
        /// Obtiene un decimal nullable del reader.
        /// </summary>
        public static decimal? GetNullableDecimal(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
        }

        /// <summary>
        /// Obtiene un decimal del reader, devolviendo 0 si es null.
        /// </summary>
        public static decimal GetDecimalOrZero(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
        }
    }

}
