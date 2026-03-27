using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoSalon
{
    public class MainForm : Form
    {
        private readonly SessionUser _user;
        private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };
        private readonly DataGridView _gridCatalog = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly DataGridView _gridCart = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly DataGridView _gridOrders = new DataGridView { Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
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
            LoadCart();
            LoadOrders();
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
            _tabs.TabPages.Add(BuildCartTab());
            _tabs.TabPages.Add(BuildOrdersTab());
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
            var tab = new TabPage("Каталог");
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6), BackColor = UiAssets.Background };
            _cmbSort.Items.AddRange(new object[] { "По цене ↑", "По цене ↓", "По названию" });
            _cmbSort.SelectedIndex = 0;
            var btnFilter = new Button { Text = "Поиск/Сортировка" };
            btnFilter.Click += (_, __) => LoadCatalog();
            var btnAddToCart = new Button { Text = "Добавить в корзину" };
            btnAddToCart.Click += (_, __) => AddToCart();
            UiAssets.StyleSecondaryButton(btnFilter);
            UiAssets.StylePrimaryButton(btnAddToCart);
            _txtSearch.Width = 260;
            _cmbSort.Width = 160;

            top.Controls.Add(new Label { Text = "Поиск", Width = 45, TextAlign = ContentAlignment.MiddleLeft });
            top.Controls.Add(_txtSearch);
            top.Controls.Add(_cmbSort);
            top.Controls.Add(btnFilter);
            top.Controls.Add(btnAddToCart);

            _gridCatalog.ReadOnly = true;
            _gridCatalog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridCatalog.RowPrePaint += GridCatalogOnRowPrePaint;
            UiAssets.ApplyGridStyle(_gridCatalog);

            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridCatalog);
            tab.Controls.Add(gridPanel);
            tab.Controls.Add(top);
            return tab;
        }

        private TabPage BuildCartTab()
        {
            var tab = new TabPage("Корзина");
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6), BackColor = UiAssets.Background };
            var btnRemove = new Button { Text = "Удалить из корзины" };
            btnRemove.Click += (_, __) => RemoveFromCart();
            var btnCheckout = new Button { Text = "Оформить заказ" };
            btnCheckout.Click += (_, __) => Checkout();
            UiAssets.StyleSecondaryButton(btnRemove);
            UiAssets.StylePrimaryButton(btnCheckout);
            top.Controls.Add(btnRemove);
            top.Controls.Add(btnCheckout);
            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridCart);
            tab.Controls.Add(gridPanel);
            tab.Controls.Add(top);
            UiAssets.ApplyGridStyle(_gridCart);
            return tab;
        }

        private TabPage BuildOrdersTab()
        {
            var tab = new TabPage("История заказов");
            var gridPanel = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(10));
            gridPanel.Controls.Add(_gridOrders);
            tab.Controls.Add(gridPanel);
            UiAssets.ApplyGridStyle(_gridOrders);
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
            var tab = new TabPage("Управление товарами");
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(10, 10, 10, 6) };
            var btnAdd = new Button { Text = "Добавить" };
            var btnEdit = new Button { Text = "Редактировать" };
            var btnDelete = new Button { Text = "Удалить" };
            btnAdd.Click += (_, __) => OpenProductEditor(null);
            btnEdit.Click += (_, __) => OpenProductEditor(GetSelectedProductId());
            btnDelete.Click += (_, __) => DeleteProduct();
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
            grid.DataSource = Database.Query("SELECT Id, Title, Description, Price, OldPrice, DiscountPercent, ImagePath FROM dbo.Products ORDER BY Id DESC");
            LocalizeGridColumns(grid);
            tab.Enter += (_, __) =>
            {
                grid.DataSource = Database.Query("SELECT Id, Title, Description, Price, OldPrice, DiscountPercent, ImagePath FROM dbo.Products ORDER BY Id DESC");
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

        private void LoadCatalog()
        {
            var orderBy = "Price ASC";
            if (_cmbSort.SelectedIndex == 1) orderBy = "Price DESC";
            if (_cmbSort.SelectedIndex == 2) orderBy = "Title ASC";

            var sql = $@"SELECT Id, Title, Description, Price, OldPrice, DiscountPercent,
CASE WHEN OldPrice IS NOT NULL AND DiscountPercent IS NOT NULL
THEN CAST(OldPrice * (100 - DiscountPercent) / 100.0 AS DECIMAL(12,2))
ELSE Price END AS NewPrice,
ImagePath
FROM dbo.Products
WHERE (@Search = '' OR Title LIKE '%' + @Search + '%' OR Description LIKE '%' + @Search + '%')
ORDER BY {orderBy}";

            _gridCatalog.DataSource = Database.Query(sql, new SqlParameter("@Search", _txtSearch.Text.Trim()));
            LocalizeGridColumns(_gridCatalog);
        }

        private void GridCatalogOnRowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _gridCatalog.Rows[e.RowIndex];
            if (row.Cells["Price"].Value == DBNull.Value) return;
            var price = Convert.ToDecimal(row.Cells["Price"].Value);
            if (price > 1000)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 247, 221);
                row.DefaultCellStyle.Font = new Font(_gridCatalog.Font, FontStyle.Bold);
            }

            if (row.Cells["OldPrice"].Value != DBNull.Value && row.Cells["DiscountPercent"].Value != DBNull.Value)
            {
                row.Cells["OldPrice"].Style.Font = new Font(_gridCatalog.Font, FontStyle.Strikeout);
                row.Cells["NewPrice"].Style.ForeColor = Color.DarkGreen;
            }
        }

        private int? GetSelectedProductId()
        {
            if (_gridCatalog.CurrentRow == null) return null;
            return Convert.ToInt32(_gridCatalog.CurrentRow.Cells["Id"].Value);
        }

        private void AddToCart()
        {
            var productId = GetSelectedProductId();
            if (!productId.HasValue)
            {
                MessageBox.Show("Выберите товар в каталоге.");
                return;
            }

            Database.Execute(@"
IF EXISTS(SELECT 1 FROM dbo.CartItems WHERE UserId=@UserId AND ProductId=@ProductId)
    UPDATE dbo.CartItems SET Quantity = Quantity + 1 WHERE UserId=@UserId AND ProductId=@ProductId
ELSE
    INSERT INTO dbo.CartItems(UserId, ProductId, Quantity) VALUES(@UserId, @ProductId, 1)",
                new SqlParameter("@UserId", _user.Id),
                new SqlParameter("@ProductId", productId.Value));
            LoadCart();
        }

        private void LoadCart()
        {
            _gridCart.DataSource = Database.Query(@"SELECT c.Id, p.Title, c.Quantity, p.Price, CAST(c.Quantity * p.Price AS DECIMAL(12,2)) AS Total
FROM dbo.CartItems c
JOIN dbo.Products p ON p.Id = c.ProductId
WHERE c.UserId = @UserId", new SqlParameter("@UserId", _user.Id));
            LocalizeGridColumns(_gridCart);
        }

        private void RemoveFromCart()
        {
            if (_gridCart.CurrentRow == null) return;
            var id = Convert.ToInt32(_gridCart.CurrentRow.Cells["Id"].Value);
            Database.Execute("DELETE FROM dbo.CartItems WHERE Id=@Id", new SqlParameter("@Id", id));
            LoadCart();
        }

        private void Checkout()
        {
            var cart = Database.Query(@"SELECT c.ProductId, c.Quantity, p.Price FROM dbo.CartItems c JOIN dbo.Products p ON p.Id = c.ProductId WHERE c.UserId=@UserId", new SqlParameter("@UserId", _user.Id));
            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }

            var total = cart.AsEnumerable().Sum(r => r.Field<int>("Quantity") * r.Field<decimal>("Price"));
            var orderId = Convert.ToInt32(Database.Scalar(@"INSERT INTO dbo.Orders(UserId, TotalAmount) OUTPUT INSERTED.Id VALUES(@UserId, @Total)",
                new SqlParameter("@UserId", _user.Id),
                new SqlParameter("@Total", total)));

            foreach (DataRow row in cart.Rows)
            {
                Database.Execute("INSERT INTO dbo.OrderItems(OrderId, ProductId, Quantity, UnitPrice) VALUES(@OrderId, @ProductId, @Quantity, @UnitPrice)",
                    new SqlParameter("@OrderId", orderId),
                    new SqlParameter("@ProductId", row["ProductId"]),
                    new SqlParameter("@Quantity", row["Quantity"]),
                    new SqlParameter("@UnitPrice", row["Price"]));
            }

            Database.Execute("DELETE FROM dbo.CartItems WHERE UserId=@UserId", new SqlParameter("@UserId", _user.Id));
            LoadCart();
            LoadOrders();
            MessageBox.Show("Заказ оформлен.");
        }

        private void LoadOrders()
        {
            var sql = _user.Role == UserRole.Менеджер || _user.Role == UserRole.Администратор
                ? "SELECT o.Id, u.FullName, o.TotalAmount, o.Status, o.CreatedAt FROM dbo.Orders o JOIN dbo.Users u ON u.Id=o.UserId ORDER BY o.CreatedAt DESC"
                : "SELECT Id, TotalAmount, Status, CreatedAt FROM dbo.Orders WHERE UserId=@UserId ORDER BY CreatedAt DESC";

            _gridOrders.DataSource = _user.Role == UserRole.Менеджер || _user.Role == UserRole.Администратор
                ? Database.Query(sql)
                : Database.Query(sql, new SqlParameter("@UserId", _user.Id));
            LocalizeGridColumns(_gridOrders);
        }

        private void OpenProductEditor(int? productId)
        {
            using (var form = new ProductEditForm(productId))
            {
                form.ShowDialog(this);
            }

            LoadCatalog();
        }

        private void DeleteProduct()
        {
            if (_user.Role != UserRole.Администратор)
            {
                MessageBox.Show("Недостаточно прав.");
                return;
            }

            var productId = GetSelectedProductId();
            if (!productId.HasValue) return;

            try
            {
                Database.Execute("DELETE FROM dbo.Products WHERE Id=@Id", new SqlParameter("@Id", productId.Value));
                LoadCatalog();
            }
            catch (SqlException)
            {
                MessageBox.Show("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.", "Ограничение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    case "Id":
                        column.HeaderText = "ID";
                        break;
                    case "Title":
                        column.HeaderText = "Название";
                        break;
                    case "Description":
                        column.HeaderText = "Описание";
                        break;
                    case "Price":
                        column.HeaderText = "Цена";
                        break;
                    case "OldPrice":
                        column.HeaderText = "Старая цена";
                        break;
                    case "DiscountPercent":
                        column.HeaderText = "Скидка, %";
                        break;
                    case "NewPrice":
                        column.HeaderText = "Цена со скидкой";
                        break;
                    case "ImagePath":
                        column.HeaderText = "Путь к изображению";
                        break;
                    case "Quantity":
                        column.HeaderText = "Количество";
                        break;
                    case "Total":
                        column.HeaderText = "Сумма";
                        break;
                    case "FullName":
                        column.HeaderText = "ФИО";
                        break;
                    case "Email":
                        column.HeaderText = "Эл. почта";
                        break;
                    case "Phone":
                        column.HeaderText = "Телефон";
                        break;
                    case "Role":
                        column.HeaderText = "Роль";
                        break;
                    case "CreatedAt":
                        column.HeaderText = "Создано";
                        break;
                    case "TotalAmount":
                        column.HeaderText = "Итого";
                        break;
                    case "Status":
                        column.HeaderText = "Статус";
                        break;
                }
            }
        }
    }
}
