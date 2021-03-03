using System;

namespace DocuSignTester
{
    class Program
    {
        static string mode;

        static void Main(string[] args)
        {
            mode = "DEV";

            Console.WriteLine("ECAR.DocuSign Tester");
            char choice;

            do
            {
                choice = Menu();
                Console.WriteLine();

                switch (choice)
                {
                    case '1':
                        DocuSignTester.Run(mode);
                        break;

                    case '2':
                        DocuSignTester.GetEnvelopes(mode);
                        break;

                    case 'D':
                    case 'd':
                        if (mode == "DEV")
                            mode = "PROD";
                        else
                            mode = "DEV";
                        break;

                    default:
                        break;

                }
            }
            while (choice != '0');

        }

        static char Menu()
        {
            Console.WriteLine();
            Console.WriteLine("----------------------- Menu -----------------------");
            Console.WriteLine("    1. Run DocuSign eSignature workflow");
            Console.WriteLine("    2. List envelopes from start date");
            Console.WriteLine();
            Console.WriteLine("    0. End program");
            Console.WriteLine("    D. Toggle mode - (Current: {0})", mode);
            Console.WriteLine("----------------------- Menu -----------------------");
            Console.WriteLine();
            Console.Write("Enter choice: ");

            return Console.ReadKey(false).KeyChar;
        }
    }
}
