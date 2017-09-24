using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker
{
    public static class Constants
    {
        public const string ConfigFileName = "ProConfig.xml";
        public const string RemotePiecesUri = "Pieces";
        public const string RemoteRecognizerUri = "Recognized";

        /// <summary>
        /// 最大のピース数
        /// </summary>
        public const int MaximumPiece = 50;

        /// <summary>
        /// ピースの最大頂点数
        /// </summary>
        public const int MaximumVertex = 16;
    }
}
