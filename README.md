# TesseractEasyTrainer
An easy project created to simplify the procedure of training Tesseract, one of the most powerfull and free OCR software. It automatically create all necessary file in order to create the final trained file.

### Prerequisites

.NET Core >= 3.1

### Releases
##### Current Version
[1.0.1](pub-releases/1.0.1.zip)

##### Old Version
[1.0.0](pub-releases/1.0.0.zip)

### How use it
Refer to Contributing section in order to get the link of original guide.
First you need to create at least one TIF file, which include all the chars you have to recognize. An example can be found in the repository (input_1.tif). More file you have and more the training will be accurate. 
Copy al your TIF file inside a folder.

The training process is based on 2 main steps:
  - Creation of BOX file
  - Train using the created BOX file
  
In order to generate the BOX file run this command:
```properties
  .\TesseractEasyTrainer.exe --tess-path "[TESSERACT_INSTALL_DIRECTORY]" -m createbox --images-directory [IMAGES_TIF_DIRECTORY] -l eng -f mystrangefont -v
```
After this command, will be created a local folder (in the same directory where you placed TesseractEasyTrainer.exe) which contains a renamed version of your TIF file and some BOX file (the number of BOX file it the same of TIF file). Now you have check that the BOX file are correct. In order to analyze them you can use a tool like jTessboxedit which you can find here:
http://vietocr.sourceforge.net/training.html
With this tool you have to find the correct path of each char for every TIF file.

Once you are sure the BOX file are correct you can procede with the second and final step of training. In order to do this run the following command:
```properties
  .\TesseractEasyTrainer.exe --tess-path "[TESSERACT_INSTALL_DIRECTORY]" -m train --images-directory [IMAGES_TIF_DIRECTORY] -l eng -f mystrangefont -v
```
  
At the end of this process you will (in the same directory of the application) a file called [lng].traineddata, this is the file of training, copy it inside your tessdata folder in order to use it. 
  
-l: specify the language name

-f: specify the font name

## Example
Tif image example for training
![Tif image](example/input_1.tif)

## Built With

* .NET Core - C#

## Contributing

Thanks to Mattias Henell which wrote this amazing guide 
https://medium.com/apegroup-texts/training-tesseract-for-labels-receipts-and-such-690f452e8f79


## Authors

* **Creti Federico - TheFedex87** - *Application development*

## License

This project is licensed under the OpenSource license

