using System;
using System.Collections.Generic;
using System.Drawing;
using Priority_Queue;

namespace ImageEffects
{
    public class Pixel : StablePriorityQueueNode
    {
        public Pixel(int x, int y, double intensity, Pixel parent)
        {
            X = x;
            Y = y;
            Intensity = intensity;
            Parent = parent;
        }

        public Pixel(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
        }

        public int X { get; }
        public int Y { get; }

        public byte R { get; private set; }
        public byte G { get; private set; }
        public byte B { get; private set; }

        public double Intensity { get; set; }

        public double IntensitySum { get; set; } = Double.MaxValue;

        public bool Marked { get; set; }
        
        public Pixel Parent { get; set; }

        public Color Color
        {
            get
            {
                return Color.FromArgb(R, G, B);
            }
            set
            {
                R = value.R;
                G = value.G;
                B = value.B;   
            }
        }

        public override string ToString()
        {
            return string.Format("Pixel({0}, {1}, {2})", R, G, B);
        }
    }
}