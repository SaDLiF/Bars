using System;
using System.IO;
using System.Xml.Serialization;

namespace ak_Bars.Settings
{
    [Serializable]
    public class Config
    {
        public string Token { get; set; }

        public Config()
        { }

        public Config(string token)
        {
            Token = token;
        }

        public static Config Read()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(Config));

            // десериализация
            try
            {
                using (FileStream fs = new FileStream("setting.xml", FileMode.OpenOrCreate))
                {
                    Config newPerson = (Config)formatter.Deserialize(fs);
                    return newPerson;
                }
            }
            catch
            {
                return new Config();
            }

        }
        public static void Write(string token)
        {
            Config conf = new Config(token);
            XmlSerializer formatter = new XmlSerializer(typeof(Config));

            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream("setting.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, conf);
            }
        }
    }
}
