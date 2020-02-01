using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace TesseractEasyTrainer
{
    public class Options
    {
        [Option('t', "tess-path", Required = true, HelpText = "Tesseract data directory")]
        public string TesseractData { get; set; }

        [Option('m', "mode", Required =true, HelpText = @"Mode type: 
createbox: It is the first process of training, it creates the box file for training.
train: use the corrected box file to create the trained file")]
        public string Mode { get; set; }

        [Option('i', "images-directory", Required = true, HelpText = "Directory path containing the TIF images for training")]
        public string ImagesPath { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Show app output")]
        public bool Verbose { get; set; }

        [Option('l', "language", Required = true, HelpText = "Training language, example: eng")]
        public string LanguageName { get; set; }

        [Option('f', "fontname", Required = false, HelpText = "Training font, example: mystrangefont")]
        public string FontName { get; set; }

        [Option('n', "no-copy", Required = false, HelpText = "If setted operate directly in the folder of images-directory, without create a local folder and copy file insde")]
        public bool NoCopy { get; set; }
    }
}
