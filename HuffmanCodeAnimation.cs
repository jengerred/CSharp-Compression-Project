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
        private readonly double levelHeight = 60;

        private readonly List<HuffmanNode> _leaves;
        private readonly string[] _finalCodes;

        public HuffmanCodeAnimation(
            Canvas canvas,
            HuffmanNode root,
            Dictionary<HuffmanNode, (double x, double y)> nodePositions,
            List<HuffmanNode> leaves, // <-- Add this parameter
            List<(HuffmanNode node, int level, int step)> allNodes,
            Color[] rowColors,
            List<char> first20Chars,
            Dictionary<char, string> codeTable)
        {
            _canvas = canvas;
            _root = root;
            _nodePositions = nodePositions;
            _leaves = leaves; // <-- Store the leaves
            _allNodes = allNodes;
            RowColors = rowColors;
            _first20Chars = first20Chars;
            _codeTable = codeTable;
            _finalCodes = new string[_first20Chars.Count];

        }


        public async void StartCodeAnimation(int intervalMs = 1000)
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
            double startX = 20;
            double y = 20;
            double codeY = y + 24;

            for (int i = 0; i < _first20Chars.Count; i++)
            {
                var text = new TextBlock
                {
                    Text = _first20Chars[i].ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = (i == highlightIndex) ? Brushes.Red : Brushes.Black
                };
                Canvas.SetLeft(text, startX + i * charSpacing);
                Canvas.SetTop(text, y);
                _canvas.Children.Add(text);

                string code = (codeProgress != null && codeProgress.ContainsKey(i)) ? codeProgress[i] : "";
                string spacedCode = string.Join(" ", code.ToCharArray());
                var codeText = new TextBlock
                {
                    Text = spacedCode,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Blue
                };
                Canvas.SetLeft(codeText, startX + i * charSpacing);
                Canvas.SetTop(codeText, codeY);
                _canvas.Children.Add(codeText);

            }
        }

        private void DrawTreeWithCodePath(char c, int codeLength)
        {
            // Find the leaf node for this character
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

            // Build the path from root to leaf (as a stack)
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
            path.Reverse(); // root to leaf

            // Limit path to codeLength (for partial highlight)
            if (codeLength < path.Count)
                path = path.GetRange(0, codeLength);

            // Draw all edges
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

            // Draw all nodes, highlighting those on the current path
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
