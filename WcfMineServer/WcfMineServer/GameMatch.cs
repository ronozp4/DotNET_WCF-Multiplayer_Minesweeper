using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WcfMineServer
{
    class GameMatch
    {
      public delegate void MoveDelegate(string opponent, int row,int col);
      public event MoveDelegate sendMove;

        public delegate void EndGameDelegate(string winner, string looser,bool isTie);
        public event EndGameDelegate endGame;
        private Dictionary<int, int[]> mineLocation;
        private Boolean turnFlag = true;
        public string playerYellow;
        public string playerBlue;
        private int playsLeft;

        // Game initialize function.
        
        public void MatchInit(string player1, string player2)
        {
            playerYellow = player1;
            playerBlue = player2;
        }

    // Players turn manager.
    

         public Boolean PlayerTurn(int row,int col, string whitchPlayer)
        {
            
              if (turnFlag == true && whitchPlayer.CompareTo(playerYellow) == 0)  //yellow player turn.
            {
                playsLeft--;
                turnFlag = false;
                //send move to other player.                    
                sendMove(playerBlue, row,col);
                if (onMine(row,col)) //check if he hit mine
                {
                    endGame(playerBlue, playerYellow,false);
                    return true;
                }
                if (IsTie()) //check if its tie
                {
                    endGame(playerBlue, playerYellow, true);
                }
                return true;
            }
            else if (turnFlag == false && whitchPlayer.CompareTo(playerBlue) == 0)   //blue player turn.   
            {
                playsLeft--;
                turnFlag = true;
                //send move to other player.                    
                sendMove(playerYellow, row, col);
                if (onMine(row, col)) //check if he hit mine
                {
                    endGame(playerYellow, playerBlue,false);
                    return true;
                }
                if (IsTie()) //check if its tie
                {
                    endGame(playerYellow, playerBlue, true);
                }

                return true;
            }
            else   //Not your turn.
            {
                return false;
            }

        }


        // Set Mine Location
        public void setMineLocation(Dictionary<int, int[]> minesLocation, int safePlaceAmount)
        {
            mineLocation = minesLocation;
            playsLeft = safePlaceAmount; // = (max amount of plays) - (mine amount)
        }

        Boolean onMine(int row,int col)
        {
            return mineLocation[row].Contains(col);
        }


          private bool IsTie()
          {
            if(playsLeft>0)
                return false; // no Tie found

            return true;
          }
    }
}

