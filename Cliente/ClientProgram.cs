using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using accessoBDD;
using System.Data;

namespace Cliente {
    internal class ClientProgram {
        static void Main(string[] args) {

            try {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 1234);
                NetworkStream clientStream = tcpClient.GetStream();

                // Leer mensaje del servidor
                byte[] message = new byte[4096];
                int bytesRead = clientStream.Read(message, 0, 4096);
                String bienvenida = (Encoding.ASCII.GetString(message, 0, bytesRead));
                string[] parts = bienvenida.Split('|');
                string objectContent = parts[1];
                string mensajeRecibido = JsonConvert.DeserializeObject<string>(objectContent);
                Console.WriteLine($"{mensajeRecibido}");

                string[] msj = null;

                
                int log = 0;
                
                while (log != 3) {
                    int opc = 0;
                    login();
                    log = int.Parse(Console.ReadLine());
                    
                    if (log == 1) {

                        UsuarioBD usr = new UsuarioBD();
                        usr.Id = log;
                        Console.Write("Ingresa el alias de jugador: ");
                        usr.Alias = Console.ReadLine();
                        EnviarJugador(tcpClient, usr);

                        message = new byte[4096];
                        bytesRead = clientStream.Read(message, 0, 4096);
                        msj = recibirMensaje(Encoding.ASCII.GetString(message, 0, bytesRead));
                        

                        if (msj[0] == "Jugador") {
                            UsuarioBD jugador = JsonConvert.DeserializeObject<UsuarioBD>(msj[1]);
                            if (jugador != null) {
                                Console.WriteLine("\n ----------------------------------------------------------\n" +
                                                    "|                  jugador autenticado                     |\n" +
                                                    " ----------------------------------------------------------\n");
                                while (opc != 4) {
                                    menu();
                                    opc = int.Parse(Console.ReadLine());
                                    if (opc == 1) {
                                                                               
                                        EnviarMensaje(tcpClient, "Jugar");

                                        message = new byte[4096];
                                        bytesRead = clientStream.Read(message, 0, 4096);

                                        msj = recibirMensaje(Encoding.ASCII.GetString(message, 0, bytesRead));
                                        DataTable dados = JsonConvert.DeserializeObject<DataTable>(msj[1]);

                                        DadoBD dado = null;
                                        while (dado == null) {
                                            PrintDados(dados);
                                            Console.Write("Seleccione el dado (ingresar el id del dado que desea): ");
                                            int idDado = int.Parse(Console.ReadLine());
                                            dado = seleccionarDado(dados, idDado);
                                            if (dado == null) {
                                                Console.WriteLine("No tengo ese dado en mi lista!!\n");
                                            }
                                        }

                                        Console.Write("ingresa el numero de dados que quieres lanzar(numero entero):  ");
                                        int n = int.Parse(Console.ReadLine());
                                        int[] juego = jugada(dado, n);
                                        
                                        RegistroBD reg = new RegistroBD(0, jugador.Id, dado.Id, n, juego);

                                        EnviarResultados(tcpClient, reg);

                                        message = new byte[4096];
                                        bytesRead = clientStream.Read(message, 0, 4096);

                                        msj = recibirMensaje(Encoding.ASCII.GetString(message, 0, bytesRead));
                                        Console.WriteLine("\n--> " + JsonConvert.DeserializeObject<String>(msj[1]) + "\n\n");



                                    } else if (opc == 2) {
                                        SolicitarResultados(tcpClient, jugador.Id.ToString());

                                        message = new byte[4096];
                                        bytesRead = clientStream.Read(message, 0, 4096);
                                        msj = recibirMensaje(Encoding.ASCII.GetString(message, 0, bytesRead));
                                        DataTable resultados = JsonConvert.DeserializeObject<DataTable>(msj[1]);

                                        if (resultados != null && resultados.Rows.Count > 0) {
                                            PrintResultados(resultados);
                                        } else {
                                            Console.WriteLine("\nEste jugador no tiene resultados!!\n");
                                        }

                                    } else if (opc == 3) {

                                        Console.Write("Ingrese la fecha de juego (aaaa-mm-dd): ");
                                        string fecha = Console.ReadLine();

                                        if (ValidateDateFormat(fecha)) {
                                            string[] enviar = { jugador.Id.ToString(), fecha.ToString() }; 
                                            SolicitarResultados(tcpClient, enviar);

                                            message = new byte[4096];
                                            bytesRead = clientStream.Read(message, 0, 4096);
                                            msj = recibirMensaje(Encoding.ASCII.GetString(message, 0, bytesRead));
                                            DataTable resultados = JsonConvert.DeserializeObject<DataTable>(msj[1]);

                                            if (resultados != null && resultados.Rows.Count > 0) {
                                                PrintResultados(resultados);
                                            } else {
                                                Console.WriteLine("\nEste jugador no tiene resultados para la fecha indicada!!\n");
                                            }

                                        } else {
                                            Console.WriteLine("\nFormato de fecha no válido. Inténtelo nuevamente.");
                                        }

                                        

                                    } else if (opc == 4) {

                                    } else {
                                        Console.WriteLine("No tengo esa opcion!!");
                                    }
                                }
                            } else {
                                Console.WriteLine("\nNo existe este jugador\n");
                            }
                            
                        } else {
                            Console.WriteLine(JsonConvert.DeserializeObject<string>(msj[1]));
                        }

                    } else if (log == 2) {
                        UsuarioBD usr = new UsuarioBD();
                        usr.Id = log;
                        Console.Write("Ingresa el alias de jugador: ");
                        usr.Alias = Console.ReadLine();

                        EnviarJugador(tcpClient, usr);

                        message = new byte[4096];
                        bytesRead = clientStream.Read(message, 0, 4096);
                        msj = recibirMensaje(Encoding.ASCII.GetString(message, 0, bytesRead));
                        Console.WriteLine("\n"+JsonConvert.DeserializeObject<string>(msj[1]));

                    } else if (log == 3) {

                    } else {
                        Console.WriteLine("No contengo esa opcion!!\n");
                    }
                    
                }

                tcpClient.Close();

                
            }catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            
        }

        private static void SendMessage(TcpClient client, string objectType, string message) {
            NetworkStream clientStream = client.GetStream();
            byte[] typeBytes = Encoding.ASCII.GetBytes(objectType + "|");
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            // Combina los bytes del tipo de objeto y el contenido del objeto
            byte[] combined = new byte[typeBytes.Length + buffer.Length];
            Buffer.BlockCopy(typeBytes, 0, combined, 0, typeBytes.Length);
            Buffer.BlockCopy(buffer, 0, combined, typeBytes.Length, buffer.Length);

            // Enviar los bytes combinados al servidor
            clientStream.Write(combined, 0, combined.Length);
            clientStream.Flush();
        }

        private static void EnviarJugador(TcpClient tcpClient, UsuarioBD usr) {
            
            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(usr);
            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "Jugador", json);
        }

        private static void EnviarResultados(TcpClient tcpClient, RegistroBD reg) {
            
            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(reg);

            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "Resultados", json);
        }

        private static void EnviarMensaje(TcpClient tcpClient, string msj) {

            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(msj);

            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "Mensaje", json);
        }

        private static void SolicitarResultados(TcpClient tcpClient, string msj) {

            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(msj);

            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "SolicitarResultados", json);
        }
        private static void SolicitarResultados(TcpClient tcpClient, string[] msj) {

            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(msj);

            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "SolicitarResultadosFecha", json);
        }

        private static void login() {
            Console.WriteLine("\n ----------------------------------------------------\n" +
                                "|                  Menu Principal                    |\n" +
                                " ----------------------------------------------------\n");
            Console.Write("1. Ingresar\n2. Registrar\n3. Salir\ningresa una opcion: ");
        }

        private static void menu() {
            Console.WriteLine("\n ----------------------------------------------------\n" +
                                "|                  Menu de Juego                     |\n" +
                                " ----------------------------------------------------\n");
            Console.Write("1. Jugar\n2. Ver Resultados\n3. Ver Resultados por Fecha\n4. Salir\ningrese una opcion: ");
        }


        private static string[] recibirMensaje(string msj) {
            string[] parts = msj.Split('|');
            if (parts.Length == 2) {
                string objectType = parts[0];
                string objectContent = parts[1];
            }

            return parts;
               
        }

        static void PrintDados(DataTable dataTable) {
            // Imprimir encabezados de columnas
            Console.WriteLine("\n----------------------------------\n" +
                                "|              DADOS              |\n" +
                                "==================================\n");
            Console.WriteLine(String.Format("|{0,-5}|{1,-20}|{2,-5}|", dataTable.Columns[0].ColumnName, dataTable.Columns[1].ColumnName, dataTable.Columns[2].ColumnName));

            // Imprimir datos de filas
            foreach (DataRow row in dataTable.Rows) {
                Console.WriteLine(String.Format("|{0,-5}|{1,-20}|{2,-5}|", row[0], row[1], row[2]));
            }
            Console.WriteLine();

        }

        static void PrintResultados(DataTable dataTable) {
             Console.WriteLine("\n -----------------------------------\n" +
                                 "|             RESULTADOS           |\n" +
                                 " -----------------------------------\n");

            // Imprimir encabezados de columnas
            Console.WriteLine(String.Format("|{0,-15}|{1,-10}|{2,-10}|{3,-15}|{4,-5}|{5,-8}|{6,-15}|",
                                              dataTable.Columns[0].ColumnName, dataTable.Columns[1].ColumnName,
                                              dataTable.Columns[2].ColumnName, dataTable.Columns[3].ColumnName,
                                              dataTable.Columns[4].ColumnName, dataTable.Columns[5].ColumnName,
                                              dataTable.Columns[6].ColumnName));

            // Imprimir datos de filas
            foreach (DataRow row in dataTable.Rows) {
                Console.Write(String.Format("|{0,-15}|{1,-10}|{2,-10}|{3,-15}|{4,-5}|{5,-8}|",
                                            row[0], row[1], row[2], row[3], row[4], row[5]));

                // Formatear la última columna (resultados)
                Int64[] resultados = (Int64[])row[6];
                PrintDynamicResultsColumn(resultados);

                Console.WriteLine("|");
            }

            Console.WriteLine("\n");
        }

        // para formatear la ultima columna correspondiente a los resultados de una jugada en funcion de la cantidad de dados lanzados
        static void PrintDynamicResultsColumn(Int64[] results) {
            Console.Write("{");

            for (int i = 0; i < results.Length; i++) {
                Console.Write(results[i]);
                if (i < results.Length - 1) {
                    Console.Write(",");
                }
            }

            Console.Write("}");
        }


        static DadoBD seleccionarDado(DataTable dataTable, int selectedId) {

            foreach (DataRow row in dataTable.Rows) {
                int id = Convert.ToInt32(row["ID"]);
                if (id == selectedId) {
                    DadoBD dado = new DadoBD(
                        Convert.ToInt32(row["ID"]),
                        row["Nombre"].ToString(),
                        Convert.ToInt32(row["Lados"])
                    );
                    return dado;
                }
            }
            return null; // No se encontró el dado
        }

        private static int[] jugada(DadoBD dado, int n) {

            int caras = dado.Lados;
            int[] result = new int[n];
            Console.Write("\nResultados:\n|");
            for(int i = 0; i < n; i++) {

                var guid = Guid.NewGuid();
                var justNumbers = new String(guid.ToString().Where(Char.IsDigit).ToArray());
                var seed = int.Parse(justNumbers.Substring(0, 4));

                var random = new Random(seed);

                result[i] = random.Next(1, caras + 1);
                Console.Write(result[i] + " | ");
            }
            Console.WriteLine();

            return result;
        }

        static bool ValidateDateFormat(string fecha) {
            // Intentar convertir la cadena a un objeto DateTime con el formato especificado
            return DateTime.TryParseExact(fecha, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _);
        }



    }
}
