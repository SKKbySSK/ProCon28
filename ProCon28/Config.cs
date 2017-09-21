using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace ProCon28
{
    public class Config
    {
        public int TCP_Port { get; set; } = 50000;

        static Lazy<Config> config = new Lazy<Config>(() =>
        {
            if (File.Exists(Linker.Constants.ConfigFileName))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Config));
                    using (StreamReader sr = new StreamReader(Linker.Constants.ConfigFileName))
                    {
                        return (Config)ser.Deserialize(sr);
                    }
                }
                catch (Exception)
                {
                    return new Config();
                }
            }
            else
                return new Config();
        });

        public static void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Config));
            using(StreamWriter sw= new StreamWriter(Linker.Constants.ConfigFileName))
            {
                ser.Serialize(sw, Current);
            }
        }

        public static Config Current
        {
            get { return config.Value; }
        }
    }
}
