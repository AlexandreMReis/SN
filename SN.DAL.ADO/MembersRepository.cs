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
    public class MembersRepository : IMembersRepository
    {
        private readonly IDbContext _dbContext;

        public MembersRepository(IDbContext dbContext)
        {
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "DevicesRepository:ctor");
            _dbContext = dbContext;
        }

        private MEMBER ReadMember(DbDataReader dataReader)
        {
            if (dataReader == null || dataReader["ID"] == null)
                return null;

            var output = new MEMBER();

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

                    case "NAME":
                        output.GetType().GetProperty(fieldName).SetValue(output, dataReader[fieldName]?.ToString(), null);
                        break;

                    default:
                        break;
                }
            }

            return output;
        }

        public bool CreateMember(string memberName)
        {
            bool output = false;
            DbNonQueryResponse dbData = null;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"CALL SP_INSERT_MEMBER('{memberName}');";

                dbData = this._dbContext.ExecuteNonQuery(commandText, null, $"{methodName}");
                if (dbData?.Success ?? false)
                    output = true;

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.MemberLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = false;
            }
            finally
            {
                sw.Stop();
                LogEngine.MemberLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName}(AFFECTED_ROWS={dbData?.AffectedRows ?? 0}) in {sw.ElapsedMilliseconds}ms");
            }
        }

        public DbQueryResponse<MEMBER> GetAllMembers()
        {
            DbQueryResponse<MEMBER> output = new DbQueryResponse<MEMBER> { };
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"SELECT ID, NAME FROM MEMBERS";

                output = this._dbContext.ExecuteQuery(commandText, null, this.ReadMember, $"{methodName}");

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.MemberLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new DbQueryResponse<MEMBER> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.MemberLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName} => OUTLEN={output.Items.Count} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public bool SP_InsertFriendship(MEMBER member1, MEMBER member2)
        {
            bool output = false;
            DbNonQueryResponse dbData = null;
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"CALL SP_INSERT_FRIENDSHIP({member1.ID}, {member2.ID});";

                dbData = this._dbContext.ExecuteNonQuery(commandText, null, $"{methodName}");
                if (dbData?.Success ?? false)
                    output = true;

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.MemberLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = false;
            }
            finally
            {
                sw.Stop();
                LogEngine.MemberLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName}(AFFECTED_ROWS={dbData?.AffectedRows ?? 0}) in {sw.ElapsedMilliseconds}ms");
            }
        }

        public DbQueryResponse<MEMBER> GetMembersByBookId(int bookId)
        {
            DbQueryResponse<MEMBER> output = new DbQueryResponse<MEMBER> { };
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"SELECT mbs.ID as ID, mbs.NAME as NAME " +
                                       $"FROM MEMBERS mbs " +
                                 $"INNER JOIN READINGS rds ON mbs.ID = rds.READER_ID " +
                                      $"WHERE rds.BOOK_ID = {bookId}";

                output = this._dbContext.ExecuteQuery(commandText, null, this.ReadMember, $"{methodName}");

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new DbQueryResponse<MEMBER> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName} => OUTLEN={output.Items.Count} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public DbQueryResponse<MEMBER> GetFriendsByMemberId(int memberId)
        {
            DbQueryResponse<MEMBER> output = new DbQueryResponse<MEMBER> { };
            string methodName = MethodBase.GetCurrentMethod().GetName();
            var sw = Stopwatch.StartNew();

            try
            {
                string commandText = $"SELECT mbs.ID as ID, mbs.NAME as NAME " +
                                       $"FROM MEMBERS mbs " +
                                 $"INNER JOIN ARE_FRIENDS fds ON mbs.ID = fds.requester_id " +
                                      $"WHERE fds.addressed_id = {memberId}";

                output = this._dbContext.ExecuteQuery(commandText, null, this.ReadMember, $"{methodName}");

                return output;
            }
            catch (Exception ex)
            {
                LogEngine.BookLogger.WriteToLog(LogLevels.Error, $"DAL.Exception: {JsonConvert.SerializeObject(ex)}");
                return output = new DbQueryResponse<MEMBER> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.BookLogger.WriteToLog(LogLevels.Debug, $"DAL.{methodName} => OUTLEN={output.Items.Count} in {sw.ElapsedMilliseconds}ms");
            }
        }

        public void Dispose()
        {
            LogEngine.DILogger.WriteToLog(LogLevels.Debug, "MembersRepository:dispose");
            ////_dbContext.Dispose();
        }


    }
}
