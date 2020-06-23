using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Minesweeper.Properties;

namespace Minesweeper
{
    public partial class MinesweeperForm : Form
    {
        private Difficulty difficulty;
        private bool timeStop= true ;   

        public MinesweeperForm()
        {
            InitializeComponent();
            this.LoadGame(null, null);

            this.tileGrid.TileFlagStatusChange += this.TileFlagStatusChanged;
            this.tileGrid.GameWinStatusChanged += this.GameWinStatusChanged;
  
        }

        private enum Difficulty {  Intermediate, Expert, Beginner }

        private void LoadGame(object sender, EventArgs e)
        {
            seconds = 0;
            timer.Start();
            int x, y, mines;
            switch (this.difficulty) //setting the Difficulty settings 
            {
                case Difficulty.Beginner: //easy
                    x = y = 9;
                    mines = 10;
                    break;
                case Difficulty.Intermediate: //normal
                    x = y = 16;
                    mines = 40;
                    break;
                case Difficulty.Expert: //hard
                    x = 30;
                    y = 16;
                    mines = 99;
                    break;
                default:
                    throw new InvalidOperationException("Unimplemented difficulty selected");

            }
            this.tileGrid.LoadGrid(new Size(x, y), mines); // load the grid
            this.MaximumSize= this.MinimumSize = new Size(this.tileGrid.Width +36,this.tileGrid.Height + 98); //set min/max size
            this.flagCounter.Text = mines.ToString(); //show mines amount
            this.flagCounter.ForeColor = Color.Black; //set starting color for flag counter 

        }

        private void MenuStrip_Game_New_Click(object sender, EventArgs e) //pressing new game
        {
            this.LoadGame(null, null);
        }

        private void MenuStrip_Game_Exit_Click(object sender, EventArgs e) //pressing exit
        {
            Application.Exit();
        }

        private void MenuStrip_Game_DifficultyChanged(object sender, EventArgs e) //selecting diffrent difficulty
        {
            this.difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), (string)((ToolStripMenuItem)sender).Tag);
            this.LoadGame(null, null);
        }

        private void TileFlagStatusChanged(object sender, TileGrid.TileFlagStatusChangedEventArgs e) //change flag counter
        {
            this.flagCounter.Text = e.Flags.ToString();
            this.flagCounter.ForeColor = e.LabelColour;
        }
        private void GameWinStatusChanged(object sender, TileGrid.GameWinStatusChangedEventArgs e) //after game end/start start/stop time and set picture
        {
             this.timeStop = e.stopTime; //time stop true/false
            if (e.WinLostNormal == 0)
            {
                this.gameButton.BackgroundImage = Resources.Normal; //normal image
            }else if (e.WinLostNormal == 1)
            {
                this.gameButton.BackgroundImage = Resources.Win; //win image
            }
            else if (e.WinLostNormal == 2)
                this.gameButton.BackgroundImage = Resources.Lose; //lose image
        }


        private class TileGrid : Panel //the tile grid panel class
        {
            private static readonly HashSet<Tile> gridSearchBlacklist = new HashSet<Tile>(); // the places you cant hit
            private static readonly Random random = new Random();
            private Size gridSize;
            private int mines;
            private int flags;
            private bool minesGenerated;

            internal event EventHandler<TileFlagStatusChangedEventArgs> TileFlagStatusChange = delegate { }; // delegate for flag status
            internal event EventHandler<GameWinStatusChangedEventArgs> GameWinStatusChanged = delegate { }; // delegate for game ending status

            private Tile this[Point point] => (Tile)this.Controls[$"Tile_{point.X}_{point.Y}"]; //set the tile point

            private void Tile_MouseDown(Object sender, MouseEventArgs e) // on click
            {

                Tile tile = (Tile)sender;

                if (!tile.Opened)
                {
                    switch (e.Button)
                    {
                        case MouseButtons.Left when !tile.Flagged:  //left button clicked
                            if (!this.minesGenerated) // if mines was no generated
                            {
                                this.GenerateMines(tile); //generate
                                GameWinStatusChanged(this, new GameWinStatusChangedEventArgs(false,0)); // start clock,set the image button on normal

                            }
                            if (tile.Mined) // hit a mine
                            {
                                this.DisableTiles(true);  //disable mines 
                                GameWinStatusChanged(this, new GameWinStatusChangedEventArgs(true,2)); //stop clock,set lose image
                            }
                            else
                            {
                                tile.TestAdjacentTiles(); // check the tile
                                gridSearchBlacklist.Clear();
                            }
                            break;
                        case MouseButtons.Right when this.flags > 0: //right button click
                            if (tile.Flagged)
                            {
                                tile.Flagged = false; //unflag
                                this.flags++;
                            }
                            else
                            {
                                tile.Flagged = true; //flag
                                this.flags--;
                            }
                            TileFlagStatusChange(this, new TileFlagStatusChangedEventArgs(this.flags, this.flags < this.mines * 0.25 ? Color.Red : Color.Black)); //flags ammount and text color
                            break;
                    }
                }

                this.CheckForWin(); // check if it win
            }

            internal void LoadGrid(Size gridSize,int mines) //set the minefield
            {
                this.minesGenerated = false;
                this.Controls.Clear();
                this.gridSize = gridSize;
                this.mines = this.flags = mines;
                this.Size=new Size(gridSize.Width *Tile.LENGTH,gridSize.Height * Tile.LENGTH );
                for (int x = 0; x < gridSize.Width; x++)
                {
                    for (int y = 0; y < gridSize.Height; y++)
                    {
                        Tile tile = new Tile(x,y);
                        tile.MouseDown += Tile_MouseDown;
                        this.Controls.Add(tile);
                    }
                }

                foreach (Tile tile in this.Controls)
                {
                    tile.SetAdjacentTiles();
                }
            }

            private void GenerateMines(Tile safeTile) // generating mines
            {
                int safeTilesCount = safeTile.AdjacentTiles.Length + 1;
                Point[] usedPositions = new Point[this.mines + safeTilesCount];
                usedPositions[0] = safeTile.GridPosition;
                for (int i = 1; i < safeTilesCount; i++)
                {
                    usedPositions[i] = safeTile.AdjacentTiles[i - 1].GridPosition;
                }
                for (int i = safeTilesCount; i < usedPositions.Length; i++)
                {
                    Point point = new Point(random.Next(this.gridSize.Width), random.Next(this.gridSize.Height));
                    if (!usedPositions.Contains(point))
                    {
                        this[point].Mine();
                        usedPositions[i] = point;
                    }
                    else
                    {
                        i--;
                    }
                }
                this.minesGenerated = true;
            }

            private void DisableTiles(bool gameLost) // disable tiles
            {
                foreach (Tile tile in this.Controls)
                {
                    tile.MouseDown -= this.Tile_MouseDown;
                    if (gameLost)
                    {
                        tile.Image = !tile.Opened && tile.Mined && !tile.Flagged ? Resources.Mine :
                            tile.Flagged && !tile.Mined ? Resources.FalseFlaggedTile : tile.Image;
                    }
                    
                }
            }

            private void CheckForWin() //chacking if its a win
            {

                if (this.flags != 0 || this.Controls.OfType<Tile>().Count(tile => tile.Opened || tile.Flagged) != this.gridSize.Width * this.gridSize.Height)
                {
                    return; //not win
                }
                GameWinStatusChanged(this, new GameWinStatusChangedEventArgs(true,1)); // stop clock ant set win image
                
                MessageBox.Show("Congratulations, you solved the game!", "Game solved", MessageBoxButtons.OK);
                
                this.DisableTiles(false);
            }

            private class Tile : PictureBox //The Tile Class
            {
                internal const int LENGTH = 25;
                private static readonly int[][] adjacentCoords= //coords array
                {
                    new[] {-1, -1}, new[] {0, -1}, new[] {1, -1}, new[] {1, 0}, new[] {1, 1}, new[] {0, 1},
                    new[] {-1, 1}, new[] {-1, 0}
                };

                private bool flagged; //check if flaged 

                internal Tile(int x,int y) //set all the tile settings
                {
                    this.Name = $"Tile_{x}_{y}";
                    this.Location = new Point(x*LENGTH ,y*LENGTH);
                    this.GridPosition = new Point(x,y);
                    this.Size = new Size(LENGTH,LENGTH);
                    this.Image = Resources.Tile;
                    this.SizeMode = PictureBoxSizeMode.Zoom;

                }
                internal Tile[] AdjacentTiles { get; private set; }
                internal Point GridPosition { get; }
                internal bool Opened { get; private set; }
                internal bool Mined { get; private set; }


                internal bool Flagged //get and set flags boolean
                {
                    get => this.flagged;
                    set
                    {
                        this.flagged = value;
                        this.Image = value ? Resources.Flag : Resources.Tile;
                    }
                }

                private int AdjacentMines => this.AdjacentTiles.Count(tile => tile.Mined);

                internal void SetAdjacentTiles()
                {
                    TileGrid tileGrid = (TileGrid)this.Parent;
                    List<Tile> adjacentTiles = new List<Tile>(8);

                    foreach (int[] adjacentCoord in adjacentCoords)
                    {
                        Tile tile = tileGrid[new Point(this.GridPosition.X + adjacentCoord[0], this.GridPosition.Y + adjacentCoord[1])];
                        if (tile != null)
                        {
                            adjacentTiles.Add(tile);
                        }
                    }
                    this.AdjacentTiles = adjacentTiles.ToArray();
                }
                internal void TestAdjacentTiles()
                {
                    if (this.flagged || gridSearchBlacklist.Contains(this)) // cant press on it
                    {
                        return;
                    }
                    gridSearchBlacklist.Add(this);
                    if (this.AdjacentMines == 0) // open all the blank spots
                    {
                        foreach (Tile tile in this.AdjacentTiles)
                        {
                            tile.TestAdjacentTiles();
                        }
                    }
                    this.Open(); // set the tile
                }

                internal void Mine()
                {
                    this.Mined = true;
                  
                }
                
                private void Open()
                {
                    this.Opened = true;
                    this.Image = (Image)Resources.ResourceManager.GetObject($"EmptyTile_{this.AdjacentMines}");
                }
            }

            internal class TileFlagStatusChangedEventArgs : EventArgs //falg status change event
            {
                internal int Flags { get; }
                internal Color LabelColour { get; }

                internal TileFlagStatusChangedEventArgs(int flags, Color labelColour)
                {
                    this.Flags = flags;
                    this.LabelColour = labelColour;
                }
            }

            internal class GameWinStatusChangedEventArgs : EventArgs //end game status change event
            {
                internal bool stopTime { get; }
                internal int WinLostNormal { get; } //normal= 0 ,win =1 , lost =2  

                internal GameWinStatusChangedEventArgs(bool stopTime,int WinLostNormal)
                {
                    this.stopTime = stopTime;
                    this.WinLostNormal = WinLostNormal;
                }
            }

        }
         int seconds = 0;
        private void timer_Tick(object sender, EventArgs e) //the timer, called evry second
        {
            if(!timeStop) //check if need to stop
            seconds++;

            this.time_lbl.Text = seconds.ToString(); //set the time
        }

        private void MinesweeperForm_FormClosed(object sender, FormClosedEventArgs e) //exit
        {
            Application.Exit();
        }
    }
}
