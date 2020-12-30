using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;

namespace OppaiBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // since we cannot make the entry method asynchronous,
            // let's pass the execution to asynchronous code
            Bot.Initialize().GetAwaiter().GetResult();
            Console.ReadKey();
        }
    }
}
