using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public class AuthForm : Form
    {
        private readonly TextBox _txtLogin = new TextBox();
        private readonly TextBox _txtPassword = new TextBox();

        public AuthForm()
        {
            Text = "AutoSalon — Вход";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(420, 260);
            MaximumSize = new Size(520, 320);
            Icon = SystemIcons.Application;

            var card = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(14));
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                RowCount = 5,
                ColumnCount = 2
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtLogin.Dock = DockStyle.Fill;
            _txtPassword.Dock = DockStyle.Fill;
            panel.Controls.Add(new Label { Text = "Email / Телефон", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            panel.Controls.Add(_txtLogin, 1, 0);
            panel.Controls.Add(new Label { Text = "Пароль", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            _txtPassword.UseSystemPasswordChar = true;
            panel.Controls.Add(_txtPassword, 1, 1);

            var btnLogin = new Button { Text = "Войти", Dock = DockStyle.Fill, Height = 36 };
            btnLogin.Click += (_, __) => Login();
            var btnRegister = new Button { Text = "Регистрация", Dock = DockStyle.Fill, Height = 36 };
            btnRegister.Click += (_, __) => Register();
            UiAssets.StylePrimaryButton(btnLogin);
            UiAssets.StyleSecondaryButton(btnRegister);
            panel.Controls.Add(btnLogin, 0, 3);
            panel.Controls.Add(btnRegister, 1, 3);

            card.Controls.Add(panel);
            Controls.Add(card);
            UiAssets.ApplyFormTheme(this);
        }

        private void Register()
        {
            using (var form = new RegisterForm())
            {
                form.ShowDialog(this);
            }
        }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(_txtLogin.Text) || string.IsNullOrWhiteSpace(_txtPassword.Text))
            {
                MessageBox.Show("Заполните логин и пароль.");
                return;
            }

            var table = Database.Query(
                "SELECT TOP 1 Id, FullName, Email, Phone, Role FROM dbo.Users WHERE (Email = @Login OR Phone = @Login) AND PasswordHash = @PasswordHash",
                new SqlParameter("@Login", _txtLogin.Text.Trim()),
                new SqlParameter("@PasswordHash", SecurityHelper.HashPassword(_txtPassword.Text)));

            if (table.Rows.Count == 0)
            {
                MessageBox.Show("Неверный логин или пароль.", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = table.Rows[0];
            var user = new SessionUser
            {
                Id = Convert.ToInt32(row["Id"]),
                FullName = row["FullName"].ToString(),
                Email = row["Email"].ToString(),
                Phone = row["Phone"].ToString(),
                Role = (UserRole)Enum.Parse(typeof(UserRole), row["Role"].ToString())
            };

            Hide();
            using (var mainForm = new MainForm(user))
            {
                mainForm.ShowDialog(this);
            }
            Show();
        }
    }
}
