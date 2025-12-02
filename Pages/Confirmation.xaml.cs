using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для Confirmation.xaml
    /// </summary>
    public partial class Confirmation : Page
    {
        public enum TypeConfirmation
        {
            Login,
            Regin
        }
        TypeConfirmation ThisTypeConfirmation;
        public int Code = 0;
        private Thread timerThread;
        public Confirmation(TypeConfirmation TypeConfirmation)
        {
            InitializeComponent();
            ThisTypeConfirmation = TypeConfirmation;
            BSendMessage.IsEnabled = false;
            SendMailCode();
        }

        public void SendMailCode()
        {
            Code = new Random().Next(100000, 999999);
            string recipient = MainWindow.mainWindow.UserLogIn.Login;

            Classes.SendMail.SendMessage($"Login code: {Code}", recipient);

            BSendMessage.IsEnabled = false;
            LTimer.Content = "Повторная отправка через 60 секунд";

            timerThread = new Thread(TimerSendMailCode) { IsBackground = true };
            timerThread.Start();
        }

        public void TimerSendMailCode()
        {
            for (int i = 60; i > 0; i--)
            {
                Dispatcher.Invoke(() =>
                {
                    LTimer.Content = $"Повторная отправка через {i} сек";
                });
                Thread.Sleep(1000);
            }

            Dispatcher.Invoke(() =>
            {
                BSendMessage.IsEnabled = true;
                LTimer.Content = "";
            });
        }

        private void SendMail(object sender, RoutedEventArgs e)
        {
            BSendMessage.IsEnabled = false;
            SendMailCode();
        }

        private void SetCode(object sender, KeyEventArgs e)
        {
            if (TbCode.Text.Length == 6)
                SetCode();
        }

        private void SetCode(object sender, RoutedEventArgs e)
        {
            SetCode();
        }

        void SetCode()
        {
            if (TbCode.Text == Code.ToString())
            {
                TbCode.IsEnabled = false;

                if (ThisTypeConfirmation == TypeConfirmation.Login)
                {
                    MessageBox.Show($"Добро пожаловать, {MainWindow.mainWindow.UserLogIn.Name}");
                    MainWindow.mainWindow.OpenPage(new Login());
                }
                else
                {
                    MainWindow.mainWindow.UserLogIn.SetUser();
                    MainWindow.mainWindow.OpenPage(new PinSetup(true));
                }
            }
            else
            {
                MessageBox.Show("Неверный код");
            }
        }

        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
