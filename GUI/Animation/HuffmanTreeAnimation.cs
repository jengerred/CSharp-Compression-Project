using CompressionProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

// HuffmanTreeAnimation is responsible for visually animating the process of building a Huffman tree.
// It draws each step of the tree construction onto a WPF Canvas, allowing users to see how nodes are merged
// and how the tree structure evolves. It also highlights which nodes are being merged at each step.
public class HuffmanTreeAnimation
{
    // The canvas where the animation is drawn.
    private readonly Canvas _canvas;

    // Keeps track of all nodes created during the animation, their level in the tree, and at which step they appeared.
    private readonly List<(HuffmanNode node, int level, int step)> _allNodes = new();

    // Stores each merge operation: the two nodes being merged, their new parent, and the step number.
    private readonly List<(HuffmanNode left, HuffmanNode right, HuffmanNode parent, int step)> _mergePairs = new();

    // Tracks which step of the animation we're currently on.
    private int _currentStep = 0;

    // Timer for automatically advancing the animation.
    private System.Windows.Threading.DispatcherTimer _timer;

    // --- Visual Layout Parameters ---

    // Controls the size and spacing of nodes and levels in the tree.
    private const double radius = 15;
    private const double margin = 30;
    private const double levelHeight = 70;
    private const double treeOffsetX = 150;
    private const double treeOffsetY = 90;
    private const int fontSize = 10;

    // Colors for different levels of the tree, to make the structure visually clear.
    public static readonly Color[] RowColors = new Color[]
    {
        Colors.LightSkyBlue, Colors.Orange, Colors.YellowGreen, Colors.MediumPurple,
        Colors.Gold, Colors.Coral, Colors.LightPink, Colors.LightSalmon,
        Colors.Turquoise, Colors.LightGray
    };
    // Color used to highlight nodes that are currently being merged.
    private static readonly Color MergeHighlightColor = Colors.Red;

    // The first 20 readable characters from the file, shown at the top of the animation.
    private readonly List<char> _first20ReadableChars;

    // Exposes all nodes for use in other animations (e.g., code assignment).
    public List<(HuffmanNode node, int level, int step)> AllNodes => _allNodes;

    // The root node of the Huffman tree (after all merges are complete).
    public HuffmanNode Root => _allNodes.Count > 0 ? _allNodes.Last().node : null;

    // Stores the calculated position of each node on the canvas.
    private Dictionary<HuffmanNode, (double x, double y)> _nodePositions;

    /// <summary>
    /// Initializes the animation by building the merge steps and assigning positions to each node.
    /// </summary>
    /// <param name="canvas">The canvas to draw the animation on.</param>
    /// <param name="frequencies">Array of character frequencies (used to create the leaves).</param>
    /// <param name="first20ReadableChars">List of the first 20 readable characters for display.</param>
    public HuffmanTreeAnimation(Canvas canvas, CharacterFrequency[] frequencies, List<char> first20ReadableChars)
    {
        _canvas = canvas;
        _first20ReadableChars = first20ReadableChars ?? new List<char>();

        // Create leaf nodes for each character with a frequency.
        var leaves = new List<HuffmanNode>();
        for (int i = 0; i < frequencies.Length; i++)
            if (frequencies[i] != null)
                leaves.Add(new HuffmanNode { Character = frequencies[i].Character, Frequency = frequencies[i].Frequency });

        // Add all leaves to the node list at level 0, step 0.
        for (int i = 0; i < leaves.Count; i++)
            _allNodes.Add((leaves[i], 0, 0));

        // Build the tree by repeatedly merging the two lowest-frequency nodes.
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

        // Calculate where each node should appear on the canvas.
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

    /// <summary>
    /// Recursively assigns X/Y positions to each node for drawing the tree.
    /// Leaf nodes are spaced evenly; parent nodes are centered above their children.
    /// </summary>
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
            // Place leaf nodes at the next available X position.
            x = nextLeafX;
            nextLeafX += nodeSpacing;
        }
        else
        {
            // Center parent nodes above their children.
            double leftX = AssignNodePositions(node.Left, level + 1, ref nextLeafX, nodePositions, nodeSpacing, yStart, levelHeight);
            double rightX = AssignNodePositions(node.Right, level + 1, ref nextLeafX, nodePositions, nodeSpacing, yStart, levelHeight);
            x = (leftX + rightX) / 2.0;
        }

        nodePositions[node] = (x, y);
        return x;
    }

    /// <summary>
    /// Returns the calculated X/Y positions of all nodes for use in other animations.
    /// </summary>
    public Dictionary<HuffmanNode, (double x, double y)> GetNodePositions()
    {
        return _nodePositions;
    }

    /// <summary>
    /// Returns a list of all leaf nodes (characters) in the tree.
    /// </summary>
    public List<HuffmanNode> GetLeaves()
    {
        var leaves = new List<HuffmanNode>();
        CollectLeaves(Root, leaves);
        return leaves;
    }

    // Helper method to collect all leaf nodes recursively.
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

    /// <summary>
    /// Starts the animation, showing each merge step in sequence.
    /// The animation advances automatically every intervalMs milliseconds.
    /// </summary>
    public void StartAnimation(int intervalMs = 500)
    {
        _currentStep = 0;
        DrawStep(_currentStep);
        _timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromMilliseconds(intervalMs) };
        _timer.Tick += (s, e) =>
        {
            _currentStep++;
            if (_currentStep <= _mergePairs.Count + 1)
                DrawStep(_currentStep);
            else
                _timer.Stop();
        };
        _timer.Start();
    }

    /// <summary>
    /// Stops the animation if it is running.
    /// </summary>
    public void StopAnimation()
    {
        if (_timer != null && _timer.IsEnabled)
            _timer.Stop();
    }

    /// <summary>
    /// Draws the tree at a specific step in the merge process.
    /// Highlights the nodes currently being merged in red.
    /// Also draws the first 20 readable characters at the top, highlighting those involved in the merge.
    /// </summary>
    private void DrawStep(int step)
    {
        _canvas.Children.Clear();

        // Determine which nodes (if any) should be highlighted for this merge step.
        HuffmanNode leftHighlight = null, rightHighlight = null, parentHighlight = null;
        if (step > 0 && step - 1 < _mergePairs.Count)
        {
            (leftHighlight, rightHighlight, parentHighlight, _) = _mergePairs[step - 1];
        }

        // --- Draw the first 20 readable characters at the top of the canvas ---
        // Characters involved in the current merge are highlighted in red.
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

        // --- Draw all the edges (lines) between nodes, so they appear under the nodes ---
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= Math.Min(step, _mergePairs.Count)))
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

        // --- Draw all the nodes (circles) on top of the edges ---
        foreach (var (node, level, nodeStep) in _allNodes.Where(n => n.step <= Math.Min(step, _mergePairs.Count)))
        {
            var (x, nodeY) = _nodePositions[node];
            int colorIndex = Math.Min(level, RowColors.Length - 1);
            Color color = RowColors[colorIndex];

            // Highlight logic:
            // - At the last merge step: highlight the last pair and the new root in red.
            // - After the last merge: only the root is red, others revert to their assigned color.
            // - In previous steps: highlight only the pair being merged.
            if (step == _mergePairs.Count && (node == leftHighlight || node == rightHighlight || node == Root) && nodeStep <= step - 1)
            {
                color = MergeHighlightColor;
            }
            else if (step > _mergePairs.Count)
            {
                if (node == Root)
                    color = MergeHighlightColor;
                else
                    color = RowColors[colorIndex];
            }
            else if (step > 0 && step < _mergePairs.Count && (node == leftHighlight || node == rightHighlight) && nodeStep <= step - 1)
            {
                color = MergeHighlightColor;
            }

            DrawNode(node, x, nodeY, radius, color);
        }
    }

    /// <summary>
    /// Draws a line (edge) between two nodes on the canvas.
    /// The line ends just before the edge of the node circle for a clean appearance.
    /// </summary>
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

    /// <summary>
    /// Draws a node (circle) on the canvas, filled with the specified color.
    /// Also draws the node's label (character and frequency, or just frequency for internal nodes).
    /// </summary>
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

        // Label: show character and frequency for leaves, just frequency for internal nodes.
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
