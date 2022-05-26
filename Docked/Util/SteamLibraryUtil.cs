using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Docked.Util
{
   public class SteamLibraryUtil
   {
      private static string downloadUrlBase = "https://steamcdn-a.akamaihd.net/steam/apps/";

      public static string GetSteamDirectory()
      {
         string steamDirectory = "";

         using(RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
         {
            if(key != null)
            {
               steamDirectory = key.GetValue("SteamPath").ToString().Replace("/", @"\");
            }
         }

         return steamDirectory;
      }

      public static Dictionary<int, string> GetInstalledSteamAppIds(string steamInstallLocation)
      {
         List<string> acfFiles = Directory.EnumerateFiles(Path.Combine(steamInstallLocation, "steamapps"))
            .Where(f => Regex.Match(Path.GetFileName(f), @"appmanifest_(\d*)\.acf").Success).ToList();

         Dictionary<int, string> results = new Dictionary<int, string>();

         foreach(string file in acfFiles)
         {
            results.Add(int.Parse(Regex.Match(Path.GetFileName(file), @"appmanifest_(\d*).acf").Groups[1].Value), file);
         }

         // Steamworks Common Redistributables, not actually a game
         results.Remove(228980);
         return results;
      }

      public static string GetSteamAppNameFromAcf(string file)
      {
         using(StreamReader sr = new StreamReader(file))
         {
            string line;

            while ((line = sr.ReadLine()) != null)
            {
               if (line.Contains("\"name\"")) {
                  string[] parts = line.Split('\"');
                  return parts[3];
               }
            }
         }

         return "";
      }

      public static string GetSteamHeaderImageLink(int appID)
      {
         return downloadUrlBase + appID + "/header.jpg";
      }

      public static string GetSteamLogoLink(int appID)
      {
         return downloadUrlBase + appID + "/logo.png";
      }

      public static string GetSteamLibraryLink(int appID)
      {
         return downloadUrlBase + appID + "/library_600x900.jpg";
      }

      /*
       https://steamcdn-a.akamaihd.net/steam/apps/977950/header.jpg
https://steamcdn-a.akamaihd.net/steam/apps/977950/logo.png
https://steamcdn-a.akamaihd.net/steam/apps/977950/library_hero.jpg
https://steamcdn-a.akamaihd.net/steam/apps/977950/library_600x900.jpg (actually 300x450)
https://steamcdn-a.akamaihd.net/steam/apps/977950/page_bg_generated.jpg
https://steamcdn-a.akamaihd.net/steam/apps/977950/page_bg_generated_v6b.jpg
       */
   }
}
