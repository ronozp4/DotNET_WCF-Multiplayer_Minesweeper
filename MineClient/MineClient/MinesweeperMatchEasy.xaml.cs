using MineClient.ServiceReference1;
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

namespace MineClient
{
    //easy player vs player game
    public partial class MinesweeperMatchEasy : Window
    {
        private const int _ROW = 9;
        private const int _MINES = 10;
        private const int _SAFE_PLACES = 71; // 
        private static string player;
        private static string opponent;
        private static string username;
        private List<Button>[] BoardManager;
        private Dictionary<int, int[]> mineLocation = new Dictionary<int, int[]>();

        public MinesweeperMatchEasy(string player1, string player2, string userName)
        {
            InitializeComponent();
            ClientCallback callback = new ClientCallback();
            MineServiceClient client = new MineServiceClient(new InstanceContext(callback));

            player = userName;
            opponent = userName == player1 ? player2 : player1;
            username = userName;

            //board initialize
            BoardManager = new List<Button>[_ROW];
            for (int i = 0; i < _ROW; ++i)
            {
                BoardManager[i] = new List<Button>();
            }
            Init();

            //mine generate
            if (userName == player2)
            {
                Turn_lbl.Content = "Yellow";
                mineLocation = client.GenerateMines(_MINES, _ROW, true);
            }
            else
            {
                mineLocation = client.GenerateMines(_MINES, _ROW, false);
                Turn_lbl.Content = "Blue";
            }
             try {
                client.SendMineLocation(mineLocation, _SAFE_PLACES, username);
            }
            catch (FaultException<UserFaultException> ex)
            {
                MessageBox.Show(ex.Detail.Message);
            }


        }

        //check how much mines ther is around 
        public int numberOfMinesAround(int x, int y)
        {
            int count = 0;
            if (mineLocation.ContainsKey(x + 1))
            {
                if (mineLocation[x + 1].Contains(y + 1))
                    count++;
                if (mineLocation[x + 1].Contains(y - 1))
                    count++;
                if (mineLocation[x + 1].Contains(y))
                    count++;
            }

            if (mineLocation.ContainsKey(x - 1))
            {
                if (mineLocation[x - 1].Contains(y + 1))
                    count++;
                if (mineLocation[x - 1].Contains(y - 1))
                    count++;
                if (mineLocation[x - 1].Contains(y))
                    count++;
            }

            if (mineLocation[x].Contains(y + 1))
                count++;
            if (mineLocation[x].Contains(y - 1))
                count++;

            return count;
        }


        //Mouse event
        private void MouseClick(object sender, RoutedEventArgs e)
        {
            ClientCallback callback = new ClientCallback();
            MineServiceClient client = new MineServiceClient(new InstanceContext(callback));
            Button button = (Button)sender;
            string TextBoxName = button.Name;
            int col = Int32.Parse(TextBoxName.Substring(3, 2));
            int row = Int32.Parse(TextBoxName.Substring(1, 2));
            try
            {
                if (button.Background == Brushes.White) //empty tile 
                {
                    if (client.PlayerTurn(row, col, username))
                    {
                        if (mineLocation[row].Contains(col)) //mined
                        {
                            button.Content = "*";
                            button.Background = Brushes.Red;
                        }
                        else //not mined
                        {
                            button.Content = "" + numberOfMinesAround(row, col);

                            button.Background = Brushes.DeepSkyBlue;
                            Turn_lbl.Content = "Yellow";
                        }
                    }
                }
            }
            catch (FaultException<UserFaultException> ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //on loading the window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Blue: " + player + " Vs Yellow: " + opponent;
        }

        //drowing the opponent
        public void DrawOpponent(int row, int col)
        {         
                    if (mineLocation[row].Contains(col))
                    {
                        BoardManager[row][col].Content = "*";
                        BoardManager[row][col].Background = Brushes.Red;
                    }
                    else
                    {
                        BoardManager[row][col].Content = "" + numberOfMinesAround(row, col);

                        BoardManager[row][col].Background = Brushes.Yellow;
                        Turn_lbl.Content = "Blue";
                    }
        }

        //game winner/loser/tie announcement
        public void GameFinished(string winner, string loser)
        {
            if (winner == loser)
            {
                MessageBox.Show("Its a Tie!", "Tie", MessageBoxButton.OK);
                this.Close();
            }
            else
            {
                if (winner.Equals(username))
                {
                    MessageBox.Show("You are the winner!", "Winner", MessageBoxButton.OK);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Oh no, you lost!", "Looser", MessageBoxButton.OK);
                    this.Close();
                }
            }

        }

        //opponent left the game
        public void OpponentLeft(string opponent)
        {
            MessageBoxResult result = MessageBox.Show(opponent + " has left the game!",
                                                    "Game finished", MessageBoxButton.OK);
            this.Close();
        }

        //mark mine location (right button click)
        private new void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = (Button)sender;
            if (!(button.Background == Brushes.Yellow) && !(button.Background == Brushes.DeepSkyBlue))
            {
                if (button.Background == Brushes.Green)
                    button.Background = Brushes.White;
                else
                    button.Background = Brushes.Green;
            }
        }

        //list for all the buttons 
        private void Init()
        {
            foreach (Button l in A1.Children) BoardManager[0].Add(l);
            foreach (Button l in A2.Children) BoardManager[1].Add(l);
            foreach (Button l in A3.Children) BoardManager[2].Add(l);
            foreach (Button l in A4.Children) BoardManager[3].Add(l);
            foreach (Button l in A5.Children) BoardManager[4].Add(l);
            foreach (Button l in A6.Children) BoardManager[5].Add(l);
            foreach (Button l in A7.Children) BoardManager[6].Add(l);
            foreach (Button l in A8.Children) BoardManager[7].Add(l);
            foreach (Button l in A9.Children) BoardManager[8].Add(l);
        }

        //game closed event
        private void Window_Closed(object sender, EventArgs e)
        {
            ClientCallback callback = new ClientCallback();
            MineServiceClient client = new MineServiceClient(new InstanceContext(callback));

            client.PlayerFinishedPlaying(username, opponent);
        }
    }

}
