using Npgsql;
using System;
using System.Data;

namespace accessoBDD {

    [Serializable]
    public class DadoBD {
        #region atributos
        private int id;
        private string nombre;
        private int lados;
        #endregion

        #region propiedades
        public int Id { get => id; set => id = value; }
        public string Nombre { get => nombre; set => nombre = value; }
        public int Lados { get => lados; set => lados = value; }
        #endregion

        #region constructor
        public DadoBD(int id, string nombre, int lados) {
            this.id = id;
            this.nombre = nombre;
            this.lados = lados;
        }
        #endregion

        #region metodo para recuperar la tabla de dados
        public static DataTable recuperarDados() {

            DataTable dataTable = new DataTable();
            string query = "Select * from public.dados";

            var conexionSql = new NpgsqlConnection(AccesoBD.conexion());

            try { 
                conexionSql.Open();
                var adaptador = new NpgsqlDataAdapter(query, conexionSql);
                adaptador.Fill(dataTable);

            }catch(Exception ex) {
                dataTable = null;

            } finally {
                if (conexionSql.State == ConnectionState.Open) conexionSql.Close();
            }

            return dataTable;

        }
        #endregion

    }
}
