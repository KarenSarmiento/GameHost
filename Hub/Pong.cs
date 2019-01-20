public class Pong
{
    private int displayID;
    private int pOneID;
    private int pTwoID;
    private bool gameInProgress;
    private int numPlayersInGame;

    public Pong(int displayID) 
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

    public int getPOneID() {
        return pOneID;
    }

    public void setPOneID(int pOneID) {
        this.pOneID = pOneID;
    }

    public int getPTwoID() {
        return pTwoID;
    }

    public void setPTwoID(int pTwoID) {
        this.pTwoID = pTwoID;
    }

    public int getDisplayID() {
        return displayID;
    }

    public void setGameInProgress(bool gameInProgress) {
        this.gameInProgress = gameInProgress;
    }

    public bool isGameInProgress() {
        return gameInProgress;
    }
}