using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Datos
{
    public class ConfP
    {
        public static SqlParameter[] CrearLista(IList<Elem> misElems)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();
            foreach (Elem e in misElems)
            {
                SqlParameter parametro = new SqlParameter();
                SqlDbType tipo = e.tipo ?? SqlDbType.VarChar;
                parametro.ParameterName = e.nombre;
                parametro.Value = e.valor;
                parametro.SqlDbType = tipo;
                if (e.lon > 0) parametro.Size = e.lon;
                parametros.Add(parametro);
                //*Quizá mas adelante apliquemos un switch case de la variable tipo para colocar mas datos, como la precisión en
                //determinado tipo de dato como Decimal, Time, etc
            }
            if (parametros.Count > 0) return parametros.ToArray();
            return null;
        }
    }
}
