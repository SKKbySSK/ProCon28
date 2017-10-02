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

        public int Camera { get; set; } = 0;
        public double PieceApprox { get; set; } = 0.01;
        public double SquareApprox { get; set; } = 0.02;
        public double MinimumArea { get; set; } = 700;
        public double SquareMaximumArcLength { get; set; } = 80;
        public int ContourIndex { get; set; } = 1;
        public double Gamma { get; set; } = 1.0;

        public double BlurThreshold { get; set; } = 10;
        public double StraightThreshold { get; set; } = 0.1;
        public bool ClockwiseSort { get; set; } = true;
        public string LastFileName { get; set; } = "result.txt";

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

        public static (bool, Exception) Save()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                using (StreamWriter sw = new StreamWriter(Linker.Constants.ConfigFileName))
                {
                    ser.Serialize(sw, Current);
                }

                return (true, null);
            }
            catch(Exception ex)
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
