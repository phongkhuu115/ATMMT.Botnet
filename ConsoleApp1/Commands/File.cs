using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Commands
{
    public class File : Command
    {
        public File(string name) : base(name)
        {
        }

        public string runFile(string fileName)
        {
            int pid = Process.Start(fileName).Id;
            return "Process started with pid = " + pid;
        }

        public string downloadFile(string url, string fileName)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile(url, fileName);
                return "Downloaded file " + fileName;
            }
        }

        public string createFile(string fileName)
        {
            System.IO.File.Create(fileName).Close();
            return "Created File " + fileName;
        }

        public string viewFile(string fileName)
        {
            string data = "";
            try
            {
                data = System.IO.File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return data;
        }

        public string writeFile(string fileName, string data)
        {
            System.IO.File.AppendAllText(fileName, data);

            return "Data has been wrote to " + fileName;
        }

        public override string execute(string[] args)
        {
            if (args.Length == 0)
            {
                return "Invalid number of Argument";
            }

            switch (args[0])
            {
                case "run":
                    if (args.Length != 2)
                    {
                        return "Expected two argument to run file";
                    }
                    else
                    {
                        return this.runFile(args[1]);
                    }
                case "download":
                    if (args.Length != 3)
                    {
                        return "Expected two argument to run file";
                    }
                    else
                    {
                        return this.downloadFile(args[1], args[2]);
                    }
                case "view":
                    if (args.Length != 2)
                    {
                        return "Expected two argument to run file";
                    }
                    else
                    {
                        return this.viewFile(args[1]);
                    }
                case "write":
                    if (args.Length != 3)
                    {
                        return "Expected two argument to run file";
                    }
                    else
                    {
                        return this.writeFile(args[1], args[2]);
                    }
                case "create":
                    if (args.Length != 2)
                    {
                        return "Expected two argument to run file";
                    }
                    else
                    {
                        return this.createFile(args[1]);
                    }
                default:
                    return "Unexpected Argument" + args[0];
            }
        }
    }
}
