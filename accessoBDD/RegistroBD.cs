using Npgsql;
using NpgsqlTypes;
using System;
using System.Data;

namespace accessoBDD {
    [Serializable]
    public class RegistroBD {

        #region atributos
        private int id;
        private int jugador;
        private int dado;
        private int cantidadDados;
        private int[] resultados;
        #endregion

        #region propiedades
        public int Id { get => id; set => id = value; }
        public int Jugador { get => jugador; set => jugador = value; }
        public int Dado { get => dado; set => dado = value; }
        public int CantidadDados { get => cantidadDados; set => cantidadDados = value; }
        public int[] Resultados { get => resultados; set => resultados = value; }
        #endregion

        #region constructor
        public RegistroBD(int id, int jugador, int dado, int cantidadDados, int[] resultados) {
            this.id = id;
            this.jugador = jugador;
            this.dado = dado;
            this.cantidadDados = cantidadDados;
            this.resultados = resultados;
        }
        #endregion

        #region metodo para obtener registros (tabla de resultados) por jugador
        public static DataTable obtenerRegistros(int jugador) {
            DataTable dt = new DataTable();
            string query = $"select r.id as \"id de juego\", to_char(r.fecha, 'YYYY/MM/DD') as fecha, j.alias, d.nombre, d.lados as caras, r.cantidad, r.resultados  from public.registros r\n" +
                           $"inner join public.jugador j on r.jugadorfk = j.id\n" +
                           $"inner join public.dados d on r.dadofk  = d.id\n" +
                           $"where r.jugadorfk = {jugador}";
            var conexionSql = new NpgsqlConnection(AccesoBD.conexion());

            try {
                conexionSql.Open();
                var adapter = new NpgsqlDataAdapter(query, conexionSql);
                adapter.Fill(dt);

            } catch {
                dt = null;
            } finally { if (conexionSql.State == ConnectionState.Open) conexionSql.Close(); }


            return dt;

        }
        #endregion

        #region metodo para obtener registros (tabla de resultados) por jugador y fecha
        public static DataTable obtenerRegistros(int jugador, string fecha) {
            DataTable dt = new DataTable();
            string formattedFecha = DateTime.Parse(fecha).ToString("yyyy-MM-dd");

            string query = $"select r.id as \"id de juego\", to_char(r.fecha, 'YYYY/MM/DD') as fecha, j.alias, d.nombre, d.lados as caras, r.cantidad, r.resultados  from public.registros r\n" +
                           $"inner join public.jugador j on r.jugadorfk = j.id\n" +
                           $"inner join public.dados d on r.dadofk  = d.id\n" +
                           $"where r.jugadorfk = {jugador}\n" +
                           $"and r.fecha::date = '{formattedFecha}';";
            var conexionSql = new NpgsqlConnection(AccesoBD.conexion());

            try {
                conexionSql.Open();
                var adapter = new NpgsqlDataAdapter(query, conexionSql);
                adapter.Fill(dt);

            } catch {
                dt = null;
            } finally { if (conexionSql.State == ConnectionState.Open) conexionSql.Close(); }


            return dt;

        }
        #endregion

        #region metodo para ingresar un registro (un resultado de una jugada)
        public static string ingresarRegistro(RegistroBD reg) {
            string respuesta = "";
            var conexionSQL = new NpgsqlConnection(AccesoBD.conexion());

            try {
                conexionSQL.Open();
                string query = "INSERT INTO public.registros(jugadorfk, dadofk, cantidad, resultados) " +
                                "values(@jugadorfk, @dadofk, @cantidad, @resultados);";

                var comandoSql = new NpgsqlCommand(query, conexionSQL);
                comandoSql.CommandType = CommandType.Text;

                var parJugadorfk = new NpgsqlParameter("@jugadorfk", NpgsqlDbType.Integer);
                parJugadorfk.Value = reg.Jugador;
                comandoSql.Parameters.Add(parJugadorfk);

                var parDadofk = new NpgsqlParameter("@dadofk", NpgsqlDbType.Integer);
                parDadofk.Value = reg.Dado;
                comandoSql.Parameters.Add(parDadofk);

                var parCantidad = new NpgsqlParameter("@cantidad", NpgsqlDbType.Integer);
                parCantidad.Value = reg.CantidadDados;
                comandoSql.Parameters.Add(parCantidad);

                var parResultados = new NpgsqlParameter("@resultados", NpgsqlDbType.Array | NpgsqlDbType.Integer);
                parResultados.Value = reg.Resultados;
                comandoSql.Parameters.Add(parResultados);

                respuesta = comandoSql.ExecuteNonQuery() == 1 ? "registro ingresado!!!" : "registro no ingresado!!";
            } catch (NpgsqlException e) {
                
                respuesta = "Error: " + e.Message;
                
            } finally {
                if (conexionSQL.State == ConnectionState.Open) conexionSQL.Close();
            }

            return respuesta;

        }
    }
    #endregion
}
