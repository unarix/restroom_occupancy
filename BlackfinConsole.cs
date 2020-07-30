using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irisys.BlackfinAPI;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using System.Threading;

namespace BlackfinAPIConsole
{
    /// <summary>
    /// Console based application demonstrating use of the .Net Irisys Blackfin API
    /// Uses Command pattern for simplicity
    /// Two nested loops for program logic (connection loop + command loop)
    /// </summary>
    public class BlackfinConsole
    {
        private readonly Blackfin blackfin;
        private bool isEnding;

        /// <summary>
        /// Creates a new BlackfinConsole instance
        /// </summary>
        /// <param name="bf"></param>
        BlackfinConsole(Blackfin bf)
        {
            blackfin = bf;
        }

        /// <summary>
        /// Stops the console application from working with the current blackfin
        /// </summary>
        void Disconnect()
        {
            isEnding = true;
        }

        /// <summary>
        /// Prompts the user to input a command
        /// and then executes it with appropriate arguments
        /// </summary>
        private void DoConsoleCommandLoop(Socket clientLabVIEW)
        {
            List<ConsoleCommand> Commands = BlackfinConsoleCommands.GetAllConsoleCommands();

            // sort commands by command name (don't take get/set prefix into account)
            Commands.Sort((o1, o2) =>
            {
                return o1.getName().Substring(3).CompareTo(o2.getName().Substring(3));
            });

            Console.WriteLine("########################");
            Console.WriteLine("Inicio de envio de comandos al aparato del infierno.");
            Console.WriteLine("########################");
            Console.WriteLine();
            
            int i = 0;
            for (i=0; i<3; i++)
            {
                // Console.WriteLine("Please enter a command to execute (or 'help'):");
                // String userString = Console.ReadLine().ToLower();
                //String userString = "getcurrentcount";
                String userString = "getlastncounts";
                //String userString = "getcounts";

                Console.WriteLine("Comando: ");
                Console.WriteLine(userString + "\n");

                if (userString.Equals("help"))
                {
                    Console.WriteLine("Help - Prints this list of available commands");
                    Console.WriteLine("Disconnect - Disconnects from the current device");
                    foreach (ConsoleCommand c in Commands)
                    {
                        Console.WriteLine(c.getName() + " - " + c.getHelp());
                    }
                }
                else
                {
                    // try and execute a command
                    bool executed = false;
                    bool bDisconnect = false;
                    foreach (ConsoleCommand c in Commands)
                    {
                        try
                        {
                            if (c.getName().ToLower().Equals(userString))
                            {
                                executed = true;

                                // assemble any required arguments
                                String[] args = new String[c.getArguments().Length];
                                args[0] = "1";
                                //args[0] = DateTime.Now.AddMinutes(-30).ToString();
                                //args[1] = DateTime.Now.ToString();

                                //int i = 0;
                                //foreach (String s in c.getArguments())
                                //{
                                //    //Console.WriteLine("Please enter a value for " + s + ":");
                                //    //args[i++] = Console.ReadLine();

                                //}

                                // execute command
                                string result = c.execute(blackfin, args);
                                {

                                    if (result == null)
                                        Console.WriteLine("There was a problem executing the command");
                                    else
                                    {
                                        int entradas = 0;
                                        int salidas = 0;
                                        Console.Write("Respuesta :\n" + result + "\n");
                                        string[] resultados = result.Split('\n'); // Corto por cada renglon
                                        foreach (string s in resultados)
                                        {
                                            try
                                            {
                                                string[] datos = s.Split(' '); // Corto por cada espacio
                                                entradas = int.Parse(datos[2]) - entradas;
                                                salidas = int.Parse(datos[3]) - salidas;
                                            }
                                            catch
                                            {
                                                // do nothing
                                            }
                                        }

                                        Bathroom b = new Bathroom();
                                        b.entradas = entradas.ToString();
                                        b.salidas = salidas.ToString();

                                        Console.WriteLine("Entradas: " + entradas);
                                        Console.WriteLine("Salidas: " + salidas);
                                        Console.WriteLine("Gente dentro: " + (entradas - salidas));

                                        using (StreamWriter file = File.CreateText(@"./web/counter.json"))
                                        {
                                            JsonSerializer serializer = new JsonSerializer();
                                            //serialize object directly into file stream
                                            serializer.Serialize(file, b);
                                        }

                                        Console.Write("OK! ");

                                        try
                                        {
                                            string size = result.Length.ToString().PadLeft(10, '0');

                                            Console.WriteLine("hecho!");
                                            Console.WriteLine();
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("Error!");
                                            Console.WriteLine(e.Message);
                                            Console.WriteLine("Program now exits ...");
                                            Environment.Exit(0);
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                        catch (CommandArgumentException e)
                        {
                            Console.WriteLine("Problem with command argument: " + e.Message);
                        }
                        catch (ConnectionResetException e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("");
                            Console.WriteLine("This command has caused the connection to be reset");
                            bDisconnect = true;
                        }
                    }
                    if (!executed)
                    {
                        Console.WriteLine("No command was found matching: " + userString);
                    }

                    //if (bDisconnect)
                    //    break;
                }

                // wait for a specific time
                Console.WriteLine("#################################################");
                Console.WriteLine("Esperando 4 segundos para la proxima ejecucion ...");
                Console.WriteLine("#################################################");
                Console.WriteLine();

                Thread.Sleep(30000); // espero 30 segundos para la proxima ejecucion.
            }
        }

        private void resetCounts(Socket clientLabVIEW)
        {
            List<ConsoleCommand> Commands = BlackfinConsoleCommands.GetAllConsoleCommands();

            // sort commands by command name (don't take get/set prefix into account)
            Commands.Sort((o1, o2) =>
            {
                return o1.getName().Substring(3).CompareTo(o2.getName().Substring(3));
            });

            Console.WriteLine("########################");
            Console.WriteLine("Inicio de envio de comandos al aparato del infierno.");
            Console.WriteLine("########################");
            Console.WriteLine();

            // Console.WriteLine("Please enter a command to execute (or 'help'):");
            // String userString = Console.ReadLine().ToLower();
            String userString = "resetcurrentcount";
            //String userString = "getlastncounts";
            //String userString = "getcounts";

            Console.WriteLine("Comando: ");
            Console.WriteLine(userString + "\n");

            if (userString.Equals("help"))
            {
                Console.WriteLine("Help - Prints this list of available commands");
                Console.WriteLine("Disconnect - Disconnects from the current device");
                foreach (ConsoleCommand c in Commands)
                {
                    Console.WriteLine(c.getName() + " - " + c.getHelp());
                }
            }
            else
            {
                // try and execute a command
                bool executed = false;
                bool bDisconnect = false;
                foreach (ConsoleCommand c in Commands)
                {
                    try
                    {
                        if (c.getName().ToLower().Equals(userString))
                        {
                            executed = true;

                            // assemble any required arguments
                            String[] args = new String[c.getArguments().Length];

                            int i = 0;
                            foreach (String s in c.getArguments())
                            {
                                Console.WriteLine("Please enter a value for " + s + ":");
                                //args[i++] = Console.ReadLine();
                                args[i++] = "y";
                            }

                            // execute command
                            string result = c.execute(blackfin, args);
                            {

                                if (result == null)
                                    Console.WriteLine("There was a problem executing the command");
                                else
                                {
                                    Console.Write("Respuesta :\n" + result + "\n");
                                    Console.Write("OK! ");

                                    try
                                    {
                                        string size = result.Length.ToString().PadLeft(10, '0');
                                        Console.WriteLine("hecho!");
                                        Console.WriteLine();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Error!");
                                        Console.WriteLine(e.Message);
                                        Console.WriteLine("Program now exits ...");
                                        Environment.Exit(0);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    catch (CommandArgumentException e)
                    {
                        Console.WriteLine("Problem with command argument: " + e.Message);
                    }
                    catch (ConnectionResetException e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine("");
                        Console.WriteLine("This command has caused the connection to be reset");
                        bDisconnect = true;
                    }
                }
                if (!executed)
                {
                    Console.WriteLine("No command was found matching: " + userString);
                }

                // wait for a specific time
                Console.WriteLine("#################################################");
                Console.WriteLine("Fin de la ejecucion ...");
                Console.WriteLine("#################################################");
                Console.WriteLine();

                Thread.Sleep(4000);
            }
        }

        /// <summary>
        /// Entry point for application.
        /// Starts the user CLI loop
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
            bool ConnectedToLabVIEW = true;

            BlackfinEngine engine = new BlackfinEngine();
            engine.StartEngine();

            Socket clientLabVIEW = null;

            try
            {
                // connect to the people counter dvice
                IPAddress ipinet = IPAddress.Parse("192.168.253.27");
                IPEndPoint endPoint = new IPEndPoint(ipinet, 4505);

                Console.WriteLine("Connecting to the device at " + ipinet.ToString());

                Socket skt = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                skt.Connect(endPoint);

                Blackfin blackfin = new Blackfin();
                BlackfinConsole console = new BlackfinConsole(blackfin);

                engine.AddNewCounterEndPoint(blackfin, skt, (bf, error, something, type) =>
                {
                    Console.WriteLine("Comms Error!");
                    if (type == BlackfinEngine.ConnectionErrorType.FATAL)
                    {
                        console.Disconnect();
                    }
                });

                Console.WriteLine("Device Connected.\n");
                console.DoConsoleCommandLoop(clientLabVIEW);
                //console.resetCounts(clientLabVIEW);
                engine.RemoveCounterEndPoint(blackfin);
                Console.WriteLine("Disconnected from device");
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: ..." + ex.Message);
                Thread.Sleep(10000);
            }

            engine.ShutdownEngine();
        }

    }

    /// <summary>
    /// This will be thrown if incorrect arguments are passed to a command
    /// </summary>
    public class CommandArgumentException : Exception
    {
        public CommandArgumentException(String ss)
            : base(ss)
        {
        }
    }

    /// <summary>
    /// This will be thrown if any of the SetClientConfig* or SetDNS* commands are used
    /// as these will cause the device to be disconnected
    /// </summary>
    public class ConnectionResetException : Exception
    {
        public ConnectionResetException(String ss)
          : base(ss)
        {
        }
    }


}
