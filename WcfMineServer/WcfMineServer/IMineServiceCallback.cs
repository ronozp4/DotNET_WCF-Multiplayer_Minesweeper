using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WcfMineServer
{
    public interface IMineServiceCallback
    {
        // Update online clients.
        [OperationContract(IsOneWay = true)]
        void UpdateClientsList(IEnumerable<string> users);

        // Send game request to opponent.
        [OperationContract(IsOneWay = true)]
        void SendGameRequest(string toClient, string fromClient, string gameType);

        // Send message to other connected client.
        [OperationContract(IsOneWay = true)]
        void SendMessageToClient(string message, string fromClient);

        // Player won the game, anounce and close the game.
        [OperationContract(IsOneWay = true)]
        void GameFinished(string winner, string looser);

        // If one of the players closed game window while playing.
        [OperationContract(IsOneWay = true)]
        void GameExit(string opponent);

        // Challenged client initiates game against challenging client.
        [OperationContract(IsOneWay = true)]
        void GameInit(string player1, string player2,string gameType);

        // Send player move to opponent.
        [OperationContract(IsOneWay = true)]
        void SendMove(int row, int col);

    }
}
