using System;

namespace DiscBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {

            string filepath = "test.dat";
            StreamStuff ss = new StreamStuff(filepath,1024*1024*1024*(long)8,8192,1000);
            ss.Logging+= (string msg)=>{Console.WriteLine(msg);};
            ss.StartCopy();

        }
    }
}
