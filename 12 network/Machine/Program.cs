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
                Console.WriteLine(str);
                Console.WriteLine("수신:" + str);


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

            Console.WriteLine("기기의 변화를 입력할 때는 사용자ID:변화내용으로 입력하세요.");
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
                else if (tokens[0].Equals("File"))
                {
                    SendFile(tokens[1]);
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
        void SendFile(string filename)
        {
            FileInfo fi = new FileInfo(filename);
            string fileLength = fi.Length.ToString();

            byte[] bDts = Encoding.Unicode.GetBytes
                ("File:" + filename + ":" + fileLength + ":");
            machineSocket.Send(bDts);

            byte[] bDtsRx = new byte[4096];
            FileStream fs = new FileStream(filename,
                FileMode.Open, FileAccess.Read,
                FileShare.None);
            long received = 0;
            while (received < fi.Length)
            {
                received += fs.Read(bDtsRx, 0, 4096);
                machineSocket.Send(bDtsRx);
                Array.Clear(bDtsRx);
            }
            fs.Close();

            Console.WriteLine("파일 송신 종료");
        }

    }
}
