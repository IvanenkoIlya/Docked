using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Docked.Util
{
   public class ImageUtil
   {
      public static byte[] ImageToByteArray(Image imageIn)
      {
         using (MemoryStream ms = new MemoryStream())
         {
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
         }
      }

      public static Image ByteArrayToImage(byte[] byteArrayIn)
      {
         Image ret;
         using (MemoryStream ms = new MemoryStream(byteArrayIn))
            ret = Image.FromStream(ms);
         return ret;
      }

      public static string ImageFilter = "";

      static ImageUtil()
      {
         ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
         string sep = string.Empty;
         foreach (ImageCodecInfo codecInfo in codecs)
         {
            string codecName = codecInfo.CodecName.Substring(8).Replace("Codec", "Files").Trim();
            ImageFilter = $"{ImageFilter}{sep}{codecName} ({codecInfo.FilenameExtension})|{codecInfo.FilenameExtension}";
            sep = "|";
         }
      }
   }
}
