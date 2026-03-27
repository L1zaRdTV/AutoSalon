using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public static class UiAssets
    {
        public static readonly Color Background = Color.FromArgb(245, 247, 252);
        public static readonly Color Surface = Color.White;
        public static readonly Color Accent = Color.FromArgb(31, 92, 175);
        public static readonly Color AccentSoft = Color.FromArgb(223, 235, 252);
        public static readonly Color TextPrimary = Color.FromArgb(34, 40, 49);
        public static readonly Color Border = Color.FromArgb(218, 223, 232);

        public static Bitmap CreateLogoBitmap(int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Background);
                using (var brush = new SolidBrush(Accent))
                {
                    g.FillEllipse(brush, 10, 20, 60, 40);
                    g.FillEllipse(brush, 55, 20, 60, 40);
                    g.FillRectangle(brush, 28, 18, 65, 30);
                }
                using (var font = new Font("Segoe UI", 20, FontStyle.Bold))
                using (var textBrush = new SolidBrush(TextPrimary))
                {
                    g.DrawString("AutoSalon", font, textBrush, 130, 30);
                }
            }
            return bmp;
        }

        public static void ApplyFormTheme(Form form)
        {
            form.BackColor = Background;
            form.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            ApplyThemeToControls(form.Controls);
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TabControl tabControl)
                {
                    tabControl.Appearance = TabAppearance.Normal;
                    tabControl.ItemSize = new Size(130, 32);
                    tabControl.SizeMode = TabSizeMode.Fixed;
                }
                else if (control is TabPage tabPage)
                {
                    tabPage.BackColor = Background;
                }
                else if (control is DataGridView grid)
                {
                    ApplyGridStyle(grid);
                }
                else if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Border;
                    button.FlatAppearance.BorderSize = 1;
                    button.BackColor = Accent;
                    button.ForeColor = Color.White;
                    button.Padding = new Padding(8, 4, 8, 4);
                    button.Cursor = Cursors.Hand;
                }
                else if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Surface;
                    textBox.ForeColor = TextPrimary;
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.BackColor = Surface;
                    comboBox.ForeColor = TextPrimary;
                }
                else if (control is NumericUpDown num)
                {
                    num.BorderStyle = BorderStyle.FixedSingle;
                    num.BackColor = Surface;
                    num.ForeColor = TextPrimary;
                }
                else if (control is Label label)
                {
                    label.ForeColor = TextPrimary;
                }
                else if (control is Panel || control is TableLayoutPanel || control is FlowLayoutPanel)
                {
                    control.BackColor = Background;
                }

                if (control.HasChildren)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        public static void ApplyGridStyle(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = Border;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = AccentSoft;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Accent;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersHeight = 36;
            grid.RowTemplate.Height = 32;
        }
    }
}
