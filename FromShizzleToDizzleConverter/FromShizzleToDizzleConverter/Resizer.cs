using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromShizzleToDizzleConverter
{
    class Resizer
    {
        /// <summary>
        /// Source image file
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// User specified width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// User specified Height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Path to target save directory
        /// </summary>
        public String SaveLocation { get; set; }

        /// <summary>
        /// Initializes SaveLocation
        /// </summary>
        /// <param name="SaveLoaction">Target save directory</param>
        public Resizer(String SaveLoaction)
        {
            this.SaveLocation = SaveLoaction;
        }

        /// <summary>
        /// Resizes the image specified by ImagePath to Width and Height
        /// </summary>
        public void Resize()
        {
            FileStream fs = new FileStream(ImagePath, FileMode.Open);
            Image image = Image.FromStream(fs);
            
            var destRect = new Rectangle(0, 0, Width, Height);
            var destImage = new Bitmap(Width, Height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            fs.Close();
            Bitmap b = new Bitmap(destImage);
            
            if (SaveLocation != null)
            {
                string fileName = ImagePath.Substring(ImagePath.LastIndexOf("\\") + 1, ImagePath.Length - (ImagePath.LastIndexOf("\\") + 1));
                ImagePath = SaveLocation + "\\" + fileName;
            }

            b.Save(ImagePath);
            return;
        }

        /// <summary>
        /// Checks whether the ratios of images are same as user specified Width/Height ratio
        /// </summary>
        /// <param name="files">List of files that have to be resized</param>
        /// <returns>String of files that don't have the user specified ratio</returns>
        public string CheckRatio(List<string> files)
        {
            string badFiles = "";
            System.Drawing.Image img;
            foreach (string file in files)
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                img = System.Drawing.Image.FromStream(fs);
                if ((double)img.Height / img.Width != (double)Height / Width)
                    badFiles += file + "\n";
                fs.Close();
            }

            return badFiles;
        }
    }
}
