using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public class ProductEditForm : Form
    {
        private readonly int? _productId;
        private readonly TextBox _txtTitle = new TextBox();
        private readonly TextBox _txtDescription = new TextBox();
        private readonly NumericUpDown _numPrice = new NumericUpDown { Maximum = 10000000, DecimalPlaces = 2 };
        private readonly NumericUpDown _numOldPrice = new NumericUpDown { Maximum = 10000000, DecimalPlaces = 2 };
        private readonly NumericUpDown _numDiscount = new NumericUpDown { Maximum = 100, Minimum = 0 };
        private readonly TextBox _txtImagePath = new TextBox { ReadOnly = true };

        public ProductEditForm(int? productId)
        {
            _productId = productId;
            Text = _productId.HasValue ? "Редактирование товара" : "Новый товар";
            Icon = SystemIcons.Application;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(560, 400);

            var card = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(12));
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 3, RowCount = 8 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label { Text = "Название" }, 0, 0);
            layout.Controls.Add(_txtTitle, 1, 0);
            layout.Controls.Add(new Label { Text = "Описание" }, 0, 1);
            layout.Controls.Add(_txtDescription, 1, 1);
            layout.Controls.Add(new Label { Text = "Цена" }, 0, 2);
            layout.Controls.Add(_numPrice, 1, 2);
            layout.Controls.Add(new Label { Text = "Старая цена" }, 0, 3);
            layout.Controls.Add(_numOldPrice, 1, 3);
            layout.Controls.Add(new Label { Text = "Скидка, %" }, 0, 4);
            layout.Controls.Add(_numDiscount, 1, 4);
            layout.Controls.Add(new Label { Text = "Изображение" }, 0, 5);
            layout.Controls.Add(_txtImagePath, 1, 5);

            var btnImage = new Button { Text = "Выбрать..." };
            btnImage.Click += (_, __) => SelectImage();
            UiAssets.StyleSecondaryButton(btnImage);
            layout.Controls.Add(btnImage, 2, 5);

            var btnSave = new Button { Text = "Сохранить", Dock = DockStyle.Fill };
            btnSave.Click += (_, __) => SaveProduct();
            UiAssets.StylePrimaryButton(btnSave);
            layout.Controls.Add(btnSave, 1, 6);

            card.Controls.Add(layout);
            Controls.Add(card);

            if (_productId.HasValue)
            {
                LoadProduct();
            }

            UiAssets.ApplyFormTheme(this);
        }

        private void SelectImage()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _txtImagePath.Text = ValidationHelper.CopyProductImage(dialog.FileName);
                }
            }
        }

        private void LoadProduct()
        {
            var table = Database.Query("SELECT TOP 1 * FROM dbo.Products WHERE Id=@Id", new SqlParameter("@Id", _productId.Value));
            if (table.Rows.Count == 0)
            {
                return;
            }

            var row = table.Rows[0];
            _txtTitle.Text = row["Title"].ToString();
            _txtDescription.Text = row["Description"].ToString();
            _numPrice.Value = Convert.ToDecimal(row["Price"]);
            _numOldPrice.Value = row["OldPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(row["OldPrice"]);
            _numDiscount.Value = row["DiscountPercent"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DiscountPercent"]);
            _txtImagePath.Text = row["ImagePath"].ToString();
        }

        private void SaveProduct()
        {
            if (string.IsNullOrWhiteSpace(_txtTitle.Text) || _numPrice.Value <= 0)
            {
                MessageBox.Show("Название и цена обязательны.");
                return;
            }

            if (_numDiscount.Value > 0 && _numOldPrice.Value <= 0)
            {
                MessageBox.Show("Для скидки необходимо указать старую цену.");
                return;
            }

            if (_productId.HasValue)
            {
                Database.Execute(@"UPDATE dbo.Products
SET Title=@Title, Description=@Description, Price=@Price, OldPrice=@OldPrice, DiscountPercent=@DiscountPercent, ImagePath=@ImagePath
WHERE Id=@Id",
                    new SqlParameter("@Title", _txtTitle.Text.Trim()),
                    new SqlParameter("@Description", _txtDescription.Text.Trim()),
                    new SqlParameter("@Price", _numPrice.Value),
                    new SqlParameter("@OldPrice", _numOldPrice.Value > 0 ? (object)_numOldPrice.Value : DBNull.Value),
                    new SqlParameter("@DiscountPercent", _numDiscount.Value > 0 ? (object)_numDiscount.Value : DBNull.Value),
                    new SqlParameter("@ImagePath", string.IsNullOrWhiteSpace(_txtImagePath.Text) ? (object)DBNull.Value : _txtImagePath.Text),
                    new SqlParameter("@Id", _productId.Value));
            }
            else
            {
                Database.Execute(@"INSERT INTO dbo.Products(Title, Description, Price, OldPrice, DiscountPercent, ImagePath)
VALUES(@Title, @Description, @Price, @OldPrice, @DiscountPercent, @ImagePath)",
                    new SqlParameter("@Title", _txtTitle.Text.Trim()),
                    new SqlParameter("@Description", _txtDescription.Text.Trim()),
                    new SqlParameter("@Price", _numPrice.Value),
                    new SqlParameter("@OldPrice", _numOldPrice.Value > 0 ? (object)_numOldPrice.Value : DBNull.Value),
                    new SqlParameter("@DiscountPercent", _numDiscount.Value > 0 ? (object)_numDiscount.Value : DBNull.Value),
                    new SqlParameter("@ImagePath", string.IsNullOrWhiteSpace(_txtImagePath.Text) ? (object)DBNull.Value : _txtImagePath.Text));
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
