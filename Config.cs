using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ussd
{
    class Config
    {
        public required SerialPortConfig SerialPort { get; set; }
        public required MqttConfig Mqtt {  get; set; }
        public required ApplicationConfig Application { get; set; }

        public static Config DefaultConfig
        {
            get
            {
                return new Config
                {
                    SerialPort = new SerialPortConfig
                    {
                        Baude = 115200
                    },
                    Mqtt = new MqttConfig
                    {
                        Broker = "tracker.hypervigi.com",
                        Port = 1883,
                        Username = "root",
                        Password = "P@sswordathv",
                        Topic = "ussd/gsm",
                        ClientId = "client_159595"
                    },
                    Application = new ApplicationConfig
                    {
                        Name = "STDev-Ussd",
                        Version = "1.0.0"
                    }
                };
            }
        }

     

        public static readonly string welcome = 
            "====================================================\n" +
            "----------\r\n|   STDev   |\r\n----------" +Environment.NewLine +
            "log-pro version beta\n" +
            "=======================================================";
        public static readonly string mode =
            "1- Mode Pro\n" +
            "2- Mode debugage";
        public static readonly string help =
            " list :" + 
                   "Affiche la liste des ports série disponibles." + Environment.NewLine +
                   Environment.NewLine +
                   " list-connected :"+ 
                   "Affiche la liste des ports série actuellement connectés." + Environment.NewLine +
                   Environment.NewLine +
                   " connect :" + 
                   "Se connecte à un port série spécifié. Vous devrez entrer le numéro du port ou le nom après cette commande." + Environment.NewLine +
                   Environment.NewLine +
                   " send:" + 
                   "Envoie une commande au port série connecté. Vous devrez entrer la commande après cette commande." + Environment.NewLine +
                   Environment.NewLine +
                   " config:" +
                   "Affiche ou modifie la configuration de l'application. Cela peut inclure des paramètres comme le sujet MQTT, le chemin du fichier, etc." + Environment.NewLine +
                   Environment.NewLine +
                   " exit:" + 
                   "Ferme l'application." + Environment.NewLine +
                   Environment.NewLine +
                   " help:" +
                   "Affiche cette aide." + Environment.NewLine +
                   Environment.NewLine;
    }
    
    class SerialPortConfig
    {
        public required int Baude { get; set; }
    }
    class MqttConfig
    {
        public required string Broker { get; set; }
        public required int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Topic { get; set; }
        public required string ClientId { get; set; }
    }
    class ApplicationConfig
    {
        public required string Name { get; set; }
        public required string Version { get; set; }
    }

}
