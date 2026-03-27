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
        private readonly Label _lblRole = new Label { Dock = DockStyle.Top, Height = 30 };

        public MainForm(SessionUser user)
        {
            _user = user;
            Text = $"AutoSalon — {_user.FullName}";
            WindowState = FormWindowState.Maximized;
            Icon = SystemIcons.Application;

            BuildUi();
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
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
            _cmbSort.Items.AddRange(new object[] { "По цене ↑", "По цене ↓", "По названию" });
            _cmbSort.SelectedIndex = 0;
            var btnFilter = new Button { Text = "Поиск/Сортировка" };
            btnFilter.Click += (_, __) => LoadCatalog();
            var btnAddToCart = new Button { Text = "Добавить в корзину" };
            btnAddToCart.Click += (_, __) => AddToCart();

            top.Controls.Add(new Label { Text = "Поиск", Width = 45, TextAlign = ContentAlignment.MiddleLeft });
            top.Controls.Add(_txtSearch);
            top.Controls.Add(_cmbSort);
            top.Controls.Add(btnFilter);
            top.Controls.Add(btnAddToCart);

            _gridCatalog.ReadOnly = true;
            _gridCatalog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _gridCatalog.RowPrePaint += GridCatalogOnRowPrePaint;

            tab.Controls.Add(_gridCatalog);
            tab.Controls.Add(top);
            return tab;
        }

        private TabPage BuildCartTab()
        {
            var tab = new TabPage("Корзина");
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42 };
            var btnRemove = new Button { Text = "Удалить из корзины" };
            btnRemove.Click += (_, __) => RemoveFromCart();
            var btnCheckout = new Button { Text = "Оформить заказ" };
            btnCheckout.Click += (_, __) => Checkout();
            top.Controls.Add(btnRemove);
            top.Controls.Add(btnCheckout);
            tab.Controls.Add(_gridCart);
            tab.Controls.Add(top);
            return tab;
        }

        private TabPage BuildOrdersTab()
        {
            var tab = new TabPage("История заказов");
            tab.Controls.Add(_gridOrders);
            return tab;
        }

        private TabPage BuildProfileTab()
        {
            var tab = new TabPage("Личный кабинет");
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 200, Padding = new Padding(16), ColumnCount = 2 };
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

            layout.Controls.Add(new Label { Text = "Имя" }, 0, 0);
            layout.Controls.Add(txtName, 1, 0);
            layout.Controls.Add(new Label { Text = "Email" }, 0, 1);
            layout.Controls.Add(txtEmail, 1, 1);
            layout.Controls.Add(new Label { Text = "Телефон" }, 0, 2);
            layout.Controls.Add(txtPhone, 1, 2);
            layout.Controls.Add(btnSave, 1, 3);
            tab.Controls.Add(layout);
            return tab;
        }

        private TabPage BuildManagementTab()
        {
            var tab = new TabPage("Управление товарами");
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44 };
            var btnAdd = new Button { Text = "Добавить" };
            var btnEdit = new Button { Text = "Редактировать" };
            var btnDelete = new Button { Text = "Удалить" };
            btnAdd.Click += (_, __) => OpenProductEditor(null);
            btnEdit.Click += (_, __) => OpenProductEditor(GetSelectedProductId());
            btnDelete.Click += (_, __) => DeleteProduct();
            panel.Controls.Add(btnAdd);
            panel.Controls.Add(btnEdit);
            panel.Controls.Add(btnDelete);
            if (_user.Role == UserRole.Менеджер)
            {
                btnDelete.Enabled = false;
                btnDelete.Text = "Удаление запрещено для Менеджера";
            }

            var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            grid.DataSource = Database.Query("SELECT Id, Title, Description, Price, OldPrice, DiscountPercent, ImagePath FROM dbo.Products ORDER BY Id DESC");
            tab.Enter += (_, __) => grid.DataSource = Database.Query("SELECT Id, Title, Description, Price, OldPrice, DiscountPercent, ImagePath FROM dbo.Products ORDER BY Id DESC");
            tab.Controls.Add(grid);
            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage BuildUsersTab()
        {
            var tab = new TabPage("Пользователи");
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44 };
            var btnRole = new Button { Text = "Сменить роль" };
            var btnDelete = new Button { Text = "Удалить пользователя" };
            btnRole.Click += (_, __) => ChangeUserRole();
            btnDelete.Click += (_, __) => DeleteUser();
            panel.Controls.Add(btnRole);
            panel.Controls.Add(btnDelete);
            tab.Controls.Add(_gridUsers);
            tab.Controls.Add(panel);
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
    }
}
