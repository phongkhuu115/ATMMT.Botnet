using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Commands
{
    public class GetName : Command
    {
        public GetName(string name) : base(name)
        {

        }

        public override string execute(string[] args)
        {
            if (args.Length != 0)
            {
                return "Invalid number of Argument";
            }
            else
            {
                return Environment.UserName;
            }
        }
    }
}
