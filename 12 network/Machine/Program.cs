using System;
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
                msgProtocol(str);

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

            string message = "ID:" + nameID + ":";
            dataID = Encoding.Unicode.GetBytes(message);
            machineSocket.Send(dataID); // 내장함수로, 서버에 dataID를 전송함
            Console.WriteLine("MessageFormat = ToID:Commad:Value");
            do
            {                
                byte[] data;
                string msg = Console.ReadLine()!;
                msgProtocol(msg);                                              
            } while (true);
        }

        void msgProtocol(string msg)
        {
            string[] tokens = msg.Split(":");
            byte[] data;
            string toID = tokens[0];
            string command = tokens[1];
            string value;

            if (command == "01")
            {
                string m = "01:" + "current temperature:" + temperature;
                data = Encoding.Unicode.GetBytes(m);
                try { MachineSocket.Send(data); } catch { }
            }
            else if (command == "02")
            {
                value = tokens[2];
                temperature += Convert.ToInt32(value);
                string m = "01:" + "current temperature:" + temperature;
                Console.WriteLine("Raised temperature by " + value + "\n" +
                    "current temperature: " + temperature);
                data = Encoding.Unicode.GetBytes(m);
                try { MachineSocket.Send(data); } catch { }
            }
            else if (command == "03")
            {
                value = tokens[2];
                temperature -= Convert.ToInt32(value);
                string m = "01:" + "current temperature:" + temperature;
                Console.WriteLine("Lowered temperature by " + value + "\n" +
                    "current temperature: " + temperature);
                data = Encoding.Unicode.GetBytes(m);
                try { MachineSocket.Send(data); } catch { }
            }            
        }
    }
}
