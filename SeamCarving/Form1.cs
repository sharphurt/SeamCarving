using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            _startImage = new Bitmap(Image.FromFile("Assets/img2.png"));
            _layoutPanel = new TableLayoutPanel {Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2};
            _layoutPanel.Layout += (sender, args) => ClientSize = new Size(_layoutPanel.Width, _layoutPanel.Height);

            Controls.Add(_layoutPanel);

            /*AddToForm(_startImage, 0, 0);*/
            var pixels = Utils.LoadPixels(_startImage);

            /*
            var grayscale = GrayscaleTask.ToGrayscale(pixels);
            var edges = SobelFilter.Filter(grayscale, new[,] {{-0.5, -1, -0.5}, {0, 0, 0}, {0.5, 1, 0.5}});
            */

            /*
            AddToForm(Utils.ConvertToBitmap(edges), 0, 0);
            var energyMatrix = ImageEffects.SeamCarving.MakeIntensityMatrix(Utils.ConvertToPixels(edges));
            */

            /*
            var representation = ImageEffects.SeamCarving.GrayScaleEnergyRepresentation(energyMatrix);
            AddToForm(Utils.ConvertToBitmap(representation), 0, 1);
            */

            var resizeFactor = 1;

            var pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill, Image = Utils.ConvertToBitmap(pixels),
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(pixels.GetLength(0) * resizeFactor, pixels.GetLength(1) * resizeFactor)
            };

            _layoutPanel.Controls.Add(pictureBox, 0, 0);

            var im = pixels;
            var progress = new Progress<(Image, int)>(t =>
            {
                pictureBox.Image = t.Item1;
                Text = $"Step: {t.Item2 + 1} in 100";
            });

            KeyDown += (sender, args) =>
           {
                if (args.KeyCode == Keys.S)
                    Task.Run(() => DoSeamCarving(progress, im));
           };
        }

        private void DoSeamCarving(IProgress<(Image, int)> progress, Pixel[,] im)
        {
            for (var i = 0; i < 100; i++)
            {
                var energyMatrix = ImageEffects.SeamCarving.MakeIntensityMatrix(im);
                var marked = ImageEffects.SeamCarving.MarkSeamVertical(im, energyMatrix);
                progress.Report((Utils.ConvertToBitmap(marked), i));
                var removed = ImageEffects.SeamCarving.RemoveSeamsVertical(marked);
                progress.Report((Utils.ConvertToBitmap(removed), i));
                
                im = removed;
                
                /*
                energyMatrix = ImageEffects.SeamCarving.MakeIntensityMatrix(im);
                marked = ImageEffects.SeamCarving.MarkSeamHorizontally(im, energyMatrix);
                progress.Report((Utils.ConvertToBitmap(marked), i));
                removed = ImageEffects.SeamCarving.RemoveSeamsHorizontally(marked);
                progress.Report((Utils.ConvertToBitmap(removed), i));
                im = removed;*/
            }
        }

        private void AddToForm(Bitmap bmp, int row, int column)
        {
            var picturebox = new PictureBox {Dock = DockStyle.Fill, Image = bmp, Size = bmp.Size};

            _layoutPanel.Controls.Add(picturebox, row, column);
        }
    }
}