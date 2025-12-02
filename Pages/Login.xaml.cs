using Org.BouncyCastle.Cmp;
using RegIN.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        string OldLogin;
        int CountSetPassword = 2;
        bool IsCapture = false;
        public Login()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.OnCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.OnIncorrectLogin += InCorrectLogin;
            if (Capture != null)
                Capture.HandlerCorrectCapture += CorrectCapture;
        }
        
        public void CorrectLogin()
        {
            if (OldLogin == TbLogin.Text) return;

            var name = MainWindow.mainWindow.UserLogIn?.Name;
            SetNotification(name != null ? "Hi " + name : "Hi", System.Windows.Media.Brushes.Black);

            try
            {
                if (MainWindow.mainWindow.UserLogIn?.Image != null)
                {
                    BitmapImage biImg = new BitmapImage();
                    MemoryStream ms = new MemoryStream(MainWindow.mainWindow.UserLogIn.Image);
                    biImg.BeginInit();
                    biImg.StreamSource = ms;
                    biImg.CacheOption = BitmapCacheOption.OnLoad;
                    biImg.EndInit();
                    IUser.Source = biImg;
                }
                else
                {
                    IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic_user.png"));
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }

            OldLogin = TbLogin.Text;
        }
        public void InCorrectLogin()
        {
            if (!string.IsNullOrEmpty(LNameUser.Content?.ToString()))
            {
                LNameUser.Content = "";
                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
                StartAnimation.Completed += delegate
                {
                    IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic_user.png"));
                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                    IUser.BeginAnimation(Image.OpacityProperty, EndAnimation);
                };
                IUser.BeginAnimation(OpacityProperty, StartAnimation);
            }
            if (TbLogin.Text.Length > 0)
            {
                SetNotification("Login is incorrect", System.Windows.Media.Brushes.Red);
            }
        }
        public void CorrectCapture()
        {
            if (Capture != null) Capture.IsEnabled = false;
            IsCapture = true;
        }
        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.mainWindow.UserLogIn.GetUserByLogin(TbLogin.Text);
                if (TbPassword.Password.Length > 0)
                    SetPassword();
            }
        }

        private void SetLogin(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.UserLogIn.GetUserByLogin(TbLogin.Text);
            if (TbPassword.Password.Length > 0)
                SetPassword();
        }
        public void SetNotification(string Message, System.Windows.Media.SolidColorBrush _Color)
        {
            LNameUser.Content = Message;
            LNameUser.Foreground = _Color;
        }
        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetPassword();
        }
        public void SetPassword()
        {
            if (string.IsNullOrEmpty(MainWindow.mainWindow.UserLogIn?.Password))
            {
                SetNotification("User has no password set", System.Windows.Media.Brushes.Red);
                return;
            }

            if (IsCapture)
            {
                if (MainWindow.mainWindow.UserLogIn.Password == TbPassword.Password)
                {
                    // Переходим на выбор метода входа (pin или mail)
                    MainWindow.mainWindow.OpenPage(new ChoiceLogin(TbLogin.Text));
                }
                else
                {
                    if (CountSetPassword > 0)
                    {
                        SetNotification($"Password is incorrect, {CountSetPassword} attempts left", System.Windows.Media.Brushes.Red);
                        CountSetPassword--;
                    }
                    else
                    {
                        Thread TBlockAuthorization = new Thread(BlockAuthorization) { IsBackground = true };
                        TBlockAuthorization.Start();
                        Classes.SendMail.SendMessage("An attempt was made to log into your account.", MainWindow.mainWindow.UserLogIn.Login);
                    }
                }
            }
            else
                SetNotification($"Enter capture", System.Windows.Media.Brushes.Red);
        }
        public void BlockAuthorization()
        {
            DateTime StartBlock = DateTime.Now.AddMinutes(3);
            Dispatcher.Invoke(() =>
            {
                TbLogin.IsEnabled = false;
                TbPassword.IsEnabled = false;
                if (Capture != null) Capture.IsEnabled = false;
            });
            for (int i = 0; i < 180; i++)
            {
                TimeSpan TimeIdle = StartBlock.Subtract(DateTime.Now);
                string s_minutes = TimeIdle.Minutes.ToString("D2");
                string s_seconds = TimeIdle.Seconds.ToString("D2");
                Dispatcher.Invoke(() =>
                {
                    SetNotification($"Reauthorization available in: {s_minutes}:{s_seconds}", System.Windows.Media.Brushes.Red);
                });
                Thread.Sleep(1000);
            }
            Dispatcher.Invoke(() =>
            {
                SetNotification("Hi " + MainWindow.mainWindow.UserLogIn?.Name, System.Windows.Media.Brushes.Black);
                TbLogin.IsEnabled = true;
                TbPassword.IsEnabled = true;
                if (Capture != null)
                {
                    Capture.CreateCapture();
                    Capture.IsEnabled = true;
                }
                IsCapture = false;
                CountSetPassword = 2;
            });
        }
        private void RecoveryPassword(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Recovery());
        }

        private void OpenRegIn(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Regin());
        }
    }
}
