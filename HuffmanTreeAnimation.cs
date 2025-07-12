using CompressionProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

public class HuffmanTreeAnimation
{
    private readonly Canvas _canvas;
    private readonly List<(HuffmanNode node, int level, int step)> _allNodes = new();
    private readonly List<(HuffmanNode left, HuffmanNode right, HuffmanNode parent, int step)> _mergePairs = new();
    private int _currentStep = 0;
    private System.Windows.Threading.DispatcherTimer _timer;

    // Visual parameters
    private const double radius = 15;      // Node radius
    private const double margin = 30;      // Horizontal margin between nodes
    private const double levelHeight = 70; // Vertical spacing between levels
    private const double treeOffsetX = 150; // Move tree further right (adjust as needed)
    private const double treeOffsetY = 90; // Move tree further down (increased for more vertical space)
    private const int fontSize = 10;

    public static readonly Color[] RowColors = new Color[]
    {
        Colors.LightSkyBlue, Colors.Orange, Colors.YellowGreen, Colors.MediumPurple,
        Colors.Gold, Colors.Coral, Colors.LightPink, Colors.LightSalmon,
        Colors.Turquoise, Colors.LightGray
    };
    private static readonly Color MergeHighlightColor = Colors.Red;

    private readonly List<char> _first20ReadableChars;

    public List<(HuffmanNode node, int level, int step)> AllNodes => _allNodes;
    public HuffmanNode Root => _allNodes.Count > 0 ? _allNodes.Last().node : null;

    // Node positions for drawing
    private Dictionary<HuffmanNode, (double x, double y)> _nodePositions;

    public HuffmanTreeAnimation(Canvas canvas, CharacterFrequency[] frequencies, List<char> first20ReadableChars)
    {
        _canvas = canvas;
        _first20ReadableChars = first20ReadableChars ?? new List<char>();

        var leaves = new List<HuffmanNode>();
        for (int i = 0; i < frequencies.Length; i++)
            if (frequencies[i] != null)
                leaves.Add(new HuffmanNode { Character = frequencies[i].Character, Frequency = frequencies[i].Frequency });

        for (int i = 0; i < leaves.Count; i++)
            _allNodes.Add((leaves[i], 0, 0));

        var working = leaves.Select(n => n).ToList();
        int step = 1;
        var nodeLevels = leaves.ToDictionary(n => n, n => 0);

        while (working.Count > 1)
        {
            working.Sort();
            var left = working[0];
            var right = working[1];
            working.RemoveRange(0, 2);
            var parent = new HuffmanNode
            {
                Character = null,
                Frequency = left.Frequency + right.Frequency,
                Left = left,
                Right = right
            };
            left.Parent = parent;
            right.Parent = parent;

            int parentLevel = Math.Max(nodeLevels[left], nodeLevels[right]) + 1;
            nodeLevels[parent] = parentLevel;
            _allNodes.Add((parent, parentLevel, step));
            _mergePairs.Add((left, right, parent, step));
            working.Add(parent);
            step++;
        }

        // Assign node positions recursively (like DecompressedTreeAnimation)
        _nodePositions = new Dictionary<HuffmanNode, (double x, double y)>();
        int leafCount = leaves.Count;
        double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 600;
        double nodeSpacing = 2 * radius + margin;
        double totalTreeWidth = leafCount * nodeSpacing;
        double startX = (canvasWidth - totalTreeWidth) / 2 + radius + treeOffsetX;
        double nextLeafX = startX;
        if (Root != null)
            AssignNodePositions(Root, 0, ref nextLeafX, _nodePositions, nodeSpacing, treeOffsetY, levelHeight);
    }

    // Recursive assignment: leaves spaced evenly, parents above midpoint of children
    private double AssignNodePositions(
        HuffmanNode node,
        int level,
        ref double nextLeafX,
        Dictionary<HuffmanNode, (double x, double y)> nodePositions,
        double nodeSpacing,
        double yStart,
        double levelHeight)
    {
        if (node == null) return 0;

        double y = yStart + level * levelHeight;
        double x;

        if (node.Left == null && node.Right == null)
        {
            x = nextLeafX;
            nextLeafX += nodeSpacing;
        }
        else
        {
            double leftX = AssignNodePositions(node.Left, level + 1, ref nextLeafX, nodePositions, nodeSpacing, yStart, levelHeight);
            double rightX = AssignNodePositions(node.Right, level + 1, ref nextLeafX, nodePositions, nodeSpacing, yStart, levelHeight);
            x = (leftX + rightX) / 2.0;
        }

        nodePositions[node] = (x, y);
        return x;
    }

    public Dictionary<HuffmanNode, (double x, double y)> GetNodePositions()
    {
        return _nodePositions;
    }

    public List<HuffmanNode> GetLeaves()
    {
        var leaves = new List<HuffmanNode>();
        CollectLeaves(Root, leaves);
        return leaves;
    }

    private void CollectLeaves(HuffmanNode node, List<HuffmanNode> leaves)
    {
        if (node == null) return;
        if (node.Left == null && node.Right == null)
            leaves.Add(node);
        else
        {
            CollectLeaves(node.Left, leaves);
            CollectLeaves(node.Right, leaves);
        }
    }

    public void StartAnimation(int intervalMs = 500)
    {
        _currentStep = 0;
        DrawStep(_currentStep);
        _timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
        _timer.Tick += (s, e) =>
        {
            _currentStep++;
            if (_currentStep <= _mergePairs.Count)
                DrawStep(_currentStep);
            else
                _timer.Stop();
        };
        _timer.Start();
    }

    public void StopAnimation()
    {
        if (_timer != null && _timer.IsEnabled)
            _timer.Stop();
    }

    private void DrawStep(int step)
    {
        _canvas.Children.Clear();

        // Determine highlights for this step
        HuffmanNode leftHighlight = null, rightHighlight = null, parentHighlight = null;
        if (step > 0 && step - 1 < _mergePairs.Count)
        {
            (leftHighlight, rightHighlight, parentHighlight, _) = _mergePairs[step - 1];
        }

        // --- Draw the 20 readable characters at the top, highlighting if needed ---
        double charSpacing = 40;
        double startX = 10;
        double y = 20;
        for (int i = 0; i < _first20ReadableChars.Count; i++)
        {
            char ch = _first20ReadableChars[i];
            bool isHighlighted = false;
            if (leftHighlight != null && leftHighlight.Character == ch) isHighlighted = true;
            if (rightHighlight != null && rightHighlight.Character == ch) isHighlighted = true;

            var text = new TextBlock
            {
                Text = ch.ToString(),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = isHighlighted ? Brushes.Red : Brushes.Black
            };
            Canvas.SetLeft(text, startX + i * charSpacing);
            Canvas.SetTop(text, y);
            _canvas.Children.Add(text);
        }

        // Draw all edges first (under nodes)
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= step))
        {
            if (node.Left != null && _nodePositions.ContainsKey(node.Left))
            {
                DrawEdge(_nodePositions[node].x, _nodePositions[node].y, _nodePositions[node.Left].x, _nodePositions[node.Left].y);
            }
            if (node.Right != null && _nodePositions.ContainsKey(node.Right))
            {
                DrawEdge(_nodePositions[node].x, _nodePositions[node].y, _nodePositions[node.Right].x, _nodePositions[node.Right].y);
            }
        }

        // Draw all nodes (on top)
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= step))
        {
            var (x, nodeY) = _nodePositions[node];
            int colorIndex = Math.Min(level, RowColors.Length - 1);
            Color color = RowColors[colorIndex];

            // Highlight merging children (red) for the current merge step
            if (step > 0 && (node == leftHighlight || node == rightHighlight) && nodeStep <= step - 1)
                color = MergeHighlightColor;

            DrawNode(node, x, nodeY, radius, color);
        }
    }

    private void DrawEdge(double x1, double y1, double x2, double y2)
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
            Stroke = Brushes.Black,
            StrokeThickness = 1
        };
        _canvas.Children.Add(line);
    }

    private void DrawNode(HuffmanNode node, double x, double y, double radius, Color color)
    {
        var ellipse = new Ellipse
        {
            Width = radius * 2.0,
            Height = radius * 2.0,
            Fill = new SolidColorBrush(color),
            Stroke = Brushes.Black,
            StrokeThickness = 1.5
        };
        Canvas.SetLeft(ellipse, x - radius);
        Canvas.SetTop(ellipse, y - radius);
        _canvas.Children.Add(ellipse);

        string label = node.Character != null
            ? $"{node.Character} ({node.Frequency})"
            : $"{node.Frequency}";
        var text = new TextBlock
        {
            Text = label,
            Foreground = Brushes.Black,
            FontWeight = FontWeights.Bold,
            FontSize = fontSize
        };
        text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double textWidth = text.DesiredSize.Width;
        double textHeight = text.DesiredSize.Height;
        Canvas.SetLeft(text, x - textWidth / 2);
        Canvas.SetTop(text, y - textHeight / 2);
        _canvas.Children.Add(text);
    }
}
