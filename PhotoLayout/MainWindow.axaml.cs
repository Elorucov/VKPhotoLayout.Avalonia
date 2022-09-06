using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PhotoLayout {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Sizes.Text = "720x1280\n720x1280\n1600x1200\n300x200\n960x1280";

            GenButton.Click += GenButton_Click;
        }

        private void GenButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            List<Size> sizes = new List<Size>();
            Result.Text = string.Empty;
            ContainerParent.Children.Clear();
            if (string.IsNullOrEmpty(Sizes.Text)) return;

            int cw = 0, ch = 0;
            if (!int.TryParse(ContainerWidth.Text, out cw)) return;
            if (!int.TryParse(ContainerHeight.Text, out ch)) return;
            if (cw <= 0 || ch <= 0) return;
            ContainerPlaceholder.Width = cw;
            ContainerPlaceholder.Height = ch;

            var sz = Sizes.Text.Split('\n');
            foreach (string size in sz) {
                int width = 0;
                int height = 0;
                var wh = size.Split('x');
                if (wh.Length != 2) return; 
                if (!int.TryParse(wh[0], out width)) return; 
                if (!int.TryParse(wh[1], out height)) return;
                if (width <= 0 || height <= 0) return;
                sizes.Add(new Size(width, height));
                if (sizes.Count == 10) break;
            }

            var result = PhotoLayout.GenerateLayout(sizes, 4, cw, ch, GenerateBorder, (s) => Result.Text += s);
            ContainerParent.Children.Add(result);
        }

        private Control GenerateBorder(Thumb thumb) {
            return new Rectangle {
                Width = thumb.Width,
                Height = thumb.Height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                UseLayoutRounding = true,
                Fill = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)),
            };
        }
    }
}