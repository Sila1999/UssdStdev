using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Security.Cryptography.X509Certificates;
using ussd;
//Send short code

//Check the list of short code supported by your network

//15 is encoding scheme as per GSM 3.38

//If you send supported short code, you should see response from network

//for example, below short on T-Mobile (US) should provide response account balance on prepaid accounts
//AT+CUSD = 1,"*104#",15

//reponse
// +CUSD: 1, "D3379B5CD68168349BAB56338250B0D90B167BC96A29D00C063382E0EFBA1CD47EA7E720F229DC7ED7E52E90F54D979741E8F49CFE4ECBCBA079D94D4FB7CB6E7A985D068DDFED76D93D2E87142005350C2FEB4131" ,15

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    // Enumération pour les modes de fonctionnement
    public enum Mode
    {
        MODEPRO = 1,
        MODEDEBUG = 2,
    }

    static async Task Main(string[] args)
    {
        // Crée un token d'annulation qui sera déclenché lorsque Ctrl+C est pressé
        var cts = new CancellationTokenSource();

        // Attacher un gestionnaire d'événements pour Ctrl+C
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("\nClose....");
           // Thread.Sleep(3000);
            Environment.Exit(0);  
            cts.Cancel();     // Signale l'annulation via le token
        };

        const string configPath = "config.json";
        Config config;

        Console.WriteLine(Config.welcome);

        // Initialisation du fichier de configuration
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            config = JsonConvert.DeserializeObject<Config>(json);
        }
        else
        {
            config = Config.DefaultConfig;
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }

        // Initialisation des composants
        SerialPortManager serialPortManager = new SerialPortManager();
        string clientId = Guid.NewGuid().ToString();
        MqttClientManager mqttClientManager = new MqttClientManager(config.Mqtt.Broker, config.Mqtt.Port, clientId, config.Mqtt.Username, config.Mqtt.Password);

        Console.WriteLine($"Version : {config.Application.Version}");
        Console.WriteLine("=====================================");

        // Choix du mode
        Mode modeSystem = Mode.MODEDEBUG;
        Console.WriteLine("Choisissez un mode:");
        Console.WriteLine(Config.mode);
        byte mode = byte.Parse(Console.ReadLine());

        if (mode == (byte)Mode.MODEPRO)
        {
            modeSystem = Mode.MODEPRO;
            Console.WriteLine("Connexion au serveur....");
            await mqttClientManager.ConnectAsync();
        }
      

        while (true)
        {
            Console.WriteLine("Veuillez entrer une commande :");
            string cmd = Console.ReadLine();

            switch (cmd.ToLower())
            {
                case "help":
                    Console.WriteLine(Config.help);
                    break;

                case "list":
                    SerialPortManager.ListAvailablePorts();
                    break;

                case "list-connected":
                    serialPortManager.ListOfConnectedPorts();
                    break;

                case "connect":
                    Console.WriteLine("Choisissez le numéro du port:");
                    SerialPortManager.ListAvailablePorts();
                    byte portNum = byte.Parse(Console.ReadLine());
                    serialPortManager.ConnectMode1(portNum, config.SerialPort.Baude);
                    break;

                case "send":
                    try
                    {
                        Console.WriteLine("Choisissez le numéro du port :");
                        serialPortManager.ListOfConnectedPorts();

                        // Lire et valider le numéro du port
                        byte __portNum;
                        string portName;

                        if (modeSystem == Mode.MODEDEBUG)
                        {
                            portName = serialPortManager.GetNameOfConnectedPortsByPosition(byte.Parse(Console.ReadLine()));
                            serialPortManager._StartListening(portName);

                            while (!cts.Token.IsCancellationRequested)
                            {
                                Console.Write(">");
                                string request = Console.ReadLine();
                                serialPortManager.SendRequest(portName, request);
                            }
                        }
                        else
                        {
                            while (true)
                            {
                                Console.Write(">");
                                if (byte.TryParse(Console.ReadLine(), out __portNum) && __portNum >= 0)
                                {
                                    portName = serialPortManager.GetNameOfConnectedPortsByPosition(__portNum);
                                    if (portName != null)
                                    {
                                        Console.WriteLine($"Port sélectionné : {portName}");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Numéro de port invalide. Veuillez réessayer.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Entrée invalide. Veuillez entrer un numéro valide.");
                                }
                            }

                            // Lire et valider le chemin d'accès du fichier
                            string? patch;
                            while (true)
                            {
                                Console.WriteLine("Entrez le chemin d'accès du fichier :");
                                Console.Write(">");
                                patch = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(patch) && File.Exists(patch))
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("Chemin d'accès invalide ou fichier non trouvé. Veuillez réessayer.");
                                }
                            }

                            Console.WriteLine("Démarrage...");
                            serialPortManager.StartListening(portName, mqttClientManager, config.Mqtt.Topic, true, filePath: patch.Trim());

                            if (serialPortManager.cmdListInFile.Any())
                            {
                                serialPortManager.SendRequest(portName, serialPortManager.cmdListInFile[0]);

                                using (CancellationTokenSource _cts = new CancellationTokenSource())
                                {
                                    await Task.Run(() =>
                                    {
                                        while (!_cts.Token.IsCancellationRequested && serialPortManager.cmdListInFile.Any())
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }, _cts.Token);

                                    Console.WriteLine("Appuyez sur une touche pour terminer...");
                                    Console.ReadKey();
                                    cts.Cancel();
                                }

                                Console.WriteLine("Envoi terminé");
                                serialPortManager.autoSendCmdMode = false;
                                serialPortManager.cmdListInFile.Clear();
                            }
                            else
                            {
                                Console.WriteLine("Aucune commande à envoyer.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Une erreur est survenue : {ex.Message}");
                    }
                    break;

                case "config":
                    
                        if (File.Exists(configPath))
                        {
                            try
                            {
                                Console.WriteLine("Voulez-vous modifier (M) ou afficher (A) le fichier de configuration ?");
                                string choice = Console.ReadLine().ToUpper();

                                switch (choice)
                                {
                                    case "M": // Modifier la configuration
                                        Console.WriteLine("Modification du fichier de configuration...");
                                        string os = Environment.OSVersion.Platform.ToString();

                                        if (os.Contains("Win"))
                                        {
                                            Process.Start(new ProcessStartInfo
                                            {
                                                FileName = "notepad.exe", // Vous pouvez remplacer par l'éditeur de texte préféré
                                                Arguments = configPath,
                                                UseShellExecute = true
                                            });
                                        }
                                        else if (os.Contains("Unix"))
                                        {
                                            Process.Start(new ProcessStartInfo
                                            {
                                                FileName = "nano", // Ou un autre éditeur disponible
                                                Arguments = configPath,
                                                UseShellExecute = true
                                            });
                                        }
                                        else if (os.Contains("Mac"))
                                        {
                                            Process.Start(new ProcessStartInfo
                                            {
                                                FileName = "open",
                                                Arguments = "-e " + configPath, // Ouvre dans l'éditeur de texte par défaut sur Mac
                                                UseShellExecute = true
                                            });
                                        }
                                        break;

                                    case "A": // Afficher la configuration
                                        Console.WriteLine("Contenu du fichier de configuration:");
                                        Console.WriteLine(File.ReadAllText(configPath));
                                        break;

                                    default:
                                        Console.WriteLine("Choix non reconnu.");
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Une erreur est survenue : {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Le fichier de configuration n'existe pas.");
                        }
                        break;

                  

                case "exit":
                    serialPortManager.DisconnectAll();
                    Environment.Exit(0);
                    return;

                default:
                    Console.WriteLine("Commande non reconnue.");
                    break;
            }
        }
    }
}
