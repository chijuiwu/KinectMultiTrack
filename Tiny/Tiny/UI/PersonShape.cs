using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tiny.UI
{
    class PersonShape
    {
        public Pen Pen { get; private set; }
        public IEnumerable<SkeletonShape> SkeletonSketches { get; private set; }

        public PersonShape(Pen pen, IEnumerable<SkeletonShape> skeletonSketches)
        {
            this.Pen = pen;
            this.SkeletonSketches = skeletonSketches;
        }
    }
}
