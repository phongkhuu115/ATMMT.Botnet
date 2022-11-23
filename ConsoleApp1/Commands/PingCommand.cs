using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Commands
{
    public class PingCommand : Command
    {
        public PingCommand(string name) : base(name)
        {

        }

        public override string execute(string[] args)
        {
            if (args.Length == 0)
            {
                return "Invalid Argument"; //ping where ?
            }
            else
            {
                if (args.Length == 1)
                {
                    Ping pinger = null;

                    try
                    {
                        pinger = new Ping();
                        string host = args[0]; //tham so dau tien cung chinh la ten host
                        PingReply reply = pinger.Send(host);
                        if (reply.Status == IPStatus.Success)
                        {
                            return "Reply from " + host.ToString() + "[" + reply.Address.ToString() + "]: " + " Successful"
                            + " Response delay = " + reply.RoundtripTime.ToString() + " ms" + "\n"; //format tra ve cua lenh ping
                        }
                    }
                    catch (PingException)
                    {
                        
                    }
                    finally
                    {
                        if (pinger != null)
                        {
                            pinger.Dispose();
                        }
                    }
                    return ""; //Dat return chuoi rong de complile
                }
                else
                {
                    return "Invalid Argument"; //Co nhieu hon 1 tham so thi sai
                }
            }
        }
    }
}
