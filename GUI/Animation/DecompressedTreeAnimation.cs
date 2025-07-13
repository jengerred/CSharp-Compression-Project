using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CompressionProject.GUI.Animation
{
    // Represents a character and its frequency, used for building the Huffman tree
    // during decompression animation. This helps track how many times each character
    // appears in the data being restored.
    public class DecompressedCharacterFrequency
    {
        // The character being tracked.
        public char Character { get; }

        // The number of times this character has appeared so far.
        public int Frequency { get; private set; }

        // When a character is first found, create a DecompressedCharacterFrequency for it.
        public DecompressedCharacterFrequency(char character)
        {
            Character = character;
            Frequency = 1;
        }

        // Call this method each time the character is found again.
        public void Increment() => Frequency++;
    }

    // Represents a single node in the Huffman tree used during decompression animation.
    // Each node can be a leaf (with a character) or an internal node (just a frequency).
    public class DecompressedHuffmanNode : IComparable<DecompressedHuffmanNode>
    {
        // The character for this node (null if it's an internal node).
        public char? Character { get; set; }

        // The frequency for this character or group of characters.
        public int Frequency { get; set; }

        // References to child nodes (left = 0, right = 1).
        public DecompressedHuffmanNode Left { get; set; }
        public DecompressedHuffmanNode Right { get; set; }

        // Reference to the parent node (useful for tracing paths).
        public DecompressedHuffmanNode Parent { get; set; }

        // Allows nodes to be compared by frequency for sorting.
        public int CompareTo(DecompressedHuffmanNode other) => Frequency.CompareTo(other.Frequency);
    }

    // Builds and manages the Huffman tree for the decompression animation.
    // This class is responsible for creating the tree structure and generating
    // the code table (mapping characters to their binary codes).
    public class DecompressedHuffmanTree
    {
        // The root node of the built Huffman tree.
        public DecompressedHuffmanNode Root { get; private set; }

        // Maps each character to its Huffman code (string of 0s and 1s).
        public Dictionary<char, string> CodeTable { get; private set; }

        // Builds the tree from an array of character frequencies.
        public void Build(DecompressedCharacterFrequency[] frequencies)
        {
            // Create a list of nodes for each character found in the data.
            var nodes = new List<DecompressedHuffmanNode>();
            for (int i = 0; i < frequencies.Length; i++)
            {
                if (frequencies[i] != null)
                    nodes.Add(new DecompressedHuffmanNode { Character = frequencies[i].Character, Frequency = frequencies[i].Frequency });
            }

            // Repeatedly combine the two nodes with the lowest frequencies
            // until only one node (the root) remains.
            while (nodes.Count > 1)
            {
                nodes.Sort();
                var left = nodes[0];
                var right = nodes[1];
                nodes.RemoveRange(0, 2);

                var parentNode = new DecompressedHuffmanNode
                {
                    Character = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };
                left.Parent = parentNode;
                right.Parent = parentNode;

                nodes.Add(parentNode);
            }

            Root = nodes.Count > 0 ? nodes[0] : null;
            CodeTable = new Dictionary<char, string>();
            if (Root != null)
                GenerateCodes(Root, "");
        }

        // Recursively generates the binary code for each character in the tree.
        private void GenerateCodes(DecompressedHuffmanNode node, string code)
        {
            if (node == null) return;
            if (node.Character != null)
            {
                CodeTable[node.Character.Value] = code;
                return;
            }
            GenerateCodes(node.Left, code + "0");
            GenerateCodes(node.Right, code + "1");
        }
    }

    // Handles the animation and visualization of the decompression process.
    // It shows how the Huffman tree is traversed to restore each character,
    // highlighting the path and displaying the restored text as the animation progresses.
    public class DecompressedTreeAnimation
    {
        private readonly Canvas _canvas;
        private readonly DecompressedHuffmanNode _root;
        private readonly Dictionary<DecompressedHuffmanNode, (double x, double y)> _nodePositions;
        private readonly List<(DecompressedHuffmanNode node, int level, int step)> _allNodes;
        private readonly Color[] RowColors;

        // Visual layout settings for drawing the tree and characters.
        private readonly double radius = 15;
        private readonly double margin = 30;
        private readonly double levelHeight = 40;
        private readonly double treeOffsetX = 150;

        // The bitstream representing the encoded data for the first 20 characters.
        private readonly string _bitStream;
        // The number of characters to restore in the animation.
        private readonly int _numCharsToRestore;
        // The list of characters that have been restored so far.
        private readonly List<char> _restoredChars = new List<char>();
        // Tracks if the animation is currently running to prevent overlapping runs.
        private bool _isAnimating = false;

        // Lists used to highlight the current path in the tree during animation.
        private List<(DecompressedHuffmanNode parent, DecompressedHuffmanNode child)> _highlightedEdges = new List<(DecompressedHuffmanNode, DecompressedHuffmanNode)>();
        private List<DecompressedHuffmanNode> _highlightedNodes = new List<DecompressedHuffmanNode>();

        /// <summary>
        /// Initializes the decompression animation by building a tree and preparing the bitstream.
        /// </summary>
        /// <param name="canvas">The canvas to draw the animation on.</param>
        /// <param name="inputFile">The file to extract the first 20 readable characters from.</param>
        public DecompressedTreeAnimation(
            Canvas canvas,
            string inputFile
        )
        {
            _canvas = canvas;
            RowColors = new Color[]
            {
                Colors.LightSkyBlue, Colors.Orange, Colors.YellowGreen, Colors.MediumPurple,
                Colors.Gold, Colors.Coral, Colors.LightPink, Colors.LightSalmon,
                Colors.Turquoise, Colors.LightGray
            };

            // 1. Extract the first 20 readable characters from the file for preview and animation.
            List<char> first20ReadableChars = new List<char>();
            using (FileStream fs = File.OpenRead(inputFile))
            {
                int b;
                while ((b = fs.ReadByte()) != -1 && first20ReadableChars.Count < 20)
                {
                    char ch = (char)b;
                    if (ch >= 32 && ch <= 126)
                        first20ReadableChars.Add(ch);
                }
            }

            // 2. Count how many times each character appears (frequency table).
            DecompressedCharacterFrequency[] frequencies = new DecompressedCharacterFrequency[256];
            foreach (char c in first20ReadableChars)
            {
                int idx = c;
                if (frequencies[idx] == null)
                    frequencies[idx] = new DecompressedCharacterFrequency(c);
                else
                    frequencies[idx].Increment();
            }

            // 3. Build the Huffman tree and code table for decompression animation.
            var tree = new DecompressedHuffmanTree();
            tree.Build(frequencies);
            _root = tree.Root;
            var codeTable = tree.CodeTable;

            // 4. Assign positions to each node for drawing the tree centered on the canvas.
            _allNodes = new List<(DecompressedHuffmanNode node, int level, int step)>();
            _nodePositions = new Dictionary<DecompressedHuffmanNode, (double x, double y)>();

            int maxLevel = GetMaxLevel(_root);
            int leafCount = GetLeafCount(_root);

            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 600;
            double nodeSpacing = 2 * radius + margin;
            double totalTreeWidth = leafCount * nodeSpacing;
            double startX = (canvasWidth - totalTreeWidth) / 2 + radius + treeOffsetX;
            double nextLeafX = startX;
            AssignNodePositions(_root, 0, ref nextLeafX, _nodePositions, nodeSpacing, 100, levelHeight + margin);

            // Build the list of all nodes for animation and drawing.
            BuildAllNodesList(_root, 0, 0, _allNodes);

            // 5. Generate a bitstream for the first 20 readable characters, using the code table.
            _bitStream = string.Concat(first20ReadableChars.Select(c => codeTable.ContainsKey(c) ? codeTable[c] : ""));
            _numCharsToRestore = first20ReadableChars.Count;
        }

        // Recursively assigns X/Y positions to each node for drawing.
        // Leaf nodes are spaced evenly; parent nodes are centered above their children.
        private double AssignNodePositions(
            DecompressedHuffmanNode node,
            int level,
            ref double nextLeafX,
            Dictionary<DecompressedHuffmanNode, (double x, double y)> nodePositions,
            double nodeSpacing,
            double yStart,
            double levelHeight)
        {
            if (node == null) return 0;

            double y = yStart + level * levelHeight;
            double x;

            if (node.Left == null && node.Right == null)
            {
                // Leaf node: assign position from left to right.
                x = nextLeafX;
                nextLeafX += nodeSpacing;
            }
            else
            {
                // Internal node: position is the midpoint of its children.
                double leftX = AssignNodePositions(node.Left, level + 1, ref nextLeafX, nodePositions, nodeSpacing, yStart, levelHeight);
                double rightX = AssignNodePositions(node.Right, level + 1, ref nextLeafX, nodePositions, nodeSpacing, yStart, levelHeight);
                x = (leftX + rightX) / 2.0;
            }

            nodePositions[node] = (x, y);
            return x;
        }

        // Helper to build _allNodes for animation and drawing.
        private void BuildAllNodesList(DecompressedHuffmanNode node, int level, int step, List<(DecompressedHuffmanNode node, int level, int step)> allNodes)
        {
            if (node == null) return;
            allNodes.Add((node, level, step));
            BuildAllNodesList(node.Left, level + 1, step * 2, allNodes);
            BuildAllNodesList(node.Right, level + 1, step * 2 + 1, allNodes);
        }

        // Returns the maximum depth of the tree.
        private int GetMaxLevel(DecompressedHuffmanNode node)
        {
            if (node == null) return -1;
            return 1 + Math.Max(GetMaxLevel(node.Left), GetMaxLevel(node.Right));
        }

        // Returns the number of leaf nodes in the tree.
        private int GetLeafCount(DecompressedHuffmanNode node)
        {
            if (node == null) return 0;
            if (node.Left == null && node.Right == null) return 1;
            return GetLeafCount(node.Left) + GetLeafCount(node.Right);
        }

        /// <summary>
        /// Starts the decompression animation, restoring each character step-by-step.
        /// Highlights the path through the tree for each decoded character.
        /// </summary>
        public async void StartDecompressionAnimation(int intervalMs = 400)
        {
            if (_isAnimating) return;
            _isAnimating = true;

            _restoredChars.Clear();
            int bitIndex = 0;

            for (int i = 0; i < _numCharsToRestore; i++)
            {
                DecompressedHuffmanNode current = _root;
                _highlightedEdges.Clear();
                _highlightedNodes.Clear();

                // Always include the root in the path.
                _highlightedNodes.Add(current);

                // Traverse the tree until a leaf is reached.
                while (current.Left != null || current.Right != null)
                {
                    if (bitIndex >= _bitStream.Length)
                    {
                        _isAnimating = false;
                        return;
                    }

                    char bit = _bitStream[bitIndex++];
                    var parent = current;
                    current = bit == '0' ? current.Left : current.Right;
                    if (current == null)
                    {
                        _isAnimating = false;
                        return;
                    }

                    // Highlight the traversed edge and node.
                    _highlightedEdges.Add((parent, current));
                    _highlightedNodes.Add(current);

                    // Animate traversal step: redraw everything to show the new highlight.
                    _canvas.Children.Clear();
                    DrawRestoredCharacters();
                    DrawTree(current);
                    await Task.Delay(200);
                }

                // After reaching a leaf, restore the character and update the display.
                _restoredChars.Add(current.Character.HasValue ? current.Character.Value : '?');
                _canvas.Children.Clear();
                DrawRestoredCharacters();
                DrawTree(current);
                await Task.Delay(intervalMs);

                // Highlight lists are cleared at the start of the next character.
            }

            _isAnimating = false;
        }

        // Draws the restored characters so far at the top of the canvas.
        private void DrawRestoredCharacters()
        {
            double charSpacing = 40;
            double startX = 10;
            double y = 20;

            for (int i = 0; i < _restoredChars.Count; i++)
            {
                var text = new TextBlock
                {
                    Text = _restoredChars[i].ToString(),
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Green
                };
                Canvas.SetLeft(text, startX + i * charSpacing);
                Canvas.SetTop(text, y);
                _canvas.Children.Add(text);
            }
        }

        // Draws the Huffman tree, highlighting the current path being followed.
        private void DrawTree(DecompressedHuffmanNode highlightNode)
        {
            // Draw all edges, highlighting those in the current path.
            foreach (var (node, level, nodeStep) in _allNodes)
            {
                if (node.Left != null && _nodePositions.ContainsKey(node.Left))
                {
                    bool isHighlighted = _highlightedEdges.Any(e => e.parent == node && e.child == node.Left);
                    DrawEdge(_nodePositions[node].x, _nodePositions[node].y,
                             _nodePositions[node.Left].x, _nodePositions[node.Left].y,
                             isHighlighted ? Brushes.DeepSkyBlue : Brushes.Black,
                             isHighlighted ? 3 : 1);
                }
                if (node.Right != null && _nodePositions.ContainsKey(node.Right))
                {
                    bool isHighlighted = _highlightedEdges.Any(e => e.parent == node && e.child == node.Right);
                    DrawEdge(_nodePositions[node].x, _nodePositions[node].y,
                             _nodePositions[node.Right].x, _nodePositions[node.Right].y,
                             isHighlighted ? Brushes.DeepSkyBlue : Brushes.Black,
                             isHighlighted ? 3 : 1);
                }
            }

            // Draw all nodes, highlighting those in the current path.
            foreach (var (node, level, nodeStep) in _allNodes)
            {
                var (x, y) = _nodePositions[node];
                Color color;
                if (_highlightedNodes.Contains(node))
                {
                    color = Colors.DeepSkyBlue;
                }
                else if (node.Left == null && node.Right == null)
                {
                    color = Colors.LightSkyBlue;
                }
                else
                {
                    int colorIndex = Math.Min(level, RowColors.Length - 1);
                    color = RowColors[colorIndex];
                }
                DrawNode(node, x, y, radius, color);
            }
        }

        // Draws a line (edge) between two nodes, with optional highlighting.
        private void DrawEdge(double x1, double y1, double x2, double y2, Brush stroke, double thickness)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist == 0) dist = 1;
            double xEnd = x2 - dx / dist * radius;
            double yEnd = y2 - dy / dist * radius;

            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = xEnd,
                Y2 = yEnd,
                Stroke = stroke,
                StrokeThickness = thickness
            };
            _canvas.Children.Add(line);
        }

        // Draws a node (circle) on the canvas, with the specified color.
        // Also draws the node's label (character and frequency, or just frequency for internal nodes).
        private void DrawNode(DecompressedHuffmanNode node, double x, double y, double radius, Color color)
        {
            var ellipse = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.Black,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(ellipse, x - radius);
            Canvas.SetTop(ellipse, y - radius);
            _canvas.Children.Add(ellipse);

            string label = node.Character != null ? $"{node.Character} ({node.Frequency})" : $"{node.Frequency}";
            var text = new TextBlock
            {
                Text = label,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 10
            };
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = text.DesiredSize.Width;
            double textHeight = text.DesiredSize.Height;
            Canvas.SetLeft(text, x - textWidth / 2);
            Canvas.SetTop(text, y - textHeight / 2);
            _canvas.Children.Add(text);
        }
    }
}
