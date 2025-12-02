using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для PinSetup.xaml
    /// </summary>
    public partial class PinSetup : Page
    {
        private bool _isRegistration;

        public PinSetup() : this(false)
        {
        }

        public PinSetup(bool isRegistration = false)
        {
            InitializeComponent();
            _isRegistration = isRegistration;

        }

        private void ConfirmPin_Click(object sender, RoutedEventArgs e)
        {
            if (PinBox.Password.Length != 4 || !PinBox.Password.All(char.IsDigit))
            {
                MessageBox.Show("PIN должен содержать 4 цифры");
                return;
            }

            var user = MainWindow.mainWindow.UserLogIn;

            if (user == null || string.IsNullOrWhiteSpace(user.Login))
            {
                MessageBox.Show("Ошибка: пользователь не найден");
                return;
            }

            using (var conn = WorkingDB.OpenConnection())
            {
                try
                {
                    user.PinHash = PinBox.Password;

                    var cmd = new MySqlCommand(@"
                        INSERT INTO users (Login, Password, Name, Image, DateUpdate, DateCreate, PinHash)
                        VALUES (@Login, @Password, @Name, @Image, NOW(), NOW(), @Pin)", conn);

                    cmd.Parameters.AddWithValue("@Login", user.Login);
                    cmd.Parameters.AddWithValue("@Password", user.Password);
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@Image", user.Image ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pin", user.PinHash);

                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }
            }

            MessageBox.Show("Регистрация завершена!");
            MainWindow.mainWindow.OpenPage(new Login());
        }


    }
}

