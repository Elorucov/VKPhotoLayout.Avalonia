using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PhotoLayout {
    public class PhotoLayout {
        private static double GetMultiThumbsHeight(List<double> ratios, double width, int margin) {
            return (width - (ratios.Count - 1) * margin) / ratios.Sum();
        }

        private static void UpdateThumb(Thumb thumb, double width, double height, bool lastColumn, bool lastRow, bool columnItem = false) {
            thumb.Width = (int)Math.Round(width);
            thumb.Height = (int)Math.Round(columnItem ? height : Math.Max(height, 50));

            if (lastColumn) thumb.LastColumn = lastColumn;
            if (lastRow) thumb.LastRow = lastRow;
            if (columnItem) thumb.ColumnItem = columnItem;
        }

        private static List<Thumb> Calculate(List<Thumb> thumbs, int margin, int maxWidth, int maxHeight) {
            List<double> photoRatios = new List<double>();
            string photoRatioTypes = String.Empty;

            foreach (Thumb thumb in thumbs) {
                if (thumb.Width == 0 && thumb.Height == 0) {
                    thumb.Width = 100;
                    thumb.Height = 100;
                }

                double ratio = Math.Max((double)thumb.Width / (double)thumb.Height, .3);

                photoRatioTypes += ratio > 1.2 ? 'w' : (ratio < .8 ? 'n' : 'q');
                photoRatios.Add(ratio);
            }

            if (thumbs.Count == 2) {
                if (photoRatioTypes == "ww" &&
                    (Math.Abs(photoRatios[1] - photoRatios[0]) < .3 || photoRatios[0] >= 2 && photoRatios[1] >= 2)) {
                    // Одинаковая высота и ширина
                    // Только если фотографии похожи по соотношению
                    // [photo]
                    // [photo]
                    double width = Math.Min(maxWidth, Math.Max(thumbs[0].Width, thumbs[1].Width));
                    double mediumHeight = Math.Min((thumbs[0].Height + thumbs[1].Height) / 2, maxHeight);
                    double height = Math.Min((width / photoRatios[0] + width / photoRatios[1]) / 2,
                      (mediumHeight - margin) / 2);

                    UpdateThumb(thumbs[0], width, height, true, false);
                    UpdateThumb(thumbs[1], width, height, true, true);
                } else {
                    // Одинаковая высота, но разная ширина
                    // [photo][photo]
                    double minWidth = Math.Min(Math.Max((double)(thumbs[0].Width + thumbs[1].Width) / 2, 200), maxWidth);
                    double widthModifier = (minWidth - margin) / (photoRatios[0] + photoRatios[1]);
                    double width1 = widthModifier * photoRatios[0];
                    double width2 = widthModifier * photoRatios[1];

                    // Одна из двух переменных точно > 100
                    if (width1 < 100) {
                        width1 = 100;
                        width2 = width2 - (100 - width1);
                    }
                    if (width2 < 100) {
                        width1 = width1 - (100 - width2);
                        width2 = 100;
                    }

                    double height = Math.Min(maxHeight, ((double)width1 / photoRatios[0] + (double)width2 / photoRatios[1]) / 2);

                    UpdateThumb(thumbs[0], width1, height, false, true);
                    UpdateThumb(thumbs[1], width2, height, true, true);
                }
            } else if (thumbs.Count == 3) {
                if (photoRatioTypes == "www") {
                    // [photo1]
                    // [p2][p3]
                    double width1 = Math.Min(maxWidth, Math.Max(thumbs[0].Width, (double)(thumbs[1].Width + thumbs[2].Width) / 2 + margin));
                    double width2 = (width1 - margin) / 2;
                    double height1 = Math.Min(width1 / photoRatios[0], (maxHeight - margin) * (2 / 3));
                    double height2 = Math.Min(maxHeight - height1 - margin, (width2 / photoRatios[1] + width2 / photoRatios[2]) / 2);

                    UpdateThumb(thumbs[0], width1, height1, true, false);
                    UpdateThumb(thumbs[1], width2, height2, false, true);
                    UpdateThumb(thumbs[2], width2, height2, true, true);
                } else {
                    // [photo1][p2]
                    // [      ][p3]
                    double mediumRightPhotosHeight = (thumbs[1].Height + thumbs[2].Height) / 2;
                    double height1 = Math.Min(maxHeight,
                      new double[] {
                        Math.Min(thumbs[0].Height, mediumRightPhotosHeight * 1.5),
                        Math.Min(mediumRightPhotosHeight, thumbs[0].Height * 1.5),
                        mediumRightPhotosHeight * .75
                    }.Max());
                    // Высота p3
                    double height2 = photoRatios[1] * (height1 - margin) / (photoRatios[2] + photoRatios[1]);
                    // Высота p2
                    double height3 = height1 - height2 - margin;
                    double width1 = Math.Min(height1 * photoRatios[0], (maxWidth - margin) * .75);
                    double width2 = Math.Min(
                      maxWidth - width1 - margin,
                      Math.Max(
                        (height3 * photoRatios[1] + height2 * photoRatios[2]) / 2,
                        width1 / 3 // (3/4) / 3 = 1/4
                      )
                    );

                    UpdateThumb(thumbs[0], width1, height1, false, true);
                    UpdateThumb(thumbs[1], width2, height3, true, false, true);
                    UpdateThumb(thumbs[2], width2, height2, true, true, true);
                }
            } else if (thumbs.Count == 4) {
                if (photoRatioTypes == "wwww") {
                    // Вытянутые по горизонтали фотографии
                    // [  photo1  ]
                    // [p2][p3][p4]
                    // const [t0, t1, t2, t3] = thumbs;
                    double minWidth = Math.Min(
                      Math.Max((double)(thumbs[1].Width + thumbs[2].Width + thumbs[3].Width) / 3, 250),
                      maxWidth
                    );
                    double widthModifier = (
                      (minWidth - margin * 2) / (photoRatios[1] + photoRatios[2] + photoRatios[3])
                    );
                    double width1 = minWidth;
                    double width2 = widthModifier * photoRatios[1];
                    double width3 = widthModifier * photoRatios[2];
                    double width4 = widthModifier * photoRatios[3];
                    double height1 = Math.Min(width1 / photoRatios[0], (minWidth - margin) * (2 / 3));
                    double height2 = Math.Min(maxHeight - height1 - margin, widthModifier);

                    UpdateThumb(thumbs[0], width1, height1, true, false);
                    UpdateThumb(thumbs[1], width2, height2, false, true);
                    UpdateThumb(thumbs[2], width3, height2, false, true);
                    UpdateThumb(thumbs[3], width4, height2, true, true);
                } else {
                    // Вытянутые по вертикали фотографии
                    // [      ][p2]
                    // [photo1][p3]
                    // [      ][p4]
                    double minHeight = Math.Min(
                      Math.Max((thumbs[1].Height + thumbs[2].Height + thumbs[3].Height) / 3, 280),
                      maxHeight
                    );
                    double heightModifier = (
                      (minHeight - margin * 2) / (1 / photoRatios[1] + 1 / photoRatios[2] + 1 / photoRatios[3])
                    );
                    double height1 = minHeight;
                    double height2 = heightModifier / photoRatios[1];
                    double height3 = heightModifier / photoRatios[2];
                    double height4 = heightModifier / photoRatios[3];
                    double width1 = Math.Min(height1 * photoRatios[0], (double)(maxWidth - margin) * ((double)2 / (double)3));
                    double width2 = Math.Min(maxWidth - width1 - margin, Math.Max(heightModifier, 65));

                    UpdateThumb(thumbs[0], width1, height1, false, true);
                    UpdateThumb(thumbs[1], width2, height2, true, false, true);
                    UpdateThumb(thumbs[2], width2, height3, true, false, true);
                    UpdateThumb(thumbs[3], width2, height4, true, true, true);
                }
            } else {
                Dictionary<List<int>, List<double>> photosLayoutVariants = new Dictionary<List<int>, List<double>>();
                photosLayoutVariants.Add(new List<int> { thumbs.Count }, new List<double> { GetMultiThumbsHeight(photoRatios, maxWidth, margin) });

                int minPhotosAtFirstRow = thumbs.Count < 7 ? 1 : 2;

                for (int i = minPhotosAtFirstRow; i < thumbs.Count - 1; i++) {
                    photosLayoutVariants.Add(new List<int> { i, thumbs.Count - i }, new List<double> {
                        GetMultiThumbsHeight(photoRatios.GetRange(0, i), maxWidth, margin),
                        GetMultiThumbsHeight(photoRatios.GetRange(i, thumbs.Count - i), maxWidth, margin)
                    });
                }

                for (int i = minPhotosAtFirstRow; i < thumbs.Count - 1; i++) {
                    for (int j = 1; j < thumbs.Count - i; j++) {
                        photosLayoutVariants.Add(new List<int> { i, j, thumbs.Count - i - j }, new List<double> {
                            GetMultiThumbsHeight(photoRatios.GetRange(0, i), maxWidth, margin),
                            GetMultiThumbsHeight(photoRatios.GetRange(i, j), maxWidth, margin),
                            GetMultiThumbsHeight(photoRatios.GetRange(i + j, thumbs.Count - (i + j)), maxWidth, margin)
                        });
                    }
                }

                // Оптимальное расположение фотографий рассчитывается путем
                // нахождения положения фотографий с наименьшей разницей между
                // высотой родителя и высотой своей сетки.
                // Все возможные положения находятся в photosLayoutVariants.
                List<int> optimalPhotosLayout = null;
                double minHeightDiff = 0;
                double minPhotoWidth = 65;
                List<Tuple<List<int>, List<double>>> reservedLayoutVariants = new List<Tuple<List<int>, List<double>>>();

                foreach (var variant in photosLayoutVariants) {
                    List<double> photosHeight = variant.Value;
                    List<int> photosInRows = variant.Key;
                    double origDiff = (photosHeight.Sum() + margin * (photosHeight.Count - 1)) - maxHeight;

                    // Слишком большая высота стоит дороже, потому что выглядит не очень красиво
                    double heightDiff = origDiff > 0 ? origDiff + 50 : -origDiff;
                    int index = 0;
                    bool isPhotoWidthLessThanMin = false;

                    reservedLayoutVariants.Add(new Tuple<List<int>, List<double>>(variant.Key, new List<double>()));
                    int reverseIndex = reservedLayoutVariants.Count - 1;

                    for (int row = 0; row < photosInRows.Count(); row++) {
                        for (int column = 0; column < photosInRows[row]; column++) {
                            double width = photoRatios[index++] * photosHeight[row];
                            reservedLayoutVariants[reverseIndex].Item2.Add(width);
                            isPhotoWidthLessThanMin = width < minPhotoWidth;
                        }
                    }

                    if (isPhotoWidthLessThanMin) {
                        continue;
                    }



                    if (optimalPhotosLayout == null || heightDiff < minHeightDiff) {
                        optimalPhotosLayout = variant.Key;
                        minHeightDiff = heightDiff;
                    }
                }

                if (optimalPhotosLayout == null) {
                    double veryMinWidth = 0;
                    foreach (var variant in reservedLayoutVariants) {
                        double minWidth = variant.Item2.Min();
                        if (veryMinWidth == 0 || minWidth < veryMinWidth) {
                            veryMinWidth = minWidth;
                            optimalPhotosLayout = variant.Item1;
                        }
                    }
                }

                var rowHeights = photosLayoutVariants[optimalPhotosLayout];
                int photoIndex = 0;

                for (int column = 0; column < optimalPhotosLayout.Count; column++) {
                    int photosInRow = optimalPhotosLayout[column];
                    double rowHeight = rowHeights[column];

                    for (int row = 0; row < photosInRow; row++) {
                        int index = photoIndex++;
                        double width = photoRatios[index] * rowHeight;
                        bool lastColumn = row == photosInRow - 1;
                        bool lastRow = column == optimalPhotosLayout.Count - 1;

                        UpdateThumb(thumbs[index], width, rowHeight, lastColumn, lastRow);
                    }
                }
            }

            return thumbs;
        }

        private static StackPanel GenerateLayoutInternal(List<Thumb> thumbs, int margin, Func<Thumb, Control> generateControlAction, Action<string>? debug = null) {
            StackPanel container = new StackPanel();

            bool generatedColumn = false;
            bool isEndFirstRow = false;
            double height = 0;

            container.Children.Add(new StackPanel { Orientation = Orientation.Horizontal });

            for (int i = 0; i < thumbs.Count; i++) {
                Thumb thumb = thumbs[i];
                debug?.Invoke($"{thumb.Width}x{thumb.Height}; colitem: {thumb.ColumnItem}; lastc: {thumb.LastColumn}; lastr: {thumb.LastRow}; end: {thumb.EndFirstRow}\n");

                StackPanel panel = (StackPanel)container.Children.Last();
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
                        var control = generateControlAction.Invoke(t);
                        control.Margin = new Thickness(0, 0, thumb.LastColumn ? 0 : margin, thumb.LastRow ? 0 : margin);
                        span.Children.Add(control);
                    }
                    panel.Children.Add(span);
                } else {
                    if (thumb.LastColumn || (nextThumb != null && nextThumb.ColumnItem)) {
                        height += thumb.Height + (thumb.LastRow ? 0 : margin);
                    }
                    var control = generateControlAction.Invoke(thumb);
                    control.Margin = new Thickness(0, 0, thumb.LastColumn ? 0 : margin, thumb.LastRow ? 0 : margin);
                    panel.Children.Add(control);
                }

                if (thumb.LastColumn && !thumb.LastRow) {
                    container.Children.Add(new StackPanel { Orientation = Orientation.Horizontal });
                }
            }

            return container;
        }

        public static StackPanel GenerateLayout(List<Size> sizes, int margin, int maxWidth, int maxHeight, Func<Thumb, Control> generateControlAction, Action<string>? debug = null) {
            Stopwatch sw = Stopwatch.StartNew();
            List<Thumb> thumbs = new List<Thumb>();
            foreach (var size in CollectionsMarshal.AsSpan(sizes)) {
                thumbs.Add(new Thumb { 
                    Width = (int)Math.Round(size.Width),
                    Height = (int)Math.Round(size.Height)
                });
            }

            var calculated = Calculate(thumbs, margin, maxWidth, maxHeight);
            var container = GenerateLayoutInternal(calculated, margin, generateControlAction, debug);
            sw.Stop();

            debug?.Invoke($"Time: {sw.ElapsedMilliseconds} ms.");
            return container;
        }
    }
}