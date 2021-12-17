using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeepCoding;
using RNG = UnityEngine.Random;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class SignAlphabetScript : ModuleScript {

	public KMSelectable SignSubmit;
	public KMSelectable LetterSubmit;
	public KMSelectable[] SignArrows;
	public KMSelectable[] LetterArrows;
	public GameObject[] SA;
	public GameObject[] LA;
	public KMNeedyModule Needy;

	public TextMesh LetterDisplay;

	public Sprite[] Signs;
	public SpriteRenderer SignDisplay;

	private int[] signArray;
	private int signIndex;
	private int letterIndex;

	private bool signSubPressable;
	private bool letterSubPressable;

	// Use this for initialization
	void Start () {
		signArray = Enumerable.Range(0, 26).ToArray().Shuffle();
		Log("The signs are ordered as follows: {0}",signArray.Select(x=>(char)(x+'A')).Join());
		SignDisplay.sprite = Signs[signArray[0]];
		signIndex = 0; letterIndex = 0;
		SignArrows.Assign(onInteract: SignMove);
		LetterArrows.Assign(onInteract: LetterMove);
		DeactivateEverything();
		SignSubmit.Assign(onInteract: () => { if(signSubPressable) Submit(SignSubmit); });
		LetterSubmit.Assign(onInteract: () => { if (letterSubPressable) Submit(LetterSubmit); });
		Needy.Assign(onNeedyActivation: NeedyActivate, onTimerExpired:()=> { Log("Time ran out. Strike."); Needy.HandleStrike(); Needy.OnNeedyDeactivation(); } ,onNeedyDeactivation:DeactivateEverything);
	}

    private void LetterMove(int obj)
    {
		ButtonEffect(LetterArrows[obj], .1f, KMSoundOverride.SoundEffect.ButtonPress);
		if (obj == 0) obj = -1;
		letterIndex = Helper.Modulo(letterIndex + obj, 26);
		LetterDisplay.text = ((char)('A' + letterIndex)).ToString();
	}

    private void SignMove(int obj)
    {
		ButtonEffect(SignArrows[obj], .1f, "clap");
		if (obj == 0) obj = -1;
		signIndex = Helper.Modulo(signIndex + obj, 26);
		SignDisplay.sprite = Signs[signArray[signIndex]];
    }

    void DeactivateEverything()
    {
		letterSubPressable = false; signSubPressable = false;
		SA.ForEach(a => a.SetActive(false)); LA.ForEach(a => a.SetActive(false));
	}

	void NeedyActivate()
    {
        
        if (RNG.Range(0, 2) == 1) //Generate sign, input letter
        {
			LA.ForEach(a => a.SetActive(true));
			signIndex = RNG.Range(0, 25);
			SignDisplay.sprite= Signs[signArray[signIndex]];
			Log("I'm giving you {0}'s sign.", ((char)(signArray[signIndex] + 'A')).ToString());
			letterSubPressable = true;
		}
        else //Generate letter, input sign
        {
            //Needy.SetNeedyTimeRemaining(99);
            SA.ForEach(a => a.SetActive(true));
			letterIndex = RNG.Range(0, 25);
			LetterDisplay.text = ((char)('A' + letterIndex)).ToString();
			Log("I'm giving you the letter {0}.", ((char)(letterIndex + 'A')).ToString());
			signSubPressable = true;
		}
    }

	void Submit(KMSelectable which)
    {
		ButtonEffect(which, 1, which == SignSubmit ? KMSoundOverride.SoundEffect.BigButtonPress : KMSoundOverride.SoundEffect.ButtonPress);
		string message = "Sign says {0} and letter says {1}".Form(((char)(signArray[signIndex] + 'A')).ToString(), ((char)(letterIndex + 'A')).ToString());
		if (signArray[signIndex] == letterIndex) { Log(message+", which is good."); Needy.HandlePass(); }
		else { Log(message + ", which results in a strike.");  Needy.HandleStrike(); Needy.HandlePass(); }
		Needy.OnNeedyDeactivation();
	}

	private bool bombSolved = false;
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"[!{0} submit X] to submit the corresponding letter.";
#pragma warning restore 414


    private IEnumerator ProcessTwitchCommand(string command)
    {
		command = command.Trim();
		if (Regex.IsMatch(command, "^SUBMIT [A-Z]$", RegexOptions.IgnoreCase))
        {
			if (!letterSubPressable && !signSubPressable) yield return "sendtochaterror You can't do that yet!";
			else
			{
				yield return null;
				List<KMSelectable> buttonPresses = GetPresses(command[command.Length - 1]);
				foreach (KMSelectable buttonPress in buttonPresses)
                {
					buttonPress.OnInteract();
					yield return new WaitForSecondsRealtime(.1f);
				}
			}
			
        }
    }

	private List<KMSelectable> GetPresses(char command=' ')
    {
		List<KMSelectable> buttonPresses = new List<KMSelectable>();
		int l;
		if(command!=' ') l=(command - 'A');
		else l = letterSubPressable? signArray[signIndex]:letterIndex;
		int dist;
		int dir;
		KMSelectable subButton;
		KMSelectable[] arrowButton;
		if (letterSubPressable)
		{
			dist = Math.Abs(l - letterIndex);
			dir = l > letterIndex ? 1 : 0;
			subButton = LetterSubmit;
			arrowButton = LetterArrows;
		}
		else
		{
			dist = Math.Abs(signArray.IndexOf(l) - signIndex);
			dir = signArray.IndexOf(l) > signIndex ? 1 : 0;
			subButton = SignSubmit;
			arrowButton = SignArrows;
		}
		if (dist > 13) { dir = 1 - dir; dist = 26 - dist; }
		while (dist-- != 0) buttonPresses.Add(arrowButton[dir]);
		buttonPresses.Add(subButton);
		return buttonPresses;
	}


	private void TwitchHandleForcedSolve()
    {
		StartCoroutine(AutoSolver());
    }

	private IEnumerator AutoSolver()
    {
        while (!bombSolved)
        {
			yield return new WaitUntil(() => letterSubPressable || signSubPressable);
			List<KMSelectable> buttonPresses = GetPresses();
			foreach (KMSelectable buttonPress in buttonPresses)
			{
				buttonPress.OnInteract();
				yield return new WaitForSecondsRealtime(.1f);
			}
		}
    }
}
