
namespace accessoBDD {
    public class AccesoBD {

        #region Metodo para crear el string de conexion a la base de datos
        public static string conexion() {
            //cadena de conexion

            string server = "";
            string port = "5432";
            string database = "";
            string user = "";
            string password = "";

            string cadenaCon = $@"Server = {server};
                                 Port = {port};
                             Database = {database};
                              User Id = {user};
                             Password = {password};";


            //se realiza la conexion dent   ro de un try catch en caso de que la conexion sea nula.

            return cadenaCon;

        }
        #endregion
    }
}
