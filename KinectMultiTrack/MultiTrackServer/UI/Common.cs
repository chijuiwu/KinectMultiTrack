using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KinectMultiTrack.UI
{
    internal class Common
    {
        private const double PEN_THICKNESS = 6;

        // Max 6 people
        public static List<Pen> PersonColors = new List<Pen>(){
            new Pen(Brushes.Red, PEN_THICKNESS),
            new Pen(Brushes.Orange, PEN_THICKNESS),
            new Pen(Brushes.Green, PEN_THICKNESS),
            new Pen(Brushes.Blue, PEN_THICKNESS),
            new Pen(Brushes.Indigo, PEN_THICKNESS),
            new Pen(Brushes.Violet, PEN_THICKNESS),
        };
    }
}
