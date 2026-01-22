using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreshCheck_CV.Defect
{
    public static class DefectImageSaver
    {
        public static void Save(Bitmap sourceBitmap, DefectType type, DateTime timestamp, string labelText)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException(nameof(sourceBitmap));

            string root = AppDomain.CurrentDomain.BaseDirectory;

            // OK / Mold만 현재 사용
            string typeFolder = (type == DefectType.Mold) ? "Mold" : "OK";

            string dateFolder = timestamp.ToString("yyyy-MM-dd");
            string fileName = $"{timestamp:HH-mm-ss-fff}.png";

            string dir = Path.Combine(root, "Defect", typeFolder, dateFolder);
            Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, fileName);

            using (Bitmap overlay = DrawLabelTopLeft(sourceBitmap, labelText))
            {
                overlay.Save(path, ImageFormat.Png);
            }
        }

        private static Bitmap DrawLabelTopLeft(Bitmap source, string text)
        {
            Bitmap bmp = new Bitmap(source);

            using (Graphics g = Graphics.FromImage(bmp))
            using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
            {
                string safeText = text ?? string.Empty;

                SizeF size = g.MeasureString(safeText, font);
                RectangleF rect = new RectangleF(8, 8, size.Width + 20, size.Height + 12);

                using (Brush bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                using (Brush fg = new SolidBrush(Color.White))
                {
                    g.FillRectangle(bg, rect);
                    g.DrawString(safeText, font, fg, 18, 14);
                }
            }

            return bmp;
        }
    }
}
