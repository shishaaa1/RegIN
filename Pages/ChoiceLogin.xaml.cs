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
    /// Логика взаимодействия для ChoiceLogin.xaml
    /// </summary>
    public partial class ChoiceLogin : Page
    {
        private string _login;
        public ChoiceLogin(string login)
        {
            InitializeComponent();
            _login = login;

            // Если в MainWindow уже загружен другой логин — загрузим нужного пользователя.
            // Это защищённый вызов: если пользователь уже загружен и совпадает — лишний запрос не делаем.
            try
            {
                var current = MainWindow.mainWindow.UserLogIn;
                if (current == null || current.Login != login)
                {
                    MainWindow.mainWindow.UserLogIn.GetUserByLogin(login);
                }
            }
            catch
            {
                // молча, UI уведомит пользователю если что-то не так
            }

            // Если PIN не установлен, скрываем кнопку PIN
            try
            {
                if (string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn.PinHash))
                {
                    BtnPin.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                BtnPin.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnPin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new PinLogin(_login));
        }

        private void BtnMail_Click(object sender, RoutedEventArgs e)
        {
            // Открываем Confirmation — Confirmation использует MainWindow.mainWindow.UserLogIn.Login как адрес
            MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Login));
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}

