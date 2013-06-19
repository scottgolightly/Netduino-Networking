using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace Netduino_Networking
{
    public class Program
    {
        public static void Main()
        {
            // write your code here
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
            int port = 80;

            // Wait 5 seconds for DHCP to assign an address
            Thread.Sleep(5000);

            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface networkInterface = Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0];

            Debug.Print("My IP Address is: " + networkInterface.IPAddress.ToString());

            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, port);
            listenerSocket.Bind(endpoint);

            listenerSocket.Listen(1);

            while (true)
            {
                Socket clientSocket = listenerSocket.Accept();
                bool dataReady = clientSocket.Poll(5000, SelectMode.SelectRead);

                if (dataReady && clientSocket.Available > 0)
                {
                    byte[] buffer = new byte[clientSocket.Available];
                    int bytesRead = clientSocket.Receive(buffer);

                    string request = new string(System.Text.Encoding.UTF8.GetChars(buffer));

                    if (request.IndexOf("ON") >= 0)
                        led.Write(true);
                    else
                        led.Write(false);

                    string statusText = "LED is " + (led.Read() ? "ON" : "OFF") + ".";

                    string response = "HTTP/1.1 200 OK\r\n" +
                        "Content-Type: text/html; charset=utf-8\r\n\r\n" +
                        "<html><head><title>Netduino Networking Example</title></head>" +
                        "<body>" + statusText + "</body></html>";

                    clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(response));
                    clientSocket.Close();
                }
            }

        }

    }
}
