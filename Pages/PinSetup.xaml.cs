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

            // авто-подтверждение при 4 цифрах
            PinBox.PasswordChanged += (s, e) =>
            {
                if (PinBox.Password.Length == 4)
                    ConfirmPin_Click(null, null);
            };
        }

        private void ConfirmPin_Click(object sender, RoutedEventArgs e)
        {
            if (PinBox.Password.Length != 4 || !PinBox.Password.All(char.IsDigit))
            {
                MessageBox.Show("PIN должен состоять из 4 цифр");
                return;
            }

            MainWindow.mainWindow.UserLogIn.UpdatePin(PinBox.Password);

            if (_isRegistration)
            {
                MessageBox.Show("Регистрация завершена. PIN установлен.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
            else
            {
                MessageBox.Show("PIN успешно изменён.");
                MainWindow.mainWindow.OpenPage(new Login());
            }
        }
    }
}

