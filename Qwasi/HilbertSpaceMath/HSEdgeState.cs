using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qwasi.GraphUI;

namespace Qwasi.HilbertSpaceMath
{
    public struct HSEdgeState
    {
        public IQGVertex Vertex1 { get; }
        public IQGVertex Vertex2 { get; }

        public override string ToString()
        {
            return "|" + this.Vertex1.GlobalID + ", " + this.Vertex2.GlobalID + ">";
        }

        public HSEdgeState(IQGVertex v1, IQGVertex v2)
        {
            this.Vertex1 = v1;
            this.Vertex2 = v2;
        }
    }
}
