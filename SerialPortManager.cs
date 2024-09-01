using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace ussd
{
    internal class SerialPortManager
    {
        //private readonly SerialPort _serialPort;
        private Dictionary<string, SerialPort> _serialPorts = new Dictionary<string, SerialPort>();
        private Dictionary<string, CancellationTokenSource> _cancellationTokenSources = new Dictionary<string, CancellationTokenSource>();
        public bool autoSendCmdMode = false;
        public string autoSendCmd = "";
        public List<string> cmdListInFile = new List<string>();

        bool ussdfileSend = false;

        public SerialPortManager()
        {

        }
       
        public static string? getPortNameByPosition(byte position)
        {
            string portName = "";
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                if (position <= ports.Length && position>0)
                   return portName = ports[position - 1];
                else
                {
                    Console.WriteLine("Le nom du port est incorrect");
                    return null;
                }
     
            }
            Console.WriteLine("Aucun ports COM disponibles");
            return null;
        }
        /// <summary>
        /// fonction de liste les port comme disponible
        /// </summary>

        public static void ListAvailablePorts()
        {
          
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                Console.WriteLine("Ports COM disponibles:");
                byte ii = 0;
                foreach (string port in ports)
                {
                    Console.WriteLine($"{++ii}-{port}");
                }
            }
            else
            {
                Console.WriteLine("Aucun ports COM disponibles");
            }
        }

        public string?GetNameOfConnectedPortsByPosition(byte position)
        {
            byte ii = 1;
            string port__ = null;
            foreach (var item in this._serialPorts)
            {
                if(ii == position)
                {
                    port__= item.Key;
                    break;
                }
                ii++;
            }
            return port__;
        }

        public void ListOfConnectedPorts()
        {
            byte ii = 0;
            foreach (var item in this._serialPorts)
            {
                Console.WriteLine($"{++ii}-{item.Key}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portNum"></param>
        /// <param name="baudRate"></param>
        /// <param name="parity"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        public void ConnectMode1(byte portNum, int baudRate/*, Parity parity, int dataBits, StopBits stopBits*/ )
        {
            string portName = "";
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0) {
                if(portNum<=ports.Length)
                    portName=ports[portNum-1];
                else
                {
                    Console.WriteLine("Le nom du port est incorrect");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Aucun ports COM disponibles");
                return;
            }
            try
            {
                if (!_serialPorts.ContainsKey(portName))
                {
                    SerialPort serialPort = new SerialPort(portName, baudRate/*, parity, dataBits, stopBits*/);
                   /* serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.DataBits = 8;
                    serialPort.Handshake = Handshake.None;*/
                 //   serialPort.RtsEnable = true;
                 //   serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    serialPort.Open();
                    _serialPorts.Add(portName, serialPort);
                 //   this.StartListening(portName);
                    Console.WriteLine($"Connecté au port série {portName}");
                 //   this._StartListening(portName);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Le port est déjà ouvert par une autre application");
            }catch (IOException){
                Console.WriteLine("Le port n'existe pas ou une autre erreur IO s'est produite");
            } catch (ArgumentException){
                Console.WriteLine("Le nom du port est incorrect");
            }
        }
        /// <summary>
        /// start listening a port com
        /// </summary>
        /// <param name="portName">name of port com</param>
        public void _StartListening(string portName)
        {
            if (_serialPorts.ContainsKey(portName) && _serialPorts[portName].IsOpen)
            {
                var cts = new CancellationTokenSource();
                _cancellationTokenSources[portName] = cts;
                var token = cts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            string message = _serialPorts[portName].ReadLine();
                            message.Trim();
                            if (message.Length > 0)
                            {
                                Console.WriteLine($"Message reçu sur {portName}: {message}");
                                message.Trim();
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"Écoute sur {portName} arrêtée.");
                    }
                }, token);
                Console.WriteLine($"Écoute démarrée sur {portName}.");
            }
           
        }


      

        public void Disconnect(string portName)
        {          
            if (_serialPorts.ContainsKey(portName) && _serialPorts[portName].IsOpen)
            {
                _serialPorts[portName].Close();
                _serialPorts.Remove(portName);
                _cancellationTokenSources[portName]?.Cancel();
                _cancellationTokenSources.Remove(portName);
                Console.WriteLine($"Port série {portName} déconnecté");
            }
            else {
                Console.WriteLine("le port n'est pas connecté");
            }   
        }
        
        public void DisconnectAll()
        {
            foreach (var port in _serialPorts.Values) {
                if (port.IsOpen)
                {
                    port.Close();
                }
                _serialPorts.Clear();
                Console.WriteLine("Tous les ports série déconnectés.");
            }
        }

        public void SendRequest(string portName, string request)
        {
            if (_serialPorts.ContainsKey(portName) && _serialPorts[portName].IsOpen)
            {
                _serialPorts[portName].Write(request + "\r");
            }
            else
            {
                Console.WriteLine($"Le port série {portName} n'est pas connecté.");
            }
        }

        /*  public void StartListening(string portName)
          {
              if (_serialPorts.ContainsKey(portName) && _serialPorts[portName].IsOpen)
              {
                  var cts = new CancellationTokenSource();
                  _cancellationTokenSources[portName] = cts;
                  var token = cts.Token;

                  Task.Run(async () =>
                  {
                      try
                      {
                          while (!token.IsCancellationRequested)
                          {
                              string message = _serialPorts[portName].ReadLine();
                              message.Trim();
                              if (message.Length > 0)
                              {
                                  Console.WriteLine($"Message reçu sur {portName}: {message}");
                                  message.Trim();
                                  if (message.IndexOf("+CUSD:") != -1)
                                  {
                                      Console.WriteLine("result:");
                                      //DecodeUssdText
                                      // +CUSD: 1, "D3379B5CD68168349BAB56338250B0D90B167BC96A29D00C063382E0EFBA1CD47EA7E720F229DC7ED7E52E90F54D979741E8F49CFE4ECBCBA079D94D4FB7CB6E7A985D068DDFED76D93D2E87142005350C2FEB4131" ,15
                                      string[] words = message.Split('\"');
                                      string result = UssdDecoder.DecodeUssdText("D3379B5CD68168349BAB56338250B0D90B167BC96A29D00C063382E0EFBA1CD47EA7E720F229DC7ED7E52E90F54D979741E8F49CFE4ECBCBA079D94D4FB7CB6E7A985D068DDFED76D93D2E87142005350C2FEB4131", true);
                                      Console.WriteLine(result);

                                  }

                                  // await mqttClientManager.PublishAsync(topic, message);
                              }
                          }
                      }
                      catch (OperationCanceledException)
                      {
                          Console.WriteLine($"Écoute sur {portName} arrêtée.");
                      }
                  }, token);
                  Console.WriteLine($"Écoute démarrée sur {portName}.");
              }
              else
              {
                  Console.WriteLine($"Le port {portName} n'est pas connecté.");
              }
          }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portNum"></param>
        /// <param name="baudRate"></param>
        /// <param name="parity"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        public void ConnectMode2(byte portNum, int baudRate, MqttClientManager mqttClientManager, string topic, bool autoMode=false,string autoModeCmd="")
        {
            string portName = "";
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                if (portNum <= ports.Length)
                    portName = ports[portNum - 1];
                else
                {
                    Console.WriteLine("Le nom du port est incorrect");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Aucun ports COM disponibles");
                return;
            }
            try
            {
                if (!_serialPorts.ContainsKey(portName))
                {
                    SerialPort serialPort = new SerialPort(portName, baudRate/*, parity, dataBits, stopBits*/);
                    /* serialPort.Parity = Parity.None;
                     serialPort.StopBits = StopBits.One;
                     serialPort.DataBits = 8;
                     serialPort.Handshake = Handshake.None;*/
                    //   serialPort.RtsEnable = true;
                    //   serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    serialPort.Open();
                    _serialPorts.Add(portName, serialPort);
                    //   this.StartListening(portName);
                    Console.WriteLine($"Connecté au port série {portName}");
                    this.StartListening(portName, mqttClientManager,topic,autoMode,autoModeCmd);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Le port est déjà ouvert par une autre application");
            }
            catch (IOException)
            {
                Console.WriteLine("Le port n'existe pas ou une autre erreur IO s'est produite");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Le nom du port est incorrect");
            }
        }


        

        public void StartListening(string portName, MqttClientManager mqttClientManager, string topic, bool autoMode = false, string autoModeCmd = "", string ?filePath = null)
        {
            this.autoSendCmdMode = autoMode;
            this.autoSendCmd = autoModeCmd;
            if(filePath!= null)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(filePath))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                Console.WriteLine(line);
                                this.cmdListInFile.Add(line.Trim());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Le fichier ne peut pas être lu:");
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Fichier introuvable");
                    return;
                }
            }

            if (_serialPorts.ContainsKey(portName) && _serialPorts[portName].IsOpen)
            {
                var cts = new CancellationTokenSource();
                _cancellationTokenSources[portName] = cts;
                var token = cts.Token;

                Task.Run(async () =>
                {
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            string message = _serialPorts[portName].ReadLine();
                            message.Trim();
                            if (message.Length > 0)
                            {
                                Console.WriteLine($"Message reçu sur {portName}: {message}");
                                message.Trim();
                                if (this.autoSendCmdMode)
                                {
                                    if (message.Trim().IndexOf("+CME ERROR:") != -1)
                                    {
                                        Console.WriteLine("Erreur");
                                        throw new Exception("Un erreur est survenu vérifier le module gsm");

                                       
                                    }
                                    if (message.Trim().IndexOf("OK") != -1)
                                    {
                                        if (this.cmdListInFile.Count > 0 && ussdfileSend == true)
                                        {
                                            this.SendRequest(portName.Trim(), this.cmdListInFile[0]);
                                            ussdfileSend = false;
                                        }
                                        if (this.cmdListInFile.Count() <= 0)
                                        {
                                            Console.WriteLine("Envoi Terminé");
                                            return;
                                        };
                                        // Console.WriteLine("gsm: Commande AT recu");
                                    }
                                   

                                    if (message.IndexOf("+CUSD:") != -1)
                                    {
                                       // Console.WriteLine("result:");
                                        string[] words = message.Split('\"');
                                       
                                        if (this.cmdListInFile.Count() > 0)
                                        {
                                            if (words[1].Length>90)
                                            {
                                                string mqttsend = "{\n\'name\':\'gsm1\',\n" + "\'code\':" + this.cmdListInFile[0] + ",\n\'response\':\'" + words[1] + "\'\n}";
                                                //Console.WriteLine(mqttsend);
                                                await mqttClientManager.PublishAsync(topic, mqttsend);
                                                this.cmdListInFile.RemoveAt(0);
                                                this.SendRequest(portName.Trim(), "AT+CUSD=2");
                                                ussdfileSend = true;
                                            }
                                            else
                                            {
                                                this.SendRequest(portName.Trim(), "AT+CUSD=2");
                                                ussdfileSend = true;
                                            }


                                            //Console.WriteLine(mqttsend);
                                          
                                    
                                            if (this.cmdListInFile.Count() <= 0)
                                            {
                                                Console.WriteLine("Envoi Terminé");
                                                return;
                                            };
                                        }
                                        else
                                        {
                                            string mqttsend = "{\n\'name\':\'gsm1\',\n" + "\'code\':" + this.autoSendCmd + ",\n\'response\':\'" + words[1] + "\'\n}";
                                            Console.WriteLine(mqttsend);
                                            await mqttClientManager.PublishAsync(topic, mqttsend);
                                            this.SendRequest(portName.Trim(), this.autoSendCmd);
                                        }
                                        Thread.Sleep(1000);
                                        Console.WriteLine("Requête AT envoyée.");

                                    }
                                    else if (message.IndexOf(this.autoSendCmd) != -1)
                                    {
                                      
                                    }
                                   
                                    else
                                    {
                                        if (this.cmdListInFile.Count() > 0)
                                        {

                                            string mqttsend = "{\n\'name\':\'gsm1\',\n" + "\'code\':" + this.cmdListInFile[0] + ",\n\'response\':\'" + message + "\'\n}";
                                            Console.WriteLine(mqttsend);
                                            await mqttClientManager.PublishAsync(topic, mqttsend);
                                            this.cmdListInFile.RemoveAt(0);
                                            this.SendRequest(portName.Trim(), this.cmdListInFile[0]);
                                            if (this.cmdListInFile.Count() <= 0)
                                            {
                                                Console.WriteLine("Envoi Terminé");
                                                return;
                                            };
                                        }
                                        else
                                        {
                                            string mqttsend = "{\n\'name\':\'gsm1\',\n" + "\'code\':" + this.autoSendCmd + ",\n\'response\':\'" + message + "\'\n}";
                                            Console.WriteLine(mqttsend);
                                            await mqttClientManager.PublishAsync(topic, mqttsend);
                                            this.SendRequest(portName.Trim(), this.autoSendCmd);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else
                                {
                                    if (message.IndexOf("+CUSD:") != -1)
                                    {
                                        Console.WriteLine("result:");
                                        string[] words = message.Split('\"');
                                        string mqttsend = "{\n\'name\':\'gsm1\',\n\'response\':\'" + words[1] + "\'\n}";
                                        Console.WriteLine(mqttsend);
                                        await mqttClientManager.PublishAsync(topic, mqttsend);
                                        Console.WriteLine("Requête AT envoyée.");

                                    }
                                    else
                                    {
                                        Console.WriteLine("result:");
                                        string mqttsend = "{\n\'name\':\'gsm1\',\n\'response\':\'" + message + "\'\n}";
                                        Console.WriteLine(mqttsend);
                                        await mqttClientManager.PublishAsync(topic, mqttsend);
                                        Console.WriteLine("Requête AT envoyée.");
                                    }
                                }
                                

                                // await mqttClientManager.PublishAsync(topic, message);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine($"Écoute sur {portName} arrêtée.");
                    }
                }, token);
                Console.WriteLine($"Écoute démarrée sur {portName}.");
            }
            else
            {
                Console.WriteLine($"Le port {portName} n'est pas connecté.");
            }
        }

        public void StopListening(string portName)
        {
            if (_cancellationTokenSources.ContainsKey(portName))
            {
                _cancellationTokenSources[portName].Cancel();
                _cancellationTokenSources.Remove(portName);
                Console.WriteLine($"Écoute arrêtée sur {portName}.");
            }
            else
            {
                Console.WriteLine($"Aucune écoute en cours sur le port {portName}.");
            }
        }

        public void StartListeningAll(MqttClientManager mqttClientManager, string topic)
        {
            foreach (var portName in _serialPorts.Keys)
            {
                StartListening(portName,mqttClientManager,topic);
            }
        }


        public void StopListeningAll()
        {
            foreach (var portName in _cancellationTokenSources.Keys)
            {
                StopListening(portName);
            }
        }

        private static void DataReceivedHandler(
                       object sender,
                       SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.Write(indata);
        }






        /*  public async Task<string> SendRequestAsync(string requet)
          {
              this._serialPort.WriteLine(requet);
              return await Task.Run(() => _serialPort.ReadLine());
          }


          public void Close()
          {
              if (this._serialPort.IsOpen)
              {
                  _serialPort.Close();
                  Console.WriteLine("Port série fermé.");
              }
          }
          */
    }
}
