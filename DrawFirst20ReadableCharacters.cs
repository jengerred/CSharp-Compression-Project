using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CompressionProject
{
    class DrawFirst20ReadableCharacters
    {
        public static void DrawFirst20ReadableChars(Canvas canvas, List<char> chars)
        {
            canvas.Children.Clear();

            double charSpacing = 40;
            double startX = 10;
            double y = 20;

            for (int i = 0; i < chars.Count; i++)
            {
                var text = new TextBlock
                {
                    Text = chars[i].ToString(),
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Green // Use a different color if you want to distinguish
                };
                Canvas.SetLeft(text, startX + i * charSpacing);
                Canvas.SetTop(text, y);
                canvas.Children.Add(text);
            }
        }


    }
}
