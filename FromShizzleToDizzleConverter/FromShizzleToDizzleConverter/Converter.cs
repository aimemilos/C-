using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace FromShizzleToDizzleConverter
{
    class Converter
    {
        /// <summary>
        /// Source image file
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// Target image format
        /// </summary>
        public System.Drawing.Imaging.ImageFormat ImageFormat { get; set; }

        /// <summary>
        /// Path to target save directory
        /// </summary>
        public String SaveLocation { get; set; }

        /// <summary>
        /// Initializes ImageFormat and SaveLocation
        /// </summary>
        /// <param name="format">Target image format</param>
        /// <param name="SaveLocation">Target save directory</param>
        public Converter(System.Drawing.Imaging.ImageFormat format, String SaveLocation)
        {
            ImageFormat = format;
            this.SaveLocation = SaveLocation;
        }

        /// <summary>
        /// Converts the image from ImagePath to specified ImageFormat
        /// </summary>
        public void Convert()
        {
            Image image = Image.FromFile(ImagePath);
            string newPath = ChangeName(ImagePath);
            if (System.IO.File.Exists(newPath))
            {
                return;
            }
            
            image.Save(newPath, ImageFormat);
        }

        
        /// <summary>
        /// Renames the file extension of file being converted
        /// </summary>
        /// <param name="name">The name of the image being converted</param>
        /// <returns>New full name of the image with its new extension</returns>
        public string ChangeName(string name)
        {
            if (!name.Contains("."))
            {
                name += "." + ImageFormat.ToString();
            }
            else
            {
                name = name.Substring(0, name.LastIndexOf(".") + 1);
                name += ImageFormat.ToString();
                if (SaveLocation != null)
                {
                    string fileName = name.Substring(name.LastIndexOf("\\")+1, name.Length - (name.LastIndexOf("\\")+1));
                    name = SaveLocation + "\\" + fileName;
                }
            }

            return name;
        }
    }
}
