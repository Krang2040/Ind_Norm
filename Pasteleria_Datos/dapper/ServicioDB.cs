using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

//SRP Miercoles 05 de Enero 2022
namespace Pasteleria_Datos.dapper
{
    /// <summary>
    /// Clase para ejecutar llamadas a la Base de Datos de una forma mas sencilla, controlada y mapeada
    /// </summary>
    public class ServicioDB
    {        
        private readonly string connStr;
        public ServicioDB()
        {
            
            connStr = "Data Source=localhost ;Initial Catalog=PasteleriaPAU ;Persist Security Info=True;";
        }

        public dynamic Consultar(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.Query(sql);
                return consulta.ToList();
            }
        }
        public dynamic Consultar(string sql, object param)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.Query(sql, param);
                return consulta.ToList();
            }
        }
        public List<T> Consultar<T>(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.Query<T>(sql);
                return consulta.ToList();
            }
        }
        public List<T> Consultar<T>(string sql, object param)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.Query<T>(sql, param);
                return consulta.ToList();
            }
        }
        public T SqlRes<T>(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.QueryFirstOrDefault<T>(sql);
                return consulta;
            }
        }
        public T SqlRes<T>(string sql, object prms)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.QueryFirstOrDefault<T>(sql, prms);
                return consulta;
            }
        }
        public dynamic SqlRes(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                return connection.QueryFirstOrDefault(sql);
            }
        }
        public dynamic SqlRes(string sql, object prms)
        {
            using (var connection = new SqlConnection(connStr))
            {
                return connection.QueryFirstOrDefault(sql, prms);
            }
        }
        public T ConsultarFirstOrDefault<T>(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.QueryFirstOrDefault<T>(sql);
                return consulta;
            }
        }
        public T ConsultarFirstOrDefault<T>(string sql, object prms)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.QueryFirstOrDefault<T>(sql, prms);
                return consulta;
            }
        }
        public bool Ejecutar(string sql)
        {
            return Ejecutar(sql, null);
        }
        public bool Ejecutar(string sql, object parametros, int? timeout = null)
        {
            using (var connection = new SqlConnection(connStr))
            {
                int rowAffected = connection.Execute(sql, parametros, null, timeout);
                return rowAffected > 0;
            }
        }
        public T EjecutarScalar<T>(string sql, object parametros)
        {
            try
            {
                using (var connection = new SqlConnection(connStr))
                {
                    T result = connection.ExecuteScalar<T>(sql, parametros);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return default;
            }
        }
        public T EjecutarInsert<T>(Helper helper)
        {
            using (var connection = new SqlConnection(connStr))
            {
                return connection.ExecuteScalar<T>(helper.InsertSqlO, helper.Parametros);
            }
        }
        public bool EjecutarInsert(Helper helper)
        {
            return Ejecutar(helper.InsertSql, helper.Parametros);
        }
        public bool EjecutarUpdate(Helper helper)
        {
            return Ejecutar(helper.UpdateSql, helper.Parametros, helper.Timeout);
        }
        public bool EjecutarDelete(Helper helper)
        {
            return Ejecutar(helper.DeleteSql, helper.Parametros, helper.Timeout);
        }
        public void EjecutarStoredProcedure(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                EjecutarStoredProcedure(sql, null);
            }
        }
        public void EjecutarStoredProcedure(string sql, object parameters)
        {
            using (var connection = new SqlConnection(connStr))
            {
                connection.Query(sql, parameters, commandType: CommandType.StoredProcedure);
            }
        }
        public List<T> EjecutarStoredProcedure<T>(string sql, object parameters)
        {
            try
            {
                using (var connection = new SqlConnection(connStr))
                {
                    return connection.Query<T>(sql, parameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<dynamic> ConsultarAsync(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = await connection.QueryAsync(sql);
                return consulta.ToList();
            }
        }
        public async Task<List<T>> ConsultarAsync<T>(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = await connection.QueryAsync<T>(sql);
                return consulta.ToList();
            }
        }
        public async Task<List<T>> ConsultarAsync<T>(string sql, object param)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = await connection.QueryAsync<T>(sql, param);
                return consulta.ToList();
            }
        }
        public async Task<T> SqlResAsync<T>(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = await connection.QueryFirstOrDefaultAsync<T>(sql);
                return consulta;
            }
        }
        public Task<T> SqlResAsync<T>(string sql, object prms)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.QueryFirstOrDefaultAsync<T>(sql, prms);
                return consulta;
            }
        }
        public Task<dynamic> SqlResAsync(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                return connection.QueryFirstOrDefaultAsync(sql);
            }
        }
        public Task<dynamic> SqlResAsync(string sql, object prms)
        {
            using (var connection = new SqlConnection(connStr))
            {
                return connection.QueryFirstOrDefaultAsync(sql, prms);
            }
        }
        public async Task<T> ConsultarFirstOrDefaultAsync<T>(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = await connection.QueryFirstOrDefaultAsync<T>(sql);
                return consulta;
            }
        }
        public async Task EjecutarAsync(string sql)
        {
            using (var connection = new SqlConnection(connStr))
                await connection.ExecuteAsync(sql);
        }
        public async Task<T> EjecutarScalarAsync<T>(string sql, object parametros)
        {
            try
            {
                using (var connection = new SqlConnection(connStr))
                {
                    T result = await connection.ExecuteScalarAsync<T>(sql, parametros);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return default;
            }
        }
        public async Task<T> EjecutarInsertAsync<T>(Helper helper)
        {
            using (var connection = new SqlConnection(connStr))
            {
                return await connection.ExecuteScalarAsync<T>(helper.InsertSqlO, helper.Parametros);
            }
        }
        public async Task EjecutarUpdateAsync(Helper helper)
        {
            using (var connection = new SqlConnection(connStr))
            {
                await connection.ExecuteAsync(helper.UpdateSql, helper.Parametros);
            }
        }
        public async Task EjecutarDeleteAsync(Helper helper)
        {
            using (var connection = new SqlConnection(connStr))
            {
                await connection.ExecuteAsync(helper.DeleteSql, helper.Parametros);
            }
        }
        public async Task EjecutarStoredProcedureAsync(string sql)
        {
            using (var connection = new SqlConnection(connStr))
            {
                await EjecutarStoredProcedureAsync(sql, null);
            }
        }
        public async Task EjecutarStoredProcedureAsync(string sql, object parameters)
        {
            using (var connection = new SqlConnection(connStr))
            {
                await connection.QueryAsync(sql, parameters, commandType: CommandType.StoredProcedure);
            }
        }
        public async Task<List<T>> EjecutarStoredProcedureAsync<T>(string sql, object parameters)
        {
            try
            {
                using (var connection = new SqlConnection(connStr))
                {
                    var resultado = await connection.QueryAsync<T>(sql, parameters, commandType: CommandType.StoredProcedure);
                    return resultado.ToList();
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Sobrecarga del metodo consultar, con Timeout
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">Query</param>
        /// <param name="param">Parametros</param>
        /// <param name="timeout">Tiempo del timeout</param>
        /// <returns></returns>
        public List<T> Consultar<T>(string sql, object param, int timeout)
        {
            using (var connection = new SqlConnection(connStr))
            {
                var consulta = connection.Query<T>(sql, param, null, true, timeout);
                return consulta.ToList();
            }
        }
    }
}
