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
    /// Логика взаимодействия для PinLogin.xaml
    /// </summary>
    public partial class PinLogin : Page
    {
        private string lastLogin = "";
        public PinLogin(string login)
        {
            InitializeComponent();
            lastLogin = login;
            MainWindow.mainWindow.UserLogIn.GetUserByLogin(login);
            MainWindow.mainWindow.UserLogIn.OnCorrectLogin += () =>
            {
                if (MainWindow.mainWindow.UserLogIn.PinHash != null)
                    this.Dispatcher.Invoke(() => {  });
            };
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (PinBox.Password.Length != 4)
            {
                ErrorText.Text = "Enter 4 digits";
                return;
            }

            if (MainWindow.mainWindow.UserLogIn.VerifyPin(PinBox.Password))
            {
                MessageBox.Show($"Welcome, {MainWindow.mainWindow.UserLogIn.Name}!");
               
            }
            else
            {
                ErrorText.Text = "Wrong PIN";
                PinBox.Password = "";
            }
        }

        private void GoToPasswordLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
