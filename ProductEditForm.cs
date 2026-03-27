using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public class ProductEditForm : Form
    {
        private readonly int? _carId;
        private readonly ComboBox _cmbBrand = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _txtModel = new TextBox();
        private readonly NumericUpDown _numYear = new NumericUpDown { Minimum = 1980, Maximum = 2100, Value = 2023 };
        private readonly NumericUpDown _numPrice = new NumericUpDown { Maximum = 100000000, DecimalPlaces = 2 };
        private readonly NumericUpDown _numMileage = new NumericUpDown { Maximum = 1000000 };
        private readonly TextBox _txtColor = new TextBox();
        private readonly TextBox _txtVin = new TextBox();
        private readonly TextBox _txtEngine = new TextBox();
        private readonly ComboBox _cmbTransmission = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cmbStatus = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

        public ProductEditForm(int? carId)
        {
            _carId = carId;
            Text = _carId.HasValue ? "Редактирование авто" : "Новый автомобиль";
            Icon = SystemIcons.Application;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(620, 520);

            var card = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(12));
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 12 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            for (var i = 0; i < 11; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            }
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _cmbTransmission.Items.AddRange(new object[] { "Automatic", "Manual", "CVT", "Robot" });
            _cmbTransmission.SelectedIndex = 0;
            _cmbStatus.Items.AddRange(new object[] { "Available", "Sold", "Reserved", "Service" });
            _cmbStatus.SelectedIndex = 0;

            layout.Controls.Add(new Label { Text = "Марка" }, 0, 0);
            layout.Controls.Add(_cmbBrand, 1, 0);
            layout.Controls.Add(new Label { Text = "Модель" }, 0, 1);
            layout.Controls.Add(_txtModel, 1, 1);
            layout.Controls.Add(new Label { Text = "Год" }, 0, 2);
            layout.Controls.Add(_numYear, 1, 2);
            layout.Controls.Add(new Label { Text = "Цена" }, 0, 3);
            layout.Controls.Add(_numPrice, 1, 3);
            layout.Controls.Add(new Label { Text = "Пробег" }, 0, 4);
            layout.Controls.Add(_numMileage, 1, 4);
            layout.Controls.Add(new Label { Text = "Цвет" }, 0, 5);
            layout.Controls.Add(_txtColor, 1, 5);
            layout.Controls.Add(new Label { Text = "VIN" }, 0, 6);
            layout.Controls.Add(_txtVin, 1, 6);
            layout.Controls.Add(new Label { Text = "Двигатель" }, 0, 7);
            layout.Controls.Add(_txtEngine, 1, 7);
            layout.Controls.Add(new Label { Text = "КПП" }, 0, 8);
            layout.Controls.Add(_cmbTransmission, 1, 8);
            layout.Controls.Add(new Label { Text = "Статус" }, 0, 9);
            layout.Controls.Add(_cmbStatus, 1, 9);

            var btnSave = new Button { Text = "Сохранить", Dock = DockStyle.Fill };
            btnSave.Click += (_, __) => SaveCar();
            UiAssets.StylePrimaryButton(btnSave);
            layout.Controls.Add(btnSave, 1, 10);

            card.Controls.Add(layout);
            Controls.Add(card);

            LoadBrands();
            if (_carId.HasValue)
            {
                LoadCar();
            }

            UiAssets.ApplyFormTheme(this);
        }

        private void LoadBrands()
        {
            var brands = Database.Query("SELECT Id, BrandName FROM dbo.Brands ORDER BY BrandName");
            _cmbBrand.DataSource = brands;
            _cmbBrand.DisplayMember = "BrandName";
            _cmbBrand.ValueMember = "Id";
        }

        private void LoadCar()
        {
            var table = Database.Query("SELECT TOP 1 * FROM dbo.Cars WHERE Id=@Id", new SqlParameter("@Id", _carId.Value));
            if (table.Rows.Count == 0)
            {
                return;
            }

            var row = table.Rows[0];
            _cmbBrand.SelectedValue = Convert.ToInt32(row["BrandId"]);
            _txtModel.Text = row["Model"].ToString();
            _numYear.Value = row["Year"] == DBNull.Value ? _numYear.Minimum : Convert.ToDecimal(row["Year"]);
            _numPrice.Value = row["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Price"]);
            _numMileage.Value = row["Mileage"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Mileage"]);
            _txtColor.Text = row["Color"].ToString();
            _txtVin.Text = row["VIN"].ToString();
            _txtEngine.Text = row["Engine"].ToString();
            SelectComboValue(_cmbTransmission, row["Transmission"].ToString());
            SelectComboValue(_cmbStatus, row["Status"].ToString());
        }

        private static void SelectComboValue(ComboBox combo, string value)
        {
            var index = combo.FindStringExact(value ?? string.Empty);
            combo.SelectedIndex = index >= 0 ? index : 0;
        }

        private void SaveCar()
        {
            if (_cmbBrand.SelectedValue == null || string.IsNullOrWhiteSpace(_txtModel.Text) || _numPrice.Value <= 0)
            {
                MessageBox.Show("Марка, модель и цена обязательны.");
                return;
            }

            var vin = _txtVin.Text.Trim();
            if (!string.IsNullOrWhiteSpace(vin) && vin.Length != 17)
            {
                MessageBox.Show("VIN должен содержать 17 символов или быть пустым.");
                return;
            }

            if (_carId.HasValue)
            {
                Database.Execute(@"UPDATE dbo.Cars
SET BrandId=@BrandId, Model=@Model, Year=@Year, Price=@Price, Mileage=@Mileage, Color=@Color, VIN=@VIN, Engine=@Engine, Transmission=@Transmission, Status=@Status
WHERE Id=@Id",
                    new SqlParameter("@BrandId", _cmbBrand.SelectedValue),
                    new SqlParameter("@Model", _txtModel.Text.Trim()),
                    new SqlParameter("@Year", (int)_numYear.Value),
                    new SqlParameter("@Price", _numPrice.Value),
                    new SqlParameter("@Mileage", (int)_numMileage.Value),
                    new SqlParameter("@Color", string.IsNullOrWhiteSpace(_txtColor.Text) ? (object)DBNull.Value : _txtColor.Text.Trim()),
                    new SqlParameter("@VIN", string.IsNullOrWhiteSpace(vin) ? (object)DBNull.Value : vin),
                    new SqlParameter("@Engine", string.IsNullOrWhiteSpace(_txtEngine.Text) ? (object)DBNull.Value : _txtEngine.Text.Trim()),
                    new SqlParameter("@Transmission", _cmbTransmission.SelectedItem.ToString()),
                    new SqlParameter("@Status", _cmbStatus.SelectedItem.ToString()),
                    new SqlParameter("@Id", _carId.Value));
            }
            else
            {
                Database.Execute(@"INSERT INTO dbo.Cars(BrandId, Model, Year, Price, Mileage, Color, VIN, Engine, Transmission, Status)
VALUES(@BrandId, @Model, @Year, @Price, @Mileage, @Color, @VIN, @Engine, @Transmission, @Status)",
                    new SqlParameter("@BrandId", _cmbBrand.SelectedValue),
                    new SqlParameter("@Model", _txtModel.Text.Trim()),
                    new SqlParameter("@Year", (int)_numYear.Value),
                    new SqlParameter("@Price", _numPrice.Value),
                    new SqlParameter("@Mileage", (int)_numMileage.Value),
                    new SqlParameter("@Color", string.IsNullOrWhiteSpace(_txtColor.Text) ? (object)DBNull.Value : _txtColor.Text.Trim()),
                    new SqlParameter("@VIN", string.IsNullOrWhiteSpace(vin) ? (object)DBNull.Value : vin),
                    new SqlParameter("@Engine", string.IsNullOrWhiteSpace(_txtEngine.Text) ? (object)DBNull.Value : _txtEngine.Text.Trim()),
                    new SqlParameter("@Transmission", _cmbTransmission.SelectedItem.ToString()),
                    new SqlParameter("@Status", _cmbStatus.SelectedItem.ToString()));
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
