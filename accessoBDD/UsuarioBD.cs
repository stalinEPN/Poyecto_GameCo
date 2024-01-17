using System;
using NpgsqlTypes;
using Npgsql;
using System.Data;

namespace accessoBDD {
    [Serializable]
    public class UsuarioBD {
        #region atributos
        private int id;
        private string alias;
        #endregion

        #region propiedades
        public int Id { get => id; set => id = value; }
        public string Alias { get => alias; set => alias = value; }
        #endregion

        #region constructores
        public UsuarioBD() { }
        public UsuarioBD(int id, string alias) {
            this.id = id;
            this.alias = alias;
        }
        #endregion

        #region Metodo Para crear usuarios
        public static string crearUsuario(UsuarioBD usuarioBD) {
            string respuesta = "";
            var conexionSQL = new NpgsqlConnection(AccesoBD.conexion());

            try {
                conexionSQL.Open();
                string query = "INSERT INTO public.jugador(alias) values(@alias);";

                var comandoSql = new NpgsqlCommand(query, conexionSQL);
                comandoSql.CommandType = CommandType.Text;

                var parAlias = new NpgsqlParameter("@alias", NpgsqlDbType.Text);
                parAlias.Value = usuarioBD.Alias;
                comandoSql.Parameters.Add(parAlias);

                respuesta = comandoSql.ExecuteNonQuery() == 1 ? "jugador creado" : "jugador no registrado";
            } catch (NpgsqlException e) {
                if (e.ErrorCode == 23505) {
                    respuesta = "El alias ya está en uso. Elija otro alias.";
                } else {
                    respuesta = "El alias ya está en uso. Elija otro alias.";
                }
            } finally {
                if (conexionSQL.State == ConnectionState.Open) conexionSQL.Close();
            }

            return respuesta;
            
        }
        #endregion

        #region Metodo para Recuperar usuario
        public static UsuarioBD recuperarUsuario(string alias) {
            string query = "SELECT * FROM public.jugador WHERE alias LIKE @alias";
            UsuarioBD jugador = null;

            using (var conexionSql = new NpgsqlConnection(AccesoBD.conexion())) {
                try {
                    conexionSql.Open();
                    using (var comandoSql = new NpgsqlCommand(query, conexionSql)) {
                        comandoSql.CommandType = CommandType.Text;

                        var parAlias = new NpgsqlParameter("@alias", NpgsqlDbType.Text);
                        parAlias.Value = alias;
                        comandoSql.Parameters.Add(parAlias);

                        using (NpgsqlDataReader reader = comandoSql.ExecuteReader()) {
                            if (reader.Read()) {
                                jugador = new UsuarioBD {
                                    Id = reader.GetInt32(0),
                                    Alias = reader.GetString(1)
                                    // Agrega más propiedades según sea necesario
                                };
                            }
                        }
                    }
                } catch (Exception ex) {
                    // Manejo de excepciones, por ejemplo, logueo del error
                }
            }

            return jugador;
        }
        #endregion
    }
}
