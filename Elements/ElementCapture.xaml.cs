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

namespace RegIN.Elements
{
    /// <summary>
    /// Логика взаимодействия для ElementCapture.xaml
    /// </summary>
    public partial class ElementCapture : UserControl
    {
        public delegate void CorrectCapture();
        public event CorrectCapture HandlerCorrectCapture;

        private string StrCapture = "";
        private readonly Random random = new Random(); // Один общий Random!
        private const int CaptchaLength = 5; // Длина капчи
        public ElementCapture()
        {
            InitializeComponent();
            CreateCapture();
        }

        public void CreateCapture()
        {
            InputCapture.Text = "";
            Capture.Children.Clear();
            StrCapture = "";

            CreateNoiseBackground();
            GenerateCaptchaText();
        }

        // Фон — шум (полупрозрачные цифры)
        void CreateNoiseBackground()
        {
            for (int i = 0; i < 50; i++) // Меньше шума
            {
                var label = new Label
                {
                    Content = random.Next(0, 10),
                    FontSize = random.Next(12, 20),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromArgb(50,
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255),
                        (byte)random.Next(100, 255))),
                    Margin = new Thickness(
                        random.Next(0, 280),
                        random.Next(0, 50),
                        0, 0)
                };
                Capture.Children.Add(label);
            }
        }

        // Основной текст капчи
        void GenerateCaptchaText()
        {
            StrCapture = "";
            int startX = 30;

            for (int i = 0; i < CaptchaLength; i++)
            {
                int digit = random.Next(0, 10);
                StrCapture += digit;

                var label = new Label
                {
                    Content = digit,
                    FontSize = 32 + random.Next(-5, 8),
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Color.FromArgb(255,
                        (byte)random.Next(0, 100),
                        (byte)random.Next(0, 100),
                        (byte)random.Next(150, 255))),
                    Margin = new Thickness(startX + i * 40 + random.Next(-10, 10),
                                          random.Next(-15, 15), 0, 0),
                    RenderTransform = new RotateTransform(random.Next(-25, 25))
                };
                Capture.Children.Add(label);
            }
        }

        public bool OnCapture()
        {
            return StrCapture == InputCapture.Text;
        }

        private void EnterCapture(object sender, KeyEventArgs e)
        {
            if (InputCapture.Text.Length == CaptchaLength)
            {
                if (!OnCapture())
                {
                    CreateCapture();
                    InputCapture.Focus();
                }
                else
                {
                    HandlerCorrectCapture?.Invoke();
                }
            }
        }

        // Дополнительно: обновление по клику (удобно)
        private void Capture_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CreateCapture();
            InputCapture.Focus();
        }
    }

}
