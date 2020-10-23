using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VoiceStreamServer
{
    class UnusedJunk
    {

        private static void UdpServer()
        {
            byte[] data = new byte[1024];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 1234);
            UdpClient newsock = new UdpClient(ipep);

            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            data = newsock.Receive(ref sender);

            Console.WriteLine("Message received from {0}:", sender.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));

            string welcome = "Welcome to my test server";
            data = Encoding.ASCII.GetBytes(welcome);
            newsock.Send(data, data.Length, sender);

            while (true)
            {
                data = newsock.Receive(ref sender);

                Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));
                newsock.Send(data, data.Length, sender);
            }
        }

        private static void TcpServer()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 1234);
            // we set our IP address as server's address, and we also set the port: 9999

            server.Start();  // this will start the server

            while (true)   //we wait for a connection
            {
                Console.WriteLine("Waiting for client connection");

                TcpClient client = server.AcceptTcpClient();  //if a connection exists, the server will accept it

                Console.WriteLine("Connected....");
                NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                byte[] hello = new byte[100];   //any message must be serialized (converted to byte array)
                hello = Encoding.Default.GetBytes("hello world");  //conversion string => byte array

                Console.WriteLine("Sending hello message");
                ns.Write(hello, 0, hello.Length);     //sending the message

                Console.WriteLine("Reading responses...");
                while (client.Connected)  //while the client is connected, we look for incoming messages
                {
                    byte[] msg = new byte[1024];     //the messages arrive as byte array
                    ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client
                                                   //Console.WriteLine(encoder.GetString(msg).Trim(' ')); //now , we write the message as string
                    Console.WriteLine("MESSAGE:" + msg);
                }
            }
        }

    }


}
