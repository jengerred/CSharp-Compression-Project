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
    private const double radius = 18; // Node radius for compactness
    private const double spacing = 14;
    private const double margin = 20;
    private const int fontSize = 10;
    private const double levelHeight = 80;


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

    public Dictionary<HuffmanNode, (double x, double y)> GetNodePositions()
    {
        var nodesByLevel = _allNodes.GroupBy(n => n.level).OrderBy(g => g.Key).ToList();
        int maxLevel = nodesByLevel.Any() ? nodesByLevel.Max(g => g.Key) : 0;
        var nodePositions = new Dictionary<HuffmanNode, (double x, double y)>();
        for (int level = 0; level <= maxLevel; level++)
        {
            if (!nodesByLevel.Any(g => g.Key == level))
                continue;
            var nodesAtLevel = nodesByLevel.First(g => g.Key == level).Select(n => n.node).ToList();
            double y = _canvas.Height - margin - radius - (levelHeight * (maxLevel - level));
            double totalWidth = nodesAtLevel.Count * (2 * radius + spacing) - spacing;
            double xStart = (_canvas.Width - totalWidth) / 2 + radius;
            for (int i = 0; i < nodesAtLevel.Count; i++)
            {
                double x = xStart + i * (2 * radius + spacing);
                nodePositions[nodesAtLevel[i]] = (x, y);
            }
        }
        return nodePositions;
    }

    public List<HuffmanNode> GetLeaves()
    {
        return _allNodes.Where(n => n.level == 0).Select(n => n.node).ToList();
    }

    public static Color[] RowColorsPublic => RowColors;

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

        // --- Draw the tree below the characters ---
        var nodesByLevel = _allNodes
            .Where(n => n.step <= step)
            .GroupBy(n => n.level)
            .OrderBy(g => g.Key)
            .ToList();

        int maxLevel = nodesByLevel.Any() ? nodesByLevel.Max(g => g.Key) : 0;

        var nodePositions = new Dictionary<HuffmanNode, (double x, double y)>();
        for (int level = 0; level <= maxLevel; level++)
        {
            if (!nodesByLevel.Any(g => g.Key == level))
                continue;
            var nodesAtLevel = nodesByLevel.First(g => g.Key == level).Select(n => n.node).ToList();
            double nodeY = _canvas.Height - margin - radius - (level * levelHeight);
    
            double totalWidth = nodesAtLevel.Count * (2 * radius + spacing) - spacing;
            double xStart = (_canvas.Width - totalWidth) / 2 + radius;
            for (int i = 0; i < nodesAtLevel.Count; i++)
            {
                double x = xStart + i * (2 * radius + spacing);
                nodePositions[nodesAtLevel[i]] = (x, nodeY);
            }
        }

        // Draw all edges first (under nodes)
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= step))
        {
            if (node.Left != null && nodePositions.ContainsKey(node.Left))
            {
                DrawEdge(nodePositions[node].x, nodePositions[node].y, nodePositions[node.Left].x, nodePositions[node.Left].y);
            }
            if (node.Right != null && nodePositions.ContainsKey(node.Right))
            {
                DrawEdge(nodePositions[node].x, nodePositions[node].y, nodePositions[node.Right].x, nodePositions[node.Right].y);
            }
        }

        // Draw all nodes (on top)
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= step))
        {
            var (x, nodeY) = nodePositions[node];
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
            Width = radius * 2.2,
            Height = radius * 2,
            Fill = new SolidColorBrush(color),
            Stroke = Brushes.Black,
            StrokeThickness = 1.5
        };
        Canvas.SetLeft(ellipse, x - radius * 1.1);
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
