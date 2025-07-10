using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionProject
{
    public class HuffmanNode : IComparable<HuffmanNode>
    {
        public char? Character { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public double X { get; set; } // Drawing X position - Tree Animation
        public double Y { get; set; } // Drawing Y position - Tree Animation
        public HuffmanNode Parent { get; set; } // Code Animation

        public int CompareTo(HuffmanNode other) => Frequency.CompareTo(other.Frequency);
    }

    // Extension method for shallow copying HuffmanNode
    public static class HuffmanNodeExtensions
    {
        public static HuffmanNode Copy(this HuffmanNode node)
        {
            return new HuffmanNode
            {
                Character = node.Character,
                Frequency = node.Frequency,
                Left = node.Left,
                Right = node.Right,
                X = node.X,
                Y = node.Y
            };
        }
    }
}
