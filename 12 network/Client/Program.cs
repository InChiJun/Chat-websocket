using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;


namespace AClient
{
    public class Client
    {
        private readonly static int BufferSize = 4096;

        public static void Main()
        {
            try
            {
                new Client().Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Press any key to exit the program.");
            Console.ReadKey();
        }


        private Socket clientSocket;
        public Socket ClientSocket
        {
            get => clientSocket;
            set => clientSocket = value;
        }
        private readonly IPEndPoint EndPoint = new(IPAddress.Parse("127.0.0.1"), 5001);

        public Client()
        {
            ClientSocket = new(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
        }

        void Init()
        {
            ClientSocket.Connect(EndPoint);
            Console.WriteLine($"Server connected.");

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
            ClientSocket.ReceiveAsync(args);

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


                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Received);
                ClientSocket.ReceiveAsync(args);
            }
            catch (Exception)
            {
                Console.WriteLine($"Server disconnected.");
                ClientSocket.Close();
            }
        }

        void Send()
        {
            byte[] dataID;
            Console.WriteLine("ID를 입력하세요");
            string nameID = Console.ReadLine()!; // 동기로 대기
            
            string message = "ID:" + nameID + ":";
            dataID = Encoding.Unicode.GetBytes(message);
            clientSocket.Send(dataID); // 내장함수로, 서버에 dataID를 전송함
            Console.WriteLine("MessageFormat = ToID:Commad:Value");

            do
            {                
                byte[] data;
                string msg = Console.ReadLine()!; // 클라이언트가 id를 입력하면 메시지를 입력할 때까지 대기(동기로 기다리고, 비동기로 실행)
                string[] tokens = msg.Split(':');
                string m;
                string toID = tokens[0];

                data = Encoding.Unicode.GetBytes(msg);
                try { ClientSocket.Send(data); } catch { }                
            } while (true);
        }
    }
}
