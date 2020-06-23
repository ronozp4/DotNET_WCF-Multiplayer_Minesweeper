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
using MineClient.ServiceReference1;
using MineClient;
using System.ServiceModel;
using Minesweeper;

namespace MineClient
{
    //game lobby
    public partial class MainWindow : Window
    {
        public MineServiceClient Client { get; set; }
        public ClientCallback Callback { get; set; }
        public string Username { get; set; }
        private Random randomNumber;

        public MainWindow()
        {
            InitializeComponent();
        }

        //the game request  
        private void GameRequestMessage(string toClient, string fromClient,string gameType)
        {
            if (!Client.PlayerAvailible(toClient))
            {
                Client.SendMessage("I'm currently playing!", Username, fromClient);
                return;
            }
            MessageBoxResult result = MessageBox.Show(fromClient + " has sent a "+ gameType + " game request! Would you like to play?",
                                                        "Game Request", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                tbConversation.Text += GetCurrentTime() + " " + fromClient + "'s game request rejected!\n";
                tbConversation.ScrollToEnd();
                Client.SendMessage("rejected your game request", Username, fromClient);
            }
            else
            {
                try
                {
                    Client.StartGame(fromClient, Username,gameType);

                    DisplayBoardWindow(fromClient, Username, gameType);
                }
                catch (FaultException<UserFaultException> ex)
                {
                    MessageBox.Show(ex.Detail.Message);
                }
            }
        }

        //update users list that in the lobby
        private void UpdateUsers(string[] users)
        {
            lbUsers.ItemsSource = users;         
        }

        //delete the account
        private void DeleteAccount(string password)
        {
            try
            {
                Client.DeleteClient(Username, password);
                MessageBox.Show("Account: " + Username + " deleted!");
                this.Close();
            }
            catch (FaultException<UserFaultException> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
        }

        //changing password
        private void ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                Client.ChangeClientPassword(Username, oldPassword, newPassword);
                MessageBox.Show("Password changed!");
            }
            catch (FaultException<UserFaultException> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
        }

        //sending move of the opponent
        private void OpponentMove(int row, int col)
        {
            sendMove(row, col);
        }

        //send messsage to the selected player
        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbMassage.Text) && lbUsers.SelectedItem != null)
            {
                Client.SendMessage(tbMassage.Text, Username, lbUsers.SelectedItem as string);
                tbMassage.Text = string.Empty;
            }
        }

        //window loaded event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            randomNumber = new Random();
            
            Callback.updateUsers += UpdateUsers;
            Callback.displayMessage += DisplayMessage;
            Callback.displayBoardWindow += DisplayBoardWindow;
            Callback.gameRequestMessage += GameRequestMessage;
            Callback.opponentmove += OpponentMove;
            Callback.closeGameBoard += CloseGameBoard;
            Callback.gamefinished += GameFinished;
            Client.SendMessage("Welcome!", "Server", Username);
        }
        // get the current time
        private string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm");
        }

        //get the current date
        private string GetCurrentDate()
        {
            return DateTime.Today.ToString("d");
        }

        //show the message
        private void DisplayMessage(string message, string fromClient)
        {
            tbConversation.Text += GetCurrentTime() + " " + fromClient + ": " + message + "\n";
        }

        //game closed
        private void CloseGameBoard(string opponent)
        {
            gameExit(opponent);
        }

        private void Window_Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to leave?", "Exit",
                MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    foreach (string user in lbUsers.Items) {
                        if (user!= Username) {
                            Client.SendMessage(Username + " has diconnected", "Server", user);
                        }
                }
                    Client.ClientDisconnected(Username);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    System.Environment.Exit(System.Environment.ExitCode);
                }
            }
        }

        //Display the board by the given type
        private void DisplayBoardWindow(string player1, string player2,string gameType)
        {
            if (gameType.Equals("easy"))
            {
                MinesweeperMatchEasy matchWindow = new MinesweeperMatchEasy(player1, player2, Username);
                matchWindow.Show();
                gameExit = matchWindow.OpponentLeft;
                sendMove = matchWindow.DrawOpponent;
                finishGame = matchWindow.GameFinished;
            }
            else if (gameType.Equals("normal"))
            {
                MinesweeperMatchNormal matchWindow = new MinesweeperMatchNormal(player1, player2, Username);
                matchWindow.Show();
                gameExit = matchWindow.OpponentLeft;
                sendMove = matchWindow.DrawOpponent;
                finishGame = matchWindow.GameFinished;
            }
            else
            {
                MinesweeperMatchHard matchWindow = new MinesweeperMatchHard(player1, player2, Username);
                matchWindow.Show();
                gameExit = matchWindow.OpponentLeft;
                sendMove = matchWindow.DrawOpponent;
               finishGame = matchWindow.GameFinished;

            }
            
            
        }

        //getting the stats
        private void GetStats(string statFactor)
        {
            try
            {
                var list = new List<dynamic>(Client.GetGameStatistics(statFactor));
                populate(list);
            }
            catch (FaultException<UserFaultException> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }
        }

        //Announcement of the winner/loser/tie
        private void GameFinished(string winner, string looser) 
        {
            finishGame(winner, looser);
        }

        private void PlayersStats_Click(object sender, RoutedEventArgs e) ////open and load players statistics
        {
            StatisticsWindow statsWindow = new StatisticsWindow("Players");
            statsWindow.Show();
            populate += statsWindow.Populate;
            statsWindow.getStats += GetStats;
        }

        private void GamesStats_Click(object sender, RoutedEventArgs e) //open and load game statistics
        {
            StatisticsWindow statsWindow = new StatisticsWindow("Games");
            statsWindow.Show();
            populate += statsWindow.Populate;
            statsWindow.getStats += GetStats;

        }

        private void LiveGames_Click(object sender, RoutedEventArgs e)//open and load live game statistics
        {
            StatisticsWindow statsWindow = new StatisticsWindow("LiveGames");
            statsWindow.Show();
            populate += statsWindow.Populate;
            statsWindow.getStats += GetStats;

        }

        private void ChangeColor_Click(object sender, RoutedEventArgs e) //the the backround color randomly
        {
            Brush brush = new SolidColorBrush(Color.FromRgb((byte)randomNumber.Next(1, 255),
                                        (byte)randomNumber.Next(1, 255), (byte)randomNumber.Next(1, 233)));
            this.Background = brush;
        }

      

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            DeleteUser deleteAccountWindow = new DeleteUser();
            deleteAccountWindow.Show();
            deleteAccountWindow.deleteAccount += DeleteAccount;
        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePassword changePasswordWindow = new ChangePassword();
            changePasswordWindow.Show();
            changePasswordWindow.passwordChange += ChangePassword;
        }

        private void LbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e) //show selected players info 
        {
            try
            {
                if (lbUsers.SelectedItem != null) 
                {
                    List<dynamic> playerStatsAsDictionary = new List<dynamic>(Client.GetPlayerStatistics(lbUsers.SelectedItem as string));

                    string playerStats = "Name: " + playerStatsAsDictionary[0];
                    playerStats += "\n" + "Games played: " + playerStatsAsDictionary[1];
                    playerStats += "\n" + "Points: " + playerStatsAsDictionary[2];
                    playerStats += "\n" + "Wins: " + playerStatsAsDictionary[3];
                    playerStats += "\n" + "Losses: " + playerStatsAsDictionary[4];
                    playerStats += "\n" + "Ties: " + playerStatsAsDictionary[5];
                    playerStats += "\n" + "Win percent: " + playerStatsAsDictionary[6] + "%";

                    tbstats.Text = playerStats;
                }
                else
                {
                    tbstats.Text = null;
                }
            }
            catch (FaultException<UserFaultException> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }

        }

        private void PlaySoloButton_Click(object sender, RoutedEventArgs e) // solo play button pressed
        {  
            var soloGameStart = new MinesweeperForm();
            soloGameStart.Show();
            }

        private void PlayEasyButton_Click(object sender, RoutedEventArgs e) // easy level button pressed
        {
            if (lbUsers.SelectedItem != null && lbUsers.SelectedItem.ToString() != Username)
            {
                Client.GameRequest(Username, lbUsers.SelectedItem.ToString(),"easy");
            }
        }

        private void PlayNormalButton_Click(object sender, RoutedEventArgs e)// normal level button pressed
        {
            if (lbUsers.SelectedItem != null && lbUsers.SelectedItem.ToString() != Username)
            {
                Client.GameRequest(Username, lbUsers.SelectedItem.ToString(),"normal");
            }
        }

        private void PlayHardButton_Click(object sender, RoutedEventArgs e) // hard level button pressed
        {
            if (lbUsers.SelectedItem != null && lbUsers.SelectedItem.ToString() != Username)
            {
                Client.GameRequest(Username, lbUsers.SelectedItem.ToString(), "hard");
            }
        }


        //The Delegates

        public delegate void MoveDelegate(int row, int col);
        public event MoveDelegate sendMove;

        public delegate void GameExitDelegate(string opponent);
        public event GameExitDelegate gameExit;

        public delegate void FinishDelegate(string winner, string looser);
        public event FinishDelegate finishGame;

        public delegate void GetStatsDelegate(List<dynamic> list);
        public event GetStatsDelegate populate;

    }
}
