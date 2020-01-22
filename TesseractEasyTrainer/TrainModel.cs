using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TesseractEasyTrainer
{
    internal class TrainModel
    {
        public List<FileInfo> Images { get; set; }

        public List<FileInfo> Boxes { get; set; }

        public List<FileInfo> Train { get; set; }

        public FileInfo Unicharset { get; set; }

        public FileInfo FontProperties { get; set; }
    }
}
