using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing;
using System.Security;
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Xml;
using Ookii;


namespace FromShizzleToDizzleConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<string> SupportedFiles = new List<string>();
        public List<string> UnsupportedFiles = new List<string>();
        public System.Drawing.Imaging.ImageFormat Format = System.Drawing.Imaging.ImageFormat.Jpeg;
        public bool OverWrite = false;
        public string LogFile = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + "-" + "log.xml";
        public string SaveLocation = null;
        

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Enables the user to browse filesystem and select images to work with
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "Images (*.BMP;*.JPG;*.Jpeg;*.GIF;*.PNG;*.Tiff)|*.BMP;*.JPG;*.Jpeg;*.GIF;*.PNG;*.Tiff|" +
                                "All files (*.*)|*.*";
            fileDialog.Multiselect = true;
            SupportedFiles = new List<string>();
            Nullable<bool> result = fileDialog.ShowDialog();

            if (result == true)
            {
                foreach (string name in fileDialog.FileNames)
                {
                    if (IsFormatSupported(name))
                    {
                        SupportedFiles.Add(name);
                    }
                    else
                    {
                        UnsupportedFiles.Add(name);
                    }
                }

                listBox1.ItemsSource = SupportedFiles;
            }
            
        }

        /// <summary>
        /// Converts selected images from "SupportedFiles" list to selected format stored in "Format"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Convert_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SupportedFiles.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No supported files selected", "No files", MessageBoxButtons.OK);
                return;
            }

            if (OverWrite)
            {

            }
            else
            {
                if (System.Windows.Forms.MessageBox.Show("Overwrite existing files?", "Overwrite", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    return;
            }

            Converter converter = new Converter(Format, SaveLocation);

            using (StreamWriter w = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                w.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                w.WriteLine("<Log>");
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;

            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                int counter = 1;
                foreach (string file in SupportedFiles)
                {
                    converter.ImagePath = file;
                    converter.Convert();
                    //doneLabel.Content = "Processing image " + counter;
                    b.ReportProgress((int)(counter/((SupportedFiles.Count)/100.0)));
                    Thread.Sleep(100);
                    counter++;
                    WriteLogInformation("Converting to " + Format.ToString(), file);
                }
                    //doneLabel.Content = "Converting Done";
            });

            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                doneLabel.Content = string.Format("{0}% Completed", args.ProgressPercentage);
                progressBar1.Value = args.ProgressPercentage;
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                doneLabel.Content = "Converting Done";
                using (StreamWriter w = new StreamWriter(LogFile, true, Encoding.UTF8))
                {
                    w.WriteLine("</Log>");
                }
            });

            bw.RunWorkerAsync();

            if (UnsupportedFiles.Count != 0)
            {
                UnsupportedFilesMessage();
            }
        }

        /// <summary>
        /// Shows a message to the user if some of the files he provided are not supported to work with
        /// </summary>
        public void UnsupportedFilesMessage()
        {
            string files = "";
            foreach (string file in UnsupportedFiles)
            {
                files += file + "\n";
            }
            System.Windows.Forms.MessageBox.Show("Following files are not supported and were not processed:\n" + files, "Unsupported files", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Checks whether user selected file is of supported format
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>True if format is supported, false otherwise</returns>
        private bool IsFormatSupported(string path)
        {

           try
           {
               BitmapImage newImage = new BitmapImage(new Uri(path));
           }
           catch (NotSupportedException)
           {
               // System.NotSupportedException:
               // No imaging component suitable to complete this operation was found.
               return false;
           }
           return true;
        }

        /// <summary>
        /// Resizes selected images from "SupportedFiles" list to selected values "WidthTextBox" and "HeightTextBox"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resize_Button_Click(object sender, RoutedEventArgs e)
        {
            if (SupportedFiles.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No files selected", "No files", MessageBoxButtons.OK);
                return;
            }



            Resizer resizer = new Resizer(SaveLocation);
            
            try
            {
                resizer.Width = int.Parse(WidthTextBox.Text);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Invalid Width");
                return;
            }

            try
            {
                resizer.Height = int.Parse(HeightTextBox.Text);
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("Invalid Height");
                return;
            }

            if (!OverWrite && !(System.Windows.Forms.MessageBox.Show("Do you want to overwrite your files?", "Overwrite", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes))
            {
                return;
            }

            string badFiles = resizer.CheckRatio(SupportedFiles);
            if (badFiles.Length != 0)
            {
                if (!(System.Windows.Forms.MessageBox.Show("Following files do not have same ratio as you specified, resize anyways?\n" + badFiles, "Resize files?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes))
                {
                    return;
                }
            }

            using (StreamWriter w = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                w.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                w.WriteLine("<Log>");
            }
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(
            delegate(object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                int counter = 1;
                foreach (string file in SupportedFiles)
                {
                    resizer.ImagePath = file;
                    resizer.Resize();
                    b.ReportProgress((int)(counter / ((SupportedFiles.Count) / 100.0)));
                    Thread.Sleep(100);
                    counter++;
                    WriteLogInformation("Resizing", file);
                }
                //doneLabel.Content = "Resizing Done";
            });


            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate(object o, ProgressChangedEventArgs args)
            {
                doneLabel.Content = string.Format("{0}% Completed", args.ProgressPercentage);
                progressBar1.Value = args.ProgressPercentage;
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate(object o, RunWorkerCompletedEventArgs args)
            {
                doneLabel.Content = "Resizing Done";
                using (StreamWriter w = new StreamWriter(LogFile, true, Encoding.UTF8))
                {
                    w.WriteLine("</Log>");
                }
            });

            bw.RunWorkerAsync();
            

            if (UnsupportedFiles.Count != 0)
            {
                UnsupportedFilesMessage();
            }

            
        }


        /// <summary>
        /// Writes log information when imae is being processed
        /// </summary>
        /// <param name="activity">Activity - converting/resizing</param>
        /// <param name="filename">Filename of currently processed image</param>
        private void WriteLogInformation(string activity, string filename)
        {
            StringBuilder sbuilder = new StringBuilder();
            using (StringWriter sw = new StringWriter(sbuilder))
            {
                using (XmlTextWriter w = new XmlTextWriter(sw))
                {
                    w.WriteStartElement("LogInfo");
                    w.WriteString(Environment.NewLine + "  ");
                    w.WriteElementString("Time", DateTime.Now.ToString());
                    w.WriteString(Environment.NewLine + "  ");
                    w.WriteElementString("Activity", activity);
                    w.WriteString(Environment.NewLine + "  ");
                    w.WriteElementString("Filename", filename);
                    w.WriteString(Environment.NewLine);
                    w.WriteEndElement();
                }
            }
            using (StreamWriter w = new StreamWriter(LogFile, true, Encoding.UTF8))
            {
                w.WriteLine(sbuilder.ToString());
            }
        }

        

        //Radio buttons methods for image format selection

        /// <summary>
        /// BMP format is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_CheckedBMP(object sender, RoutedEventArgs e)
        {
            Format = System.Drawing.Imaging.ImageFormat.Bmp;
        }

        /// <summary>
        /// PNG format is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_CheckedPNG(object sender, RoutedEventArgs e)
        {
            Format = System.Drawing.Imaging.ImageFormat.Png;
        }

        /// <summary>
        /// JPEG format is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_CheckedJPEG(object sender, RoutedEventArgs e)
        {
            Format = System.Drawing.Imaging.ImageFormat.Jpeg;
        }

        /// <summary>
        /// Tiff format is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_CheckedTiff(object sender, RoutedEventArgs e)
        {
            Format = System.Drawing.Imaging.ImageFormat.Tiff;
        }

        /// <summary>
        /// GIF format is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_CheckedGIF(object sender, RoutedEventArgs e)
        {
            Format = System.Drawing.Imaging.ImageFormat.Gif;
        }


                
        /// <summary>
        /// If checked, automaticly overwrite existing files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Checkbox1.IsChecked = true;
            OverWrite = true;
        }

        /// <summary>
        /// If unchecked, ask user to overwrite/not overwrite existing files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked_Unchecked(object sender, RoutedEventArgs e)
        {
            Checkbox1.IsChecked = false;
            OverWrite = false;
        }

        /// <summary>
        /// User may specify save directory where to save processed images, if he does not, current directory is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
            {
                SaveLocation = dialog.SelectedPath;
                PathTextBox.Text = dialog.SelectedPath;
            }
        }
    }
}
