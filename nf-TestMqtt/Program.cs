using System;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using Windows.Devices.WiFi;
using System.Net;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

namespace nf_TestMqtt
{
   
        public class Program
        {
            // Set the SSID & Password to your local WiFi network
            const string MYSSID = "WIFI_KELUARGA";
            const string MYPASSWORD = "123qweasd";

            public static void Main()
            {
                try
                {
                    // Get the first WiFI Adapter
                    WiFiAdapter wifi = WiFiAdapter.FindAllAdapters()[0];

                    // Set up the AvailableNetworksChanged event to pick up when scan has completed
                    wifi.AvailableNetworksChanged += Wifi_AvailableNetworksChanged;

                    // Loop forever scanning every 30 seconds
                    while (true)
                    {
                        Console.WriteLine("starting WiFi scan");
                        wifi.ScanAsync();

                        Thread.Sleep(30000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("message:" + ex.Message);
                    Console.WriteLine("stack:" + ex.StackTrace);
                }

                Thread.Sleep(Timeout.Infinite);
            }

            /// <summary>
            /// Event handler for when WiFi scan completes
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private static void Wifi_AvailableNetworksChanged(WiFiAdapter sender, object e)
            {
                Console.WriteLine("Wifi_AvailableNetworksChanged - get report");

                // Get Report of all scanned WiFi networks
                WiFiNetworkReport report = sender.NetworkReport;

                // Enumerate though networks looking for our network
                foreach (WiFiAvailableNetwork net in report.AvailableNetworks)
                {
                    // Show all networks found
                    Console.WriteLine($"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts.ToString()},  signal : {net.SignalBars.ToString()}");

                    // If its our Network then try to connect
                    if (net.Ssid == MYSSID)
                    {
                        // Disconnect in case we are already connected
                        sender.Disconnect();

                        // Connect to network
                        WiFiConnectionResult result = sender.Connect(net, WiFiReconnectionKind.Automatic, MYPASSWORD);

                        // Display status
                        if (result.ConnectionStatus == WiFiConnectionStatus.Success)
                        {
                            Console.WriteLine("Connected to Wifi network");
                            SetupMqtt();
                        }
                        else
                        {
                            Console.WriteLine($"Error {result.ConnectionStatus.ToString()} connecting o Wifi network");
                        }
                    }
                }
            }
        static MqttClient client;
        static void SetupMqtt()
        {
            const string MQTT_BROKER_ADDRESS = "13.76.142.227";
            // create client instance 
            client = new MqttClient(IPAddress.Parse(MQTT_BROKER_ADDRESS));

            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId,"mifmasterz","123qweasd");

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { "mifmasterz/artos/data" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            Thread th1 = new Thread(new ThreadStart(TelemetryLoop));
            th1.Start();

        }

        static void TelemetryLoop()
        {
            while (true)
            {
                string SampleData = $"MQTT on Nanoframework - {DateTime.UtcNow.ToString("HH:mm:ss")}";
                client.Publish("mifmasterz/artos/data", Encoding.UTF8.GetBytes(SampleData), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
                Thread.Sleep(3000);
            }
        }
 
    static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
{
            string Message = new string(Encoding.UTF8.GetChars(e.Message));
            System.Console.WriteLine("Message received: " + Message);
        } 
        }
    
}
