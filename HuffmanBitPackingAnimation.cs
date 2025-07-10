using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CompressionProject
{
    public class HuffmanBitPackingAnimation
    {
        private readonly Canvas _canvas;
        private readonly List<char> _first20Chars;
        private readonly string[] _finalCodes;
        private readonly double charSpacing = 40;
        private readonly double startX = 10;
        private readonly double y = 20;
        private readonly double codeY = 44;
        private readonly double bitY = 80;
        private readonly double bitSpacing = 10;

        private static readonly Color[] ByteGroupColors = new Color[]
        {
Colors.OrangeRed, Colors.MediumSeaGreen, Colors.DodgerBlue, Colors.Goldenrod,
Colors.MediumVioletRed, Colors.Teal, Colors.SlateBlue, Colors.Crimson,
Colors.DarkCyan, Colors.OliveDrab, Colors.CadetBlue, Colors.DarkOrange,
Colors.MediumSlateBlue, Colors.DarkKhaki, Colors.Firebrick, Colors.MediumTurquoise
        };

        public HuffmanBitPackingAnimation(
        Canvas canvas,
        List<char> first20Chars,
        string[] finalCodes
        )
        {
            _canvas = canvas;
            _first20Chars = first20Chars;
            _finalCodes = finalCodes;
        }

        public async void StartBitPackingAnimation(int intervalMs = 900)
        {
            _canvas.Children.Clear();

            // --- Character and Code Wrapping ---
            double canvasWidth = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            int charsPerLine = Math.Max(1, (int)((canvasWidth - startX - 20) / charSpacing));
            int charRows = (_first20Chars.Count + charsPerLine - 1) / charsPerLine;

            // Draw characters and codes, wrapped
            for (int i = 0; i < _first20Chars.Count; i++)
            {
                int row = i / charsPerLine;
                int col = i % charsPerLine;

                var text = new TextBlock
                {
                    Text = _first20Chars[i].ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(text, startX + col * charSpacing);
                Canvas.SetTop(text, y + row * 24);
                _canvas.Children.Add(text);

               
            }

            // --- Bitstream Preparation ---
            StringBuilder bitStream = new StringBuilder();
            for (int i = 0; i < _first20Chars.Count; i++)
                bitStream.Append(_finalCodes[i]);
            string bits = bitStream.ToString();

            int bitsPerRow = Math.Max(8, (int)Math.Floor((canvasWidth - (startX + 70)) / bitSpacing));
            int bitRows = (bits.Length + bitsPerRow - 1) / bitsPerRow;
            int byteCount = (bits.Length + 7) / 8;
            int bytesPerRow = bitsPerRow / 8;
            if (bytesPerRow < 1) bytesPerRow = 1;
            double rowSpacing = 28;

            // --- Row Y positions ---
            double binaryRowY = codeY + charRows * 24 + 20;
            double byteGroupsRowY = binaryRowY + rowSpacing * bitRows + 10;
            double hexRowY = byteGroupsRowY + rowSpacing * ((byteCount + bytesPerRow - 1) / bytesPerRow) + 10;
            double hexSummaryY = hexRowY + rowSpacing * ((byteCount + bytesPerRow - 1) / bytesPerRow) + 30;

            // --- Draw Static Labels ---
            var binaryLabel = new TextBlock
            {
                Text = "Bitstream:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(binaryLabel, startX);
            Canvas.SetTop(binaryLabel, binaryRowY);
            _canvas.Children.Add(binaryLabel);

            var byteLabel = new TextBlock
            {
                Text = "Byte Groups:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(byteLabel, startX);
            Canvas.SetTop(byteLabel, byteGroupsRowY);
            _canvas.Children.Add(byteLabel);

            var hexLabel = new TextBlock
            {
                Text = "Hex:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(hexLabel, startX);
            Canvas.SetTop(hexLabel, hexRowY);
            _canvas.Children.Add(hexLabel);

            var finalHexLabel = new TextBlock
            {
                Text = "Final Hex Output:",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(finalHexLabel, startX);
            Canvas.SetTop(finalHexLabel, hexSummaryY);
            _canvas.Children.Add(finalHexLabel);

            // --- Animate byte-by-byte, showing cumulative rows for bitstream, byte groups, and hex ---
            for (int b = 0; b < byteCount; b++)
            {
                // Remove only dynamic highlights from previous step
                var toRemove = new List<UIElement>();
                foreach (UIElement child in _canvas.Children)
                {
                    if (child is Rectangle || (child is TextBlock tb && tb.Tag != null && tb.Tag.ToString() == "dynamic"))
                        toRemove.Add(child);
                }
                foreach (var el in toRemove)
                    _canvas.Children.Remove(el);

                // --- Bitstream: Show all bits up to the current byte (cumulative), wrapping
                int bitsToShow = Math.Min((b + 1) * 8, bits.Length);
                for (int i = 0; i < bitsToShow; i++)
                {
                    int group = i / 8;
                    int bitCol = i % bitsPerRow;
                    int bitRow = i / bitsPerRow;

                    var bitText = new TextBlock
                    {
                        Text = bits[i].ToString(),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(ByteGroupColors[group % ByteGroupColors.Length]),
                        Tag = "dynamic"
                    };
                    Canvas.SetLeft(bitText, startX + 90 + bitCol * bitSpacing);
                    Canvas.SetTop(bitText, binaryRowY + bitRow * rowSpacing);
                    _canvas.Children.Add(bitText);
                }

                // --- Byte Groups: Show all byte rectangles, decimals, and hex up to current byte (cumulative)
                for (int j = 0; j <= b; j++)
                {
                    int start = j * 8;
                    int len = Math.Min(8, bits.Length - start);
                    string byteStr = bits.Substring(start, len).PadRight(8, '0');
                    byte byteVal = Convert.ToByte(byteStr, 2);

                    int byteCol = j % bytesPerRow;
                    int byteRow = j / bytesPerRow;
                    Color groupColor = ByteGroupColors[j % ByteGroupColors.Length];

                    // Highlight this group of 8 bits (rectangle)
                    var rect = new Rectangle
                    {
                        Width = bitSpacing * 8,
                        Height = 22,
                        Fill = new SolidColorBrush(Color.FromArgb(80, groupColor.R, groupColor.G, groupColor.B)),
                        RadiusX = 5,
                        RadiusY = 5,
                        Tag = "dynamic"
                    };
                    Canvas.SetLeft(rect, startX + 110 + byteCol * bitSpacing * 8 - 2);
                    Canvas.SetTop(rect, byteGroupsRowY + byteRow * rowSpacing - 2);
                    _canvas.Children.Add(rect);

                    // Draw decimal value
                    var decText = new TextBlock
                    {
                        Text = byteVal.ToString(),
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        Tag = "dynamic"
                    };
                    Canvas.SetLeft(decText, startX + 110 + byteCol * bitSpacing * 8);
                    Canvas.SetTop(decText, byteGroupsRowY + byteRow * rowSpacing);
                    _canvas.Children.Add(decText);

                    // Draw hex value in group color
                    var hexText = new TextBlock
                    {
                        Text = byteVal.ToString("X2"),
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(groupColor),
                        Tag = "dynamic"
                    };
                    Canvas.SetLeft(hexText, startX + 110 + byteCol * bitSpacing * 8);
                    Canvas.SetTop(hexText, hexRowY + byteRow * rowSpacing);
                    _canvas.Children.Add(hexText);
                }

                // --- Final Hex Output: Show all bytes up to and including this one (cumulative)
                double hexValuesX = startX + 160;
                int hexPerRow = (int)Math.Floor((canvasWidth - hexValuesX) / 32);
                if (hexPerRow < 1) hexPerRow = 1;
                for (int j = 0; j <= b; j++)
                {
                    Color color = ByteGroupColors[j % ByteGroupColors.Length];
                    int hStart = j * 8;
                    int hLen = Math.Min(8, bits.Length - hStart);
                    string hByteStr = bits.Substring(hStart, hLen).PadRight(8, '0');
                    byte hByteVal = Convert.ToByte(hByteStr, 2);

                    int hexRow = j / hexPerRow;
                    int hexCol = j % hexPerRow;

                    var hexBlock = new TextBlock
                    {
                        Text = hByteVal.ToString("X2") + " ",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(color),
                        Tag = "dynamic"
                    };
                    Canvas.SetLeft(hexBlock, hexValuesX + hexCol * 32);
                    Canvas.SetTop(hexBlock, hexSummaryY + hexRow * rowSpacing);
                    _canvas.Children.Add(hexBlock);
                }

                await System.Threading.Tasks.Task.Delay(intervalMs);
            }
        }
    }
}