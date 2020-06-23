using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using MineClient.ServiceReference1;

namespace MineClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ClientCallback : IMineServiceCallback
    {
        // Update online clients.
        public delegate void UpdateListDelegate(string[] users);
        public event UpdateListDelegate updateUsers;
        public void UpdateClientsList(string[] users)
        {
            updateUsers(users);
        }


        // Send message to other connected client.
        public delegate void DisplaymessageDelegate(string message, string fromClient);
        public event DisplaymessageDelegate displayMessage;
        public void SendMessageToClient(string message, string fromClient)
        {
            displayMessage(message, fromClient);
        }

        // Send game request to opponent.
        public delegate void RequestDelegate(string toClient, string fromClient, string gameType);
        public event RequestDelegate gameRequestMessage;
        public void SendGameRequest(string toClient, string fromClient, string gameType)
        {
            gameRequestMessage(toClient, fromClient, gameType);
        }

        // Challenged client initiates game against challenging client.
        public delegate void StartGameDelegate(string player1, string player2, string gameType);
        public event StartGameDelegate displayBoardWindow;
        public void GameInit(string player1, string player2, string gameType)
        {
            displayBoardWindow(player1, player2, gameType);
        }

        // Pass the players move to  the opponent.
        public delegate void OpponentMoveDelegate(int row, int col);
        public event OpponentMoveDelegate opponentmove;
        public void SendMove(int row,int col)
        {
            opponentmove(row,col);
        }

        // If one of the players closed game window while playing.
        public delegate void GameExitDelegate(string opponent);
        public event GameExitDelegate closeGameBoard;
        public void GameExit(string opponent)
        {
            closeGameBoard(opponent);
        }

        //If Player won the game, anounce and close the game.
        public delegate void GameFinishedDelegate(string winner, string loser);
        public event GameFinishedDelegate gamefinished;
        public void GameFinished(string winner, string loser)
        {
            gamefinished(winner, loser);
        }

    }
}
