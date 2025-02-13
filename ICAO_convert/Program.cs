namespace ICAO_convert;



    class Program
    {
        static void Main(string[] args)
        {
            string code = ICAOConverter.icao_to_n("a45935");
            Console.WriteLine(code);
            Console.WriteLine(ICAOConverter.NToIcao(code));
        }
    }
