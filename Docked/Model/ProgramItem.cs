using Docked.Util;
using Docked.Util.MongoDB;
using Docked.Util.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MoreLinq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Drawing = System.Drawing;

namespace Docked.Model
{
   [BsonIgnoreExtraElements]
   public class ProgramItem : INotifyPropertyChanged
   {
      private string _programName;
      private string _executeCommand;
      private Brush _backgroundColor = new SolidColorBrush(Colors.DarkGray);

      #region Properties
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
         Tags = new ObservableCollection<string>();
         Icon = new ProgramItemIcon();
      }

      public ProgramItem(string programName, string executeCommand, params string[] tags) : this()
      {
         ProgramName = programName;
         ExecuteCommand = executeCommand;
         tags.ForEach(x => Tags.Add(x));
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

      [BsonId]
      public ObjectId Id { get; private set; }
      [BsonElement("fileLocation")]
      public string FileLocation { get; private set; }
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
         Icon = DefaultIcon;
      }

      [BsonConstructor]
      public ProgramItemIcon(ObjectId id, string fileLocation)
      {
         FileLocation = fileLocation;
         Id = id;
         LoadIconFromDB();
      }

      public void SetIcon(string iconLocation)
      {
         FileLocation = iconLocation;
         Icon = ImageUtil.ImageToByteArray(Drawing.Image.FromFile(FileLocation));
      }

      public void LoadIconFromDB()
      {
         Icon = MongoDBManager.Instance.IconBucket.DownloadAsBytes(Id);
      }

      public void SaveIconToDB()
      {
         if (Id != DefaultIconId)
            Id = MongoDBManager.Instance.IconBucket.UploadFromBytes(FileLocation, Icon);
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
