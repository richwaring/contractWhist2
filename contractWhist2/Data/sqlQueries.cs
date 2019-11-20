using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using contractWhist2.Models;

namespace contractWhist2.Data
{
    public class sqlQueries
    {
        public static db myDb = new db(); 
        
        public static void runSql(string sql)
        {
            //var conn = new SqlConnection(dbConnectionString);
            SqlCommand command = new SqlCommand(sql, myDb.con);

            myDb.con.Open();

            command.ExecuteNonQuery();

            myDb.con.Close();
        }

        public static int executeSqlSp(string sp_name, Dictionary<string, string> sp_params, string outputParamName)

        {
            myDb.con.Open();

            SqlCommand command = new SqlCommand(sp_name, myDb.con);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            addCommandParameters(command, sp_params);

            command.Parameters.Add(outputParamName, System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;

            command.ExecuteNonQuery();

            int outval = (int)command.Parameters[outputParamName].Value;

            Console.WriteLine("Return value: n/a, Out value: {0}", //retval, 
                outval);

            myDb.con.Close();

            return outval;
        }

        public static DataTable readSqlSp(string sp_name, Dictionary<string, string> sp_params)
        {
            myDb.con.Open();

            DataTable myDataTable = new DataTable();

            SqlCommand command = new SqlCommand(sp_name, myDb.con);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            addCommandParameters(command, sp_params);

            SqlDataAdapter tableFiller = new SqlDataAdapter(command);

            tableFiller.Fill(myDataTable);

            myDb.con.Close();

            return myDataTable;
        }
        public static void executeWriteOnlySqlSp(string sp_name, Dictionary<string, string> sp_params)

        {
            myDb.con.Open();

            SqlCommand command = new SqlCommand(sp_name, myDb.con);

            command.CommandType = System.Data.CommandType.StoredProcedure;

            addCommandParameters(command, sp_params);

            command.ExecuteNonQuery();

            myDb.con.Close();

            return ;
        }

        public static SqlCommand addCommandParameters(SqlCommand command, Dictionary<string, string> sp_params)
        {
            foreach (var item in sp_params)
            {
                command.Parameters.AddWithValue(item.Key, item.Value).Direction = System.Data.ParameterDirection.Input;
            }
            return command;
        }


    }
}
