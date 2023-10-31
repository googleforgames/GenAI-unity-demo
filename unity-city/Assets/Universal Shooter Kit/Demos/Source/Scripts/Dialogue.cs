using System.Collections.Generic;

public class Dialogue
{
    public string PlayerName { get; private set; }
    public string NPCName { get; private set; }
    public List<string> AnswerList { get; private set; }

    public Dialogue(string playerName, string npcName, List<string> answerList)
    {
        PlayerName = playerName;
        NPCName = npcName;
        AnswerList = answerList;
    }
}
