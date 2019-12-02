using System;
using MySql.Data.MySqlClient;

namespace CRMPhone
{
    public static class MysqlDecorator
    {
        public static string GetNullableString(this MySqlDataReader reader, string fieldName)
        {
            var index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetString(index);
            }
            return null;
        }
        public static int? GetNullableInt(this MySqlDataReader reader, string fieldName)
        {
            var index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetInt32(index);
            }
            return null;
        }

        public static DateTime? GetNullableDateTime(this MySqlDataReader reader, string fieldName)
        {
            var index = reader.GetOrdinal(fieldName);
            if (!reader.IsDBNull(index))
            {
                return reader.GetDateTime(index);
            }
            return null;
        }
    }
}