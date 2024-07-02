using Pasteleria_Datos.dapper;
using Pasteleria_Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Datos
{
    public class Pasteleria_Inventario
    {
        ServicioDB db;
        public Pasteleria_Inventario()
        {
            db = new ServicioDB();
        }

        public List<E_Productos> Productos(string[] _clientes, string IdDest)
        {
            string sql;

            sql = "Select * from Productos ";

            var lsDt = Conexion.EjecutarConsulta();

            List<E_Productos> lsProductos = new List<E_Productos>();

            return lsProductos; 
        }

    }
}
