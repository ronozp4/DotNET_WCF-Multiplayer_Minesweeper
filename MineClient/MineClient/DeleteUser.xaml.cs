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
    // Delete User window
    public partial class DeleteUser : Window
    {
        public DeleteUser()
        {
            InitializeComponent();
        }
        public delegate void DeleteDelegate(string password);
        public event DeleteDelegate deleteAccount;

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbPassword.Password))
            {
                deleteAccount(tbPassword.Password.Trim());
                this.Close();
            }
        }
    }
}
