using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public class MainForm : Form
    {
        private readonly SessionUser _user;
        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly DataGridView _gridCatalog = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly DataGridView _gridSales = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly DataGridView _gridTestDrives = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly DataGridView _gridUsers = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly TextBox _txtSearch = new TextBox();
        private readonly ComboBox _cmbSort = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label _lblRole = new Label { Dock = DockStyle.Top, Height = 42 };

        public MainForm(SessionUser user)
        {
            _user = user;
            Text = $"Автосалон — {_user.FullName}";
            WindowState = FormWindowState.Maximized;
            Icon = SystemIcons.Application;

            BuildUi();
            UiAssets.ApplyFormTheme(this);
            LoadCatalog();
            LoadSales();
            LoadTestDrives();
            if (_user.Role == UserRole.Администратор)
            {
                LoadUsers();
            }
        }

        private void BuildUi()
        {
            _lblRole.Text = $"Роль: {_user.Role}";
            _lblRole.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _lblRole.Padding = new Padding(16, 10, 0, 0);
            _lblRole.BackColor = UiAssets.Accent;
            _lblRole.ForeColor = Color.White;

            Controls.Add(_tabs);
            Controls.Add(_lblRole);

            _tabs.TabPages.Add(BuildCatalogTab());
            _tabs.TabPages.Add(BuildSalesTab());
            _tabs.TabPages.Add(BuildTestDrivesTab());
            _tabs.TabPages.Add(BuildProfileTab());

            if (_user.Role == UserRole.Менеджер || _user.Role == UserRole.Администратор)
            {
                _tabs.TabPages.Add(BuildManagementTab());
            }

            if (_user.Role == UserRole.Администратор)
            {
                _tabs.TabPages.Add(BuildUsersTab());
            }
        }

        private TabPage BuildCatalogTab()
        {
            var tab = new TabPage("Каталог авто");
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6), BackColor = UiAssets.Background };
            _cmbSort.Items.AddRange(new object[] { "По цене ↑", "По цене ↓", "По модели" });
            _cmbSort.SelectedIndex = 0;
            var btnFilter = new Button { Text = "Поиск/Сортировка" };
            btnFilter.Click += (_, __) => LoadCatalog();
            UiAssets.StyleSecondaryButton(btnFilter);
            _txtSearch.Width = 260;
            _cmbSort.Width = 160;

            top.Controls.Add(new Label { Text = "Поиск", Width = 45, TextAlign = ContentAlignment.MiddleLeft });
            top.Controls.Add(_txtSearch);
            top.Controls.Add(_cmbSort);
            top.Controls.Add(btnFilter);

            _gridCatalog.ReadOnly = true;
            _gridCatalog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            UiAssets.ApplyGridStyle(_gridCatalog);

            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridCatalog);
            tab.Controls.Add(gridPanel);
            tab.Controls.Add(top);
            return tab;
        }

        private TabPage BuildSalesTab()
        {
            var tab = new TabPage("Продажи");
            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridSales);
            tab.Controls.Add(gridPanel);
            UiAssets.ApplyGridStyle(_gridSales);
            return tab;
        }

        private TabPage BuildTestDrivesTab()
        {
            var tab = new TabPage("Тест-драйвы");
            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridTestDrives);
            tab.Controls.Add(gridPanel);
            UiAssets.ApplyGridStyle(_gridTestDrives);
            return tab;
        }

        private TabPage BuildProfileTab()
        {
            var tab = new TabPage("Личный кабинет");
            var shell = UiAssets.CreateSurfacePanel(DockStyle.Top, new Padding(18));
            shell.Height = 260;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Height = 220, ColumnCount = 2 };
            var txtName = new TextBox { Text = _user.FullName };
            var txtEmail = new TextBox { Text = _user.Email };
            var txtPhone = new TextBox { Text = _user.Phone };
            var btnSave = new Button { Text = "Сохранить" };
            btnSave.Click += (_, __) =>
            {
                if (!ValidationHelper.IsValidEmail(txtEmail.Text) || !ValidationHelper.IsValidPhone(txtPhone.Text))
                {
                    MessageBox.Show("Неверный email или телефон.");
                    return;
                }

                Database.Execute("UPDATE dbo.Users SET FullName=@FullName, Email=@Email, Phone=@Phone WHERE Id=@Id",
                    new SqlParameter("@FullName", txtName.Text.Trim()),
                    new SqlParameter("@Email", txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", txtPhone.Text.Trim()),
                    new SqlParameter("@Id", _user.Id));

                _user.FullName = txtName.Text.Trim();
                _user.Email = txtEmail.Text.Trim();
                _user.Phone = txtPhone.Text.Trim();
                MessageBox.Show("Данные обновлены.");
            };
            UiAssets.StylePrimaryButton(btnSave);

            layout.Controls.Add(new Label { Text = "Имя" }, 0, 0);
            layout.Controls.Add(txtName, 1, 0);
            layout.Controls.Add(new Label { Text = "Эл. почта" }, 0, 1);
            layout.Controls.Add(txtEmail, 1, 1);
            layout.Controls.Add(new Label { Text = "Телефон" }, 0, 2);
            layout.Controls.Add(txtPhone, 1, 2);
            layout.Controls.Add(btnSave, 1, 3);
            shell.Controls.Add(layout);
            tab.Controls.Add(shell);
            return tab;
        }

        private TabPage BuildManagementTab()
        {
            var tab = new TabPage("Управление авто");
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6) };
            var btnAdd = new Button { Text = "Добавить" };
            var btnEdit = new Button { Text = "Редактировать" };
            var btnDelete = new Button { Text = "Удалить" };
            btnAdd.Click += (_, __) => OpenCarEditor(null);
            btnEdit.Click += (_, __) => OpenCarEditor(GetSelectedCarId());
            btnDelete.Click += (_, __) => DeleteCar();
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(btnDelete);
            UiAssets.StylePrimaryButton(btnAdd);
            UiAssets.StyleSecondaryButton(btnEdit);
            UiAssets.StyleSecondaryButton(btnDelete);
            if (_user.Role == UserRole.Менеджер)
            {
                btnDelete.Enabled = false;
                btnDelete.Text = "Удаление запрещено для Менеджера";
            }

            var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            UiAssets.ApplyGridStyle(grid);
            grid.DataSource = QueryCars();
            LocalizeGridColumns(grid);
            tab.Enter += (_, __) =>
            {
                grid.DataSource = QueryCars();
                LocalizeGridColumns(grid);
            };
            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(grid);
            tab.Controls.Add(gridPanel);
            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage BuildUsersTab()
        {
            var tab = new TabPage("Пользователи");
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6) };
            var btnRole = new Button { Text = "Сменить роль" };
            var btnDelete = new Button { Text = "Удалить пользователя" };
            btnRole.Click += (_, __) => ChangeUserRole();
            btnDelete.Click += (_, __) => DeleteUser();
            panel.Controls.Add(btnRole);
            panel.Controls.Add(btnDelete);
            UiAssets.StyleSecondaryButton(btnRole);
            UiAssets.StyleSecondaryButton(btnDelete);
            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridUsers);
            tab.Controls.Add(gridPanel);
            tab.Controls.Add(panel);
            UiAssets.ApplyGridStyle(_gridUsers);
            return tab;
        }

        private DataTable QueryCars()
        {
            return Database.Query(@"SELECT c.Id, b.BrandName, c.Model, c.Year, c.Price, c.Mileage, c.Color, c.Engine, c.Transmission, c.Status
FROM dbo.Cars c
JOIN dbo.Brands b ON b.Id = c.BrandId
ORDER BY c.Id DESC");
        }

        private void LoadCatalog()
        {
            var orderBy = "c.Price ASC";
            if (_cmbSort.SelectedIndex == 1) orderBy = "c.Price DESC";
            if (_cmbSort.SelectedIndex == 2) orderBy = "c.Model ASC";

            var sql = $@"SELECT c.Id, b.BrandName, c.Model, c.Year, c.Price, c.Mileage, c.Color, c.Engine, c.Transmission, c.Status
FROM dbo.Cars c
JOIN dbo.Brands b ON b.Id = c.BrandId
WHERE (@Search = '' OR c.Model LIKE '%' + @Search + '%' OR b.BrandName LIKE '%' + @Search + '%')
ORDER BY {orderBy}";

            _gridCatalog.DataSource = Database.Query(sql, new SqlParameter("@Search", _txtSearch.Text.Trim()));
            LocalizeGridColumns(_gridCatalog);
        }

        private void LoadSales()
        {
            _gridSales.DataSource = Database.Query(@"SELECT s.Id, b.BrandName, c.Model, cu.LastName + ' ' + cu.FirstName AS CustomerName,
       e.LastName + ' ' + e.FirstName AS EmployeeName, s.SaleDate, s.FinalPrice, s.Discount
FROM dbo.Sales s
JOIN dbo.Cars c ON c.Id = s.CarId
JOIN dbo.Brands b ON b.Id = c.BrandId
JOIN dbo.Customers cu ON cu.Id = s.CustomerId
JOIN dbo.Employees e ON e.Id = s.EmployeeId
ORDER BY s.SaleDate DESC");
            LocalizeGridColumns(_gridSales);
        }

        private void LoadTestDrives()
        {
            _gridTestDrives.DataSource = Database.Query(@"SELECT td.Id, b.BrandName, c.Model, cu.LastName + ' ' + cu.FirstName AS CustomerName,
       ISNULL(e.LastName + ' ' + e.FirstName, N'—') AS EmployeeName, td.TestDate, td.DurationMinutes, td.Feedback
FROM dbo.TestDrives td
JOIN dbo.Cars c ON c.Id = td.CarId
JOIN dbo.Brands b ON b.Id = c.BrandId
JOIN dbo.Customers cu ON cu.Id = td.CustomerId
LEFT JOIN dbo.Employees e ON e.Id = td.EmployeeId
ORDER BY td.TestDate DESC");
            LocalizeGridColumns(_gridTestDrives);
        }

        private int? GetSelectedCarId()
        {
            if (_gridCatalog.CurrentRow == null) return null;
            return Convert.ToInt32(_gridCatalog.CurrentRow.Cells["Id"].Value);
        }

        private void OpenCarEditor(int? carId)
        {
            using (var form = new ProductEditForm(carId))
            {
                form.ShowDialog(this);
            }

            LoadCatalog();
            LoadSales();
            LoadTestDrives();
        }

        private void DeleteCar()
        {
            if (_user.Role != UserRole.Администратор)
            {
                MessageBox.Show("Недостаточно прав.");
                return;
            }

            var carId = GetSelectedCarId();
            if (!carId.HasValue) return;

            try
            {
                Database.Execute("DELETE FROM dbo.Cars WHERE Id=@Id", new SqlParameter("@Id", carId.Value));
                LoadCatalog();
            }
            catch (SqlException)
            {
                MessageBox.Show("Невозможно удалить автомобиль, так как он связан с продажами/тест-драйвами/сервисом.", "Ограничение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadUsers()
        {
            _gridUsers.DataSource = Database.Query("SELECT Id, FullName, Email, Phone, Role, CreatedAt FROM dbo.Users ORDER BY Id DESC");
            LocalizeGridColumns(_gridUsers);
        }

        private void ChangeUserRole()
        {
            if (_gridUsers.CurrentRow == null) return;
            var id = Convert.ToInt32(_gridUsers.CurrentRow.Cells["Id"].Value);
            var currentRole = _gridUsers.CurrentRow.Cells["Role"].Value.ToString();
            var next = currentRole == UserRole.Пользователь.ToString() ? UserRole.Менеджер.ToString() : UserRole.Пользователь.ToString();
            Database.Execute("UPDATE dbo.Users SET Role=@Role WHERE Id=@Id", new SqlParameter("@Role", next), new SqlParameter("@Id", id));
            LoadUsers();
        }

        private void DeleteUser()
        {
            if (_gridUsers.CurrentRow == null) return;
            var id = Convert.ToInt32(_gridUsers.CurrentRow.Cells["Id"].Value);
            if (id == _user.Id)
            {
                MessageBox.Show("Нельзя удалить текущего администратора.");
                return;
            }

            Database.Execute("DELETE FROM dbo.Users WHERE Id=@Id", new SqlParameter("@Id", id));
            LoadUsers();
        }

        private static void LocalizeGridColumns(DataGridView grid)
        {
            if (grid?.Columns == null || grid.Columns.Count == 0)
            {
                return;
            }

            foreach (DataGridViewColumn column in grid.Columns)
            {
                switch (column.Name)
                {
                    case "Id": column.HeaderText = "ID"; break;
                    case "BrandName": column.HeaderText = "Марка"; break;
                    case "Model": column.HeaderText = "Модель"; break;
                    case "Year": column.HeaderText = "Год"; break;
                    case "Price": column.HeaderText = "Цена"; break;
                    case "Mileage": column.HeaderText = "Пробег"; break;
                    case "Color": column.HeaderText = "Цвет"; break;
                    case "Engine": column.HeaderText = "Двигатель"; break;
                    case "Transmission": column.HeaderText = "КПП"; break;
                    case "Status": column.HeaderText = "Статус"; break;
                    case "SaleDate": column.HeaderText = "Дата продажи"; break;
                    case "FinalPrice": column.HeaderText = "Итоговая цена"; break;
                    case "Discount": column.HeaderText = "Скидка"; break;
                    case "CustomerName": column.HeaderText = "Клиент"; break;
                    case "EmployeeName": column.HeaderText = "Сотрудник"; break;
                    case "TestDate": column.HeaderText = "Дата тест-драйва"; break;
                    case "DurationMinutes": column.HeaderText = "Длительность (мин)"; break;
                    case "Feedback": column.HeaderText = "Отзыв"; break;
                    case "FullName": column.HeaderText = "ФИО"; break;
                    case "Email": column.HeaderText = "Эл. почта"; break;
                    case "Phone": column.HeaderText = "Телефон"; break;
                    case "Role": column.HeaderText = "Роль"; break;
                    case "CreatedAt": column.HeaderText = "Создано"; break;
                }
            }
        }
    }
}
