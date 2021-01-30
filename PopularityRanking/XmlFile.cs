using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PopularityRanking
{
    public static class XmlFile
    {
        public static T Import<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {
                Export(path, new T());
            }

            using StreamReader reader = new StreamReader(path);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);
        }

        public static void Export<T>(string path, T item)
        {
            using StreamWriter writer = new StreamWriter(path);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, item);
        }
    }
}
