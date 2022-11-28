using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace migrarAcc
{
    public partial class frmInicio : Form
    {
        private MySqlConnection mysqlCon = new MySqlConnection("Server=localhost;Port=33006;User ID=root;Password=123456;Database=biblioteca");
        private OleDbConnection oledbCon;

        public frmInicio()
        {
            InitializeComponent();
        }

        private void Migrar(object sender, EventArgs e)
        {
            using (Prompt prompt = new Prompt("Password BD Access"))
            {
                string pass = prompt.Result;
                oledbCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=F:\\Repo\\PERSONAL\\MiBiblioteca.mdb;Persist Security Info=True;Jet OLEDB:Database Password=" + pass);
            }
            //MigrarAUTOR();
            //MigrarTEMA();
            //MigrarOBRAS();
            //MigrarAUTOROBRA();
            //MigrarEdicionLibro();
        }

        private void MigrarAUTOR()
        {
            oledbCon.Open();
            OleDbDataAdapter oledbDA = new OleDbDataAdapter("SELECT * FROM AUTOR", oledbCon);
            oledbDA.TableMappings.Add("Table", "AUTOR");
            DataSet autorDS = new DataSet();
            oledbDA.Fill(autorDS);
            DataView autorDV = new DataView(autorDS.Tables["AUTOR"]);

            // Esta función tarda mucho menos usando los DataView (ver MigrarAutorObra y MigrarEdicionLibro)
            mysqlCon.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = mysqlCon;

            // Migramos tabla AUTOR
            String nombre;
            object añoNac;
            String comentario;
            int nfilas = autorDV.Count;
            IEnumerator enmAutor = autorDV.GetEnumerator();
            this.txtLog.Text = "TABLA AUTOR... ";
            int i = 0;
            while (enmAutor.MoveNext())
            {
                DataRowView fila = (DataRowView)enmAutor.Current;

                // El apóstrofe indica principio y final de cadena de texto.
                // En los campos de tipo String lo reemplazamos por doble apóstrofe
                // para evitar errores en el comando INSERT
                nombre = "'" + ((String)fila["Nombre"]).Replace("'", "''") + "'";
                añoNac = fila["AñoNac"] == DBNull.Value ? "null" : fila["AñoNac"];
                comentario = fila["Comentario"] == DBNull.Value
                    ? "null"
                    : "'" + ((String)fila["Comentario"]).Replace("'", "''") + "'";

                cmd.CommandText = $"INSERT INTO AUTOR(NOMBRE,ANYNAC,COMENT) VALUES({nombre},{añoNac},{comentario})";
                cmd.ExecuteNonQuery();
                i++;
            }
            this.txtLog.AppendText($"Total registros: {nfilas} - Migrados: {i}\n");
            mysqlCon.Close();
            oledbCon.Close();
        }

        private void MigrarTEMA()
        {
            oledbCon.Open();
            OleDbDataAdapter oledbDA = new OleDbDataAdapter("SELECT * FROM TEMA", oledbCon);
            oledbDA.TableMappings.Add("Table", "TEMA");
            DataSet temaDS = new DataSet();
            oledbDA.Fill(temaDS);
            DataView temaDV = new DataView(temaDS.Tables["TEMA"]);

            mysqlCon.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = mysqlCon;

            // Migramos tabla TEMA
            String shNombre;
            String nombre;
            String comentario;
            int nfilas = temaDV.Count;
            IEnumerator enmTema = temaDV.GetEnumerator();

            this.txtLog.AppendText("TABLA TEMA... ");
            int i = 0;
            while (enmTema.MoveNext())
            {
                DataRowView fila = (DataRowView)enmTema.Current;
                shNombre = "'" + ((String)fila["IdTema"]).Replace("'","''") + "'";
                nombre = fila["NombreTema"] == DBNull.Value
                    ? "null"
                    : "'" + ((String)fila["NombreTema"]).Replace("'", "''") + "'";
                comentario = fila["Comentario"] == DBNull.Value
                    ? "null"
                    : "'" + ((String)fila["Comentario"]).Replace("'", "''") + "'";

                cmd.CommandText = $"INSERT INTO TEMA(SHNOMBRE, NOMBRE, COMENT) VALUES({shNombre},{nombre},{comentario})";
                cmd.ExecuteNonQuery();
                i++;
            }
            this.txtLog.AppendText($"Total registros: {nfilas} - Migrados: {i}\n");
            mysqlCon.Close();
            oledbCon.Close();
        }

        private void MigrarOBRAS()
        {
            oledbCon.Open();
            OleDbDataAdapter oledbDA = new OleDbDataAdapter("SELECT * FROM OBRA", oledbCon);
            oledbDA.TableMappings.Add("Table", "OBRA");
            DataSet obraDS = new DataSet();
            oledbDA.Fill(obraDS);
            DataView obraDV = new DataView(obraDS.Tables["OBRA"]);

            mysqlCon.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = mysqlCon;

            String titulo;
            String comentario;
            String idTema;
            long idxTema;

            int nfilas = obraDV.Count;
            IEnumerator enmObra = obraDV.GetEnumerator();

            this.txtLog.AppendText("TABLA OBRA... ");
            int i = 0;
            while (enmObra.MoveNext())
            {
                DataRowView fila = (DataRowView)enmObra.Current;
                titulo = "'" + ((String)fila["Titulo"]).Replace("'", "''") + "'";
                comentario = fila["Comentario"] == DBNull.Value
                    ? "null"
                    : "'" + ((String)fila["Comentario"]).Replace("'", "''") + "'";

                // Buscamos el id correspondiente en la base de datos de MySql.
                idTema = (String)fila["idTema"];
                cmd.CommandText = $"SELECT ID FROM TEMA WHERE SHNOMBRE='{idTema}';";
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                idxTema = reader.GetInt64(0);
                reader.Close();

                cmd.CommandText = $"INSERT INTO OBRA(TITULO, COMENT, IDTEMA) VALUES({titulo}, {comentario}, {idxTema})";
                cmd.ExecuteNonQuery();
                i++;
            }
            this.txtLog.AppendText($"Total registros: {nfilas} - Migrados: {i}");
            mysqlCon.Close();
            oledbCon.Close();
        }

        private void MigrarAUTOROBRA()
        {
            // En Mysql recorremos la tabla AUTOR, guardando el id y el nombre.
            // En Access buscamos el id que corresponde a ese nombre y filtramos la tabla OBRA_AUTOR por ese id del autor.
            // En Access obtenemos todos los idObra con que está relacionado el autor.
            // En Access para cada idObra, obtenemos el título de la obra y el coment
            // En Mysql filtramos la tabla OBRA por ese título y coment. Comprobamos que el resultado es único.
            // En Mysql obtenemos el id de la obra filtrada y añadimos el registro idAutor-idObra en la tabla AUTOR_OBRA

            // Creamos las vistas de la base de datos Access y las ordenamos por el campo por el que buscaremos.
            oledbCon.Open();
            OleDbDataAdapter oledbDA = new OleDbDataAdapter("SELECT * FROM AUTOR, OBRA, OBRA_AUTOR", oledbCon);
            oledbDA.TableMappings.Add("Table", "AUTOR");
            oledbDA.TableMappings.Add("Table1", "OBRA");
            oledbDA.TableMappings.Add("Table2", "OBRA_AUTOR");
            DataSet oledbDS = new DataSet();
            oledbDA.Fill(oledbDS);
            DataView autorDV = new DataView(oledbDS.Tables["AUTOR"]);
            autorDV.Sort = "Nombre";
            DataView obraDV= new DataView(oledbDS.Tables["OBRA"]);
            obraDV.Sort = "idObra";
            DataView obraAutorDV = new DataView(oledbDS.Tables["OBRA_AUTOR"]);
            obraAutorDV.Sort = "idAutor";

            int nfilas = obraAutorDV.Count;

            // Cargamos las tablas de la base de datos MySql
            MySqlDataAdapter mysqlAdapter = new MySqlDataAdapter();
            mysqlAdapter.SelectCommand = new MySqlCommand("SELECT * FROM AUTOR; SELECT * FROM OBRA; SELECT * FROM AUTOR_OBRA;", mysqlCon);
            mysqlAdapter.TableMappings.Add("Table", "AUTOR");
            mysqlAdapter.TableMappings.Add("Table1", "OBRA");
            mysqlAdapter.TableMappings.Add("Table2", "AUTOR_OBRA");

            DataSet mysqlDS = new DataSet();
            mysqlAdapter.Fill(mysqlDS);
            DataView autorMysqlDV = new DataView(mysqlDS.Tables["AUTOR"]);
            DataView obraMysqlDV = new DataView(mysqlDS.Tables["OBRA"]);
            obraMysqlDV.Sort = "TITULO"; // Para buscar después por título.

            DataRowView autor;
            long idAutor;
            long idObra;
            string nombre;
            string titulo;
            object coment;
            int contador = 0;
            IEnumerator enmAutor = autorMysqlDV.GetEnumerator();
            while (enmAutor.MoveNext()) // Recorremos la tabla autores de la base de datos MySql
            {
                autor = (DataRowView)enmAutor.Current;
                idAutor = (long)autor[0];
                nombre = (string)autor[1];
                // Buscamos el nombre en la tabla AUTOR de Access.
                DataRowView[] autores = autorDV.FindRows(nombre);
                if (autores.Length > 1)
                {
                    MessageBox.Show($"Nombre {nombre} duplicado");
                }
                else
                {
                    // Obtenemos su id y buscamos las obras relacionadas con este id.
                    int idAutorOrigen = (int)autores[0]["IdAutor"];
                    DataRowView[] autorObras = obraAutorDV.FindRows(idAutorOrigen);
                    for (int i = 0; i < autorObras.Length; i++)
                    {
                        int idObraOrigen = (int)autorObras[i]["idObra"];        // Obtenemos el id de la obra del autor.
                        int idxObra = obraDV.Find(idObraOrigen);                // Obtenemos el índice de la obra en dvObra.
                        titulo = (string)obraDV[idxObra]["Titulo"];             // Obtenemos su título y subtítulo (comentario).
                        coment = obraDV[idxObra]["Comentario"];
                        DataRowView[] obras = obraMysqlDV.FindRows(titulo);     // Buscamos en Mysql las obras que tienen ese título.
                        if (obras.Length > 1)
                        {
                            int j = 0;
                            while (j < obras.Length && !coment.Equals(obras[j]["COMENT"])) j++;
                            if (j == obras.Length)
                            {
                                MessageBox.Show($"Error: subtítulo de la obra {titulo} no encontrado");
                                continue;
                            }
                            else idObra = (long)obras[j][0];
                        }
                        else
                        {
                            idObra = (long)obras[0][0];
                        }
                        DataRow newRow = mysqlDS.Tables["AUTOR_OBRA"].NewRow();
                        newRow["idAutor"] = idAutor;
                        newRow["idObra"] = idObra;
                        mysqlDS.Tables["AUTOR_OBRA"].Rows.Add(newRow);  // Insertamos nuevo registro en AUTOR_OBRA
                    }
                }
                contador++;
            }
            MySqlCommand cmd = new MySqlCommand("INSERT INTO AUTOR_OBRA (idAutor, idObra) VALUES (@idAutor, @idObra);", mysqlCon);
            cmd.Parameters.Add(new MySqlParameter("@idAutor", MySqlDbType.Int64, 8, "idAutor"));
            cmd.Parameters.Add(new MySqlParameter("@idObra", MySqlDbType.Int64, 8, "idObra"));
            mysqlAdapter.InsertCommand = cmd;
            int addedRows = mysqlAdapter.Update(mysqlDS.Tables["AUTOR_OBRA"]); // Actualizamos los registros insertados en la base de datos.
            this.txtLog.AppendText($"Total registros AUTOR_OBRA: {nfilas} - Migrados: {addedRows}\n");
            oledbCon.Close();
        }

        private void MigrarEdicionLibro()
        {
            // Recorreremos todas las obras de la base de datos MySql
            // Para cada obra, buscamos su título en la base de datos Access, para obtener su id y ver sus ediciones y libros.
            // Filtramos por idObra para obtener todas las ediciones y libros relacionados.
            // Si hay algún libro con dos o más ediciones, aparecerán como libros distintos con la misma etiqueta.

            // Creamos las vistas de la base de datos Access y las ordenamos por el campo por el que buscaremos.
            oledbCon.Open();
            OleDbDataAdapter oledbDA = new OleDbDataAdapter("SELECT * FROM OBRA, EDICION, LIBRO, OBRA_EDICION_LIBRO", oledbCon);
            oledbDA.TableMappings.Add("Table", "OBRA");
            oledbDA.TableMappings.Add("Table1", "EDICION");
            oledbDA.TableMappings.Add("Table2", "LIBRO");
            oledbDA.TableMappings.Add("Table3", "OBRA_EDICION_LIBRO");
            DataSet oledbDS = new DataSet();
            oledbDA.Fill(oledbDS);
            DataView obraDV = new DataView(oledbDS.Tables["OBRA"]);
            obraDV.Sort = "Titulo";
            DataView edicionDV = new DataView(oledbDS.Tables["EDICION"]);
            edicionDV.Sort = "idObra, idEdicion";
            DataView libroDV = new DataView(oledbDS.Tables["LIBRO"]);
            libroDV.Sort = "idLibro";
            DataView obraEdicionLibroDV = new DataView(oledbDS.Tables["OBRA_EDICION_LIBRO"]);
            obraEdicionLibroDV.Sort = "idObra";

            int nfilas = libroDV.Count;

            // Cargamos las tablas de la base de datos MySql
            MySqlDataAdapter mysqlAdapter = new MySqlDataAdapter();
            mysqlAdapter.SelectCommand = new MySqlCommand("SELECT * FROM OBRA; SELECT * FROM EDICION;", mysqlCon);
            mysqlAdapter.TableMappings.Add("Table", "OBRA");
            mysqlAdapter.TableMappings.Add("Table1", "EDICION");

            DataSet mysqlDS = new DataSet();
            mysqlAdapter.Fill(mysqlDS);
            DataView obraMysqlDV = new DataView(mysqlDS.Tables["OBRA"]);

            IEnumerator enmObra = obraMysqlDV.GetEnumerator();
            while (enmObra.MoveNext())
            {
                DataRowView obra = (DataRowView)enmObra.Current;
                long idObra = (long)obra[0];
                string titulo = (string)obra[1];
                object coment = obra[2];
                // Buscamos el título en la tabla OBRA de Access.
                DataRowView[] obras = obraDV.FindRows(titulo);
                int idObraOrigen;
                if (obras.Length > 1)
                {
                    // Si hay más de una decidimos según el campo coment.
                    int i = 0;
                    while (i < obras.Length && !coment.Equals(obras[i]["Comentario"])) i++;
                    if (i == obras.Length)
                    {
                        MessageBox.Show($"Error: subtítulo de la obra {titulo} no encontrado");
                        continue;
                    }
                    else idObraOrigen = (int)obras[i][0];
                }
                else idObraOrigen = (int)obras[0][0];
                DataRowView[] obrasEdicionLibro = obraEdicionLibroDV.FindRows(idObraOrigen); // Obtenemos todas las ediciones y libros relacionados.
                for (int i=0; i<obrasEdicionLibro.Length; i++) // Para cada registro obra-edicion-libro...
                {
                    int idEdicion = (int) obrasEdicionLibro[i]["idEdicion"];
                    int idxEdicion = edicionDV.Find(new object[] { idObraOrigen, idEdicion });
                    DataRowView edicion = edicionDV[idxEdicion];                       // Datos de la edición.
                    int idLibro = (int)obrasEdicionLibro[i]["idLibro"];
                    int idxLibro = libroDV.Find(idLibro);
                    DataRowView libro = libroDV[idxLibro];                             // Datos del libro.
                    DataRow newRow = mysqlDS.Tables["EDICION"].NewRow();
                    newRow["ID_OBRA"] = idObra;
                    newRow["ETIQUETA"] = libro["Etiqueta"];
                    newRow["EDITORIAL"] = edicion["Editorial"];
                    newRow["COLECCION"] = edicion["Coleccion"];
                    newRow["LUGAR_ED"] = edicion["LugarEd"];
                    newRow["ANY_ED"] = edicion["AñoEd"];
                    newRow["TRADUCTOR"] = edicion["Traductor"];
                    newRow["NUM_PAG"] = edicion["NumPag"].Equals(System.DBNull.Value) ? libro["NumPagLib"] : edicion["NumPag"];
                    newRow["NOTAS_ED"] =
                        edicion["Notas"].Equals(System.DBNull.Value) ? libro["NotasLib"]
                        : libro["NotasLib"].Equals(System.DBNull.Value) ? edicion["Notas"]
                        : edicion["Notas"].ToString() + libro["NotasLib"].ToString();
                    mysqlDS.Tables["EDICION"].Rows.Add(newRow);  // Insertamos nuevo registro en EDICION
                }
            }
            MySqlCommand cmd = new MySqlCommand("INSERT INTO EDICION " +
                "(ID_OBRA, ETIQUETA, EDITORIAL, COLECCION, LUGAR_ED, ANY_ED, TRADUCTOR, NUM_PAG, NOTAS_ED) " +
                "VALUES (@ID_OBRA, @ETIQUETA, @EDITORIAL, @COLECCION, @LUGAR_ED, @ANY_ED, @TRADUCTOR, @NUM_PAG, @NOTAS_ED);", mysqlCon);
            cmd.Parameters.Add(new MySqlParameter("@ID_OBRA", MySqlDbType.Int64, 8, "ID_OBRA"));
            cmd.Parameters.Add(new MySqlParameter("@ETIQUETA", MySqlDbType.VarChar, 30, "ETIQUETA"));
            cmd.Parameters.Add(new MySqlParameter("@EDITORIAL", MySqlDbType.VarChar, 60, "EDITORIAL"));
            cmd.Parameters.Add(new MySqlParameter("@COLECCION", MySqlDbType.VarChar, 60, "COLECCION"));
            cmd.Parameters.Add(new MySqlParameter("@LUGAR_ED", MySqlDbType.VarChar, 60, "LUGAR_ED"));
            cmd.Parameters.Add(new MySqlParameter("@ANY_ED", MySqlDbType.UInt32, 4, "ANY_ED"));
            cmd.Parameters.Add(new MySqlParameter("@TRADUCTOR", MySqlDbType.VarChar, 60, "TRADUCTOR"));
            cmd.Parameters.Add(new MySqlParameter("@NUM_PAG", MySqlDbType.UInt32, 4, "NUM_PAG"));
            cmd.Parameters.Add(new MySqlParameter("@NOTAS_ED", MySqlDbType.Text, 65535, "NOTAS_ED"));
            mysqlAdapter.InsertCommand = cmd;
            int addedRows = mysqlAdapter.Update(mysqlDS.Tables["EDICION"]); // Actualizamos los registros insertados en la base de datos.
            this.txtLog.AppendText($"Total registros EDICION: {nfilas} - Migrados: {addedRows}\n");
            oledbCon.Close();
        }
    }
}
