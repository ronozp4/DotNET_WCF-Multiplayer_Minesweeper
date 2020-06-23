using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WcfMineServer
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
ConcurrencyMode = ConcurrencyMode.Multiple)]

    public class MineService : IMineService
    {
        //properties
        private SortedDictionary<string, IMineServiceCallback> clients ;
        private List<string> playingClients;
        private List<GameMatch> liveGames;
        private static int liveGamesCounter;
        private static int gamesCounter;
        private static Dictionary<int, List<int>> mineLocation;
        private Random rnd = new Random();
        private int randomNumber1;
        private int randomNumber2;



        // MineService C'tor.
        public MineService()
        {
            liveGames = new List<GameMatch>();
            clients = new SortedDictionary<string, IMineServiceCallback>();
            playingClients = new List<string>(); 
            liveGamesCounter = 0;
            //set gamesCounter.
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    var gameCount = (from g in cdb.Games
                                     select g).Count();
                    gamesCounter = gameCount;
                }
                catch
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Some error in DataBase while openning server."
                    };
                    throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
                }
            }

            //Clean LiveGames table.
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    var liveGame = (from g in cdb.LiveGames
                                    select g);
                    cdb.LiveGames.RemoveRange(liveGame);
                    cdb.SaveChanges();
                }
                catch
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Some error in DataBase while openning server."
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
        }

        //Registration 

        public void Register(string userName, string password)
        {
            Boolean userExistFlag = true;
            Client newClient = new Client
            {
                Name = userName,
                Password = password
            };

            try
            {
                if (!IsValidName(newClient.Name)) throw new Exception();
            }
            catch
            {
                UserFaultException fault = new UserFaultException()
                {
                    Message = "Usernamme should be at least 3 charecters long and contain only numbers and digits"
                };
                throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message)); ;
            }
            try
            {
                if (!IsValidPassword(newClient.Password)) throw new Exception();
            }
            catch
            {
                UserFaultException fault = new UserFaultException()
                {
                    Message = "Password should be at least 6 charecters long and contain only numbers and digits"
                };
                throw new FaultException<UserFaultException>(fault);
            }
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    if (cdb.Clients.Any(o => o.Name == newClient.Name)) throw new Exception();
                    else userExistFlag = false;
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Usernamme is taken"
                    };
                    throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
                }
                try
                {
                    if (!userExistFlag)
                    {
                        cdb.Clients.Add(newClient);
                        cdb.SaveChanges();
                    }
                    else throw new Exception(); //Unknown error reached.

                }
                catch (Exception)
                {
                    //error adding new user to database.
                    UserFaultException fault = new UserFaultException()
                    { Message = "Something went wrong, try again." };
                    throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
                }
            }
        }

        // Check the given name

        private bool IsValidName(string name)
        {
            return Regex.IsMatch(name, @"^[A-Za-z0-9]{3,15}$");
        }
        
        //Check the given password
        private bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password, @"^[A-Za-z0-9]{6,15}$");
        }

        //Check if the client is exist in DB and not connected
        public Boolean ClientExist(string username, string password)
        {
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            { 
                try
                {
                    if (clients.ContainsKey(username))
                        throw new Exception();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "You are already connected"
                    };

                    throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
                }
                try
                {
                    if (cdb.Clients.Any(o => o.Name == username && o.Password == password))
                    {
                        
                        return true;
                    }
                    else throw new Exception();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Usernamme or Password are incorrect"
                    };

                    throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
                }

            }
        }

        //Client connected event
        public void ClientConnected(string username, string password)
        {
            Boolean userExistFlag = false;
            Client newClient = new Client
            {
                Name = username,
                Password = password
            };           
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    if (cdb.Clients.Any(o => o.Name == newClient.Name && o.Password == newClient.Password))
                    {
                        userExistFlag = true;
                    }
                    else throw new Exception();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Usernamme or Password are incorrect"
                    };
                    
                    throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
                }
            }
            try
            {
                if (userExistFlag)
                {
                    IMineServiceCallback callback =OperationContext.Current.GetCallbackChannel<IMineServiceCallback>();
                    clients.Add(newClient.Name, callback);
                    Thread updateThread = new Thread(UpdateClientsLists);
                    updateThread.Start();
                }
                else throw new Exception(); //Unknown error reached.

            }
            catch (Exception)
            {
                //user name is already in clients dictionary => already connected.
                UserFaultException fault = new UserFaultException()
                { Message = "You are already connected" };
                throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
            }
        
        }

        //The client has disconected
        public void ClientDisconnected(string userName)
        {
            clients.Remove(userName);
            Thread updateThread = new Thread(UpdateClientsLists);
            updateThread.Start();
        }

        //update the client list
        private void UpdateClientsLists()
        {
            List<string> availible = new List<string>();
            foreach (var player in clients.Keys)
            {
                if (!playingClients.Contains(player))
                {
                    availible.Add(player);
                }
            }
            foreach (var player in clients.Keys)
            {
                clients[player].UpdateClientsList(availible);
            }
        }  
        
        // Refresh client list
         public void RefreshClientListFromUser()
        {
        Thread updateThread = new Thread(UpdateClientsLists);
        updateThread.Start();
         }

        //check if he client contains in the playing players list
        public Boolean PlayerAvailible(string player)
        {
            return playingClients.Contains(player) ? false : true;
        }

        //send message from one client to another
        public void SendMessage(string message, string fromClient, string toClient)
        {
            if (clients.ContainsKey(toClient))
            {
                Thread sendThread = new Thread(() =>
                        clients[toClient].SendMessageToClient(message, fromClient));
                sendThread.Start();
            }
        }

        //Delete client from the DB
        public void DeleteClient(string userName, string password)
        {
            Boolean userPassFlag = false;
            Client remClient = new Client
            {
                Name = userName,
                Password = password
            };
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    var remFriend = cdb.Clients.Where(o => o.Name == remClient.Name &&
                       o.Password == remClient.Password).FirstOrDefault();
                    if (remFriend != null)
                    {
                        userPassFlag = true;
                        cdb.Clients.Remove(remFriend);
                        cdb.SaveChanges();
                    }
                    else throw new Exception();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Password is incorrect"
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
                try
                {
                    if (userPassFlag)
                    {

                        clients.Remove(remClient.Name);
                        Thread updateThread = new Thread(UpdateClientsLists);
                        updateThread.Start();
                    }
                    else throw new Exception(); //Unknown error reached.
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    { Message = "Something went wrong, try again." };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
        }

        //change the clients password
        public void ChangeClientPassword(string userName, string oldPassword, string newPassword)
        {
            Client passClient = new Client
            {
                Name = userName,
                Password = oldPassword
            };

            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    if (!IsValidPassword(newPassword))
                        throw new Exception();
                    var passChange = (from c in cdb.Clients
                                      where c.Name == passClient.Name && c.Password == passClient.Password
                                      select c).FirstOrDefault();
                    if (passChange == null) throw new Exception();
                    else passChange.Password = newPassword;
                    cdb.SaveChanges();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    { Message = "Password change failed" };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
        }

        //Get Statistics of players

        public List<dynamic> GetPlayerStatistics(string playerName)
        {
            List<dynamic> playerStats = new List<dynamic>();
            int winPercent = 0;

            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    var player = (from c in cdb.Clients
                                  where c.Name == playerName
                                  select c).SingleOrDefault();
                    if ((player.GamesPlayed != 0))
                    {
                        winPercent = ((player.GamesWon * 100) / player.GamesPlayed);
                    }
                    playerStats.Add(player.Name);
                    playerStats.Add(player.GamesPlayed);
                    playerStats.Add(player.Points);
                    playerStats.Add(player.GamesWon);
                    playerStats.Add(player.GamesLost);
                    playerStats.Add(player.GamesTie);
                    playerStats.Add(winPercent);
                }
                catch
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Error in showing player statistics"
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
            return playerStats;
        }

        //start a game request thread
        public void GameRequest(string fromClient, string toClient,string gameType)
        {
             try
            {
                if (clients.ContainsKey(toClient) &&  !playingClients.Contains(fromClient))
                {
                    Thread sendThread = new Thread(() => clients[toClient].SendGameRequest(toClient, fromClient, gameType));
                    sendThread.Start();
                }
            }
            catch (Exception)
            {
                UserFaultException fault = new UserFaultException()
                {
                    Message = "SendGameRequest Error"
                };
                throw new FaultException<UserFaultException>(fault, new FaultReason(fault.Message));
            }
        }

        //manage the game starting
        public void StartGame(string Player1, string Player2,string gameType)
        {
            
            try
            {
               RefreshLiveGameTable(Player1, Player2);
            }
            catch (FaultException<UserFaultException> error)
            {
                throw error;
            }
            try
            {
                playingClients.Add(Player1);
                playingClients.Add(Player2);
                
                Thread updateThread = new Thread(() => UpdateClientsLists());
                updateThread.Start();
                
                Thread messageToAllThread = new Thread(() => SendGameMessageToAllAvailible(Player1, Player2));
                messageToAllThread.Start();
                
                GameMatch match = new GameMatch();
                match.sendMove += SendMoveToOpponent;
                match.endGame += EndGame;
                Task matchTask = Task.Factory.StartNew(() => match.MatchInit(Player1, Player2));
                Task.WaitAny(matchTask);
                liveGames.Add(match);
                
                Thread player1Thread = new Thread(() => clients[Player1].GameInit(Player1, Player2, gameType));
                player1Thread.Start();        
            }
            catch (Exception)
            {
                UserFaultException fault = new UserFaultException()
                {
                    Message = "Server game starting error"
                };
                throw new FaultException<UserFaultException>(fault);
            }
            
        }

        //Send game stat message to all the waiting players in the lobby
        private void SendGameMessageToAllAvailible(string player1, string player2)
        {
            List<string> availible = new List<string>();
            foreach (var player in clients.Keys)
            {
                if (!playingClients.Contains(player))
                {
                    availible.Add(player);
                }
            }
            foreach (var player in clients.Keys)
            {
                if (availible.Contains(player))
                    clients[player].SendMessageToClient(player1 + " started to play against " + player2, "Server");
            }
        }

        //Player make move
        public Boolean PlayerTurn(int row,int col, string playerName)
        {
            try
            {
                GameMatch match = liveGames.Find(g => g.playerYellow == playerName); //find the match
                if (match == null)
                    match = liveGames.Find(g => g.playerBlue == playerName);//if you dont find with the yellow player
                if (match == null)
                    throw new Exception("Game error, Please return to main window.");
                if (match.PlayerTurn(row,col, playerName))
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                UserFaultException fault = new UserFaultException()
                {
                    Message = e.Message
                };
                throw new FaultException<UserFaultException>(fault);
            }
        }

        //after the game ended
        public void EndGame(string winner, string loser,bool isTie)
        {
            try
            {
                EndGameDBUpdate(winner, loser,isTie);
                GamesCounterAdd();
            }
            catch (Exception e)
            {
                UserFaultException fault = new UserFaultException()
                {
                    Message = e.Message
                };
                throw new FaultException<UserFaultException>(fault);
            }
            playingClients.Remove(winner);
            playingClients.Remove(loser);
            UpdateClientsLists();
            
            GameMatch match = liveGames.Find(g => g.playerYellow == winner);
            if (match == null)
                match = liveGames.Find(g => g.playerBlue == winner);
            if (match != null)
                liveGames.Remove(match);
            LiveGamesCounterReduce();
            
            if (isTie)
            {
                Thread gameFinishedTie1 = new Thread(() => clients[winner].GameFinished(winner, winner));
                Thread gameFinishedTie2 = new Thread(() => clients[loser].GameFinished(loser, loser));
                gameFinishedTie1.Start();
                gameFinishedTie2.Start();
            }
            else
            {
                Thread gameFinishedWinner = new Thread(() => clients[winner].GameFinished(winner, loser));
                Thread gameFinishedLooser = new Thread(() => clients[loser].GameFinished(winner, loser));
                gameFinishedWinner.Start();
                gameFinishedLooser.Start();
            }
        }

        //Refresh the live game table 
        private void RefreshLiveGameTable(string Player1, string Player2)
        {
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    LiveGame liveGame = new LiveGame
                    {
                        Serial = LiveGamesCounterAdd(),
                        Player1Name = Player1,
                        Player2Name = Player2,
                        GameStarted = GetCurrentDateTime()
                    };
                    cdb.LiveGames.Add(liveGame);
                    cdb.Entry(liveGame).State = System.Data.Entity.EntityState.Added;
                    cdb.SaveChanges();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Server error."
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
        }

        //get the current date
        private DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }

        //Send your move to the opponent
        public void SendMoveToOpponent(string opponent, int row,int col)
        {
            Task moveTask = Task.Factory.StartNew(() => clients[opponent].SendMove(row, col));
            Task.WaitAny(moveTask);
        }

        //Game Conter +
        private static int GamesCounterAdd()
        {
            return ++gamesCounter;
        }

        //Live Games +
        private static int LiveGamesCounterAdd()
        {
            return ++liveGamesCounter;
        }

        //Live Games -
        private static void LiveGamesCounterReduce()
        {
            --liveGamesCounter;
        }
        //Update the DB after game ends
        private void EndGameDBUpdate(string winner, string loser, bool isTie)
        {
            //Remove game from Livegames table.
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    LiveGame endedGame = (from g in cdb.LiveGames
                                          where g.Player1Name == winner || g.Player2Name == winner
                                          select g).SingleOrDefault();
                    if (endedGame != null)
                    {
                        cdb.LiveGames.Remove(endedGame);
                        cdb.Entry(endedGame).State = System.Data.Entity.EntityState.Deleted;
                        cdb.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Server error."
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
            //Add game to Games table.
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    Game game = new Game
                    {
                        Serial = gamesCounter,
                        Winner = winner,
                        Loser = loser,
                        Tie=isTie,
                        GameEnded = GetCurrentDateTime()
                    };
                    cdb.Games.Add(game);
                    cdb.Entry(game).State = System.Data.Entity.EntityState.Added;
                    cdb.SaveChanges();
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Server error."
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
            //Update Clients table properties.
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {

                    if (!isTie)
                    {
                        Client Winner = (from w in cdb.Clients
                                         where w.Name == winner
                                         select w).SingleOrDefault();
                        Winner.GamesPlayed++;
                        Winner.Points++;
                        Winner.GamesWon++;
                        
                        Client Loser = (from l in cdb.Clients
                                        where l.Name == loser
                                        select l).SingleOrDefault();
                        Loser.GamesPlayed++;
                        Loser.GamesLost++;


                        
                        cdb.Entry(Winner).State = System.Data.Entity.EntityState.Modified;
                        cdb.Entry(Loser).State = System.Data.Entity.EntityState.Modified;
                        cdb.SaveChanges();

                    }
                    else //its Tie
                    {
                        Client Winner = (from w in cdb.Clients
                                         where w.Name == winner
                                         select w).SingleOrDefault();
                        Winner.GamesPlayed++;
                        Winner.GamesTie++;
                        
                        Client Loser = (from l in cdb.Clients
                                        where l.Name == loser
                                        select l).SingleOrDefault();
                        Loser.GamesPlayed++;
                        Loser.GamesTie++;

                        cdb.Entry(Winner).State = System.Data.Entity.EntityState.Modified;
                        cdb.Entry(Loser).State = System.Data.Entity.EntityState.Modified;
                        cdb.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Server error."
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
        }

        //Send the mines location list and the safe places ammount
        public void SendMineLocation(Dictionary<int, int[]> mineLocation, int safePlaceAmount, string playerName)
        {
            try
            {
                GameMatch match = liveGames.Find(g => g.playerYellow == playerName);
                if (match == null)
                    match = liveGames.Find(g => g.playerBlue == playerName);
                if (match == null)
                    throw new Exception();
                match.setMineLocation(mineLocation, safePlaceAmount);
            }catch (Exception)
            {
                UserFaultException fault = new UserFaultException()
                { Message = "Send MineLocation fail. / match not exist" };
                throw new FaultException<UserFaultException>(fault);
            }
        
        }

        //get the statistics of Games
        public List<dynamic> GetGameStatistics(string getFactor)
        {
            List<dynamic> wantedStats = new List<dynamic>();
            List<dynamic> allWantedStats = new List<dynamic>();

            switch (getFactor)
            {
                case "Players":
                    {
                        try
                        {
                            using (var cdb = new minesweeper_RonOzeritskyEntities())
                            {
                                foreach (var player in cdb.Clients)
                                {
                                    wantedStats = GetPlayerStatistics(player.Name);
                                    allWantedStats.AddRange(wantedStats);
                                }
                                return allWantedStats;
                            }
                        }
                        catch (Exception)
                        {
                            UserFaultException fault = new UserFaultException()
                            { Message = "Database fail." };
                            throw new FaultException<UserFaultException>(fault);
                        }
                    }
                case "Games":
                    {
                        try
                        {
                            using (var cdb = new minesweeper_RonOzeritskyEntities())
                            {
                                foreach (var game in cdb.Games)
                                {
                                    if (game.Tie)
                                    {
                                        allWantedStats.Add("Tie");
                                        allWantedStats.Add("Tie");
                                    }
                                    else
                                    {
                                        allWantedStats.Add(game.Winner);
                                        allWantedStats.Add(game.Loser);
                                    }
                                        allWantedStats.Add(String.Format("{0:g}", game.GameEnded));
                                }
                                return allWantedStats;
                            }
                        }
                        catch (Exception)
                        {
                            UserFaultException fault = new UserFaultException()
                            { Message = "Database fail." };
                            throw new FaultException<UserFaultException>(fault);
                        }
                    }
                case "LiveGames":
                    {
                        try
                        {
                            using (var cdb = new minesweeper_RonOzeritskyEntities())
                            {
                                foreach (var game in cdb.LiveGames)
                                {
                                    allWantedStats.Add(game.Player1Name);
                                    allWantedStats.Add(game.Player2Name);
                                    allWantedStats.Add(String.Format("{0:g}", game.GameStarted));
                                }
                                return allWantedStats;
                            }
                        }
                        catch (Exception)
                        {
                            UserFaultException fault = new UserFaultException()
                            { Message = "Database fail." };
                            throw new FaultException<UserFaultException>(fault);
                        }
                    }
                    default:
                    {
                        return null;
                    }
            }
        }

        // The player has finished playing
        public void PlayerFinishedPlaying(string playerName, string opponent)
        {
            using (var cdb = new minesweeper_RonOzeritskyEntities())
            {
                try
                {
                    LiveGame endedGame = (from g in cdb.LiveGames
                                          where g.Player1Name == playerName || g.Player2Name == playerName
                                          select g).SingleOrDefault();
                    if (endedGame != null)
                    {
                        cdb.LiveGames.Remove(endedGame);
                        cdb.Entry(endedGame).State = System.Data.Entity.EntityState.Deleted;
                        cdb.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    UserFaultException fault = new UserFaultException()
                    {
                        Message = "Server error."
                    };
                    throw new FaultException<UserFaultException>(fault);
                }
            }
            if (playingClients.Contains(playerName))
            {
                playingClients.Remove(playerName);
                playingClients.Remove(opponent);
                UpdateClientsLists();
                GameMatch match = liveGames.Find(g => g.playerYellow == playerName);
                if (match == null)
                    match = liveGames.Find(g => g.playerBlue == playerName);
                LiveGamesCounterReduce();
                if (match != null)
                    liveGames.Remove(match);
                Thread gameExit = new Thread(() => clients[opponent].GameExit(playerName));
                gameExit.Start();
            }
        }

        //Generating Mines (Only one time for couple)
        public Dictionary<int, List<int>> GenerateMines(int minesAmmount , int RowsNum,bool regenerate)
        {
            if (regenerate)
            {
                int ColNum =RowsNum;

                if (minesAmmount == 99)
                    ColNum = 30;

                mineLocation = new Dictionary<int, List<int>>();
                for (int i = 0; i < RowsNum; i++)
                    mineLocation.Add(i, new List<int>());

                for (int i = 0; i < minesAmmount; i++)
                {
                    randomNumber1 = rnd.Next(0, RowsNum);
                    randomNumber2 = rnd.Next(0, ColNum);

                    if (!mineLocation[randomNumber1].Contains(randomNumber2))
                      mineLocation[randomNumber1].Add(randomNumber2);
                    else
                      i--;
                    
                }
            }
            return mineLocation;
        }
    }
}
