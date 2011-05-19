using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EvoX.ResourceLibrary
{
    public class ImageConverter
    {
        public static ImageSource BitmapToWPFImage(object image)
        {
            if (image == null)
                return null;
            MemoryStream ms = new MemoryStream();
            if (image is Bitmap)
            {
                ((Bitmap)image).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            }
            if (image is Icon)
            {
                ((Icon)image).Save(ms);
            }
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            return bitmapImage;
        }

    }
}