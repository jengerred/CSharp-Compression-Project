using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionProject
{
    // HuffmanNode represents a single point (node) in the Huffman tree.
    // Each node can store a character and its frequency, or act as an internal branch.
    // The tree structure is used to create efficient codes for data compression.
    public class HuffmanNode : IComparable<HuffmanNode>
    {
        // The character this node represents (if it's a leaf node).
        // If this is null, the node is an internal branch, not a character.
        public char? Character { get; set; }

        // How many times this character (or group of characters) appears in the data.
        // For leaf nodes, this is the character's frequency.
        // For internal nodes, this is the sum of the frequencies of all child nodes.
        public int Frequency { get; set; }

        // Points to the left child in the tree.
        // In Huffman coding, moving left usually means adding a '0' to the code.
        public HuffmanNode Left { get; set; }

        // Points to the right child in the tree.
        // In Huffman coding, moving right usually means adding a '1' to the code.
        public HuffmanNode Right { get; set; }

        // X position for drawing this node on the screen (used in visualizations or animations).
        public double X { get; set; }

        // Y position for drawing this node on the screen (used in visualizations or animations).
        public double Y { get; set; }

        // Points to this node's parent in the tree.
        // Useful for tracing paths or animating the tree, but not required for basic compression.
        public HuffmanNode Parent { get; set; }

        // This method lets us compare two nodes based on their frequency.
        // It's used to help sort nodes, so we can always find the ones with the lowest frequency first.
        public int CompareTo(HuffmanNode other) => Frequency.CompareTo(other.Frequency);
    }

    // HuffmanNodeExtensions provides extra methods for working with HuffmanNode objects.
    // Here, we add a method to make a copy of a node.
    public static class HuffmanNodeExtensions
    {
        // Makes a shallow copy of a HuffmanNode.
        // This means it copies the values, but not the parent pointer (to avoid confusion or loops).
        // Useful if you want to duplicate a node without messing up the tree structure.
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
                // Parent is intentionally not copied to avoid circular references
            };
        }
    }
}
