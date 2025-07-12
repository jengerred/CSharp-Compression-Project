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
    public static class Raw20CharactersAnimation
    {
        public static void DrawFirst20RawCharacters(Canvas canvas, List<char> chars)
        {
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
                    Foreground = Brushes.Blue
                };
                Canvas.SetLeft(text, startX + i * charSpacing);
                Canvas.SetTop(text, y);
                canvas.Children.Add(text);
            }
        }
    }
}
