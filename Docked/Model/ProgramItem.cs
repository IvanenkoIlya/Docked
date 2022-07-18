using Docked.Util;
using Docked.Util.MongoDB;
using Docked.Util.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MoreLinq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Drawing = System.Drawing;

namespace Docked.Model
{
   [BsonIgnoreExtraElements]
   public class ProgramItem : INotifyPropertyChanged
   {
      public static int Instances = 0;
      private static Random rand = new Random();

      private string _programName;
      private string _executeCommand;
      private string _argumentParameters;
      private Brush _backgroundColor = new SolidColorBrush(Colors.DarkGray);

      #region Properties
      [BsonId]
      public ObjectId Id { get; private set; }

      [BsonElement("program_name")]
      public string ProgramName
      {
         get => _programName;
         set
         {
            _programName = value;
            OnPropertyChanged();
         }
      }

      [BsonElement("execute_command")]
      public string ExecuteCommand
      {
         get => _executeCommand;
         set
         {
            _executeCommand = value;
            OnPropertyChanged();
         }
      }

      [BsonElement("argument_parameters")]
      public string ArgumentParameters
      {
         get => _argumentParameters;
         set
         {
            _argumentParameters = value;
            OnPropertyChanged();
         }
      }

      [BsonElement("tags")]
      public ObservableCollection<string> Tags { get; private set; }

      [BsonElement("icon")]
      public ProgramItemIcon Icon { get; private set; }

      [BsonElement("background_color")]
      [BsonSerializer(typeof(XamlSerializer<Brush>))]
      public Brush BackgroundColor
      {
         get => _backgroundColor;
         set
         {
            _backgroundColor = value;
            OnPropertyChanged();
         }
      }
      #endregion

      public ProgramItem()
      {
         Instances++;
         Tags = new ObservableCollection<string>();
         Icon = new ProgramItemIcon();
         BackgroundColor = RandomColorBrush();
      }

      public ProgramItem(string programName, string executeCommand, params string[] tags) : this()
      {
         ProgramName = programName;
         ExecuteCommand = executeCommand;
         tags.ForEach(x => Tags.Add(x));
      }

      public static ProgramItem FromFile(string file)
      {
         var retVal = new ProgramItem();
         if (File.Exists(file))
         {
            retVal.ProgramName = Path.GetFileNameWithoutExtension(file);
            retVal.ExecuteCommand = GetExecuteCommandFromFile(file);
            retVal.Icon = new ProgramItemIcon(Path.GetFileName(file), IconFromFile(file));
            if (file.EndsWith(".lnk"))
               retVal.ArgumentParameters = WshShellUtil.GetLinkArguments(file);
         }
         return retVal;
      }

      private static string GetExecuteCommandFromFile(string file)
      {
         var type = file.Substring(file.LastIndexOf('.'));
         switch (type)
         {
            case ".exe":
               return file;
            case ".url":
               return ExecuteCommandFromUrl(file);
            case ".lnk":
               return WshShellUtil.GetLinkPath(file);
         }
         return "";
      }

      private static string ExecuteCommandFromUrl(string file)
      {
         var url = File.ReadLines(file).First(x => x.StartsWith("URL="));
         return url.Substring(4);
      }

      private static byte[] IconFromFile(string file)
      {
         var bitmap = Drawing.Icon.ExtractAssociatedIcon(file).ToBitmap();
         Drawing.ImageConverter converter = new Drawing.ImageConverter();
         return (byte[])converter.ConvertTo(bitmap, typeof(byte[]));

         //using (MemoryStream memoryStream = new MemoryStream())
         //{
         //   Drawing.Icon.ExtractAssociatedIcon(file).Save(memoryStream);
         //   return memoryStream.ToArray();
         //}
      }

      private static SolidColorBrush RandomColorBrush()
      {
         byte[] rgb = new byte[3];
         rand.NextBytes(rgb);
         return new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
      }

      public void SetIcon(string iconLocation)
      {
         if (!File.Exists(iconLocation))
            throw new FileNotFoundException("No icon file found at location", iconLocation);
         Icon.SetIcon(iconLocation);
      }

      public override string ToString()
      {
         return $"{ProgramName}\n\tCommand=\"{ExecuteCommand}\"\n\tBackgroundColor={BackgroundColor}\n\tTags=({string.Join(",", Tags)})";
      }

      #region INotifyPropertyChanged implementation
      public event PropertyChangedEventHandler PropertyChanged;

      private void OnPropertyChanged([CallerMemberName] string prop = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      #endregion
   }

   public class ProgramItemIcon : INotifyPropertyChanged
   {
      public static string defaultIconName = "defaultIcon";
      public static ObjectId DefaultIconId;
      public static byte[] DefaultIcon;

      private byte[] _icon;
      private bool _dirty;

      [BsonId]
      public ObjectId Id { get; private set; }
      [BsonElement("fileName")]
      public string FileName { get; private set; }
      [BsonIgnore]
      public byte[] Icon
      {
         get => _icon;
         set
         {
            _icon = value;
            OnPropertyChanged();
         }
      }

      static ProgramItemIcon()
      {
         FilterDefinition<GridFSFileInfo> filter = Builders<GridFSFileInfo>.Filter.Where(x => x.Filename == defaultIconName);
         GridFSFileInfo defaultFileInfo = MongoDBManager.Instance.IconBucket.Find(filter).FirstOrDefault();
         DefaultIconId = defaultFileInfo.Id;
         DefaultIcon = MongoDBManager.Instance.IconBucket.DownloadAsBytes(DefaultIconId);
      }

      public ProgramItemIcon()
      {
         Id = DefaultIconId;
         FileName = defaultIconName;
         Icon = DefaultIcon;
      }

      public ProgramItemIcon(string fileName, byte[] image)
      {
         Id = ObjectId.GenerateNewId();
         FileName = fileName;
         Icon = image;
         _dirty = true;
      }

      [BsonConstructor]
      public ProgramItemIcon(ObjectId id, string fileName)
      {
         Id = id;
         FileName = fileName;
         try
         {
            Icon = MongoDBManager.Instance.IconBucket.DownloadAsBytes(Id);
         }
         catch // if some issue occurs, set default instead
         {
            Id = DefaultIconId;
            FileName = defaultIconName;
            Icon = DefaultIcon;
         }
      }

      public void SetIcon(string iconLocation)
      {
         Id = ObjectId.GenerateNewId();
         FileName = iconLocation;
         Icon = ImageUtil.ImageToByteArray(Drawing.Image.FromFile(FileName));
         _dirty = true;
      }

      public void SaveIconToDB()
      {
         if (Id != DefaultIconId && _dirty)
            Id = MongoDBManager.Instance.IconBucket.UploadFromBytes(FileName, Icon);
      }

      #region INotifyPropertyChanged implementation
      public event PropertyChangedEventHandler PropertyChanged;

      private void OnPropertyChanged([CallerMemberName] string prop = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
      }
      #endregion
   }
}
