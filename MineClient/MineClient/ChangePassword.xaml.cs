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
using System.Windows.Shapes;

namespace MineClient
{
    // Changing the password
    public partial class ChangePassword : Window
    {
        public ChangePassword()
        {
            InitializeComponent();
        }

        public delegate void ChangePassDelegate(string oldPassword, string newPassword);
        public event ChangePassDelegate passwordChange;

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPassword.Password) &&
                    !string.IsNullOrEmpty(tbnewPassword.Text)) //not empty
            {
                passwordChange(tbPassword.Password.Trim(), tbnewPassword.Text.Trim());
                this.Close();
            }
        }
    }
}
