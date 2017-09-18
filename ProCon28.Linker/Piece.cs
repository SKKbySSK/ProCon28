﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCon28.Linker
{
    public class Piece : ICloneable
    {
        public PointCollection Vertexes { get; } = new PointCollection();
        public float Rotation { get; set; }

        public object Clone()
        {
            Piece p = new Piece();
            p.Vertexes.AddRange(Vertexes);
            p.Rotation = Rotation;
            return p;
        }
    }
}
