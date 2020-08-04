using System.Collections.Generic;

namespace SN.DAL.Models
{
    public class DbResponse
    {
        public bool Success { get; set; }
    }

    public class DbNonQueryResponse : DbResponse
    {
        public int AffectedRows { get; set; }

        public DbNonQueryResponse()
        {
            this.Success = true;
        }
    }

    public class DbQueryResponse<T> : DbResponse
    {
        public List<T> Items { get; set; }

        public DbQueryResponse()
        {
            this.Success = true;
            this.Items = new List<T>();
        }
    }

    public class DbStoredProcedureResponse : DbResponse
    {
        public Dictionary<string, object> DbOutputValues { get; set; }

        public DbStoredProcedureResponse()
        {
            this.Success = true;
            this.DbOutputValues = new Dictionary<string, object> { };
        }
    }
}
