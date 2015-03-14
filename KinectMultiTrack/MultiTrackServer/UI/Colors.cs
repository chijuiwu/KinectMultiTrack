using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KinectMultiTrack.UI
{
    internal class Colors
    {
        // Max 6 people
        private const double SKELETON_THICKNESS = 6;
        public static readonly List<Pen> SKELETON = new List<Pen>(){
            new Pen(Brushes.Red, SKELETON_THICKNESS),
            new Pen(Brushes.Orange, SKELETON_THICKNESS),
            new Pen(Brushes.Green, SKELETON_THICKNESS),
            new Pen(Brushes.Blue, SKELETON_THICKNESS),
            new Pen(Brushes.Indigo, SKELETON_THICKNESS),
            new Pen(Brushes.Violet, SKELETON_THICKNESS),
        };

        // Background
        public static readonly Brush BACKGROUND_BRUSH = Brushes.Black;

        // Joints
        public const double JOINT_THICKNESS = 3;
        public static readonly Brush TRACKED_JOINT_BRUSH = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        public static readonly Brush INFERRED_JOINT_BRUSH = Brushes.Yellow;

        // Bones
        public static readonly Pen TRACKED_BONE_PEN = new Pen(Brushes.Blue, 6);
        public static readonly Pen INFERRED_BONE_PEN = new Pen(Brushes.Gray, 1);
        public static readonly Pen AVG_BONE_PEN = new Pen(Brushes.White, 6);
    }
}
