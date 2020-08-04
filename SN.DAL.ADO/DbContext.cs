using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using SN.DAL.Interfaces;
using SN.DAL.Models;
using SN.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace SN.DAL.ADO
{
    public class DbContext : IDbContext, IDisposable
    {
        private readonly string _connectionString = null;
        private NpgsqlConnection _connection = null;
        private string _connectionGuid = null;
        private object _locker = new { };

        public DbContext(IConfigurationRoot configuration)
        {
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "DbContext:ctor");
            _connectionString = configuration.GetConnectionString("DbConnection");
        }

        public DbContext(string connectionString)
        {
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "DbContext:ctor");
            _connectionString = connectionString;
        }

        #region Private
        private void Init()
        {
            var sw = Stopwatch.StartNew();

            try
            {
                lock (this._locker)
                {
                    bool newInstance = false;

                    if (this._connection == null)
                    {
                        this._connection = new NpgsqlConnection(this._connectionString);
                        this._connection.StateChange += this.Connection_StateChange;
                        newInstance = true;
                    }

                    switch (this._connection.State)
                    {
                        case ConnectionState.Closed:
                        case ConnectionState.Broken:
                            this._connection.Open();
                            LogEngine.DILogger.WriteToLog(LogLevels.Info, $"DbContext:init:{newInstance.ToString().ToLower()}:open({this._connectionGuid})");
                            break;

                        default:
                            LogEngine.DILogger.WriteToLog(LogLevels.Debug, $"DbContext:init:{newInstance.ToString().ToLower()}:{this._connection.State.ToString().ToLower()}({this._connectionGuid})");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogEngine.DILogger.WriteToLog(LogLevels.Error, $"DbContext:init:Exception: {JsonConvert.SerializeObject(ex)}");
            }
            finally
            {
                sw.Stop();
                LogEngine.DILogger.WriteToLog(LogLevels.Debug, $"DbContext:init:({this._connectionGuid}) in {sw.ElapsedMilliseconds}ms");
            }
        }

        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if (e.OriginalState == ConnectionState.Closed && e.CurrentState == ConnectionState.Open)
                this._connectionGuid = Guid.NewGuid().ToString();

            LogEngine.DILogger.WriteToLog(LogLevels.Debug, $"DbContext:Connection_StateChange:({this._connectionGuid}): {e.OriginalState.ToString()} >>> {e.CurrentState.ToString()}");
        }
        #endregion

        public DbQueryResponse<T> ExecuteQuery<T>(string commandText, List<NpgsqlParameter> parameters, Func<DbDataReader, T> readRowFunc, string inputLogMessage)
        {
            DbQueryResponse<T> output = new DbQueryResponse<T>();

            var sw = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrEmpty(commandText))
                {
                    return output;
                }

                this.Init();
                using (NpgsqlCommand command = new NpgsqlCommand(commandText, this._connection))
                {
                    command.CommandType = CommandType.Text;

                    if (parameters != null && parameters.Any())
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    using (NpgsqlDataReader dataReader = command.ExecuteReader())
                    {
                        sw.Stop();

                        if (dataReader.HasRows)
                        {
                            while (dataReader.Read())
                            {
                                var dbEntry = readRowFunc(dataReader);
                                if (dbEntry != null)
                                {
                                    output.Items.Add(dbEntry);
                                }
                            }
                        }

                        dataReader.Close();
                    }
                }

                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                LogEngine.Logger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return new DbQueryResponse<T> { Success = false };
            }
            finally
            {
                var inputLogMessageSplitParts = inputLogMessage.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
                if (inputLogMessageSplitParts.Length > 1)
                {
                    LogHelper.WriteMetric($"{inputLogMessageSplitParts[0]}", sw.Elapsed, output.Success);
                }
                else
                {
                    LogHelper.WriteMetric($"{inputLogMessage}", sw.Elapsed, output.Success);
                }
            }
        }

        public DbNonQueryResponse ExecuteNonQuery(string commandText, List<NpgsqlParameter> parameters, string inputLogMessage)
        {
            DbNonQueryResponse output = new DbNonQueryResponse();

            var sw = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrEmpty(commandText))
                {
                    return output;
                }

                using (NpgsqlCommand command = new NpgsqlCommand(commandText))
                {
                    command.CommandType = CommandType.Text;

                    if (parameters != null && parameters.Any())
                    {
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.Add(parameter);
                        }
                    }

                    this.Init();
                    command.Connection = this._connection;
                    output.AffectedRows = command.ExecuteNonQuery();
                    sw.Stop();
                }

                output.Success = true;
                return output;
            }
            catch (Exception ex)
            {
                LogEngine.Logger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return new DbNonQueryResponse { Success = false };
            }
            finally
            {
                var inputLogMessageSplitParts = inputLogMessage.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries);
                if (inputLogMessageSplitParts.Length > 1)
                {
                    LogHelper.WriteMetric($"{inputLogMessageSplitParts[0]}", sw.Elapsed, output.Success);
                }
                else
                {
                    LogHelper.WriteMetric($"{inputLogMessage}", sw.Elapsed, output.Success);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                if (this._connection != null)
                {
                    switch (this._connection.State)
                    {
                        case ConnectionState.Open:
                        case ConnectionState.Broken:
                            LogEngine.DILogger.WriteToLog(LogLevels.Info, $"DbContext:dispose:close({this._connectionGuid})");
                            this._connection.Close();
                            break;

                        default:
                            LogEngine.DILogger.WriteToLog(LogLevels.Debug, $"DbContext:dispose:{this._connection.State.ToString().ToLower()}({this._connectionGuid})");
                            break;
                    }

                    this._connection.Dispose();
                    this._connection = null;
                }
                else
                {
                    LogEngine.DILogger.WriteToLog(LogLevels.Debug, $"DbContext:disposed({this._connectionGuid})");
                }
            }
            catch (Exception ex)
            {
                LogEngine.DILogger.WriteToLog(LogLevels.Error, $"DbContext:dispose:Exception: {JsonConvert.SerializeObject(ex)}");
            }
        }
    }
}
