namespace ICAO_convert;



    class Program
    {
        static void Main(string[] args)
        {
            ICAOConverter convert = new ICAOConverter();
            string code = convert.IcaoToN("a547b1");
            Console.WriteLine(code);
            Console.WriteLine(convert.NToIcao(code));
        }
    }
