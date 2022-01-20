using System.Drawing;
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
   }
}
