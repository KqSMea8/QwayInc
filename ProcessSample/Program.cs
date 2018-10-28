using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessSample
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "1", "2" };
#endif
            //test001(args);
            test002(args);

            //Console.ReadLine();
        }
        static void test002(string[] args)
        {
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] in Progress Sample. Arguments:");
            for (Int32 index = 0; index < args.Length; ++index)
            {
                Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] - [{args[index]}]");
            }
            //if (System.IO.File.Exists("Log.log"))
            //{
            //    Console.WriteLine("A========================");
            //    Console.WriteLine(System.IO.File.ReadAllText("Log.log"));
            //    Console.WriteLine("B========================");
            //}
            for (Int32 index = 0; index < 10; ++index)
            {
                Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] - [{index}]");
            }
            if (Convert.ToInt32(args[0]) > 2)
            {
                //Console.Error.WriteLine($"END");
                Environment.Exit(-123);
            }
            Console.Error.WriteLine($"STOP");
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] End.");
            System.IO.File.AppendAllText("Log.log", $"[LOG] [{DateTime.Now: HH:mm:ss.fff}] End.\n");
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] EXIT.");
        }
        static void test001(string[] args)
        {
            Console.Error.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] ERROR - 001.");
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] in Progress Sample. Arguments:");
            for (Int32 index = 0; index < args.Length; ++index)
            {
                Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] [{index}]: [{args[index]}]");
                System.Threading.Thread.Sleep(100);
            }
            Console.Error.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] ERROR - 002.");
            for (Int32 index = 0; index < 10; ++index)
            {
                Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] - [{index}]");
                System.Threading.Thread.Sleep(100);
            }
            for (Int32 index = 0; index < 10; ++index)
            {
                Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] - [{index}] - [ERROR]");
                System.Threading.Thread.Sleep(100);
            }
            Console.Error.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] ERROR - 003.");
            Console.WriteLine($"[{DateTime.Now: HH:mm:ss.fff}] End.");

        }
    }
}
