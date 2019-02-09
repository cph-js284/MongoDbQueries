using System;

namespace mongoTwitter1
{
    class Program
    {
        static void Main(string[] args)
        {
            SettingUp setup = new SettingUp();
            setup.DownLoadZipFile();

            // allow async methods to finish.
            // Keep console open - till user input.
            Console.ReadKey(); 
        }
    }
}
