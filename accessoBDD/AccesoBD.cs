
namespace accessoBDD {
    public class AccesoBD {

        #region Metodo para crear el string de conexion a la base de datos
        public static string conexion() {
            //cadena de conexion

            string server = "ep-royal-dawn-84894339.us-east-2.aws.neon.fl0.io";
            string port = "5432";
            string database = "GameCo";
            string user = "fl0user";
            string password = "X4zQgHSuAjm5";

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
