using System;

namespace MarkdownBuild.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine(@"Usage: 
mdbuild <input dir> <output dir>");
                return;
            }
            var sourceDirectory = args[0];
            var destinationDirectory = args[1];
            var builder = new Builder();
            try
            {
                builder.TransformFiles(sourceDirectory, destinationDirectory);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Error.WriteLine("Error: {0}", exception.Message);
                if (args.Length == 3 && args[2] == "--debug")
                    Console.Error.WriteLine(exception.StackTrace);
            }
            Console.ResetColor();
        }
    }
}
