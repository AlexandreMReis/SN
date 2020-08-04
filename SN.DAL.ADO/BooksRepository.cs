using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using SN.DAL.Interfaces;
using SN.DAL.Models;
using SN.Logger;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace SN.DAL.ADO
{
    public class BooksRepository : IBooksRepository
    {
        private readonly IDbContext _dbContext;

        public BooksRepository(IDbContext dbContext)
        {
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "BooksRepository:ctor");
            _dbContext = dbContext;
        }

        private VW_BOOK ReadBook(DbDataReader dataReader)
        {
            if (dataReader == null || dataReader["ID"] == null)
                return null;

            var output = new VW_BOOK();

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                string fieldName = dataReader.GetName(i).ToUpper();
                switch (fieldName)
                {
                    case "ID":
                        int intValue;
                        if (int.TryParse(dataReader[fieldName]?.ToString(), out intValue))
                            output.GetType().GetProperty(fieldName).SetValue(output, intValue, null);
                        break;
                        
                    case "TITLE":
                        output.GetType().GetProperty(fieldName).SetValue(output, dataReader[fieldName]?.ToString(), null);
                        break;

                    case "AUTHORS":

                        var aba = dataReader[fieldName]?.ToString();

                        var authorsNames = dataReader[fieldName]?.ToString().Split(',').Select(a => a.Trim()).ToList();
                        output.GetType().GetProperty(fieldName).SetValue(output, authorsNames, null);
                        break;

                    default:
                        break;
                }
            }

            return output;
        }

        public bool SP_InsertBook(SPInsertBookInput input)
        {
            bool output = false;
            DbNonQueryResponse dbData = null;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                if (input != null)
                {
                    string commandText = $"CALL SP_INSERT_BOOK('{input.BOOK_TITLE}', ARRAY [{string.Join(",", input.AUTHORS_NAMES.Select(a => $"'{a}'"))}]);";

                    dbData = this._dbContext.ExecuteNonQuery(commandText, null, $"{methodName}");
                    if (dbData?.Success ?? false)
                        output = true;
                }

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = false;
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName}(SUCCESS={dbData?.Success}) in {sw.ElapsedMilliseconds}ms");
            }
        }

        public DbQueryResponse<VW_BOOK> GetAllBooks()
        {
            DbQueryResponse<VW_BOOK> output = new DbQueryResponse<VW_BOOK> { };
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"SELECT ID, TITLE, AUTHORS FROM VW_BOOKS";

                output = this._dbContext.ExecuteQuery(commandText, null, this.ReadBook, $"{methodName}");

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new DbQueryResponse<VW_BOOK> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName} => OUTLEN={output.Items.Count} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public bool SP_InsertReading(int bookId, int memberId, LikedRating rating)
        {
            bool output = false;
            DbNonQueryResponse dbData = null;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"CALL SP_INSERT_READING({bookId}, {memberId}, {(int) rating});";

                dbData = this._dbContext.ExecuteNonQuery(commandText, null, $"{methodName}");
                if (dbData?.Success ?? false)
                    output = true;

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = false;
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName}(SUCCESS={dbData?.Success}) in {sw.ElapsedMilliseconds}ms");
            }
        }

        public DbQueryResponse<VW_BOOK> GetBooksByMemberId(int memberId, List<LikedRating> likedRatings = null)
        {
            DbQueryResponse<VW_BOOK> output = new DbQueryResponse<VW_BOOK> { };
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"SELECT bks.ID as ID, bks.TITLE as TITLE, bks.AUTHORS " +
                                       $"FROM VW_BOOKS bks " +
                                 $"INNER JOIN READINGS rds ON bks.ID = rds.BOOK_ID " +
                                      $"WHERE rds.READER_ID = {memberId}";

                if (likedRatings != null && likedRatings.Any())
                {
                    commandText = $"{commandText} AND rds.liked_rating IN ({string.Join(",", likedRatings.Select(c => $"{(int) c}"))})";
                }

                output = this._dbContext.ExecuteQuery(commandText, null, this.ReadBook, $"{methodName}");

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new DbQueryResponse<VW_BOOK> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName} => OUTLEN={output.Items.Count} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public DbQueryResponse<VW_BOOK> GetBooksByAuthorName(string authorName)
        {
            DbQueryResponse<VW_BOOK> output = new DbQueryResponse<VW_BOOK> { };
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {

                string commandText = $"SELECT ID, TITLE, AUTHORS " +
                                       $"FROM VW_BOOKS bks " +
                                      $"WHERE AUTHORS LIKE '%{authorName}%'";

                output = this._dbContext.ExecuteQuery(commandText, null, this.ReadBook, $"{methodName}");

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new DbQueryResponse<VW_BOOK> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName} => OUTLEN={output.Items.Count} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public void Dispose()
        {
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "BooksRepository:dispose");
            ////_dbContext.Dispose();
        }
    }
}
