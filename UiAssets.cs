using System.Drawing;

namespace AutoSalon
{
    public static class UiAssets
    {
        public static Bitmap CreateLogoBitmap(int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.WhiteSmoke);
                using (var brush = new SolidBrush(Color.FromArgb(30, 80, 150)))
                {
                    g.FillEllipse(brush, 10, 20, 60, 40);
                    g.FillEllipse(brush, 55, 20, 60, 40);
                    g.FillRectangle(brush, 28, 18, 65, 30);
                }
                using (var font = new Font("Segoe UI", 20, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.FromArgb(20, 20, 20)))
                {
                    g.DrawString("AutoSalon", font, textBrush, 130, 30);
                }
            }
            return bmp;
        }
    }
}
