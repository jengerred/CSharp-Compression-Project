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
    private readonly List<List<HuffmanNode>> _mergeSteps = new();
    private int _currentStep = 0;
    private DispatcherTimer _timer;

    // You may adjust these for compactness
    private const double radius = 15;      // Circle radius (smaller for more nodes)
    private const double spacing = 10;     // Space between circles
    private const double margin = 10;      // Margin from canvas edge
    private const int fontSize = 9;        // Font size for node labels

    public HuffmanTreeAnimation(Canvas canvas, CharacterFrequency[] frequencies)
    {
        _canvas = canvas;
        var nodes = new List<HuffmanNode>();
        for (int i = 0; i < frequencies.Length; i++)
            if (frequencies[i] != null)
                nodes.Add(new HuffmanNode { Character = frequencies[i].Character, Frequency = frequencies[i].Frequency });

        // Record each merge step
        var working = nodes.Select(n => n.Copy()).ToList();
        _mergeSteps.Add(working.Select(n => n.Copy()).ToList());
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
            working.Add(parent);
            _mergeSteps.Add(working.Select(n => n.Copy()).ToList());
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
            if (_currentStep < _mergeSteps.Count)
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
        var nodes = _mergeSteps[step];

        // Calculate columns based on canvas width and node size
        int maxColumns = Math.Max(1, (int)((_canvas.Width - 2 * margin + spacing) / (2 * radius + spacing)));
        int rows = (int)Math.Ceiling((double)nodes.Count / maxColumns);

        for (int i = 0; i < nodes.Count; i++)
        {
            int row = i / maxColumns;
            int col = i % maxColumns;
            double x = margin + col * (2 * radius + spacing) + radius;
            double y = margin + row * (2 * radius + spacing) + radius;
            DrawNode(nodes[i], x, y, radius, Colors.LightSkyBlue);
        }
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
