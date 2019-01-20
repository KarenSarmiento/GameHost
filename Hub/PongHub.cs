using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace GameHost.Hubs
{
    public static class Globals
    {
        public static Dictionary<string, Pong> gameInstances = new Dictionary<string, Pong>();
        public static Dictionary<int, string> userIdToConnectionId = new Dictionary<int, string>(); 
        public static int gameInstancePin = 0;    
    }

    public class PongHub : Hub
    {

        // 1. Called by display to begin a game of Pong.
        // Then display waits for connection requests (with pin) from controllers.
        public void startPong(int userId) {
            updatedUserIdToConnectionIdMap(userId, Context.ConnectionId);
            string pin = generateRandomGamePin();
            Pong pong = new Pong(userId);
            Globals.gameInstances.Add(pin, pong);
            Clients.Client(getConnectionIdFromUserId(userId)).SendAsync("ackStartPong", pin);
        }

        public void notifyChangeConnectionId(int userId) {
            updatedUserIdToConnectionIdMap(userId, Context.ConnectionId);
        }

        // 2. Controller attempts to conect to a game with a pin.
        // If pin false, then return FAILED
        // Else if the games is empty, wait for more player connection requests and return WAITING
        // If one player already is waiting return SUCCESS
        public void controlConnectionRequest(int userId, string pin) {
            updatedUserIdToConnectionIdMap(userId, Context.ConnectionId);
            Pong instance = new Pong(0);
            bool existsGameInstance = Globals.gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance && !instance.isGameInProgress()) {
                int numPlayersInGame = instance.getNumPlayersInGame();
                if (numPlayersInGame == 0) {
                    instance.setPOneID(userId);
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackControlConnectionRequest", "WAIITNG");
                } else {
                    instance.setPTwoID(userId);
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackControlConnectionRequest", "SUCCESS");
                    Clients.Client(getConnectionIdFromUserId(instance.getPTwoID())).SendAsync("ackControlConnectionRequest", "SUCCESS");
                    Clients.Client(getConnectionIdFromUserId(instance.getDisplayID())).SendAsync("ackControlConnectionRequest", "SUCCESS");
                    instance.setGameInProgress(true);
                }
                instance.incrementNumPlayersInGame();
            } else {
                Clients.Client(Context.ConnectionId).SendAsync("ackControlConnectionRequest", "FAILED");
            }
        }

        // 3. In game, controller sends what arrows are pressed. This is redirected to display.
        public void buttonPressed(int userId, string pin, string button) {
            updatedUserIdToConnectionIdMap(userId, Context.ConnectionId);
            Pong instance;
            bool existsGameInstance = Globals.gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance) {
                Clients.Client(getConnectionIdFromUserId(instance.getDisplayID())).SendAsync("ackButtonPressed", button);
            }
        }

        // 4. Display signals when game is over. This is redirected to controllers.
        public void endGame(int userId, string pin, string winner) {
            System.Console.Error.WriteLine("CALLED END GAME");
            updatedUserIdToConnectionIdMap(userId, Context.ConnectionId);
            Pong instance;
            bool existsGameInstance = Globals.gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance) {
                if (string.Equals(winner, "1")) {
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackEndGame", "WINNER");
                    Clients.Client(getConnectionIdFromUserId(instance.getPTwoID())).SendAsync("ackEndGame", "LOSER");
                } else if (string.Equals(winner, "2")) {
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackEndGame", "LOSER");
                    Clients.Client(getConnectionIdFromUserId(instance.getPTwoID())).SendAsync("ackEndGame", "WINNER");
                } else {
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackEndGame", "UNKNOWN");
                    Clients.Client(getConnectionIdFromUserId(instance.getPTwoID())).SendAsync("ackEndGame", "UNKNOWN");
                    System.Console.Error.WriteLine("Invalid endGame string received.");
                }
                Globals.gameInstances.Remove(pin);
            }
        }

        // Controller signals if it decides to leave the game despite it not having finished.
        public void endGameFromController(int userId, string pin) {
            updatedUserIdToConnectionIdMap(userId, Context.ConnectionId);
            Pong instance;
            bool existsGameInstance = Globals.gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance) {
                if (Context.ConnectionId == getConnectionIdFromUserId(instance.getPOneID())) {
                    Clients.Client(getConnectionIdFromUserId(instance.getPTwoID())).SendAsync("ackEndGameFromController");
                } else if (Context.ConnectionId == getConnectionIdFromUserId(instance.getPTwoID())) {
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackEndGameFromController");
                } else {
                    Clients.Client(getConnectionIdFromUserId(instance.getPOneID())).SendAsync("ackEndGameFromController");
                    Clients.Client(getConnectionIdFromUserId(instance.getPTwoID())).SendAsync("ackEndGameFromController");
                    System.Console.Error.WriteLine("Dont know which controller attempted to end game.");
                }
                Clients.Client(getConnectionIdFromUserId(instance.getDisplayID())).SendAsync("ackEndGameFromController");
                Globals.gameInstances.Remove(pin);
            }
        }


        private string generateRandomGamePin() {
            Globals.gameInstancePin++;
            return Globals.gameInstancePin.ToString();
        }

        private string getConnectionIdFromUserId(int userId) {
            return Globals.userIdToConnectionId[userId];
        }

        private void updatedUserIdToConnectionIdMap(int userId, string connectionId) {
            Globals.userIdToConnectionId[userId] = connectionId;
        }
    }
}