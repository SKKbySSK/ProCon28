using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ProCon28.OpenCV
{
    public interface ICamera : IDisposable
    {
        void Begin();
        void Stop();
        Mat RetrieveMat(bool UseFilters);
        IList<Action<Mat>> Interruptions { get; }
        IList<Func<Mat, Mat>> Filters { get; }
    }
}
