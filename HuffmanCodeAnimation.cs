using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CompressionProject
{
    public class HuffmanCodeAnimation
    {
        private readonly Canvas _canvas;
        private readonly HuffmanNode _root;
        private readonly Dictionary<HuffmanNode, (double x, double y)> _nodePositions;
        private readonly List<(HuffmanNode node, int level, int step)> _allNodes;
        private readonly Color[] RowColors;
        private readonly List<char> _first20Chars;
        private readonly Dictionary<char, string> _codeTable;
        private readonly double radius = 15;
        private readonly double margin = 10;
        private readonly double levelHeight = 40;

        private readonly List<HuffmanNode> _leaves;
        private readonly string[] _finalCodes;

        // Offset to move the tree further down in the canvas
        private readonly double treeYOffset = 50; // Increase this value to move the tree down

        public HuffmanCodeAnimation(
            Canvas canvas,
            HuffmanNode root,
            Dictionary<HuffmanNode, (double x, double y)> nodePositions,
            List<HuffmanNode> leaves,
            List<(HuffmanNode node, int level, int step)> allNodes,
            Color[] rowColors,
            List<char> first20Chars,
            Dictionary<char, string> codeTable)
        {
            _canvas = canvas;
            _root = root;
            _nodePositions = OffsetNodePositions(nodePositions, treeYOffset); // Apply vertical offset
            _leaves = leaves;
            _allNodes = allNodes;
            RowColors = rowColors;
            _first20Chars = first20Chars;
            _codeTable = codeTable;
            _finalCodes = new string[_first20Chars.Count];
        }

        /// <summary>
        /// Shifts the Y coordinate for all node positions so the tree is drawn further down in the canvas,
        /// avoiding overlap with the bitstream and character labels.
        /// </summary>
        private Dictionary<HuffmanNode, (double x, double y)> OffsetNodePositions(Dictionary<HuffmanNode, (double x, double y)> original, double yOffset)
        {
            var shifted = new Dictionary<HuffmanNode, (double x, double y)>();
            foreach (var kvp in original)
            {
                shifted[kvp.Key] = (kvp.Value.x, kvp.Value.y + yOffset);
            }
            return shifted;
        }

        public async void StartCodeAnimation(int intervalMs = 300)
        {
            for (int i = 0; i < _first20Chars.Count; i++)
            {
                char c = _first20Chars[i];
                if (!_codeTable.ContainsKey(c))
                    continue;

                string code = _codeTable[c];
                StringBuilder partial = new StringBuilder();

                for (int j = 0; j < code.Length; j++)
                {
                    partial.Append(code[j]);
                    _canvas.Children.Clear();

                    // Prepare codes to display: completed + current partial
                    var codeProgress = new Dictionary<int, string>();
                    for (int k = 0; k < _first20Chars.Count; k++)
                        if (!string.IsNullOrEmpty(_finalCodes[k]))
                            codeProgress[k] = _finalCodes[k];
                    codeProgress[i] = partial.ToString();

                    DrawOriginalCharsAndCodes(codeProgress, highlightIndex: i);
                    DrawTreeWithCodePath(c, j + 1);

                    await System.Threading.Tasks.Task.Delay(100);
                }

                // Store the final code for this character
                _finalCodes[i] = code;

                // After the full code, keep all codes displayed
                _canvas.Children.Clear();
                var finalCodeProgress = new Dictionary<int, string>();
                for (int k = 0; k < _first20Chars.Count; k++)
                    if (!string.IsNullOrEmpty(_finalCodes[k]))
                        finalCodeProgress[k] = _finalCodes[k];
                DrawOriginalCharsAndCodes(finalCodeProgress, highlightIndex: i);
                DrawTreeWithCodePath(c, code.Length);

                await System.Threading.Tasks.Task.Delay(intervalMs);
            }
        }

        public void DrawOriginalCharsAndCodes(Dictionary<int, string> codeProgress = null, int highlightIndex = -1)
        {
            double charSpacing = 40;
            double startX = 10;
            double y = 20;
            double codeY = y + 24;

            for (int i = 0; i < _first20Chars.Count; i++)
            {
                var text = new TextBlock
                {
                    Text = _first20Chars[i].ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = (i == highlightIndex) ? Brushes.Red : Brushes.Black
                };
                Canvas.SetLeft(text, startX + i * charSpacing);
                Canvas.SetTop(text, y);
                _canvas.Children.Add(text);
            }

            StringBuilder bitStream = new StringBuilder();
            for (int i = 0; i < _first20Chars.Count; i++)
            {
                string code = (codeProgress != null && codeProgress.ContainsKey(i)) ? codeProgress[i] : "";
                bitStream.Append(code);
            }
            string bits = bitStream.ToString();

            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            double bitSpacing = 10;
            double bitRowY = codeY + 30;
            int bitsPerRow = (int)Math.Floor((canvasWidth - (startX + 60)) / bitSpacing);
            if (bitsPerRow < 8) bitsPerRow = 8;

            var binaryLabel = new TextBlock
            {
                Text = "Binary:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(binaryLabel, startX);
            Canvas.SetTop(binaryLabel, bitRowY);
            _canvas.Children.Add(binaryLabel);

            int bitRow = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                int group = i / 8;
                int bitCol = i % bitsPerRow;
                if (bitCol == 0 && i != 0) bitRow++;

                var bitText = new TextBlock
                {
                    Text = bits[i].ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Blue
                };
                Canvas.SetLeft(bitText, startX + 65 + bitCol * bitSpacing);
                Canvas.SetTop(bitText, bitRowY + bitRow * 28);
                _canvas.Children.Add(bitText);
            }
        }

        private void DrawTreeWithCodePath(char c, int codeLength)
        {
            HuffmanNode leaf = null;
            foreach (var (node, level, step) in _allNodes)
            {
                if (node.Character == c && node.Left == null && node.Right == null)
                {
                    leaf = node;
                    break;
                }
            }
            if (leaf == null)
                return;

            List<(HuffmanNode node, bool isLeft)> path = new List<(HuffmanNode, bool)>();
            HuffmanNode current = leaf;
            while (current != _root)
            {
                if (current.Parent == null)
                    break;
                bool isLeft = (current.Parent.Left == current);
                path.Add((current, isLeft));
                current = current.Parent;
            }
            path.Reverse();

            if (codeLength < path.Count)
                path = path.GetRange(0, codeLength);

            foreach (var (node, level, nodeStep) in _allNodes)
            {
                if (node.Left != null && _nodePositions.ContainsKey(node.Left))
                {
                    bool highlightEdge = false;
                    foreach (var (pNode, _) in path)
                        if (pNode == node.Left && node == node.Left.Parent)
                            highlightEdge = true;
                    DrawEdge(
                        _nodePositions[node].x, _nodePositions[node].y,
                        _nodePositions[node.Left].x, _nodePositions[node.Left].y,
                        highlightEdge ? Brushes.DeepSkyBlue : Brushes.Black,
                        highlightEdge ? 3 : 1
                    );
                }
                if (node.Right != null && _nodePositions.ContainsKey(node.Right))
                {
                    bool highlightEdge = false;
                    foreach (var (pNode, _) in path)
                        if (pNode == node.Right && node == node.Right.Parent)
                            highlightEdge = true;
                    DrawEdge(
                        _nodePositions[node].x, _nodePositions[node].y,
                        _nodePositions[node.Right].x, _nodePositions[node.Right].y,
                        highlightEdge ? Brushes.DeepSkyBlue : Brushes.Black,
                        highlightEdge ? 3 : 1
                    );
                }
            }

            foreach (var (node, level, nodeStep) in _allNodes)
            {
                var (x, y) = _nodePositions[node];
                bool highlightNode = false;
                foreach (var (pNode, _) in path)
                    if (pNode == node)
                        highlightNode = true;
                int colorIndex = Math.Min(level, RowColors.Length - 1);
                Color color = highlightNode ? Colors.DeepSkyBlue : RowColors[colorIndex];
                DrawNode(node, x, y, radius, color);
            }
        }

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

        private void DrawNode(HuffmanNode node, double x, double y, double radius, Color color)
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

        public string[] GetFinalCodes() => _finalCodes;
    }
}
