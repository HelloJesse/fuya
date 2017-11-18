using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VLP.NAV;

namespace TestConsole
{
    class Program
    {
        

        static void Main(string[] args)
        {


            while(true)
            {
                var sendmsg = new NavSendMessage(1, new Nordasoft.Data.Sql.DataBase(""));
                string msg;
                //开始发送命令
                byte[] mybyte = sendmsg.Nav_AirpointSwith(2,2,out msg);
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8010);

                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                server.SendTo(mybyte, mybyte.Length, SocketFlags.None, ip);

                Thread.Sleep(100);
            }
            




            Console.ReadLine();

            return;

            var endPoint = new DnsEndPoint("foyoung.f3322.net", 3333);
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Beging to connect");

            try
            {
                clientSocket.Connect(endPoint);
                Console.WriteLine("Connected");

                //receive message
                string recStr = "";
                byte[] recBytes = new byte[5];
                int bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
                if (bytes == 5 && recBytes[3].ToString("X2") == "C0")
                {
                    Console.WriteLine("Beging to send password");

                    string password = "123456xx";
                    var data = new byte[4];

                    data[0] = 0xEC;
                    data[1] = 0x91;
                    data[3] = 0xC1;

                    data = data.Concat(Encoding.Unicode.GetBytes(password)).ToArray();
                    data[2] = Convert.ToByte(Common.putintTo16string(data.Length + 1,2),16);

                    long n = 0;
                    for (int i = 0; i < data.Length-1; i++)
                    {
                        n += data[i];
                    }

                    data = data.Concat(new[] { Convert.ToByte(Common.putintTo16string(n, 2), 16) }).ToArray(); 

                    var sendBytes = clientSocket.Send(data);
                    Console.WriteLine(sendBytes + " 位长度密码已发送");

                    while (true)
                    {
                        var ReceiveData = new byte[512];
                        bytes = clientSocket.Receive(ReceiveData);
                        Console.WriteLine(bytes + "Received from server");
                        System.Threading.Thread.Sleep(200);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                clientSocket.Close();
                clientSocket.Dispose();
            }
            var c =  Console.ReadLine();
        }
    }
}
