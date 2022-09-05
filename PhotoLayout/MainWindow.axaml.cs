using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoLayout {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Sizes.Text = "720x1280\n720x1280\n1600x1200\n300x200\n960x1280";

            GenButton.Click += GenButton_Click;
        }

        private void GenButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
            List<Thumb> thumbs = new List<Thumb>();
            Result.Text = string.Empty;
            Container.Children.Clear();
            if (string.IsNullOrEmpty(Sizes.Text)) return;

            int cw = 0, ch = 0;
            if (!int.TryParse(ContainerWidth.Text, out cw)) return;
            if (!int.TryParse(ContainerHeight.Text, out ch)) return;
            if (cw <= 0 || ch <= 0) return;
            Container.Width = cw;
            ContainerPlaceholder.Width = cw;
            ContainerPlaceholder.Height = ch;

            var sizes = Sizes.Text.Split('\n');
            foreach (string size in sizes) {
                int width = 0;
                int height = 0;
                var wh = size.Split('x');
                if (wh.Length != 2) return; 
                if (!int.TryParse(wh[0], out width)) return; 
                if (!int.TryParse(wh[1], out height)) return;
                if (width <= 0 || height <= 0) return;
                thumbs.Add(new Thumb {
                    Width = width,
                    Height = height
                });
                if (thumbs.Count == 10) break;
            }

            var result = PhotoLayout.Calculate(thumbs, 4, cw, ch);
            GenerateLayout(result);
        }

        private void GenerateLayout(List<Thumb> thumbs) {
            bool generatedColumn = false;
            bool isEndFirstRow = false;
            double height = 0;

            Container.Children.Add(new StackPanel { Orientation = Orientation.Horizontal });

            for (int i = 0; i < thumbs.Count; i++) {
                Thumb thumb = thumbs[i];
                Result.Text += $"{thumb.Width}x{thumb.Height}; colitem: {thumb.ColumnItem}; lastc: {thumb.LastColumn}; lastr: {thumb.LastRow}; end: {thumb.EndFirstRow}\n";

                StackPanel panel = (StackPanel)Container.Children.Last();
                Thumb nextThumb = i < thumbs.Count - 1 ? thumbs[i + 1] : null;

                if (generatedColumn && thumb.ColumnItem) {
                    continue;
                }

                if (thumb.LastColumn && !isEndFirstRow) {
                    thumb.EndFirstRow = true;
                    isEndFirstRow = true;
                }

                if (thumb.ColumnItem) {
                    generatedColumn = true;

                    StackPanel span = new StackPanel();
                    foreach (Thumb t in thumbs.Where(t => t.ColumnItem)) {
                        var control = GenerateRect(t);
                        control.Margin = new Thickness(0, 0, thumb.LastColumn ? 0 : 4, thumb.LastRow ? 0 : 4);
                        span.Children.Add(control);
                    }
                    panel.Children.Add(span);
                } else {
                    if (thumb.LastColumn || (nextThumb != null && nextThumb.ColumnItem)) {
                        height += thumb.Height + (thumb.LastRow ? 0 : 4);
                    }
                    var control = GenerateRect(thumb);
                    control.Margin = new Thickness(0, 0, thumb.LastColumn ? 0 : 4, thumb.LastRow ? 0 : 4);
                    panel.Children.Add(control);
                }

                if (thumb.LastColumn && !thumb.LastRow) {
                    Container.Children.Add(new StackPanel { Orientation = Orientation.Horizontal });
                }
            }
        }

        private Control GenerateRect(Thumb thumb) {
            return new Rectangle {
                Width = thumb.Width,
                Height = thumb.Height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Fill = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128))
            };
        }
    }
}