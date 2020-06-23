using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using MineClient.ServiceReference1;
using Minesweeper;
namespace MineClient
{
    //choose the game type window
    public partial class GameTypeChooseWindow : Window
    {
        string clientName;
        string clientPassword;
        public GameTypeChooseWindow(string userName, string password)
        {
            InitializeComponent();
            clientName = userName;
            clientPassword = password;

        }

        //solo game button
        private void SoloGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var soloGameClicked = new MinesweeperForm();
            soloGameClicked.Show();
        }

        //player vs player button
        private void pvpBtn_Click(object sender, RoutedEventArgs e)
        {
            ConnetFunc();
        }

        //connect to the online players
        private void ConnetFunc()
        {
                ClientCallback callback = new ClientCallback();
                MineServiceClient client = new MineServiceClient(new InstanceContext(callback));


                try
                {
                    client.ClientConnected(clientName, clientPassword);
                    MainWindow mainWindow = new MainWindow
                    {
                        Client = client,
                        Callback = callback,
                        Username = clientName,
                        Title = clientName
                    };
                    this.Close();
                    mainWindow.Show();
                }
                catch (FaultException<UserFaultException> ex)
                {
                    MessageBox.Show(ex.Detail.Message);
                }         
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}