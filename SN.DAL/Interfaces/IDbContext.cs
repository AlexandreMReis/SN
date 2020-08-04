using Npgsql;
using SN.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SN.DAL.Interfaces
{
    public interface IDbContext : IDisposable
    {
        DbQueryResponse<T> ExecuteQuery<T>(string commandText, List<NpgsqlParameter> parameters, Func<DbDataReader, T> readRowFunc, string inputLogMessage);
        DbNonQueryResponse ExecuteNonQuery(string commandText, List<NpgsqlParameter> parameters, string inputLogMessage);
    }
}
