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
        private int Code = 0;
        private Thread timerThread;
        private bool _isProcessing = false;
        public Confirmation(TypeConfirmation type)
        {
            InitializeComponent();
            ThisTypeConfirmation = type;  // ← правильное присваивание
            BSendMessage.IsEnabled = false;
            SendMailCode(); // ← отправка сразу при открытии
        }

        private void SendMailCode()
        {
            Code = new Random().Next(100000, 999999);

            string recipient = MainWindow.mainWindow?.UserLogIn?.Login;

            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show("Ошибка: email не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool result = Classes.SendMail.SendMessage($"Ваш код подтверждения: {Code}", recipient);

            if (!result)
            {
                MessageBox.Show("Ошибка отправки письма. Проверь лог.", "SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LTimer.Content = "Повторная отправка через 60 секунд";
            BSendMessage.IsEnabled = false;

            timerThread?.Abort();
            timerThread = new Thread(() =>
            {
                for (int i = 60; i > 0; i--)
                {
                    Dispatcher.Invoke(() => LTimer.Content = $"Повторная отправка через {i} сек");
                    Thread.Sleep(1000);
                }
                Dispatcher.Invoke(() =>
                {
                    BSendMessage.IsEnabled = true;
                    LTimer.Content = "Можно отправить код повторно";
                });
            })
            { IsBackground = true };

            timerThread.Start();
        }


        private void SendMail(object sender, RoutedEventArgs e) => SendMailCode();

        private void SetCode(object sender, RoutedEventArgs e)
        {
            // 1. Если мы уже в процессе обработки успешного кода — выходим.
            if (_isProcessing) return;

            if (TbCode.Text.Length != 6) return;

            if (TbCode.Text == Code.ToString())
            {
                // 2. Ставим флаг, что обработка началась
                _isProcessing = true;

                TbCode.IsEnabled = false;
                BSendMessage.IsEnabled = false;

                if (ThisTypeConfirmation == TypeConfirmation.Login)
                {
                    MessageBox.Show($"Добро пожаловать, {MainWindow.mainWindow.UserLogIn.Name}!");
                    MainWindow.mainWindow.OpenPage(new Login());
                }
                else
                {
                    MessageBox.Show("Подтверждение успешно!");
                    MainWindow.mainWindow.OpenPage(new PinSetup(true));
                }
            }
            else
            {
                MessageBox.Show("Неверный код", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TbCode.Text = "";

                _isProcessing = false;

                TbCode.Focus();
            }
        }

        private void SetCode(object sender, KeyEventArgs e) => SetCode(sender, (RoutedEventArgs)e);

        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}

