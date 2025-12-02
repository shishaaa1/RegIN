using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Imaging = Aspose.Imaging;
using static RegIN.Classes.User;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для Regin.xaml
    /// </summary>
    public partial class Regin : Page
    {
        OpenFileDialog FileDialogImage = new OpenFileDialog();
        private bool IsLoginValid = false;
        private bool IsPasswordValid = false;
        private bool IsConfirmPasswordValid = false;
        private bool IsImageSelected = false;
        public Regin()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.OnCorrectLogin += () =>
            {
                SetNotification("Этот email уже зарегистрирован", Brushes.Red);
                IsLoginValid = false;
            };

            MainWindow.mainWindow.UserLogIn.OnIncorrectLogin += () =>
            {
                SetNotification("", Brushes.Black);
                IsLoginValid = true;
                TryGoToConfirmation();
            };

            FileDialogImage.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Все файлы (*.*)|*.*";
            FileDialogImage.Title = "Выберите аватар";
        }

        private void SetLogin(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ValidateLogin(); }
        private void SetLogin(object sender, RoutedEventArgs e) => ValidateLogin();

        private void SetPassword(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ValidatePassword(); }
        private void SetPassword(object sender, RoutedEventArgs e) => ValidatePassword();

        private void SetName(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsLetter(e.Text[0]);
        }

        private void ValidateLogin()
        {
            var regex = new Regex(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (string.IsNullOrWhiteSpace(TbLogin.Text) || !regex.IsMatch(TbLogin.Text))
            {
                SetNotification("Некорректный email", Brushes.Red);
                IsLoginValid = false;
                return;
            }

            MainWindow.mainWindow.UserLogIn.GetUserByLogin(TbLogin.Text);
        }

        private void ValidatePassword()
        {
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*~_])(?=.{10,})[A-Za-z0-9!@#$%^&*~_]+$");

            if (string.IsNullOrEmpty(TbPassword.Password) || !regex.IsMatch(TbPassword.Password))
            {
                SetNotification("Пароль слабый: минимум 10 символов, большая и маленькая буква + символ", Brushes.Red);
                IsPasswordValid = false;
            }
            else
            {
                IsPasswordValid = true;
                if (TbConfirmPassword.Password.Length > 0)
                    ValidateConfirmPassword();
                else
                    SetNotification("", Brushes.Black);
            }

            TryGoToConfirmation();
        }

        private void ValidateConfirmPassword()
        {
            if (TbPassword.Password != TbConfirmPassword.Password)
            {
                SetNotification("Пароли не совпадают", Brushes.Red);
                IsConfirmPasswordValid = false;
            }
            else
            {
                SetNotification("Пароли совпадают", Brushes.Green);
                IsConfirmPasswordValid = true;
            }
            TryGoToConfirmation();
        }

        private void TryGoToConfirmation()
        {
            if (IsLoginValid &&
                IsPasswordValid &&
                IsConfirmPasswordValid &&
                !string.IsNullOrWhiteSpace(TbName.Text))
            {
                var user = MainWindow.mainWindow.UserLogIn;
                user.Login = TbLogin.Text.Trim();
                user.Password = TbPassword.Password;
                user.Name = TbName.Text.Trim();
                user.DateUpdate = DateTime.Now;
                user.DateCreate = DateTime.Now;

                if (IsImageSelected && File.Exists("IUser.jpg"))
                {
                    user.Image = File.ReadAllBytes("IUser.jpg");
                }

                MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Regin));
            }
        }

        private void SetNotification(string text, SolidColorBrush color)
        {
            LNameUser.Content = text;
            LNameUser.Foreground = color;
        }
        private void SelectImage(object sender, MouseButtonEventArgs e)
        {
            if (FileDialogImage.ShowDialog() != true) return;

            try
            {
                using (var img = Imaging.Image.Load(FileDialogImage.FileName))
                {
                    int w = img.Width > img.Height ? (int)(img.Width * (256f / img.Height)) : 256;
                    int h = img.Width > img.Height ? 256 : (int)(img.Height * (256f / img.Width));
                    img.Resize(w, h);
                    img.Save("IUser.jpg");
                }

                using (var raster = (Imaging.RasterImage)Imaging.Image.Load("IUser.jpg"))
                {
                    if (!raster.IsCached) raster.CacheData();
                    int x = Math.Max(0, (raster.Width - 256) / 2);
                    int y = Math.Max(0, (raster.Height - 256) / 2);
                    var rect = new Imaging.Rectangle(x, y, 256, 256);
                    raster.Crop(rect);
                    raster.Save("IUser.jpg");
                }

                IUser.Source = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "IUser.jpg")));

                var anim = new DoubleAnimation(0.3, 1, TimeSpan.FromSeconds(0.6));
                IUser.BeginAnimation(OpacityProperty, anim);

                IsImageSelected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
        private void TbConfirmPassword_LostFocus(object sender, RoutedEventArgs e) => ValidateConfirmPassword();
        private void TbConfirmPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ValidateConfirmPassword();
        }
    }
}
