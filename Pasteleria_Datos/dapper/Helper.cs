using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Datos.dapper
{
    public class Helper
    {
        List<Parametro> parametros;
        List<Parametro> parametrosWhere;

        private readonly string tabla = string.Empty;
        private readonly string whereSql = string.Empty;
        private string outputID = string.Empty;

        public Helper(string _tabla, string _whereSql = null)
        {
            tabla = _tabla;
            whereSql = _whereSql;
            parametros = new List<Parametro>();
            parametrosWhere = new List<Parametro>();
            outputID = string.Empty;
        }

        public Helper Agrega(string _campo, object _valor, DbType? _sqlDbType = null, int _lon = 0, bool _esOperacion = false)
        {
            if (parametros.Exists(p => p.campo == _campo))
                throw new DuplicateNameException($"El '{_campo}' ya fué declarado");
            parametros.Add(encapsula(_campo, _valor, _sqlDbType, _lon, _esOperacion));
            return this;
        }

        public Helper AgregaCondicion(string _campo, object _valor, DbType? _sqlDbType = null, int _lon = 0)
        {
            if (parametros.Exists(p => p.campo == _campo) || parametrosWhere.Exists(p => p.campo == _campo))
                throw new DuplicateNameException($"El '{_campo}' ya fué declarado");
            Parametro pw = encapsula(_campo, _valor, _sqlDbType, _lon);
            parametrosWhere.Add(pw);
            return this;
        }

        /// <summary>
        /// ESTE ID corresponde a la llave primaria, Identity, o la que quieras regresar despues del insert
        /// Si no se llama 'ID' entonces debes cambiarla de lo contrario obtendrás un error
        /// </summary>
        /// <param name="_outputID"></param>
        /// <returns></returns>
        public Helper OutputID(string _outputID)
        {
            outputID = _outputID;
            return this;
        }

        private Parametro encapsula(string _campo, object _valor, DbType? _sqlDbType, int _lon = 0, bool _esOperacion = false)
        {
            Parametro param = new Parametro();
            param.nombre = _campo;
            param.campo = _campo;
            param.esOpe = _esOperacion;
            param.tipo = _sqlDbType;

            if (_valor is string && _valor.ToString().Trim().ToUpper() == "NULL")
            {
                param.valor = null; //DBNull.Value;
            }
            else if (_valor == null)
            {
                param.valor = null; //DBNull.Value;
            }
            else if (_valor is string && !_esOperacion)
            {
                if (compruebaStringFH(_valor.ToString()))
                {
                    DateTime fecha = Convert.ToDateTime(_valor);
                    param.valor = fecha;
                }
                else
                {
                    if (_valor.ToString().Contains("'")) _valor.ToString().Replace("'", "''");
                    param.valor = _valor;
                }
            }
            else { param.valor = _valor; }
            return param;
        }

        private bool compruebaStringFH(string _valor)
        {
            string[] charsFecha = new string[] { "/", "A. M.", "P. M." };
            foreach (string val in charsFecha)
            {
                if (_valor.ToUpper().Contains(val))
                {
                    if (DateTime.TryParse(_valor, out _)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Quita un campo de los parametros
        /// </summary>
        /// <param name="_campo"></param>
        public void Remover(string _campo)
        {
            parametros.RemoveAll(p => p.campo == _campo);
        }

        public string InsertSql
        {
            get
            {
                if (parametros.Count == 0)
                    throw new Exception("Intento de realizar una inserción sin ningún parametro de entrada");

                //Crear una lista separada por comas
                var campos = string.Join(", ", parametros.Select(p => p.campo));

                IList<string> aux = new List<string>();
                foreach (var p in parametros) if (p.esOpe) aux.Add($"{p.valor}"); else aux.Add($"@{p.campo}");

                string oid = string.IsNullOrEmpty(outputID) ? "ID" : outputID;

                var valores = string.Join(", ", aux);
                return $"INSERT INTO {tabla}({campos}) " +
                       $"VALUES({valores}) ";
            }
        }

        public string InsertSqlO
        {
            get
            {
                if (parametros.Count == 0)
                    throw new Exception("Intento de realizar una inserción sin ningún parametro de entrada");

                //Crear una lista separada por comas
                var campos = string.Join(", ", parametros.Select(p => p.campo));

                IList<string> aux = new List<string>();
                foreach (var p in parametros) if (p.esOpe) aux.Add($"{p.valor}"); else aux.Add($"@{p.campo}");

                string oid = string.IsNullOrEmpty(outputID) ? "ID" : outputID;

                var valores = string.Join(", ", aux);
                return "DECLARE @output table(ID bigint); " +
                        $"INSERT INTO {tabla}({campos}) " +
                        $"OUTPUT INSERTED.[{oid}] " +
                        "INTO @output " +
                        $"VALUES({valores}) " +
                        "SELECT * FROM @output;";
            }
        }

        public string UpdateSql
        {
            get
            {
                if (string.IsNullOrEmpty(whereSql))
                    throw new Exception("Intento de actualización sin clausula Where");

                if (parametrosWhere.Count == 0)
                    throw new Exception("No se ha proporcionado ninguna condición para la actualización");

                var sb = new StringBuilder();
                foreach (var p in parametros)
                {
                    if (p.esOpe) sb.Append($"{p.campo} = {p.valor}, ");
                    else sb.Append($"{p.campo} = @{p.campo}, ");
                }

                return $"UPDATE {tabla} SET {sb.ToString().Substring(0, sb.Length - 2)} {whereSql};";
            }
        }

        public string DeleteSql
        {
            get
            {
                if (string.IsNullOrEmpty(whereSql))
                    throw new Exception("Intento de borrar sin clausula Where");

                if (parametrosWhere.Count == 0)
                    throw new Exception("No se ha proporcionado ninguna condición para el borrado de datos");

                return $"DELETE FROM {tabla} {whereSql}";
            }
        }

        public int? Timeout { get; set; }

        public DynamicParameters Parametros
        {
            get
            {
                var prms = new DynamicParameters();
                List<Parametro> todos = new List<Parametro>();
                todos.AddRange(parametros);
                todos.AddRange(parametrosWhere);

                foreach (var p in todos)
                {
                    if (!p.esOpe)
                    {
                        if (p.tipo != null && p.lon > 0)
                        {
                            prms.Add(p.campo, p.valor, p.tipo, null, p.lon);
                        }
                        else if (p.tipo != null && p.lon == 0)
                        {
                            prms.Add(p.campo, p.valor, p.tipo);
                        }
                        else
                        {
                            prms.Add(p.campo, p.valor);
                        }
                    }
                }
                return prms;
            }
        }
    }

    /// <summary>
    /// Este tipo de objeto/modelo solo se ocupará en el Helper
    /// </summary>
    public class Parametro
    {
        public string nombre { get; set; } //Nombre del parámetro El nombre del parametro debe ser distinto en el predicado, puede que se repita en la declacion de campos
        public string campo { get; set; } //Campo de la bd relacionado al parametro
        public object valor { get; set; } //Valor que se asignará al parámetro
        public DbType? tipo { get; set; }
        public int lon { get; set; }
        public bool esOpe { get; set; } = false;
    }
}
