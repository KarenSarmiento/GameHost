using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace GameHost.Hubs
{
    public class PongHub : Hub
    {
        private Dictionary<string, Pong> gameInstances = new Dictionary<string, Pong>();
        private int gameInstancePin = 0;

        // 1. Called by display to begin a game of Pong.
        // Then display waits for connection requests (with pin) from controllers.
        public void startPong() {
            string pin = generateRandomGamePin();
            Pong pong = new Pong(Context.ConnectionId);
            gameInstances.Add(pin, pong);
            Clients.Client(Context.ConnectionId).SendAsync("ackStartPong", pin);
        }

        // 2. Controller attempts to conect to a game with a pin.
        // If pin false, then return FAILED
        // Else if the games is empty, wait for more player connection requests and return WAITING
        // If one player already is waiting return SUCCESS
        public void controlConnectionRequest(string pin) {
            Pong instance;
            bool existsGameInstance = gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance && !instance.isGameInProgress()) {
                int numPlayersInGame = instance.getNumPlayersInGame();
                if (numPlayersInGame == 0) {
                    instance.setPOneID(Context.ConnectionId);
                    Clients.Client(instance.getPOneID()).SendAsync("ackControlConnectionRequest", "WAIITNG");
                } else {
                    instance.setPTwoID(Context.ConnectionId);
                    Clients.Client(instance.getPOneID()).SendAsync("ackControlConnectionRequest", "SUCCESS");
                    Clients.Client(instance.getPTwoID()).SendAsync("ackControlConnectionRequest", "SUCCESS");
                    Clients.Client(instance.getDisplayID()).SendAsync("ackControlConnectionRequest", "SUCCESS");
                    instance.setGameInProgress(true);
                }
                instance.incrementNumPlayersInGame();
            } else {
                Clients.Client(Context.ConnectionId).SendAsync("ackControlConnectionRequest", "FAILURE");
            }
        }

        // 3. In game, controller sends what arrows are pressed. This is redirected to display.
        public void buttonPressed(string pin, string button) {
            Pong instance;
            bool existsGameInstance = gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance) {
                Clients.Client(instance.getDisplayID()).SendAsync("ackButtonPressed", button);
            }
        }

        // 4. Display signals when game is over. This is redirected to controllers.
        public void endGame(string pin, string winner) {
            Pong instance;
            bool existsGameInstance = gameInstances.TryGetValue(pin, out instance);
            if (existsGameInstance) {
                if (string.Equals(winner, "1")) {
                    Clients.Client(instance.getPOneID()).SendAsync("ackEndGame", "WINNER");
                    Clients.Client(instance.getPTwoID()).SendAsync("ackEndGame", "LOSER");
                } else if (string.Equals(winner, "2")) {
                    Clients.Client(instance.getPOneID()).SendAsync("ackEndGame", "LOSER");
                    Clients.Client(instance.getPTwoID()).SendAsync("ackEndGame", "WINNER");

                } else {
                    Clients.Client(instance.getPOneID()).SendAsync("ackEndGame", "UNKNOWN");
                    Clients.Client(instance.getPTwoID()).SendAsync("ackEndGame", "UNKNOWN");
                    System.Console.Error.WriteLine("Invalid endGame string received.");
                }
                gameInstances.Remove(pin);
            }
        }

        private string generateRandomGamePin() {
            gameInstancePin++;
            return gameInstancePin.ToString();
        }
    }
}