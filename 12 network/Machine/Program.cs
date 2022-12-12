using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;


namespace AMachine
{
    public class Machine
    {
        private readonly static int BufferSize = 4096;

        int temperature = 0;

        public static void Main()
        {
            try
            {
                new Machine().Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press any key to exit the program.");
            Console.ReadKey();
        }


        private Socket machineSocket;
        public Socket MachineSocket
        {
            get => machineSocket;
            set => machineSocket = value;
        }
        private readonly IPEndPoint EndPoint = new(IPAddress.Parse("127.0.0.1"), 5001);

        public Machine()
        {
            MachineSocket = new(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
        }

        void Init()
        {
            MachineSocket.Connect(EndPoint);
            Console.WriteLine($"Server connected.");

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
            MachineSocket.ReceiveAsync(args);

            Send(); // 메시지가 입력되면 실행
        }


        void Received(object? sender, SocketAsyncEventArgs e)
        {
            try
            {
                byte[] data = new byte[BufferSize];
                Socket server = (Socket)sender!;
                int n = server.Receive(data);

                string str = Encoding.Unicode.GetString(data);
                str = str.Replace("\0", ""); // 널 문자 제거
                Console.WriteLine("수신:" + str);
                string[] tokens = str.Split(':');
                
                if (tokens[1].Equals("02"))
                {
                    messageProtocol(server, str);
                }

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                MachineSocket.ReceiveAsync(args);
            }
            catch (Exception)
            {
                Console.WriteLine($"Server disconnected.");
                MachineSocket.Close();
            }
        }

        void Send()
        {
            byte[] dataID;
            Console.WriteLine("ID를 입력하세요");
            string nameID = Console.ReadLine()!; // 동기로 대기
            //
            string message = "ID:" + nameID + ":";
            dataID = Encoding.Unicode.GetBytes(message);
            machineSocket.Send(dataID); // 내장함수로, 서버에 dataID를 전송함
            //

            Console.WriteLine("기기의 변화를 입력할 때는 사용자ID:02or03:변화내용으로 입력하세요.");
            do
            {
                byte[] data;
                string msg = Console.ReadLine()!; // 클라이언트가 id를 입력하면 메시지를 입력할 때까지 대기(동기로 기다리고, 비동기로 실행)
                string[] tokens = msg.Split(':');
                string m;
                if (tokens[0].Equals("BR"))
                {
                    //
                    m = "BR:" + nameID + ":" + tokens[1] + ":";

                    data = Encoding.Unicode.GetBytes(m);
                    Console.WriteLine("[전체전송]{0}", tokens[1]);
                    try { MachineSocket.Send(data); } catch { }
                }
                else //  (tokens[0].Equals("TO"))
                {
                    //
                    m = "TO:" + nameID + ":" + tokens[0] + ":" + tokens[1] + ":";
                    data = Encoding.Unicode.GetBytes(m);
                    Console.WriteLine("[{0}에게 전송]:{1}", tokens[0], tokens[1]);
                    try { MachineSocket.Send(data); } catch { }
                }




            } while (true);
        }

        void messageProtocol(Socket s, string msg)
        {
            string[] tokens = msg.Split(':');
            string code = tokens[1];

            if (code.Equals("01"))
            {

                try { MachineSocket.Send(BitConverter.GetBytes(temperature)); } catch { }
                /***
                clientNum++;
                fromID = tokens[1].Trim();
                Console.WriteLine("[접속{0}]ID:{1},{2}",
                    clientNum, fromID, s.RemoteEndPoint);
                //
                connectedClients.Add(fromID, s);
                s.Send(Encoding.Unicode.GetBytes("ID_REG_Success:"));
                Broadcast(s, m);
            }
            else if (code.Equals("02"))
            {
                fromID = tokens[1].Trim();
                string msg = tokens[2];
                Console.WriteLine("[전체]{0}:{1}", fromID, msg);
                //
                Broadcast(s, m);
                s.Send(Encoding.Unicode.GetBytes("BR_Success:"));
            }
            else if (code.Equals("03"))
            {
                fromID = tokens[1].Trim();
                toID = tokens[2].Trim();
                string msg = tokens[3];
                string rMsg = "[From:" + fromID + "][TO:" + toID + "]" + msg;
                Console.WriteLine(rMsg);

                //
                SendTo(toID, m);
                s.Send(Encoding.Unicode.GetBytes("To_Success:"));
            }
            else if (code.Equals("File"))
            {
                ReceiveFile(s, m);
            }
            else
            {
                Broadcast(s, m);
            }
                ***/
            }
        }

    }
}
