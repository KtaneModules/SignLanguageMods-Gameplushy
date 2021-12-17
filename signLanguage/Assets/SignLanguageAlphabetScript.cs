using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeepCoding;
using System;
using RNG = UnityEngine.Random;

public class SignLanguageAlphabetScript : ModuleScript
{

    public KMSelectable[] Arrows;
    public KMSelectable Submit;

    public Sprite[] Signs;
    public SpriteRenderer Sign;

    Entry entry;
    char[] caesaredWord;
    int index;

    int offset;
    int desiredAnswer;

    bool userInputPossible;

    // Use this for initialization
    void Start()
    {
        userInputPossible = false;
        Submit.Assign(onInteract: CheckAnswer);
        Arrows.Assign(onInteract: MoveIndex);
        entry = Data.WordList[RNG.Range(0, Data.WordList.Length)];
        index = RNG.Range(0, entry.Word.Length);
        caesaredWord = new char[entry.Word.Length];
        CaesarTime();
        UIChangeSign(Helper.Alphabet.IndexOf(caesaredWord[index]));
        CalculateAnswer();
        userInputPossible = true;
    }

    private void UIChangeSign(int index)
    {
        PlaySound("clap");
        Sign.sprite = Signs[index];
    }

    private void CaesarTime()
    {
        if (RNG.Range(0, 2) == 0)
            offset = 5;
        else
            offset = 14;
        for (int i = 0; i < caesaredWord.Length; i++)
            caesaredWord[i] = (char)((entry.Word[i] - 'A' + offset) % 26 + 'A');
        Log("Offset is {0}. {1}->{2}", offset, entry.Word, caesaredWord);
    }

    private void CalculateAnswer()
    {
        char chosenSign;
        if (offset == 5)
            chosenSign = caesaredWord[0];
        else
            chosenSign = caesaredWord[caesaredWord.Length - 1];

        int indexoffset = Data.FingersLift[Helper.Alphabet.IndexOf(chosenSign)];
        Log("Letter {0} has {1} fingers lift.", chosenSign, indexoffset);
        desiredAnswer = (entry.SolutionIndex + indexoffset) % caesaredWord.Length;
        Log("You want to press the {0} letter.", Helper.ToOrdinal(desiredAnswer + 1));
    }

    private void CheckAnswer()
    {
        ButtonEffect(Submit, 1, KMSoundOverride.SoundEffect.BigButtonPress);
        if (userInputPossible && !IsSolved)
        {
            if (index == desiredAnswer)
                StartCoroutine(SolveAnim());
            else
                StartCoroutine(StrikeAnim());
        }
    }

    IEnumerator SolveAnim()
    {
        userInputPossible = false;
        Log("Right letter pressed. Module solved!");
        int[] listToDo = { 1, 17, 0, 21, 14 };
        foreach (int sign in listToDo)
        {
            UIChangeSign(sign);
            yield return new WaitForSecondsRealtime(.25f);
        }
        PlaySound("applause");
        Solve();
        userInputPossible = true;
    }

    IEnumerator StrikeAnim()
    {
        userInputPossible = false;
        Log("You pressed the {0} letter while you were supposed to press the {1} one. Strike!", Helper.ToOrdinal(index + 1), Helper.ToOrdinal(desiredAnswer + 1));
        int[] listToDo = { 22, 17, 14, 13, 6 };
        foreach (int sign in listToDo)
        {
            UIChangeSign(sign);
            yield return new WaitForSecondsRealtime(.25f);
        }
        Strike();
        UIChangeSign(Helper.Alphabet.IndexOf(caesaredWord[index]));
        userInputPossible = true;
    }

    private void MoveIndex(int obj)
    {
        ButtonEffect(Arrows[obj], .2f,KMSoundOverride.SoundEffect.ButtonPress);
        if (userInputPossible)
        {
            int move = obj == 0 ? -1 : 1;
            index = Helper.Modulo(index + move, caesaredWord.Length);
            UIChangeSign(Helper.Alphabet.IndexOf(caesaredWord[index]));
        }

    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"[!{0} cycle] to cycle through all the signs. [!{0} left/right #] to press the left/right arrow # times. Omitting # will press the arrow once. [!{0} submit] to submit your current answer.";
#pragma warning restore 414


    private IEnumerator ProcessTwitchCommand(string command)
    {
        string[] comSplit = command.Trim().ToUpper().Split();
        if (comSplit.Length == 1 && comSplit[0] == "CYCLE")
        {
            yield return new WaitUntil(() => userInputPossible);
            for (int i = 0; i < caesaredWord.Length; i++)
            {
                Arrows[1].OnInteract();
                yield return new WaitForSecondsRealtime(1f);
            }
        }
        else
        {
            List<KMSelectable> presses = new List<KMSelectable>();
            int move=-1;
            if ((comSplit[0] == "LEFT" || comSplit[0] == "RIGHT") && ((comSplit.Length == 2 && int.TryParse(comSplit[1], out move))||comSplit.Length==1))
            {
                if (move == -1) move = 1;
                KMSelectable arrowToUse = (comSplit[0] == "LEFT") ? Arrows[0] : Arrows[1];
                for (int i = 0; i < move; i++)
                {
                    presses.Add(arrowToUse);
                }
            }
            else if (comSplit.Length == 1 && comSplit[0] == "SUBMIT") presses.Add(Submit);
            if (presses.Count!=0)
            {
                yield return new WaitUntil(() => userInputPossible);
                foreach (KMSelectable butt in presses)
                {
                    butt.OnInteract();
                    yield return new WaitForSecondsRealtime(.1f);
                }
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        int delta = index - desiredAnswer;
        KMSelectable arrow = (Math.Sign(delta) == -1) ? Arrows[1] : Arrows[0];
        for (int i = 0; i < Math.Abs(delta); i++)
        {
            arrow.OnInteract();
            yield return new WaitForSecondsRealtime(.1f);
        }
        Submit.OnInteract();
    }
}
