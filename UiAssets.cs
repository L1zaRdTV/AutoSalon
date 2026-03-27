using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public static class UiAssets
    {
        public static readonly Color Background = Color.FromArgb(236, 233, 216);
        public static readonly Color Surface = Color.FromArgb(236, 233, 216);
        public static readonly Color SurfaceMuted = Color.FromArgb(246, 245, 237);
        public static readonly Color Accent = Color.FromArgb(10, 36, 106);
        public static readonly Color AccentHover = Color.FromArgb(29, 56, 134);
        public static readonly Color AccentSoft = Color.FromArgb(198, 211, 255);
        public static readonly Color TextPrimary = Color.Black;
        public static readonly Color TextSecondary = Color.FromArgb(64, 64, 64);
        public static readonly Color Border = Color.FromArgb(172, 168, 153);
        public static readonly Color Success = Color.FromArgb(0, 128, 0);

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
            form.Font = new Font("Tahoma", 9f, FontStyle.Regular);
            form.ForeColor = TextPrimary;
            ApplyThemeToControls(form.Controls);
        }

        public static Panel CreateSurfacePanel(DockStyle dock, Padding? padding = null)
        {
            return new Panel
            {
                Dock = dock,
                BackColor = Surface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = padding ?? new Padding(0),
                Margin = new Padding(12)
            };
        }

        public static void StylePrimaryButton(Button button)
        {
            button.UseVisualStyleBackColor = true;
            button.FlatStyle = FlatStyle.Standard;
            button.BackColor = SystemColors.Control;
            button.ForeColor = TextPrimary;
            button.Padding = new Padding(8, 4, 8, 4);
            button.Cursor = Cursors.Hand;
            button.AutoSize = true;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.Font = new Font("Tahoma", 9f, FontStyle.Regular);
        }

        public static void StyleSecondaryButton(Button button)
        {
            StylePrimaryButton(button);
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TabControl tabControl)
                {
                    tabControl.Appearance = TabAppearance.Normal;
                    tabControl.ItemSize = new Size(140, 28);
                    tabControl.SizeMode = TabSizeMode.Fixed;
                    tabControl.Padding = new Point(10, 4);
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
                    textBox.BorderStyle = BorderStyle.Fixed3D;
                    textBox.BackColor = Surface;
                    textBox.ForeColor = TextPrimary;
                    textBox.Margin = new Padding(3, 5, 3, 5);
                }
                else if (control is ComboBox comboBox)
                {
                    comboBox.FlatStyle = FlatStyle.Standard;
                    comboBox.BackColor = Surface;
                    comboBox.ForeColor = TextPrimary;
                    comboBox.Margin = new Padding(3, 5, 3, 5);
                }
                else if (control is NumericUpDown num)
                {
                    num.BorderStyle = BorderStyle.Fixed3D;
                    num.BackColor = Surface;
                    num.ForeColor = TextPrimary;
                    num.Margin = new Padding(3, 5, 3, 5);
                }
                else if (control is Label label)
                {
                    label.ForeColor = TextPrimary;
                    label.Font = new Font("Tahoma", 9f, FontStyle.Regular);
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
                    control.BackColor = Surface;
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
            grid.BorderStyle = BorderStyle.Fixed3D;
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
            grid.DefaultCellStyle.Font = new Font("Tahoma", 9, FontStyle.Regular);
            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceMuted;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9, FontStyle.Bold);
            grid.ColumnHeadersHeight = 30;
            grid.RowTemplate.Height = 26;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }
    }
}
