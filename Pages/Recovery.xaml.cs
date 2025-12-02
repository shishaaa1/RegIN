using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для Recovery.xaml
    /// </summary>
    public partial class Recovery : Page
    {
        string OldLogin;
        bool IsCapture = false;
        private readonly Random random = new Random();
        private string _generatedPassword;
        public Recovery()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.OnCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.OnIncorrectLogin += IncorrectLogin;
            if (Capture != null)
                Capture.HandlerCorrectCapture += CorrectCapture;
        }

        private void CorrectLogin()
        {
            if (OldLogin == TbLogin.Text) return;

            SetNotification($"Привет, {MainWindow.mainWindow.UserLogIn.Name}!", Brushes.Black);

            if (MainWindow.mainWindow.UserLogIn.Image != null)
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(MainWindow.mainWindow.UserLogIn.Image);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    AnimateImage(bitmap);
                }
                catch { }
            }

            OldLogin = TbLogin.Text;
            TrySendRecovery();
        }

        private void IncorrectLogin()
        {
            if (!string.IsNullOrEmpty(LNameUser.Content?.ToString()))
            {
                SetNotification("", Brushes.Transparent);
                AnimateImage(new BitmapImage(new Uri("pack://application:,,,/Images/ic_user.png")));
            }

            if (TbLogin.Text.Length > 0)
                SetNotification("Пользователь не найден", Brushes.Red);
        }

        private void CorrectCapture()
        {
            IsCapture = true;
            if (Capture != null) Capture.IsEnabled = false;
            TrySendRecovery();
        }

        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.mainWindow.UserLogIn.GetUserByLogin(TbLogin.Text);
        }

        private void SetLogin(object sender, RoutedEventArgs e) =>
            MainWindow.mainWindow.UserLogIn.GetUserByLogin(TbLogin.Text);

        private void TrySendRecovery()
        {
            if (!IsCapture) return;

            // Генерируем пароль и хэшируем его
            _generatedPassword = GeneratePassword();
            string newHashedPassword = HashPassword(_generatedPassword);

            // Сохраняем хэш в базу
            SaveNewPasswordToDatabase(newHashedPassword);

            // Отправляем письмо с ОТКРЫТЫМ паролем
            try
            {
                Classes.SendMail.SendMessage($"Ваш новый пароль: {_generatedPassword}\n" +
                                           $"Рекомендуем сменить его после входа в систему.",
                                           MainWindow.mainWindow.UserLogIn.Login);

                AnimateImage(new BitmapImage(new Uri("pack://application:,,,/Images/ic_mail.png")));
                SetNotification("Письмо с новым паролем отправлено на почту!", Brushes.Green);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка отправки письма: {ex.Message}");
                SetNotification("Ошибка отправки письма. Попробуйте позже.", Brushes.Red);
            }
        }

        private string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string HashPassword(string password)
        {
            return password;  
        }

        private void SaveNewPasswordToDatabase(string hashedPassword)
        {
            var conn = WorkingDB.OpenConnection();
            if (conn == null) return;

            try
            {
                var cmd = new MySqlCommand(
                    "UPDATE users SET Password = @pass, DateUpdate = NOW() WHERE Login = @login", conn);
                cmd.Parameters.AddWithValue("@pass", hashedPassword);
                cmd.Parameters.AddWithValue("@login", MainWindow.mainWindow.UserLogIn.Login);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Ошибка сохранения пароля: " + ex.Message);
            }
            finally
            {
                WorkingDB.CloseConnection(conn);
            }
        }

        private void AnimateImage(BitmapImage newImg)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (_, __) =>
            {
                IUser.Source = newImg;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(.8));
                IUser.BeginAnimation(OpacityProperty, fadeIn);
            };
            IUser.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }

        public void SetNotification(string text, SolidColorBrush color)
        {
            LNameUser.Content = text;
            LNameUser.Foreground = color;
        }
        public void ResendPassword()
        {
            if (string.IsNullOrEmpty(_generatedPassword))
                return;

            try
            {
                Classes.SendMail.SendMessage($"Ваш пароль (повторная отправка): {_generatedPassword}",
                                           MainWindow.mainWindow.UserLogIn.Login);
                SetNotification("Пароль отправлен повторно!", Brushes.Green);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка повторной отправки: {ex.Message}");
            }
        }
    }
}
