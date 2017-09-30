using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProCon28.Linker;

namespace ProCon28.Algo
{
    static class Shared
    {
        public static ObservableCollection<Piece> Pieces { get; } = new ObservableCollection<Piece>();
    }
}
