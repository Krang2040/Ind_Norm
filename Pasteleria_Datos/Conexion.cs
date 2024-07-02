using Pasteleria_Datos.dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace Pasteleria_Datos
{
    public class Conexion
    {
        private static readonly string connStr = "Data Source=localhost ;Initial Catalog=PasteleriaPAU ;Persist Security Info=True;";

               
        public static bool EjecutarOperacion(string sentencia, List<SqlParameter> listaParametros, CommandType tipoComando, SqlTransaction sqlTran = null)
        {
            int resultado;
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand comando = new SqlCommand
                    {
                        CommandText = sentencia,
                        CommandType = tipoComando,
                        Connection = conn,
                        CommandTimeout = 600
                    };

                    if (sqlTran != null)
                    {
                        comando.Transaction = sqlTran;
                    }

                    foreach (SqlParameter parametro in listaParametros)
                    {
                        comando.Parameters.Add(parametro);
                    }
                    resultado = comando.ExecuteNonQuery();
                    comando.Dispose();
                    if (resultado > 0)
                    {
                        return true;
                    }
                    else { return false; }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Ejecucion de store procedure con valor de retorno entero
        /// </summary>
        /// <param name="_nombreSP">nombre del store procedure (no es necesario colocar el dbo si se encuentra en el mismo scheme)</param>
        /// <param name="_listaParametros">lista de parametros con valor (aqui no se colocan parametros output)</param>
        /// <returns></returns>
        public static int EjecutarStoreProcedure(string _nombreSP, List<SqlParameter> _listaParametros)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand comando = new SqlCommand())
                {
                    conn.Open();

                    comando.Connection = conn;
                    comando.CommandType = System.Data.CommandType.StoredProcedure;
                    comando.CommandText = _nombreSP;
                    comando.CommandTimeout = 10;

                    foreach (SqlParameter parametro in _listaParametros)
                    {
                        comando.Parameters.Add(parametro);
                    }
                    //Parametro de retorno
                    var returnParameter = comando.Parameters.Add("@ReturnVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    comando.CommandTimeout = 600;
                    SqlDataReader reader = comando.ExecuteReader();
                    var resultado = returnParameter.Value;
                    return Convert.ToInt32(resultado);
                }
            }
        }
        public static DataTable EjecutarConsulta(string sentencia, List<SqlParameter> listaParametros, CommandType tipoComando)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter
                    {
                        SelectCommand = new SqlCommand(sentencia, conn)
                    };
                    adaptador.SelectCommand.CommandType = tipoComando;
                    adaptador.SelectCommand.CommandTimeout = 600;

                    foreach (SqlParameter parametro in listaParametros)
                    {
                        adaptador.SelectCommand.Parameters.Add(parametro);
                    }
                    DataSet resultado = new DataSet();
                    adaptador.Fill(resultado);
                    adaptador.Dispose();
                    return resultado.Tables[0];
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static DataSet EjecutarConsultaDS(string sentencia, List<SqlParameter> listaParametros, CommandType tipoComando)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter adaptador = new SqlDataAdapter
                    {
                        SelectCommand = new SqlCommand(sentencia, conn)
                    };
                    adaptador.SelectCommand.CommandType = tipoComando;
                    adaptador.SelectCommand.CommandTimeout = 600;

                    foreach (SqlParameter parametro in listaParametros)
                    {
                        adaptador.SelectCommand.Parameters.Add(parametro);
                    }
                    DataSet resultado = new DataSet();
                    adaptador.Fill(resultado);
                    adaptador.Dispose();
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static string SQLRes(string cadSQL)
        {
            SqlDataReader reader;
            SqlCommand cmd;
            string cadena = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    cmd = new SqlCommand(cadSQL, conn)
                    {
                        CommandType = CommandType.Text,
                        CommandTimeout = 600
                    };
                    reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        cadena = reader[0].ToString().Trim();
                    }
                    reader.Close();

                    return cadena;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar consulta simple." + ex.Message);
            }
        }
        public static DateTime FH_BD()
        {
            string sql = "SELECT GetDate()";
            return Convert.ToDateTime(SQLRes(sql));
        }
        internal static bool EjecutaComando(SqlCommand cmd)
        {
            bool ok = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    cmd.Connection = conn;
                    cmd.ExecuteNonQuery();
                    ok = true; return ok;
                }
                catch (Exception)
                {
                    return ok;
                }
                finally
                {
                    conn?.Close();
                }
            }
        }
        internal static bool EjecutaComando(SqlCommand cmd, SqlParameter[] parametros = null, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            bool ok = false;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlTransaction tran = null;
                try
                {
                    conn.Open();
                    tran = conn.BeginTransaction(level);
                    cmd.Connection = conn;
                    cmd.Transaction = tran;
                    if (parametros != null) cmd.Parameters.AddRange(parametros);
                    cmd.ExecuteNonQuery();
                    tran.Commit();
                    ok = true; return ok;
                }
                catch (Exception ex)
                {
                    _ = ex.Message;
                    tran.Rollback(); return ok;
                }
                finally
                {
                    conn?.Close();
                }
            }
        }
        #region Ejecutar consultas con transaccion
        //Para ejecutar operaciones con transaccion debemos mantener la conexion abierta durante la transaccion.
        //por lo que no podemos usar los metodos de arriba que abren y cierrar la conexion
        public static SqlConnection ConexionManual()
        {
            try
            {
                SqlConnection conn2 = new SqlConnection(connStr);
                if (conn2 != null && conn2.State != ConnectionState.Closed) conn2.Close();
                conn2.Open();
                return conn2;
            }
            catch (Exception ex)
            {
                return null;
                throw new Exception("No se pudo conectar a la BD." + ex.Message);
            }
        }
        public static Boolean EjecutarOperacion(SqlConnection connAbierta, SqlTransaction sqlTran, string sentencia, List<SqlParameter> listaParametros, CommandType tipoComando)
        {
            try
            {
                SqlCommand comando = new SqlCommand
                {
                    CommandText = sentencia,
                    CommandType = tipoComando,
                    Connection = connAbierta,
                    CommandTimeout = 120
                };

                if (sqlTran != null)
                {
                    comando.Transaction = sqlTran;
                }

                foreach (SqlParameter parametro in listaParametros)
                {
                    comando.Parameters.Add(parametro);
                }
                comando.ExecuteNonQuery();
                comando.Dispose();
                comando.Parameters.Clear();
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }
        public static string SQLRes(SqlConnection conexionManual, SqlTransaction sqlTran, string cadSQL)
        {
            SqlDataReader reader;
            SqlCommand cmd;
            string cadena = "";

            try
            {
                cmd = new SqlCommand(cadSQL, conexionManual)
                {
                    CommandType = CommandType.Text,
                    Transaction = sqlTran
                };
                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    cadena = reader[0].ToString().Trim();
                }
                reader.Close();
                return cadena;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void CerrarConexionManual(SqlConnection conexionManual)
        {
            if (conexionManual != null && conexionManual.State != ConnectionState.Closed) conexionManual.Close();
        }
        #endregion fin de operaciones con transaccion
        internal static bool EjecutaComando(SqlConnection connAbierta, SqlTransaction sqlTran, SqlCommand cmd)
        {
            bool ok = false;
            try
            {
                SqlTransaction tran = sqlTran;
                cmd.Connection = connAbierta;
                cmd.Transaction = tran;
                cmd.ExecuteNonQuery();
                ok = true; return ok;
            }
            catch (Exception)
            {
                return ok;
            }
        }
        internal static bool SubirArchivo(string filename, string CarpetaDestino, string sTipoSrv)
        {
            string ftpServerIP;
            string ftpUserID;
            string ftpPassword;
            switch (sTipoSrv)
            {
                case "CFDI":
                    //ftpServerIP = SQLRes("Select valor from mar_Parametros where Parametro = 'ToySrvFtp'");
                    //ftpUserID = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyUsuFtp'");
                    //ftpPassword = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyPassFtp'");
                    ftpServerIP = "secureftp.nadglobal.com";
                    ftpUserID = "NADmxftpTransMarva";
                    ftpPassword = "syt40#p3slp";
                    break;
                case "ToyRpt":
                    //ftpServerIP = SQLRes("Select valor from mar_Parametros where Parametro = 'FTP1.TOYOTA.COM'");
                    //ftpUserID = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyUsuFtp'");
                    //ftpPassword = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyPassFtp'");

                    ftpServerIP = "FTP1.TOYOTA.COM";
                    ftpUserID = "tovc_p";
                    ftpPassword = "SPOt@ma0Ln6SAji";
                    break;
                case "ToyResp_MRV":
                    ftpServerIP = "40.84.17.239";
                    ftpUserID = "toyota|toyota";
                    ftpPassword = "u5cTet0y#1937";
                    break;
                default:
                    ftpServerIP = SQLRes(" select valor from mar_parametros where Parametro='FTP_Servidor' ");
                    ftpUserID = SQLRes(" select valor from mar_parametros where Parametro='FTP_Usuario' ");
                    ftpPassword = SQLRes(" select valor from mar_parametros where Parametro='FTP_Passwd' ");
                    break;
            }

            FileInfo fileInf = new FileInfo(filename);           
            FtpWebRequest reqFTP;

            // Create FtpWebRequest object from the Uri provided
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + CarpetaDestino + "/" + fileInf.Name));

            // Provide the WebPermission Credentials
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            reqFTP.KeepAlive = false;

            // Specify the command to be executed.
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            reqFTP.UseBinary = true;

            // Notify the server about the size of the uploaded file
            reqFTP.ContentLength = fileInf.Length;

            // The buffer size is set to 2kb
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
            FileStream fs = fileInf.OpenRead();

            try
            {
                // Stream to which the file to be upload is written
                Stream strm = reqFTP.GetRequestStream();

                // Read from the file stream 2kb at a time
                contentLen = fs.Read(buff, 0, buffLength);

                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                strm.Close();
                fs.Close();
                //MessageBox.Show("Archivo: " + filename + "\nSubido Exitosamente...", "Atención!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception )
            {
                return false;
                //MessageBox.Show(ex.Message, "Upload Error");
            }
            return true;// ftp://marvaftp@40.84.17.239/Toyota/Tender/TYA.202205260526
        }
        internal static void DescargarArchivo(string filename, string sCarpetaO, string carpetaDestino, string sTipoSrv)
        {
            string ftpServerIP = string.Empty;
            string ftpUserID = string.Empty;
            string ftpPassword = string.Empty;

            switch (sTipoSrv)
            {
                case "CFDI":
                    ftpServerIP = SQLRes("Select valor from mar_Parametros where Parametro = 'FtpCFDI'");
                    ftpUserID = SQLRes("Select valor from mar_Parametros where Parametro = 'FtpUsuCFDI'");
                    ftpPassword = SQLRes("Select valor from mar_Parametros where Parametro = 'FtpPassCFDI'");
                    break;

                case "ToyRpt":
                    //_ = SQLRes("Select valor from mar_Parametros where Parametro = 'FTP1.TOYOTA.COM'");
                    //_ = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyUsuFtp'");
                    //_ = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyPassFtp'");
                    ftpServerIP = "FTP1.TOYOTA.COM";
                    ftpUserID = "tovc_p";
                    ftpPassword = "SPOt@ma0Ln6SAji";

                    break;
                default:
                    break;
            }

            try
            {
                FtpWebRequest reqFTP;
                FileInfo fileInf = new FileInfo(filename);
                reqFTP = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerIP + "/" + sCarpetaO + "/" + fileInf.Name);
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream writeStream = new FileStream(carpetaDestino + fileInf.Name, FileMode.Create);

                int Length = 2048;/*el tamaño limitado a bloques de 1024 bytes*/
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);

                while (bytesRead > 0)
                {

                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);

                }
                writeStream.Close();
                response.Close();
            }
            catch (WebException wEx)
            {
                throw wEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static List<string> ListaArchivo(string sCarpeta, string sTipoSrv)
        {
            string ftpServerIP=string.Empty;
            string ftpUserID=string.Empty;
            string ftpPassword = string.Empty;

            switch (sTipoSrv)
            {
                case "ToyRpt":
                    //ftpServerIP = SQLRes("Select valor from mar_Parametros where Parametro = 'FTP1.TOYOTA.COM'");
                    //ftpUserID = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyUsuFtp'");
                    //ftpPassword = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyPassFtp'");
                    ftpServerIP = "FTP1.TOYOTA.COM";
                    ftpUserID = "tovc_p";
                    ftpPassword = "SPOt@ma0Ln6SAji";
                    break;

                default:
                    break;
            }
            // Obtiene el objeto que se utiliza para comunicarse con el servidor.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerIP + "/" + sCarpeta + "/");
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            // Este ejemplo asume que el sitio FTP utiliza autenticación anónima.
            request.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());

            List<string> directories = new List<string>();

            string line = streamReader.ReadLine();
            //Obtiene el contenido y lo agrega al List<string>.
            while (!string.IsNullOrEmpty(line))
            {
                directories.Add(line);
                line = streamReader.ReadLine();
            }

            streamReader.Close();
            Console.WriteLine("Estatus al listar el contenido del folter {0}", response.StatusDescription);
            response.Close();
            return directories;
        }
        public static bool EliminaArchivoFTP(string filename, string sCarpeta, string sTipoSrv)
        {
            string ftpServerIP = string.Empty;
            string ftpUserID = string.Empty;
            string ftpPassword = string.Empty;

            switch (sTipoSrv)
            {
                case "ToyRpt":
                    //ftpServerIP = SQLRes("Select valor from mar_Parametros where Parametro = 'FTP1.TOYOTA.COM'");
                    //ftpUserID = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyUsuFtp'");
                    //ftpPassword = SQLRes("Select valor from mar_Parametros where Parametro = 'ToyPassFtp'");
                    ftpServerIP = "FTP1.TOYOTA.COM";
                    ftpUserID = "tovc_p";
                    ftpPassword = "SPOt@ma0Ln6SAji";
                    break;

                default:
                    break;
            }
            // Obtiene el objeto que se utiliza para comunicarse con el servidor.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + ftpServerIP + "/" + sCarpeta + "/" + filename);

            request.Method = WebRequestMethods.Ftp.DeleteFile;

            // Este ejemplo asume que el sitio FTP utiliza autenticación anónima.
            request.Credentials = new NetworkCredential(ftpUserID, ftpPassword);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            _ = new StreamReader(response.GetResponseStream());

            Console.WriteLine("Delete status: {0}", response.StatusDescription);
            response.Close();
            return true;
        }
        public static void DecargaDeIntelisis(string iD, string folderGuardar, string cfdi, ref bool pdf, ref bool xml, string folioIdSila, string tabla)
        {
            try
            {
                //if (!string.IsNullOrEmpty(folioIdSila) && !string.IsNullOrEmpty(tabla))
                //{
                string url = $"https://192.168.1.161/ServicioCfdiWCF/DescIntelisis/DescargaIntelisis.svc/DescArchIntelisis?ideSila={folioIdSila}&idTabla={tabla}";
                var wRequest = (HttpWebRequest)WebRequest.Create(url);
                wRequest.Timeout = 60000;

                wRequest.ServerCertificateValidationCallback = (sender, cert, chain, error) => true;
                WebResponse wResponse = wRequest.GetResponse();

                var respuesta = new StreamReader(wResponse.GetResponseStream()).ReadToEnd();
                if (respuesta != null && respuesta.ToString() != "")
                {
                    List<string> splitResp1 = respuesta.Split(new string[] { "<pdfBytes>" }, StringSplitOptions.None).ToList();
                    string respdf = string.Empty;
                    string resxml = string.Empty;
                    if (splitResp1.Count() >= 2)
                        respdf = respuesta.Split(new string[] { "<pdfBytes>" }, StringSplitOptions.None)[1].Split(new string[] { "</pdfBytes>" }, StringSplitOptions.None)[0];

                    splitResp1.Clear();
                    splitResp1 = respuesta.Split(new string[] { "<xmlBytes>" }, StringSplitOptions.None).ToList();
                    if (splitResp1.Count() >= 2)
                        resxml = respuesta.Split(new string[] { "<xmlBytes>" }, StringSplitOptions.None)[1].Split(new string[] { "</xmlBytes>" }, StringSplitOptions.None)[0];

                    if (!string.IsNullOrEmpty(respdf)) File.WriteAllBytes(folderGuardar + "\\" + cfdi + "_CFDII.pdf", Convert.FromBase64String(respdf));
                    if (!string.IsNullOrEmpty(resxml)) File.WriteAllBytes(folderGuardar + "\\" + cfdi + "_CFDII.xml", Convert.FromBase64String(resxml));
                }

                pdf = File.Exists(folderGuardar + "\\" + cfdi + "_CFDII.pdf");
                xml = File.Exists(folderGuardar + "\\" + cfdi + "_CFDII.xml");
                //}

            }
            catch (Exception )
            {
            }
        }
    }
}
