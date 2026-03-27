using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace AutoSalon
{
    public class RegisterForm : Form
    {
        private readonly TextBox _txtName = new TextBox();
        private readonly TextBox _txtEmail = new TextBox();
        private readonly TextBox _txtPhone = new TextBox();
        private readonly TextBox _txtPassword = new TextBox();

        public RegisterForm()
        {
            Text = "Регистрация";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(540, 350);
            Icon = SystemIcons.Application;

            var card = UiAssets.CreateSurfacePanel(DockStyle.Fill, new Padding(14));
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 6 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            panel.Controls.Add(new Label { Text = "ФИО" }, 0, 0);
            panel.Controls.Add(_txtName, 1, 0);
            panel.Controls.Add(new Label { Text = "Email" }, 0, 1);
            panel.Controls.Add(_txtEmail, 1, 1);
            panel.Controls.Add(new Label { Text = "Телефон" }, 0, 2);
            panel.Controls.Add(_txtPhone, 1, 2);
            panel.Controls.Add(new Label { Text = "Пароль" }, 0, 3);
            _txtPassword.UseSystemPasswordChar = true;
            panel.Controls.Add(_txtPassword, 1, 3);

            var btn = new Button { Text = "Создать аккаунт", Dock = DockStyle.Fill };
            btn.Click += (_, __) => CreateUser();
            UiAssets.StylePrimaryButton(btn);
            panel.Controls.Add(btn, 1, 4);

            card.Controls.Add(panel);
            Controls.Add(card);
            UiAssets.ApplyFormTheme(this);
        }

        private void CreateUser()
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text) ||
                !ValidationHelper.IsValidEmail(_txtEmail.Text.Trim()) ||
                !ValidationHelper.IsValidPhone(_txtPhone.Text.Trim()) ||
                !ValidationHelper.IsStrongPassword(_txtPassword.Text))
            {
                MessageBox.Show("Проверьте данные: имя, email, телефон и пароль (минимум 6 символов, буквы и цифры).", "Валидация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Database.Execute(
                    "INSERT INTO dbo.Users(FullName, Email, Phone, PasswordHash, Role) VALUES(@FullName, @Email, @Phone, @PasswordHash, @Role)",
                    new SqlParameter("@FullName", _txtName.Text.Trim()),
                    new SqlParameter("@Email", _txtEmail.Text.Trim()),
                    new SqlParameter("@Phone", _txtPhone.Text.Trim()),
                    new SqlParameter("@PasswordHash", SecurityHelper.HashPassword(_txtPassword.Text)),
                    new SqlParameter("@Role", UserRole.Пользователь.ToString()));

                MessageBox.Show("Регистрация прошла успешно.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Не удалось создать пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
