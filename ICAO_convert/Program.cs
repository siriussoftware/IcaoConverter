namespace ICAO_convert;



    class Program
    {
        static void Main(string[] args)
        {
            ICAOConverter convert = new ICAOConverter();
            string code = convert.IcaoToN("c0624c");
            Console.WriteLine(code);
            Console.WriteLine(convert.NToIcao(code));
        }
    }
