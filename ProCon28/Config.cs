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
        public double PieceApprox { get; set; } = CameraParams.PieceApprox;
        public double SquareApprox { get; set; } = CameraParams.SquareApprox;
        public double MinimumArea { get; set; } = CameraParams.MinimumArea;
        public double SquareMaximumArcLength { get; set; } = CameraParams.SquareMaximumArcLength;
        public double Gamma { get; set; } = CameraParams.Gamma;

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
            if (Instance.SaveConfig)
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
                catch (Exception ex)
                {
                    return (false, ex);
                }
            }
            else
                return (true, null);
        }

        public static Config Current
        {
            get { return config.Value; }
        }
    }
}
