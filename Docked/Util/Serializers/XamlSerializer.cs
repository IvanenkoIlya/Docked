using MongoDB.Bson.Serialization;
using System;
using System.Text;
using System.Windows.Markup;
using System.Xml;

namespace Docked.Util.Serializers
{
   public class XamlSerializer<T> : IBsonSerializer
   {
      public Type ValueType => typeof(T);

      public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
      {
         context.Writer.WriteStartDocument();
         context.Writer.WriteName("xaml");
         context.Writer.WriteString(XamlSerialize(value));
         context.Writer.WriteEndDocument();
      }

      public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
      {
         context.Reader.ReadStartDocument();
         string xamlString = context.Reader.ReadString();
         context.Reader.ReadEndDocument();
         return XamlDeserialize(xamlString);
      }

      private string XamlSerialize(object value)
      {
         var settings = new XmlWriterSettings
         {
            Indent = true,
            NewLineOnAttributes = true,
            ConformanceLevel = ConformanceLevel.Fragment
         };

         var sb = new StringBuilder();
         var writer = XmlWriter.Create(sb, settings);

         var manager = new XamlDesignerSerializationManager(writer);
         manager.XamlWriterMode = XamlWriterMode.Expression;
         XamlWriter.Save(value, manager);

         return sb.ToString();
      }

      private object XamlDeserialize(string xamlText)
      {
         var doc = new XmlDocument();
         doc.LoadXml(xamlText);
         return XamlReader.Load(new XmlNodeReader(doc));
      }
   }
}
