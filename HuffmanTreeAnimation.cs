using CompressionProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

public class HuffmanTreeAnimation
{
    private readonly Canvas _canvas;
    private readonly List<(HuffmanNode node, int level, int step)> _allNodes = new();
    private readonly List<(HuffmanNode left, HuffmanNode right, HuffmanNode parent, int step)> _mergePairs = new();
    private int _currentStep = 0;
    private DispatcherTimer _timer;

    // Visual parameters
    private const double radius = 15;
    private const double spacing = 10;
    private const double margin = 10;
    private const int fontSize = 9;
    private const double levelHeight = 60; // Adjust for more vertical space

    // Define a color palette for each row/level - public to allow integration with other animations
    public static readonly Color[] RowColors = new Color[]
    {
        Colors.LightSkyBlue, // Level 0 (leaves)
        Colors.Orange, // Level 1
        Colors.YellowGreen, // Level 2
        Colors.MediumPurple, // Level 3
        Colors.Gold, // Level 4
        Colors.Coral, // Level 5
        Colors.LightPink, // Level 6
        Colors.LightSalmon, // Level 7
        Colors.Turquoise, // Level 8
        Colors.LightGray // Level 9+
    };

    private static readonly Color MergeHighlightColor = Colors.Red; // Highlight merging nodes as red


    // --- Public properties/methods for integration ---

    /// <summary>
    /// List of all nodes with their level and step (for code animation).
    /// </summary>
    public List<(HuffmanNode node, int level, int step)> AllNodes => _allNodes;

    /// <summary>
    /// The root of the Huffman tree.
    /// </summary>
    public HuffmanNode Root => _allNodes.Count > 0 ? _allNodes.Last().node : null;

    /// <summary>
    /// Returns a dictionary mapping each node to its (x, y) position on the canvas.
    /// </summary>
    public Dictionary<HuffmanNode, (double x, double y)> GetNodePositions()
    {
        var nodesByLevel = _allNodes
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
            double y = _canvas.Height - margin - radius - (level * levelHeight);
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

    /// <summary>
    /// Returns a list of all leaf nodes (nodes at level 0).
    /// </summary>
    public List<HuffmanNode> GetLeaves()
    {
        return _allNodes.Where(n => n.level == 0).Select(n => n.node).ToList();
    }

    /// <summary>
    /// Exposes the row color palette for use in other animations.
    /// </summary>
    public static Color[] RowColorsPublic => RowColors;

    // --- End public additions ---





    public HuffmanTreeAnimation(Canvas canvas, CharacterFrequency[] frequencies)
    {
        _canvas = canvas;
        var leaves = new List<HuffmanNode>();
        for (int i = 0; i < frequencies.Length; i++)
            if (frequencies[i] != null)
                leaves.Add(new HuffmanNode { Character = frequencies[i].Character, Frequency = frequencies[i].Frequency });

        // Add all leaves as level 0, step 0
        for (int i = 0; i < leaves.Count; i++)
            _allNodes.Add((leaves[i], 0, 0));

        // Build the tree and record all parents with their step and level
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

    public void StartAnimation(int intervalMs = 200)
    {
        _currentStep = 0;
        DrawStep(_currentStep);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
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

        // Get all nodes created up to this step, grouped by level
        var nodesByLevel = _allNodes
        .Where(n => n.step <= step)
        .GroupBy(n => n.level)
        .OrderBy(g => g.Key)
        .ToList();

        int maxLevel = nodesByLevel.Any() ? nodesByLevel.Max(g => g.Key) : 0;

        // Calculate positions for each node (leaves at bottom, root at top)
        var nodePositions = new Dictionary<HuffmanNode, (double x, double y)>();
        for (int level = 0; level <= maxLevel; level++)
        {
            if (!nodesByLevel.Any(g => g.Key == level))
                continue;
            var nodesAtLevel = nodesByLevel.First(g => g.Key == level).Select(n => n.node).ToList();
            double y = _canvas.Height - margin - radius - (level * levelHeight);
            double totalWidth = nodesAtLevel.Count * (2 * radius + spacing) - spacing;
            double xStart = (_canvas.Width - totalWidth) / 2 + radius;
            for (int i = 0; i < nodesAtLevel.Count; i++)
            {
                double x = xStart + i * (2 * radius + spacing);
                nodePositions[nodesAtLevel[i]] = (x, y);
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

        // Determine highlights for this step
        HuffmanNode leftHighlight = null, rightHighlight = null, parentHighlight = null;
        if (step > 0 && step - 1 < _mergePairs.Count)
        {
            (leftHighlight, rightHighlight, parentHighlight, _) = _mergePairs[step - 1];
        }

        // Draw all nodes (on top)
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= step))
        {
            var (x, y) = nodePositions[node];
            int colorIndex = Math.Min(level, RowColors.Length - 1);
            Color color = RowColors[colorIndex];

            // Highlight merging children (red) for the current merge step
            if (step > 0 && (node == leftHighlight || node == rightHighlight) && nodeStep <= step - 1)
                color = MergeHighlightColor;

            // The parent node is never red; it always takes its row color immediately

            DrawNode(node, x, y, radius, color);
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
