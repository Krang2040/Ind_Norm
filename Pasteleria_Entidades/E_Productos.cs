using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pasteleria_Entidades
{
    public class E_Productos
    {
        private int _Id_Producto;
        private string _sDescrpcion;
        private int _iCantiadad;
        private string _CveProducto;

        public int Id_Producto { get => _Id_Producto; set => _Id_Producto = value; }
        public string SDescrpcion { get => _sDescrpcion; set => _sDescrpcion = value; }
        public int ICantiadad { get => _iCantiadad; set => _iCantiadad = value; }
        public string CveProducto { get => _CveProducto; set => _CveProducto = value; }
    }
}
