using CROSS.SECRYPT;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
namespace CROSS.DATABASE
{
    public class ManagerDataBase : IManagerDataBase
    {
        private readonly DBOptions dBOption;
        private readonly IManagerSecrypt secrypt;

        public ManagerDataBase(IConfiguration configuration, IManagerSecrypt secrypt)
        {
            this.dBOption = new DBOptions();
            configuration.GetSection("database").Bind(this.dBOption);
            this.secrypt = secrypt;
        }

        public async Task<Tuple<bool, string>> Check(string name)
        {
            Tuple<bool, string> conexion;
            DBOptionItems item = this.dBOption.Connections.First(x => x.Name == name);
            using (SqlConnection sqlConnection = new SqlConnection(Connection(item)))
            {
                try
                {
                    try
                    {
                        await sqlConnection.OpenAsync();
                        conexion = new Tuple<bool, string>(true, $"BATABASE: {item.DataBase}; SERVER: {item.Server}; USER: {item.User}");
                    }
                    catch (Exception ex)
                    {
                        conexion = new Tuple<bool, string>(false, $"COULD NOT CONNECT TO DATABASE: {item.DataBase} SERVER: {item.Server}; USER: {item.User}; EXCEPTION: {ex.Message.ToUpper()}");
                    }
                    finally
                    {
                        await sqlConnection.CloseAsync();
                        SqlConnection.ClearAllPools();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error",ex);
                    conexion = new Tuple<bool, string>(false, $"CONFIG PARMETER DATABASE: {name}; EXCEPTION: {ex.Message.ToUpper()}");
                }

                return conexion;
            }
        }

        public async Task<bool> ExecuteStoredProcedure(string name, string query, List<SqlParameter> parameter)
        {
            using (SqlConnection conexion = new SqlConnection(Connection(this.dBOption.Connections.First(x => x.Name == name))))
            {
                try
                {
                    SqlCommand comando = new SqlCommand(query, conexion)
                    {
                        CommandType = CommandType.StoredProcedure,
                        CommandTimeout = 600000,
                    };
                    foreach (SqlParameter item in parameter)
                    {
                        if (item.Value == null)
                        {
                            item.Value = DBNull.Value;
                        }

                        comando.Parameters.Add(item);
                    }

                    await conexion.OpenAsync();
                    await comando.ExecuteNonQueryAsync();
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                    return true;
                }
                catch (SqlException error)
                {
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                    Log.Error("Error", error);
                    return false;
                }
            }
        }

        public async Task<bool> Execute(string name, string query)
        {
            using (SqlConnection conexion = new SqlConnection(Connection(this.dBOption.Connections.First(x => x.Name == name))))
            {
                try
                {
                    SqlCommand comando = new SqlCommand(query, conexion)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = 600000,
                    };
                    await conexion.OpenAsync();
                    await comando.ExecuteNonQueryAsync();
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                    return true;
                }
                catch (SqlException error)
                {
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                    Log.Error("error", error);
                    return false;
                }
            }
        }

        public async Task<DataTable> SelectStoredProcedure(string name, string query, List<SqlParameter> parameter)
        {
            DataTable consulta = new DataTable();
            using (SqlConnection conexion = new SqlConnection(Connection(this.dBOption.Connections.First(x => x.Name == name))))
            {
                try
                {
                    SqlConnection.ClearAllPools();
                    SqlDataAdapter comando = new SqlDataAdapter(query, conexion);
                    comando.SelectCommand.CommandType = CommandType.StoredProcedure;
                    comando.SelectCommand.CommandTimeout = 3600000;
                    foreach (SqlParameter item in parameter)
                    {
                        if (item.Value == null)
                        {
                            item.Value = DBNull.Value;
                        }

                        comando.SelectCommand.Parameters.Add(item);
                    }

                    await conexion.OpenAsync();
                    comando.Fill(consulta);
                }
                catch (SqlException error)
                {
                    Log.Error("error", error);
                }
                finally
                {
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                }

                return consulta;
            }
        }

        public async Task<List<DataTable>> SelectStoredProcedureMultiTable(string name, string query, List<SqlParameter> parameter)
        {
            List<DataTable> consulta = new List<DataTable>();
            using (SqlConnection conexion = new SqlConnection(Connection(this.dBOption.Connections.First(x => x.Name == name))))
            {
                try
                {
                    SqlConnection.ClearAllPools();
                    SqlDataAdapter comando = new SqlDataAdapter(query, conexion);
                    comando.SelectCommand.CommandType = CommandType.StoredProcedure;
                    comando.SelectCommand.CommandTimeout = 3600000;
                    foreach (SqlParameter item in parameter)
                    {
                        if (item.Value == null)
                        {
                            item.Value = DBNull.Value;
                        }

                        comando.SelectCommand.Parameters.Add(item);
                    }

                    await conexion.OpenAsync();
                    DataSet dataSet = new DataSet();
                    comando.Fill(dataSet);
                    dataSet.Tables.Cast<DataTable>().ToList().ForEach(x => consulta.Add(x));
                }
                catch (SqlException error)
                {
                    Log.Error("error", error);
                }
                finally
                {
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                }

                return consulta;
            }
        }

        public async Task<DataTable> Select(string name, string query)
        {
            DataTable consulta = new DataTable();
            using (SqlConnection conexion = new SqlConnection(Connection(this.dBOption.Connections.First(x => x.Name == name))))
            {
                try
                {
                    SqlConnection.ClearAllPools();
                    SqlDataAdapter comando = new SqlDataAdapter(query, conexion);
                    comando.SelectCommand.CommandType = CommandType.Text;
                    comando.SelectCommand.CommandTimeout = 3600000;
                    await conexion.OpenAsync();
                    comando.Fill(consulta);
                }
                catch (SqlException error)
                {
                    Log.Error("error", error);
                }
                finally
                {
                    await conexion.CloseAsync();
                    SqlConnection.ClearAllPools();
                }

                return consulta;
            }
        }

        private string Connection(DBOptionItems database)
        {
            string connection;
            try
            {
                string password = this.secrypt.Desencriptar(database.Password);
                connection = "Encrypt=True;TrustServerCertificate=True;User ID=" + database.User + ";Pwd=" + password + ";Server=" + database.Server + ";Database=" + database.DataBase + ";Application Name =" + dBOption.Name;
            }
            catch (Exception ex)
            {
                Log.Error("ex",ex);
                throw;
            }

            return connection;
        }
    }
}
