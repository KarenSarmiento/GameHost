public class Pong
{
    private string displayID;
    private string pOneID;
    private string pTwoID;
    private bool gameInProgress;
    private int numPlayersInGame;

    public Pong(string displayID) 
    {
        this.displayID = displayID;
        gameInProgress = false;
        numPlayersInGame = 0;
    }

    public void incrementNumPlayersInGame() {
        numPlayersInGame++;
    }

    public int getNumPlayersInGame() {
        return numPlayersInGame;
    }

    public string getPOneID() {
        return pOneID;
    }

    public void setPOneID(string pOneID) {
        this.pOneID = pOneID;
    }

    public string getPTwoID() {
        return pTwoID;
    }

    public void setPTwoID(string pTwoID) {
        this.pTwoID = pTwoID;
    }

    public string getDisplayID() {
        return displayID;
    }

    public void setGameInProgress(bool gameInProgress) {
        this.gameInProgress = gameInProgress;
    }

    public bool isGameInProgress() {
        return gameInProgress;
    }
}