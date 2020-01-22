using System;
using System.Collections.Generic;
using System.Text;

namespace TesseractEasyTrainer
{
    internal class Logger
    {
        private bool verbose;

        public Logger(bool verbose)
        {
            this.verbose = verbose;
        }

        public void Log(string sentence, bool forceLog = false)
        {
            if(verbose || forceLog)
            {
                Console.WriteLine(sentence);
            }
        }
    }
}
