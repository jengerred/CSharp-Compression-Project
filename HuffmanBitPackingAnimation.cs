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
    public class HuffmanBitPackingAnimation
    {
        private readonly Canvas _canvas;
        private readonly List<char> _first20Chars;
        private readonly string[] _finalCodes;
        private readonly double charSpacing = 40;
        private readonly double startX = 20;
        private readonly double y = 20;
        private readonly double codeY = 44;
        private readonly double bitY = 80;
        private readonly double byteY = 110;
        private readonly double hexY = 132;
        private readonly double bitSpacing = 10;

        // Color palette for byte groups (expand as needed)
        private static readonly Color[] ByteGroupColors = new Color[]
        {
            Colors.OrangeRed,
            Colors.MediumSeaGreen,
            Colors.DodgerBlue,
            Colors.Goldenrod,
            Colors.MediumVioletRed,
            Colors.Teal,
            Colors.SlateBlue,
            Colors.CadetBlue,
            Colors.DarkOrange,
            Colors.MediumSlateBlue,
            Colors.DarkKhaki,
            Colors.Firebrick,
            Colors.Crimson,
            Colors.DarkCyan,
            Colors.OliveDrab,
            Colors.MediumTurquoise
        };

        public HuffmanBitPackingAnimation(
            Canvas canvas,
            List<char> first20Chars,
            string[] finalCodes // array of Huffman codes for each char, in order
        )
        {
            _canvas = canvas;
            _first20Chars = first20Chars;
            _finalCodes = finalCodes;
        }

        public async void StartBitPackingAnimation(int intervalMs = 900)
        {
            _canvas.Children.Clear();

            // 1. Draw original characters and their Huffman codes
            for (int i = 0; i < _first20Chars.Count; i++)
            {
                var text = new TextBlock
                {
                    Text = _first20Chars[i].ToString(),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(text, startX + i * charSpacing);
                Canvas.SetTop(text, y);
                _canvas.Children.Add(text);

                var codeText = new TextBlock
                {
                    Text = _finalCodes[i],
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Blue
                };
                Canvas.SetLeft(codeText, startX + i * charSpacing);
                Canvas.SetTop(codeText, codeY);
                _canvas.Children.Add(codeText);
            }

            // 2. Concatenate all codes into a single bitstream
            StringBuilder bitStream = new StringBuilder();
            for (int i = 0; i < _first20Chars.Count; i++)
                bitStream.Append(_finalCodes[i]);
            string bits = bitStream.ToString();

            // 3. Draw the concatenated bitstream below the codes, coloring by byte group
            for (int i = 0; i < bits.Length; i++)
            {
                int group = i / 8;
                var bitText = new TextBlock
                {
                    Text = bits[i].ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(ByteGroupColors[group % ByteGroupColors.Length])
                };
                Canvas.SetLeft(bitText, startX + i * bitSpacing);
                Canvas.SetTop(bitText, bitY);
                _canvas.Children.Add(bitText);
            }

            // 4. Animate grouping into bytes and show decimal/hex, matching group color
            int byteCount = (bits.Length + 7) / 8;
            for (int b = 0; b < byteCount; b++)
            {
                int start = b * 8;
                int len = Math.Min(8, bits.Length - start);
                string byteStr = bits.Substring(start, len).PadRight(8, '0');
                byte byteVal = Convert.ToByte(byteStr, 2);

                Color groupColor = ByteGroupColors[b % ByteGroupColors.Length];

                // Highlight this group of 8 bits
                var rect = new Rectangle
                {
                    Width = bitSpacing * 8,
                    Height = 22,
                    Fill = new SolidColorBrush(Color.FromArgb(80, groupColor.R, groupColor.G, groupColor.B)),
                    RadiusX = 5,
                    RadiusY = 5
                };
                Canvas.SetLeft(rect, startX + start * bitSpacing - 2);
                Canvas.SetTop(rect, byteY - 2);
                _canvas.Children.Add(rect);

                // Draw decimal value
                var decText = new TextBlock
                {
                    Text = byteVal.ToString(),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(decText, startX + start * bitSpacing);
                Canvas.SetTop(decText, byteY);
                _canvas.Children.Add(decText);

                // Draw hex value in group color
                var hexText = new TextBlock
                {
                    Text = byteVal.ToString("X2"),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(groupColor)
                };
                Canvas.SetLeft(hexText, startX + start * bitSpacing);
                Canvas.SetTop(hexText, hexY);
                _canvas.Children.Add(hexText);

                await System.Threading.Tasks.Task.Delay(intervalMs);
            }

            // 5. Display the full compressed output as hex at the bottom, each in group color
            double hexSummaryY = hexY + 30;
            double hexSummaryX = startX;

            // Add the label "Compressed Output (Hex):"
            var hexLabel = new TextBlock
            {
                Text = "Compressed Output (Hex):",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(hexLabel, hexSummaryX);
            Canvas.SetTop(hexLabel, hexSummaryY);
            _canvas.Children.Add(hexLabel);

            // Offset for the colored hex values so they appear after the label
            double hexValuesX = hexSummaryX + 220; // Adjust as needed for your UI

            for (int b = 0; b < byteCount; b++)
            {
                Color groupColor = ByteGroupColors[b % ByteGroupColors.Length];
                int start = b * 8;
                int len = Math.Min(8, bits.Length - start);
                string byteStr = bits.Substring(start, len).PadRight(8, '0');
                byte byteVal = Convert.ToByte(byteStr, 2);

                var hexBlock = new TextBlock
                {
                    Text = byteVal.ToString("X2") + " ",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(groupColor)
                };
                Canvas.SetLeft(hexBlock, hexValuesX + b * 32); // Space out each hex value
                Canvas.SetTop(hexBlock, hexSummaryY);
                _canvas.Children.Add(hexBlock);
            }

        }
    }
}
