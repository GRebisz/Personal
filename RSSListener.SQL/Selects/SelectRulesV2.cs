using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using RSSListener.Data.Model;

namespace RSSListener.SQL.Selects
{
    public class SelectRulesV2 : RSSListener.SQL.Interface.Selects<RSSListener.Data.Model.Rule2>
    {
        private SqlConnection conn = null;
        private List<Rule2> rules = null;
        private cLogger logger = null;
        private const string storedproc = "SelectRulesV2";
        public SelectRulesV2(SqlConnection connection, cLogger logger)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            else if (connection.State != System.Data.ConnectionState.Open)
            {
                throw new Exception("connection's state is not open");
            }
            else
            {
                conn = connection;
                this.logger = logger;
            }
        }
        public List<Data.Model.Rule2> getAll()
        {
            List<Rule2> resultset = new List<Rule2>();
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            comm.CommandText = storedproc;
            comm.CommandType = System.Data.CommandType.StoredProcedure;
            SqlDataReader dr = comm.ExecuteReader();
            if (dr != null)
            {
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Rule2 rule = new Rule2();
                        rule.id = Guid.Parse(dr["id"].ToString());
                        rule.Code = dr["Code"].ToString();
                        rule.MethodName = dr["MethodName"].ToString();
                        rule.MethodSignature = dr["MethodSignature"].ToString();
                        rule.ClassName = dr["ClassName"].ToString();
                        rule.Includes = dr["Includes"].ToString();
                        rule.ReturnType = dr["ReturnType"].ToString();
                        resultset.Add(rule);
                    }
                }
                if (!dr.IsClosed)
                {
                    dr.Close();
                }
            }
            rules = resultset;
            return resultset;
        }

        public Data.Model.Rule2 getByID(Guid id)
        {
            List<Rule2> rulelist = null;
            if (rules == null)
            {
                rulelist = getAll();
            }
            else
            {
                rulelist = rules;
            }
            foreach (Rule2 a in rules)
            {
                if (a.id == id)
                {
                    return a;
                }
            }
            return null;
        }

        public Data.Model.Rule2 getByName(string name)
        {
            return null;
        }
    }
}
