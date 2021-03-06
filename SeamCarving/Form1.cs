using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageEffects;

namespace SeamCarving
{
    public class Form1 : Form
    {
        private Bitmap _startImage;

        private Bitmap _resultImage;

        private TableLayoutPanel _layoutPanel;

        public Form1()
        {
            _startImage = new Bitmap(Image.FromFile("Assets/vladek.jpg"));
            _layoutPanel = new TableLayoutPanel {Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2};
            _layoutPanel.Layout += (sender, args) => ClientSize = new Size(_layoutPanel.Width, _layoutPanel.Height);

            Controls.Add(_layoutPanel);

            var pixels = Utils.LoadPixels(_startImage);


            var resizeFactor = 1;

            var pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                Image = Utils.ConvertToBitmap(pixels),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Size = new Size(pixels.GetLength(0) * resizeFactor, pixels.GetLength(1) * resizeFactor)
            };

            _layoutPanel.Controls.Add(pictureBox, 0, 0);

            var im = pixels;
            var progress = new Progress<(Image, double)>(t =>
            {
                pictureBox.Image = t.Item1;
                Text = $"Average iteration time:: {t.Item2}";
            });

            KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.S)
                    Task.Run(() => DoSeamCarving(progress, im))
                        .ContinueWith(task => Text = $"Average iteration time: {task.Result} ms");
            };
        }

        private double DoSeamCarving(IProgress<(Image, double)> progress, Pixel[,] im)
        {
            var times = new List<double>();
            for (var i = 0; i < 100; i++)
            {
                var stopWatch = new Stopwatch();
                stopWatch.Restart();
                var energyMap = ImageEffects.SeamCarving.MakeEnergyMap(im);
                var indexMap = ImageEffects.SeamCarving.MakeVerticalIndexMap(energyMap);
                var sums = ImageEffects.SeamCarving.CalculateSeams(energyMap, indexMap);
                var bestSeam = ImageEffects.SeamCarving.GetBestSeam(sums, indexMap);
                var marked = ImageEffects.SeamCarving.MarkSeam(im, bestSeam);

             //   progress.Report((Utils.ConvertToBitmap(marked), i));

                var removed = ImageEffects.SeamCarving.RemoveSeamsVertical(marked, bestSeam);

                im = removed;
                progress.Report((Utils.ConvertToBitmap(im), stopWatch.ElapsedMilliseconds));
            }

            return times.Average();
        }

        private void AddToForm(Bitmap bmp, int row, int column)
        {
            var picturebox = new PictureBox {Dock = DockStyle.Fill, Image = bmp, Size = bmp.Size};

            _layoutPanel.Controls.Add(picturebox, row, column);
        }
    }
}