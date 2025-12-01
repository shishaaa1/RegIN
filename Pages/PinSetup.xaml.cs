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
        public PinSetup()
        {
            InitializeComponent();
            PinBox.PasswordChanged += (s, e) =>
            {
                var pb = s as PasswordBox;
                if (pb.Password.Length == 4) ConfirmPin_Click(null, null);
            };
        }

        private void ConfirmPin_Click(object sender, RoutedEventArgs e)
        {
            if (PinBox.Password.Length != 4 || !PinBox.Password.All(char.IsDigit))
            {
                Info.Text = "PIN must be 4 digits!";
                return;
            }

            MainWindow.mainWindow.UserLogIn.UpdatePin(PinBox.Password);
            MessageBox.Show("PIN set successfully! You can now log in with it.");
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
