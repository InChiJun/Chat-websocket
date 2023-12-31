﻿using System.Net.Sockets;
using System.Net;
using System.Text;

namespace AServer
{
    public class Server
    {
        private readonly static int BufferSize = 4096;

        public static void Main()
        {
            try
            {
                new Server().Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        private Dictionary<string, Socket> connectedClients = new(); // 소켓 값이 들어감

        public Dictionary<string, Socket> ConnectedClients
        {
            get => connectedClients;
            set => connectedClients = value;
        }

        private Socket ServerSocket; // 서버 소켓

        private readonly IPEndPoint EndPoint = new(IPAddress.Parse("127.0.0.1"), 5001);

        int clientNum;
        Server()
        {
            ServerSocket = new( // TCP 소켓 생성
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            clientNum = 0;
        }

        void Init()
        {
            ServerSocket.Bind(EndPoint); // 주소랑 포트번호 묶어주기
            ServerSocket.Listen(100);
            Console.WriteLine("Waiting connection request.");

            Accept();

        }


        void Accept()
        {
            do
            {
                Socket client = ServerSocket.Accept();

                Console.WriteLine($"Client accepted: {client.RemoteEndPoint}.");

                // 비동기 Receive 추가: client가 데이터전송 대기
                // 비동기로 백그라운드 보내서 실행하고 다음으로 넘어감
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                client.ReceiveAsync(args);

            } while (true);
        }

        void Disconnected(Socket client)
        {
            Console.WriteLine($"Client disconnected: {client.RemoteEndPoint}.");
            foreach (KeyValuePair<string, Socket> clients in connectedClients)
            {
                if (clients.Value == client)
                {
                    ConnectedClients.Remove(clients.Key);
                    clientNum--;
                }
            }
            client.Disconnect(false);
            client.Close();
        }

        void Received(object? sender, SocketAsyncEventArgs e) //sender는 client 정보
        {
            Socket client = (Socket)sender!;
            byte[] data = new byte[BufferSize];
            try
            {
                int n = client.Receive(data); // 데이터 받기
                if (n > 0)
                {
                    MessageProc(client, data);

                    SocketAsyncEventArgs argsR = new SocketAsyncEventArgs();
                    argsR.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                    client.ReceiveAsync(argsR);
                }
                else { throw new Exception(); }
            }
            catch (Exception)
            {
                Disconnected(client);
            }
        }

        void MessageProc(Socket s, byte[] bytes)
        {
            string m = Encoding.Unicode.GetString(bytes);
            //
            string[] tokens = m.Split(':');
            string fromID;
            string toID = tokens[0];

            if (toID.Equals("ID")) // ID
            {
                clientNum++;
                fromID = tokens[1].Trim();
                Console.WriteLine("[접속{0}]ID:{1},{2}",
                    clientNum, fromID, s.RemoteEndPoint);
                //
                connectedClients.Add(fromID, s);
                s.Send(Encoding.Unicode.GetBytes("ID_REG_Success:"));
            }
            else if (toID.Equals("01") || toID.Equals("02")) // TO
            {
                Console.WriteLine("[to " + toID + " sended");
                SendTo(toID, m);
            }
            else if (toID.Equals("File"))
            {
                ReceiveFile(s, m);
            }
            else
            {
                Broadcast(s, m);
            }
        }
        void ReceiveFile(Socket s, string m)
        {
            string output_path = @"FileDown\";
            if (!Directory.Exists(output_path))
            {
                Directory.CreateDirectory(output_path); 
            }
            string[] tokens = m.Split(':');
            string fileName = tokens[1].Trim();
            long fileLength = Convert.ToInt64(tokens[2].Trim());
            string FileDest = output_path +fileName;

            long flen = 0;
            FileStream fs = new FileStream(FileDest, 
                                FileMode.OpenOrCreate,
                            FileAccess.Write, FileShare.None);
            while (flen < fileLength)
            {
                byte[] fdata = new byte[4096];
                int r = s.Receive(fdata, 0, 4096,
                    SocketFlags.None);
                fs.Write(fdata, 0, r);
                flen+=r;
            }
            fs.Close();
        }
        void SendTo(string id, string msg)
        {
            Socket socket;
            byte[] bytes = Encoding.Unicode.GetBytes(msg);
            if (connectedClients.ContainsKey(id))
            {
                //
                connectedClients.TryGetValue(id, out socket!);
                try { socket.Send(bytes); } catch { }
            }
        }
        void Broadcast(Socket s, string msg) // 5-2ㅡ모든 클라이언트에게 Send
        {
            byte[] bytes = Encoding.Unicode.GetBytes(msg);
            //
            foreach (KeyValuePair<string, Socket> client in connectedClients.ToArray())
            {
                try
                {
                    //5-2 send
                    //
                    if (s != client.Value)
                        client.Value.Send(bytes);

                }
                catch (Exception)
                {
                    Disconnected(client.Value);
                }
            }
        }

    }
}