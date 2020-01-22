using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TesseractEasyTrainer
{
    internal class Trainer
    {
        private const string LOCAL_FOLDER_TEMP = @".\training_temp";

        private DirectoryInfo imagesDirectory;
        private DirectoryInfo tesseractDirectory;
        private EMode emode;
        private string languageName;
        private string fontName;
        private bool verbose;
        private Logger logger;

        internal Trainer(DirectoryInfo imagesDirectory, DirectoryInfo tesseractDirectory, EMode emode, string languageName, string fontName, bool verbose = false, Logger logger = null)
        {
            this.imagesDirectory = imagesDirectory;
            this.tesseractDirectory = tesseractDirectory;
            this.emode = emode;
            this.languageName = languageName;
            this.fontName = fontName;
            this.verbose = verbose;
            this.logger = logger;
        }

        internal void Train()
        {
            //Generate local folder if not exists
            if(!Directory.Exists(LOCAL_FOLDER_TEMP))
            {
                Directory.CreateDirectory(LOCAL_FOLDER_TEMP);
            }

            Directory.SetCurrentDirectory(LOCAL_FOLDER_TEMP);

            // Check the presence of Tesseract exe inside folder
            FileInfo tesseractExe = new FileInfo(Path.Combine(tesseractDirectory.FullName, "tesseract.exe"));
            if (!tesseractExe.Exists)
            {
                throw new Exception("Tesseract.exe has not been found inside the provided path. Please provide a valid Tesseract data folder");
            }

            if (emode == EMode.BOX_CREATE)
            {
                logger?.Log("Starting process of box creation...", true);

                // Retrieve TIF images for training
                var images = retrieveTifImages(imagesDirectory);

                // Copy TIF images locally
                var localImages = copyImagesLocally(images);

                // Create boxes
                createBoxes(localImages, tesseractExe);

                logger?.Log("Box creation completed. Please check they are correct and start training with mode 'train'", true);
            } 
            else if(emode == EMode.TRAIN)
            {
                logger?.Log("Starting process of training...", true);

                var trainModel = new TrainModel();

                var localDirectoryInfo = new DirectoryInfo(".");

                trainModel.Images = retrieveTifImages(localDirectoryInfo);

                trainModel.Boxes = retrieveBoxesFile(localDirectoryInfo);

                // Creation of train file
                createTrainFile(trainModel, tesseractExe);
                trainModel.Train = retrieveTrainFiles(localDirectoryInfo);

                // Creation of unicharset file
                createUnicharset(trainModel, localDirectoryInfo);

                // Creation of font properties file
                createFontPropertiesFile(trainModel);

                // Creation of clustering file
                createClusteringFile(trainModel);

                // Creation of mftraining file
                createMfTrainingFile(trainModel);

                // Creation of cntraining file
                createCnTrainingFile(trainModel);

                // Creation of unicharambigs file
                createUnicharambigs(tesseractExe);

                // Rename all file
                renameFile(localDirectoryInfo);

                // Combine all data in order to generate final training file
                combineData();

            }
            else
            {
                throw new Exception(string.Format("Invalid mode: {0}", emode.ToString()));
            }
        }

        private List<FileInfo> retrieveTifImages(DirectoryInfo imagesDirectory)
        {
            return new List<FileInfo>(imagesDirectory.GetFiles("*.tif"));
        }

        private List<FileInfo> copyImagesLocally(List<FileInfo> images)
        {  
            var localImages = new List<FileInfo>();
            foreach(var image in images)
            {
                StringBuilder localImageName = new StringBuilder(languageName).Append(".").Append(fontName).Append(".exp").Append(images.IndexOf(image)).Append(".tif");
                string localImagePath = Path.Combine(".", localImageName.ToString());

                image.CopyTo(localImagePath, true);

                localImages.Add(new FileInfo(localImagePath));
            }

            return localImages;
        }

        private void createBoxes(List<FileInfo> images, FileInfo tesseractExe)
        {
            Process tesseractProcess = new Process();
            tesseractProcess.StartInfo.FileName = tesseractExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            tesseractProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            foreach (var image in images)
            {
                StringBuilder args = new StringBuilder(image.FullName);
                args.Append(" ");
                args.Append(image.FullName.Substring(0, image.FullName.Length - 4));
                args.Append(" batch.nochop");
                args.Append(" makebox");
                tesseractProcess.StartInfo.Arguments = args.ToString();

                tesseractProcess.Start();
                tesseractProcess.WaitForExit();
            }
        }

        private List<FileInfo> retrieveBoxesFile(DirectoryInfo boxesDirectory)
        {
            return new List<FileInfo>(boxesDirectory.GetFiles("*.box"));
        }

        private void createTrainFile(TrainModel trainModel, FileInfo tesseractExe)
        {
            logger?.Log("Generating tr file...");

            Process tesseractProcess = new Process();
            tesseractProcess.StartInfo.FileName = tesseractExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            tesseractProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            foreach (var image in trainModel.Images)
            {
                StringBuilder args = new StringBuilder(image.FullName);
                args.Append(" ");
                args.Append(image.FullName.Substring(0, image.FullName.Length - 4));
                args.Append(" box.train");
                tesseractProcess.StartInfo.Arguments = args.ToString();

                tesseractProcess.Start();
                tesseractProcess.WaitForExit();
            }

            logger?.Log("Generation of tr file completed");
        }

        private List<FileInfo> retrieveTrainFiles(DirectoryInfo trainDirectory)
        {
            return new List<FileInfo>(trainDirectory.GetFiles("*.tr"));
        }

        private void createUnicharset(TrainModel trainModel, DirectoryInfo localTempDirectory)
        {
            logger?.Log("Generating unicharset file...");

            var unicharsetExtractorExe = new FileInfo(Path.Combine(tesseractDirectory.FullName, "unicharset_extractor.exe"));

            Process unicharsetExtractorProcess = new Process();
            unicharsetExtractorProcess.StartInfo.FileName = unicharsetExtractorExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            unicharsetExtractorProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            unicharsetExtractorProcess.StartInfo.Arguments = new StringBuilder("--output_unicharset ").Append(Path.Combine(".", string.Format("{0}.unicharset", languageName))).Append(" ").ToString();
            foreach (var box in trainModel.Boxes)
            {
                StringBuilder args = new StringBuilder(box.FullName);
                args.Append(" ");
                unicharsetExtractorProcess.StartInfo.Arguments += args.ToString();      
            }
            

            unicharsetExtractorProcess.Start();
            unicharsetExtractorProcess.WaitForExit();

            trainModel.Unicharset = new FileInfo(localTempDirectory.GetFiles("*unicharset")[0].FullName);

            logger.Log("Generation of unicharset file completed");
        }

        private void createFontPropertiesFile(TrainModel trainModel)
        {
            logger.Log("Generating font properties file...");

            logger.Log(string.Format("Insert font properties parameters [{0} 0 0 0 0 0]", fontName), true);

            var fontProperties = Console.ReadLine();
            if(string.IsNullOrWhiteSpace(fontProperties))
            {
                fontProperties = string.Format("{0} 0 0 0 0 0", fontName);
            }

            string fontPropertiesPath = Path.Combine(".", string.Format("{0}.font_properties", languageName));
            var stream = new StreamWriter(fontPropertiesPath);
            stream.WriteLine(fontProperties);
            stream.Close();
            stream.Dispose();

            trainModel.FontProperties = new FileInfo(fontPropertiesPath);

            logger.Log("Generation of font properties file completed");
        }

        private void createClusteringFile(TrainModel trainModel)
        {
            logger.Log("Generating clustering file...");

            var clusteringExe = new FileInfo(Path.Combine(tesseractDirectory.FullName, "shapeclustering.exe"));

            Process shapeClusteringProcess = new Process();
            shapeClusteringProcess.StartInfo.FileName = clusteringExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            shapeClusteringProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            //shapeClusteringProcess.StartInfo.Arguments = new StringBuilder("--output_trainer ").Append(Path.Combine(LOCAL_FOLDER_TEMP, "shapetable")).Append(" ").ToString();
            shapeClusteringProcess.StartInfo.Arguments += new StringBuilder("-F ").Append(trainModel.FontProperties).Append(" ").ToString();
            shapeClusteringProcess.StartInfo.Arguments += new StringBuilder("-U ").Append(trainModel.Unicharset).Append(" ").ToString();
            foreach (var train in trainModel.Train)
            {
                StringBuilder args = new StringBuilder(train.FullName);
                args.Append(" ");
                shapeClusteringProcess.StartInfo.Arguments += args.ToString();
            }

            shapeClusteringProcess.Start();
            shapeClusteringProcess.WaitForExit();

            logger.Log("Generation of clustering file completed");
        }

        private void createMfTrainingFile(TrainModel trainModel)
        {
            logger.Log("Generating mftraining file...");

            var mFTrainingExe = new FileInfo(Path.Combine(tesseractDirectory.FullName, "mftraining.exe"));

            Process mFTrainingProcess = new Process();
            mFTrainingProcess.StartInfo.FileName = mFTrainingExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            mFTrainingProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            //mFTrainingProcess.StartInfo.Arguments = new StringBuilder("--output_trainer ").Append(LOCAL_FOLDER_TEMP).Append(" ").ToString();
            mFTrainingProcess.StartInfo.Arguments += new StringBuilder("-F ").Append(trainModel.FontProperties).Append(" ").ToString();
            mFTrainingProcess.StartInfo.Arguments += new StringBuilder("-U ").Append(trainModel.Unicharset).Append(" ").ToString();
            foreach (var train in trainModel.Train)
            {
                StringBuilder args = new StringBuilder(train.FullName);
                args.Append(" ");
                mFTrainingProcess.StartInfo.Arguments += args.ToString();
            }

            mFTrainingProcess.Start();
            mFTrainingProcess.WaitForExit();

            logger.Log("Generation of mftraining file completed");
        }

        private void createCnTrainingFile(TrainModel trainModel)
        {
            logger.Log("Generating cntraining file...");

            var mFTrainingExe = new FileInfo(Path.Combine(tesseractDirectory.FullName, "cntraining.exe"));

            Process mFTrainingProcess = new Process();
            mFTrainingProcess.StartInfo.FileName = mFTrainingExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            mFTrainingProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            //mFTrainingProcess.StartInfo.Arguments = new StringBuilder("--output_trainer ").Append(LOCAL_FOLDER_TEMP).Append(" ").ToString();
            foreach (var train in trainModel.Train)
            {
                StringBuilder args = new StringBuilder(train.FullName);
                args.Append(" ");
                mFTrainingProcess.StartInfo.Arguments += args.ToString();
            }

            mFTrainingProcess.Start();
            mFTrainingProcess.WaitForExit();

            logger.Log("Generation of cntraining file completed");
        }

        private void createUnicharambigs(FileInfo tesseractExe)
        {
            logger.Log("Generating unicharambigs file...");

            logger.Log("Are you using a version of tesseract greater than 3.03?[Yn]: ", true);
            var isTesseractGreaterThan3_03 = Console.ReadLine();

            string fontPropertiesPath = Path.Combine(".", string.Format("{0}.unicharambigs", languageName));
            var stream = new StreamWriter(fontPropertiesPath);
            if(string.IsNullOrWhiteSpace(isTesseractGreaterThan3_03) || isTesseractGreaterThan3_03 == "Y")
            {
                stream.WriteLine("v2");
            }
            else
            {
                stream.WriteLine("v1");
            }
            stream.Close();
            stream.Dispose();

            logger.Log("Generation of unicharambigs file completed");
        }

        private void renameFile(DirectoryInfo localTempDirectory)
        {
            foreach (var file in localTempDirectory.GetFiles().Where(x => !x.Name.StartsWith(languageName + ".")))
            {
                File.Move(file.Name, string.Format("{0}.{1}", languageName, file.Name), true);
            }
        }

        private void combineData()
        {
            logger.Log("Combining file...");

            var combineTessdataExe = new FileInfo(Path.Combine(tesseractDirectory.FullName, "combine_tessdata.exe"));

            Process mFTrainingProcess = new Process();
            mFTrainingProcess.StartInfo.FileName = combineTessdataExe.FullName;
            //tesseractProcess.StartInfo.RedirectStandardOutput = false;
            //tesseractProcess.StartInfo.RedirectStandardError = verbose;
            mFTrainingProcess.StartInfo.CreateNoWindow = !verbose;
            //tesseractProcess.StartInfo.UseShellExecute = false;
            mFTrainingProcess.StartInfo.Arguments = string.Format("{0}.", languageName);
            mFTrainingProcess.Start();
            mFTrainingProcess.WaitForExit();

            if(File.Exists(Path.Combine(".", string.Format("{0}.traineddata", languageName))))
            {
                logger.Log("File combined");
                File.Copy(Path.Combine(".", string.Format("{0}.traineddata", languageName)), Path.Combine("..", string.Format("{0}.traineddata", languageName)), true);
                logger.Log(string.Format("Trained data file {0} has been generated inside the directory {1}, please copy it inside your tesseract data folder!", string.Format("{0}.traineddata", languageName), new DirectoryInfo("..").FullName));
            }
        }
    }
}
