using Bot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Commands
{
    //lop xu ly cac command
    public class CommandHandler
    {
        List<Command> commands;

        public CommandHandler()
        {
            this.commands = new List<Command>();

            this.commands.Add(new PingCommand("ping")); //them lenh ping vao list cac lenh co the thuc thi

            this.commands.Add(new GetName("name"));

            this.commands.Add(new File("file"));
        }

        public string runCommand(string cmd)
        {
            string[] cmdParams = cmd.Split(' '); // cac tham so cua cac lenh
            string name = cmdParams.First(); //lay ten cua lenh
            string[] args = cmdParams.Skip(1).ToArray(); //lay cac tham so con lai

            foreach (Command c in commands) //check tung lenh trong list 
            {
                if (c.name.ToLower() == name)
                {
                    return c.execute(args); //thuc thi lenh
                }
            }
            return "Command " + name + " not found"; //khong tim thay lenh
        }
    }
}
