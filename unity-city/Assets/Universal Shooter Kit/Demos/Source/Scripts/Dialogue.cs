using System.Collections.Generic;

public class Dialogue
{
    public List<string> AnswerList { get; private set; }
    public string LastAnswer { get; private set; }

    private int _nextAnswerIndex = 0;

    public Dialogue(List<string> answerList)
    {
        AnswerList = answerList;
    }

    public string GetAnswer(string question)
    {
        if (_nextAnswerIndex < AnswerList.Count)
        {
            LastAnswer = AnswerList[_nextAnswerIndex++];
        }

        return LastAnswer;
    }
}
