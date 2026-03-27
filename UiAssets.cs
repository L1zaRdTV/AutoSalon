using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public static class UiAssets
    {
        public static readonly Color Background = Color.FromArgb(243, 247, 255);
        public static readonly Color Surface = Color.White;
        public static readonly Color SurfaceMuted = Color.FromArgb(248, 250, 255);
        public static readonly Color Accent = Color.FromArgb(58, 111, 220);
        public static readonly Color AccentHover = Color.FromArgb(47, 96, 196);
        public static readonly Color AccentSoft = Color.FromArgb(225, 235, 255);
        public static readonly Color TextPrimary = Color.FromArgb(28, 36, 52);
        public static readonly Color TextSecondary = Color.FromArgb(96, 109, 132);
        public static readonly Color Border = Color.FromArgb(214, 223, 240);
        public static readonly Color Success = Color.FromArgb(39, 139, 88);

        public static Bitmap CreateLogoBitmap(int width, int height)
        {
            var bmp = new Bitmap(width, height);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Background);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var shadow = new SolidBrush(Color.FromArgb(70, 88, 127, 196)))
                using (var brush = new SolidBrush(Accent))
                {
                    g.FillEllipse(shadow, 11, 26, 116, 38);
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
            form.ForeColor = TextPrimary;
            ApplyThemeToControls(form.Controls);
        }

        public static Panel CreateSurfacePanel(DockStyle dock, Padding? padding = null)
        {
            return new Panel
            {
                Dock = dock,
                BackColor = Surface,
                Padding = padding ?? new Padding(0),
                Margin = new Padding(12)
            };
        }

        public static void StylePrimaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Accent;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = AccentHover;
            button.FlatAppearance.MouseDownBackColor = AccentHover;
            button.BackColor = Accent;
            button.ForeColor = Color.White;
            button.Padding = new Padding(10, 5, 10, 5);
            button.Cursor = Cursors.Hand;
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        }

        public static void StyleSecondaryButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = SurfaceMuted;
            button.BackColor = Surface;
            button.ForeColor = TextPrimary;
            button.Padding = new Padding(10, 5, 10, 5);
            button.Cursor = Cursors.Hand;
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TabControl tabControl)
                {
                    tabControl.Appearance = TabAppearance.Normal;
                    tabControl.ItemSize = new Size(150, 36);
                    tabControl.SizeMode = TabSizeMode.Fixed;
                    tabControl.Padding = new Point(18, 8);
                    tabControl.BackColor = Background;
                }
                else if (control is TabPage tabPage)
                {
                    tabPage.BackColor = Background;
                    tabPage.Padding = new Padding(12);
                }
                else if (control is DataGridView grid)
                {
                    ApplyGridStyle(grid);
                }
                else if (control is Button button)
                {
                    StylePrimaryButton(button);
                }
                else if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    textBox.BackColor = Surface;
                    textBox.ForeColor = TextPrimary;
                    textBox.Margin = new Padding(3, 5, 3, 5);
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.FlatStyle = FlatStyle.Flat;
                    comboBox.BackColor = Surface;
                    comboBox.ForeColor = TextPrimary;
                    comboBox.Margin = new Padding(3, 5, 3, 5);
                }
                else if (control is NumericUpDown num)
                {
                    num.BorderStyle = BorderStyle.FixedSingle;
                    num.BackColor = Surface;
                    num.ForeColor = TextPrimary;
                    num.Margin = new Padding(3, 5, 3, 5);
                }
                else if (control is Label label)
                {
                    label.ForeColor = TextPrimary;
                    label.Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold);
                    label.Margin = new Padding(3, 8, 3, 3);
                }
                else if (control is GroupBox group)
                {
                    group.ForeColor = TextPrimary;
                    group.BackColor = Surface;
                    group.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else if (control is TableLayoutPanel)
                {
                    control.BackColor = Background;
                }
                else if (control is FlowLayoutPanel flow)
                {
                    flow.BackColor = Background;
                    flow.WrapContents = false;
                    flow.AutoScroll = true;
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
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Accent;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.ColumnHeadersHeight = 40;
            grid.RowTemplate.Height = 34;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }
    }
}
