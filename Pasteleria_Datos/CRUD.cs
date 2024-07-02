using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Datos
{
    public class CRUD:IDisposable
    {

        private bool isDisposed;
        string table = string.Empty;
        SqlCommand cmd = null;
        string cc = string.Empty, cv = string.Empty, cw = string.Empty; //Cadena del cuerpo y valores sql
        List<Elem> elems = null;
        List<Elem> where = null;
        private readonly SqlConnection con = null;
        private readonly SqlTransaction tran = null;
        /* readonly representa un modificador que hace referencia a la funcionalidad de sólo lectura sobre lo que se declara. ... 
         * El modificador readonly indica que la asignación del valor se puede realizar en la propia declaración del campo, o bien, 
         * en un constructor de la misma clase.*/

        #region Constructores
        /// <summary>
        /// La clase CRUD (Create,Read,Update,Delete) requiere de una conexión y transacción activa
        /// </summary>
        /// <param name="con"></param>
        /// <param name="tran"></param>
        public CRUD(SqlConnection _con, SqlTransaction _tran) : this()
        {
            con = _con;
            tran = _tran;
        }

        /// <summary>
        /// Contructor por defecto, no requiere de transacciones ni conexiones activas
        /// </summary>
        public CRUD()
        {

        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                // free managed resources
                cmd.Dispose();
                elems = null;
                where = null;
                //Otra case que sea desechable que dependa de esta
                //managedResource.Dispose();
            }

            isDisposed = true;
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~CRUD()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public void tabla(string _Tabla)
        {
            table = _Tabla;
            elems = new List<Elem>();
            where = new List<Elem>();
            cc = string.Empty; //Cadena campos
            cv = string.Empty; //Cadena valores
            cw = string.Empty; //Cadena where
        }

        /// <summary>
        /// Agrega un campo a las colección, PROCURA NO USAR ESTE MÉTODO, aquí no va parametrizado
        /// </summary>
        /// <param name="_campo">Nombre del campo de la tabla sql</param>
        /// <param name="_valor">Valor del campo sin comillas</param>
        public void agregar(string _campo, object _valor, bool _esOperacionSql = false)
        {
            Elem param = new Elem();
            param.campo = _campo;
            param.esCalc = _esOperacionSql;

            if (_esOperacionSql) //En caso de ser una operación sql se coloca tal cual, por ejemplo campoSql1 + campoSql2 ó campoSql1 + [numero]
            {
                param.valor = _valor;
            }
            else if (_valor is string && _valor.ToString().Trim().ToUpper() == "NULL")
            {
                param.valor = _valor.ToString();
            }
            else if (_valor == null)
            {
                param.valor = "NULL";
            }
            else if (_valor is string)
            {
                if (compruebaStringFH(_valor.ToString()))
                {
                    DateTime fecha = Convert.ToDateTime(_valor);
                    param.valor = "'" + string.Format("{0:dd/MM/yyyy HH:mm}", fecha) + "'";
                }
                else
                {
                    if (_valor.ToString().Contains("'")) _valor.ToString().Replace("'", "''");
                    param.valor = "'" + _valor.ToString() + "'";
                }
            }
            else if (_valor is bool)
            {
                param.valor = ((bool)_valor == true ? 1 : 0);
            }
            else if (_valor is DateTime)
            {
                param.valor = "'" + string.Format("{0:dd/MM/yyyy HH:mm}", _valor) + "'";
            }
            else { param.valor = _valor.ToString(); }
            elems.Add(param);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_campo">Nombre del campo en la BD, se utilizará como nombre de parametro también</param>
        /// <param name="_valor">Valor de campo, sin comillas</param>
        /// <param name="_sqlDbType">Tipo de dato del campo sql</param>
        /// <param name="_lon">Longitud (Debes consultar en la BD), dejalo sin valor en caso de que no aplique, como en el caso del int, por default se pone cero, en la función no se tomará en cuenta</param>
        /// <param name="_esOperacionSql">En caso de hacer una operación con un campo de la BD debes colocarle true ejemplo: @miCampo + 1</param>
        public void agregar(string _campo, object _valor, SqlDbType _sqlDbType, int _lon = 0)
        {
            elems.Add(encapsula(_campo, _valor, _sqlDbType, _lon));
        }

        public void donde(string _campo, object _valor, SqlDbType _sqlDbType, int _lon = 0)
        {
            Elem e = encapsula(_campo, _valor, _sqlDbType, _lon);
            e.nombre = $"wh_{e.nombre}"; //Le cambio el nombre al parametro por si se necesita comparar un valor diferente al que ya trae
            where.Add(e);
        }

        private Elem encapsula(string _campo, object _valor, SqlDbType _sqlDbType, int _lon = 0)
        {
            Elem param = new Elem();
            param.nombre = _campo;
            param.campo = _campo;
            param.esCalc = false; //Por el momento no se aceptan calculos aquí
            param.tipo = _sqlDbType;

            if (_valor is string && _valor.ToString().Trim().ToUpper() == "NULL")
            {
                param.valor = DBNull.Value;
            }
            else if (_valor == null)
            {
                param.valor = DBNull.Value;
            }
            else if (_valor is string)
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

        public void remover(string _campo)
        {
            elems.RemoveAll(p => p.campo == _campo);
        }

        public bool ejecutarINSERT(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            if (validaCRUD(Func.insert))
            {
                cmd = new SqlCommand();
                cmd.CommandText = $"INSERT INTO {table} ";

                IList<Elem> elems_noP = elems.Where(e => e.tipo == null || e.esCalc).ToList();
                IList<Elem> elems_Par = elems.Where(e => e.tipo != null && !e.esCalc).ToList();

                foreach (Elem e in elems_noP)
                {
                    cc += $"{(cc == "" ? "" : ",")}{e.campo}";
                    cv += $"{(cv == "" ? "" : ",")}{e.valor}";
                }

                foreach (Elem e in elems_Par)
                {
                    cc += $"{(cc == "" ? "" : ",")}{e.campo}";
                    cv += $"{(cv == "" ? "" : ",")}@{e.nombre}";
                }

                cmd.CommandText += $"({cc}) VALUES ({cv})";

                SqlParameter[] parametros = ConfP.CrearLista(elems_Par);

                limpiaCRUD();

                return Conexion.EjecutaComando(cmd, parametros);

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicado">Coloca un predicado no parametrizado. Si quieres parametrizar usa la funcion "CRUD.donde"</param>
        /// <returns></returns>
        public bool ejecutarUPDATE(string predicado = "")
        {
            if (validaCRUD(Func.update, predicado))
            {
                cmd = new SqlCommand();
                cmd.CommandText = $"UPDATE {table} SET ";

                IList<Elem> elems_noP = elems.Where(e => e.tipo == null || e.esCalc).ToList();
                IList<Elem> elems_Par = elems.Where(e => e.tipo != null && !e.esCalc).ToList();
                foreach (Elem e in elems_noP)
                {
                    cc += $"{(cc == "" ? "" : ",")}{e.campo}={e.valor}";
                }
                foreach (Elem e in elems_Par)
                {
                    cc += $"{(cc == "" ? "" : ",")}{e.campo}=@{e.nombre}";
                }

                //Checamos si tenemos where parametrizado
                var todo = new List<Elem>();
                todo.AddRange(elems_Par);
                if (where.Count > 0)
                {
                    todo.AddRange(where);
                    foreach (Elem e in where)
                    {
                        cw += $"{(cw == "" ? "" : " AND ")}{e.campo}=@{e.nombre}";
                    }
                    if (string.IsNullOrEmpty(predicado)) cw = $" WHERE {cw} ";
                    else cw = $" WHERE {predicado} AND {cw} ";
                }
                else
                {
                    cw = $" WHERE {predicado} ";
                }
                SqlParameter[] parametros = ConfP.CrearLista(todo);

                cmd.CommandText += $" {cc} {cw}";
                limpiaCRUD();

                return Conexion.EjecutaComando(cmd, parametros);

            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="predicado">Coloca un predicado no parametrizado. Si quieres parametrizar usa la funcion "CRUD.donde"</param>
        /// <returns></returns>
        public bool ejecutarDELETE(string predicado = "")
        {
            if (validaCRUD(Func.delete, predicado))
            {
                cmd = new SqlCommand();

                //Checamos si tenemos where parametrizado
                if (where.Count > 0)
                {
                    foreach (Elem e in where)
                    {
                        cw += $"{(cw == "" ? "" : " AND ")}{e.campo}=@{e.nombre}";
                    }
                    if (string.IsNullOrEmpty(predicado)) cw = $" WHERE {cw} ";
                    else cw = $" WHERE {predicado} AND {cw} ";
                }
                else
                {
                    cw = $" WHERE {predicado} ";
                }
                SqlParameter[] parametros = ConfP.CrearLista(where);

                cmd.CommandText = $"DELETE FROM {table} {cw}";

                limpiaCRUD();

                return Conexion.EjecutaComando(cmd, parametros);

            }
            else
            {
                return false;
            }
        }

        bool validaCRUD(Func func, string predicado = "")
        {
            if (table == "")
            {
                throw new Exception("No se ha definido la tabla");
            }

            if (func == Func.insert || func == Func.update)
            {
                if (elems.Count <= 0)
                {
                    throw new Exception($"No existen campos en la coleccion {func}");
                }
            }

            if (func == Func.update || func == Func.delete)
            {
                if (string.IsNullOrEmpty(predicado) && where.Count == 0)
                    throw new Exception($"No se encontró un predicado o where parametrizado en la función {func}");
            }
            return true;
        }
        void limpiaCRUD()
        {
            table = string.Empty; elems.Clear(); where.Clear();
        }

    }

    enum Func
    {
        insert,
        update,
        delete
    }

    /// <summary>
    /// Este tipo de objeto/modelo solo se ocupará en el CRUD
    /// </summary>
    public class Elem
    {
        public string nombre { get; set; } //Nombre del parámetro El nombre del parametro debe ser distinto en el predicado, puede que se repita en la declacion de campos
        public string campo { get; set; } //Campo de la bd relacionado al parametro
        public object valor { get; set; } //Valor que se asignará al parámetro
        public SqlDbType? tipo { get; set; }
        public int lon { get; set; }
        public bool esCalc { get; set; } = false;
    }
}
