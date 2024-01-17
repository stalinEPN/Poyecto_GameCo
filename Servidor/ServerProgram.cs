using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using accessoBDD;
using Newtonsoft.Json;
using System.Data;

namespace Servidor {
    internal class ServerProgram {
        static TcpListener tcpListener;
        static Thread listenerThread;

        static void Main(string[] args) {
            tcpListener = new TcpListener(IPAddress.Any, 1234);
            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.Start();
        }

        private static void ListenForClients() {
            tcpListener.Start();

            while (true) {
                // Esperar la conexión del cliente
                TcpClient client = tcpListener.AcceptTcpClient();

                // Crear un hilo para manejar la comunicación con el cliente
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);

                Console.WriteLine("Servidor Escuchando.......");

                // Enviar mensaje al cliente
                
                EnviarMensaje(client, "Bienvenido a nuestro juego de dados GameCO!!");

                // Imprimir en la consola del servidor
                Console.WriteLine("Cliente conectado");
            }
        }

        private static void HandleClientComm(object clientObj) {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true) {
                bytesRead = 0;

                try {
                    bytesRead = clientStream.Read(message, 0, 4096);
                } catch {
                    break;
                }

                if (bytesRead == 0)
                    break;

                string receivedData = Encoding.ASCII.GetString(message, 0, bytesRead);
                // Separar el tipo de objeto y el contenido
                string[] parts = receivedData.Split('|');
                if (parts.Length == 2) {
                    string objectType = parts[0];
                    string objectContent = parts[1];
                    string msaj = null;
                    // Manejar el objeto según su tipo
                    switch (objectType) {
                        case "Jugador":
                            // Es un objeto Jugador
                            UsuarioBD jugador = JsonConvert.DeserializeObject<UsuarioBD>(objectContent);
                            if (jugador.Id == 1) {
                                UsuarioBD usr = UsuarioBD.recuperarUsuario(jugador.Alias);
                                //Console.WriteLine(usr);
                                EnviarJugador(tcpClient, usr);
                                Console.WriteLine("jugador autenticado");
                            } else {
                                 msaj = UsuarioBD.crearUsuario(jugador);
                                EnviarMensaje(tcpClient, msaj);
                                Console.WriteLine(msaj);
                            }
                            break;

                        case "Resultados":
                            // Es un objeto DataTable
                            RegistroBD resultado = JsonConvert.DeserializeObject<RegistroBD>(objectContent);
                            msaj = RegistroBD.ingresarRegistro(resultado);
                            EnviarMensaje(tcpClient, msaj);
                            Console.WriteLine(msaj);
                            break;

                        case "Mensaje":
                            // Es un mensaje de texto
                            //Console.WriteLine($"Mensaje recibido: {objectContent}");
                            if(JsonConvert.DeserializeObject<string>(objectContent) == "Jugar") {
                                EnviarDados(tcpClient, DadoBD.recuperarDados()); //el metodo recuperarDados devuelve un DataTable
                                Console.WriteLine("registros enviados");
                            }
                            break;

                        case "SolicitarResultados":
                            Console.WriteLine("solicitando resultados");
                            EnviarResultados(tcpClient, RegistroBD.obtenerRegistros(int.Parse(JsonConvert.DeserializeObject<string>(objectContent))));
                            break;

                        case "SolicitarResultadosFecha":
                            Console.WriteLine("solicitando resultados");
                            string[] recibido = JsonConvert.DeserializeObject<string[]>(objectContent);
                            EnviarResultados(tcpClient, RegistroBD.obtenerRegistros(int.Parse(recibido[0]), recibido[1]));
                            break;

                        default:
                            Console.WriteLine($"Tipo de objeto no reconocido: {objectType}");
                            break;
                    }
                } else {
                    // Mensaje mal formateado
                    Console.WriteLine("Mensaje mal formateado. Debe cont");
                }

                
            }
            tcpClient.Close();
        }


        private static void EnviarJugador(TcpClient tcpClient, UsuarioBD usr) {

            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(usr);
            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "Jugador", json);
        }

        private static void EnviarResultados(TcpClient tcpClient, DataTable reg) {

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

        private static void EnviarDados(TcpClient tcpClient, DataTable dados) {

            // Serializar el objeto a JSON
            string json = JsonConvert.SerializeObject(dados);

            // Enviar el tipo de objeto y el JSON al servidor
            SendMessage(tcpClient, "Dados", json);
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
    }
}




