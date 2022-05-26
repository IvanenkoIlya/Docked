using IWshRuntimeLibrary;

namespace Docked.Util
{
   public class WshShellUtil
   {
      public static string GetLinkPath(string file)
      {
         WshShell shell = new WshShell();
         IWshShortcut link = (IWshShortcut)shell.CreateShortcut(file);

         return link.TargetPath;
      }

      public static string GetLinkArguments(string file)
      {
         WshShell shell = new WshShell();
         IWshShortcut link = (IWshShortcut)shell.CreateShortcut(file);

         return link.Arguments;
      }
   }
}
