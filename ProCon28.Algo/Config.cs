using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace ProCon28.Algo
{
    public class Config
    {
        public double ItemHeight { get; set; } = 150;
        public double ItemWidth { get; set; } = 150;
        public string TCP_IP { get; set; } = "";
        public int TCP_Port { get; set; } = 50000;
        public double Threshold { get; set; } = 0.01;
        public string LastFilePath { get; set; } = "Pieces.pbin";

        public int MT_Limitation { get; set; } = 50;
        public int MT_Retry { get; set; } = 100;

        static Lazy<Config> config = new Lazy<Config>(() =>
        {
            if (File.Exists(Linker.Constants.AlgoConfigFileName))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Config));
                    using (StreamReader sr = new StreamReader(Linker.Constants.AlgoConfigFileName))
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

        public static (bool, Exception) Save()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                using (StreamWriter sw = new StreamWriter(Linker.Constants.AlgoConfigFileName))
                {
                    ser.Serialize(sw, Current);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex);
            }
        }

        public static Config Current
        {
            get { return config.Value; }
        }
    }
}
