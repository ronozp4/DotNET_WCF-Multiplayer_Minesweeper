using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfMineServer
{
    [ServiceContract(CallbackContract = typeof(IMineServiceCallback))]
    public interface IMineService
    {
        // All the functions Statements from MineService


        [FaultContract(typeof(UserFaultException))]
        [OperationContract]
        void ClientConnected(string username,string password);

        [OperationContract]
        void ClientDisconnected(string username);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void Register(string userName, string password);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void RefreshClientListFromUser();

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        Boolean PlayerAvailible(string player);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void GameRequest(string fromClient, string toClient, string gameType);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void DeleteClient(string userName, string password);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        Boolean PlayerTurn(int row, int col, string playerName);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void ChangeClientPassword(string userName, string oldPassword, string newPassword);

        [OperationContract(IsOneWay = true)]
        void StartGame(string Player1, string Player2, string gameType);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        List<dynamic> GetPlayerStatistics(string playerName);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        Boolean ClientExist(string username, string password);

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string fromClient, string toClient);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        Dictionary<int, List<int>> GenerateMines(int minesAmmount, int RowsNum,bool regenerate);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void SendMineLocation(Dictionary<int, int[]> mineLocation,int safePlaceAmount, string playerName);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void SendMoveToOpponent(string opponent, int row,int col);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        void PlayerFinishedPlaying(string playerName, string opponent);

        [OperationContract]
        [FaultContract(typeof(UserFaultException))]
        List<dynamic> GetGameStatistics(string getFactor);

    }

}
