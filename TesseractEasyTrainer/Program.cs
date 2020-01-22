using CommandLine;
using System;
using System.IO;
using System.Text;

namespace TesseractEasyTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);

            if(parseResult.Tag == ParserResultType.Parsed)
            {
                Parsed<Options> parsed = (Parsed<Options>)parseResult;
                var logger = new Logger(parsed.Value.Verbose);

                try
                {
                    var trainer = trainerFactory(parsed.Value, logger);
                    trainer.Train();
                }
                catch(ArgumentException ex)
                {
                    Console.WriteLine(new StringBuilder("Argument exception: ").Append(ex.Message));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(new StringBuilder("Generic exception: ").Append(ex.Message));
                }

            }
            Console.WriteLine("...END");
            Console.ReadLine();
        }

        static Trainer trainerFactory(Options args, Logger logger)
        {
            // Check the correct values of args
            DirectoryInfo imagesPath = new DirectoryInfo(args.ImagesPath);
            if (!imagesPath.Exists)
            {
                throw new ArgumentException("Invalid image path exception. Please provide a valid and existing directory path.");
            }

            DirectoryInfo tesseractPath = new DirectoryInfo(args.TesseractData);
            if (!tesseractPath.Exists)
            {
                throw new ArgumentException("Invalid tesseract path exception. Please provide a valid and existing directory path.");
            }

            string mode = args.Mode;
            EMode eMode;
            switch(mode)
            {
                case "createbox":
                    eMode = EMode.BOX_CREATE;
                    break;
                case "train":
                    eMode = EMode.TRAIN;
                    break;
                default:
                    throw new ArgumentException("Invalid mode exception. Accepted value are: createbox or train. Please run TesseractEasyTrainer.exe --help to get help.");
            }

            var trainer = new Trainer(imagesPath, tesseractPath, eMode, args.LanguageName, args.FontName, args.Verbose, logger);

            return trainer;
        }
    }
}
