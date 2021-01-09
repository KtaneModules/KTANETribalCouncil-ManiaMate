using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Collections;

public class tribalCouncilScript : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public TextMesh[] namesText;
    public string[] names;
    public Color[] fontcolor;
    public TextMesh parchmenttext;

    private readonly int[,] chart = new int[,]
    {
        {0, 8, 5, 30, 10, 49, 3, 482, 921, 69},
        {9, 0, 2, 7, 6, 3, 1, 66, 111, 10},
        {11, 7, 0, 420, 18, 21, 22, 4, 2, 0},
        {28, 7, 2, 0, 6, 22, 4, 9, 311, 3},
        {5, 0, 8, 92, 0, 7, 31, 820, 6, 8},
        {59, 414, 55, 3, 14, 0, 2, 4, 9, 15},
        {5, 13, 24, 9, 4, 12, 0, 3, 23, 7},
        {88, 0, 60, 7, 20, 888, 8, 0, 26, 14},
        {2, 3, 52, 5, 19, 56, 21, 94, 0, 191},
        {1, 47, 3, 91, 38, 495, 15, 10, 111, 0}
    };

    private readonly List<int> chosennames = new List<int>();
    private string correctanswer;
    private string correctanswer2;
    private bool strike;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable button in buttons)
        {
            var pressedButton = button;
            button.OnInteract += delegate
            {
                ButtonPress(pressedButton);
                return false;
            };
        }
    }

    void Start()
    {
        PickNames();
        TribalCouncilPerform();
    }

    void PickNames()
    {
        for (int i = 0; i < 6; i++)
        {
            var index = UnityEngine.Random.Range(0, 10);
            while (chosennames.Contains(index))
            {
                index = UnityEngine.Random.Range(0, 10);
            }

            chosennames.Add(index);
            namesText[i].text = names[index];
        }

        Debug.LogFormat("[Tribal Council #{0}] The possible people to vote off going clockwise are {1}.", moduleId,
            namesText.Select(x => x.text).Join(", "));
    }

    void TribalCouncilPerform()
    {
        var totalnames = new[]
            {namesText[0].text, namesText[1].text, namesText[2].text, namesText[3].text, namesText[4].text};
        var alphabet = Enumerable.Range('A', 'Z' - 'A' + 1).Select(x => (char) x).ToArray();
        var basenum = Array.IndexOf(alphabet, bomb.GetSerialNumberLetters().First()) + 1;
        Debug.LogFormat("[Tribal Council #{0}] The first charac of the SN is {1}.", moduleId,
            bomb.GetSerialNumberLetters().First());
        Debug.LogFormat("[Tribal Council #{0}] The base number starts off as {1}.", moduleId, basenum);
        var litinds = bomb.GetOnIndicators().Count();
        if (litinds > 0)
        {
            basenum *= litinds;
        }
        else
        {
            basenum += 21;
        }

        Debug.LogFormat("[Tribal Council #{0}] After modifying with lit indicators, the base number is {1}.", moduleId,
            basenum);
        var nename = Array.IndexOf(names, namesText[1].text);
        var swname = Array.IndexOf(names, namesText[4].text);
        if (nename == 6 && swname == 1 || nename == 9 && swname == 1 || nename == 5 && swname == 4 ||
            nename == 1 && swname == 5 || nename == 2 && swname == 5 ||
            nename == 9 && swname == 5 || nename == 5 && swname == 9 || nename == 7 && swname == 9 ||
            nename == 8 && swname == 9)
        {
            basenum -= chart[nename, swname];
        }
        else
        {
            basenum += chart[nename, swname];
        }

        Debug.LogFormat("[Tribal Council #{0}] Chart- Row: {1} ({2})    Col: {3} ({4})", moduleId, nename + 1,
            namesText[1].text, swname + 1, namesText[4].text);
        Debug.LogFormat("[Tribal Council #{0}]The base number after the chart is {1}.", moduleId, basenum);
        if (basenum < 0)
        {
            basenum = Math.Abs(basenum);
        }

        basenum = basenum % 20;
        Debug.LogFormat("[Tribal Council #{0}] The base number after the modulo is {1}.", moduleId, basenum);
        basenum = basenum % 6;
        var closestally = namesText[basenum].text;
        string normalally1;
        string normalally2;
        if (basenum == 0)
        {
            normalally1 = namesText[5].text;
            normalally2 = namesText[1].text;
        }
        else
        {
            normalally1 = namesText[(basenum - 1) % 6].text;
            normalally2 = namesText[(basenum + 1) % 6].text;
        }

        Debug.LogFormat(
            "[Tribal Council #{0}] Going clockwise by my base number, my closest ally is {1}, and my normal allies are {2} and {3}.",
            moduleId, closestally, normalally1, normalally2);
        var moduleNames = new[]
            {namesText[(basenum + 2) % 6].text, namesText[(basenum + 3) % 6].text, namesText[(basenum + 4) % 6].text};
        Debug.LogFormat("[Tribal Council #{0}] The other people not your allies are {1}, {2}, {3}.", moduleId,
            moduleNames[0], moduleNames[1], moduleNames[2]);
        int clockcount = (bomb.GetPortCount()) * (bomb.GetBatteryCount());
        Debug.LogFormat("[Tribal Council #{0}] Ports * Batteries = {1}", moduleId, clockcount);
        var intendedvote = moduleNames[clockcount % 3];
        Debug.LogFormat("[Tribal Council #{0}] The intended vote is {1}.", moduleId, intendedvote);
        string answer;
        var answer2 = "";
        if (Equals(closestally, "Bob"))
        {
            answer = normalally1;
            answer2 = normalally2;
            Debug.LogFormat(
                "[Tribal Council #{0}] Since your closest ally is Bob, the correct person to vote off is {1} or {2}.",
                moduleId, answer, answer2);
        }
        else if (Equals(closestally, "Louise"))
        {
            answer = namesText[(Array.IndexOf(totalnames, intendedvote) + 1) % 6].text;
            Debug.LogFormat(
                "[Tribal Council #{0}] Since your closest ally is Louise, the correct person to vote off is {1}.",
                moduleId, answer);
        }
        else if (Equals(closestally, "Jonathan"))
        {
            answer = moduleNames[((clockcount % 3) + 1) % 3];
            Debug.LogFormat(
                "[Tribal Council #{0}] Since your closest ally is Jonathan, the correct person to vote off is {1}.",
                moduleId, answer);
        }
        else if (Equals(closestally, "Stacy"))
        {
            answer = namesText[(Array.IndexOf(totalnames, intendedvote) + 3) % 6].text;
            int temp = 0;
            while (Equals(answer, normalally1) || Equals(answer, normalally2) || Equals(answer, closestally))
            {
                answer = namesText[(Array.IndexOf(totalnames, intendedvote) + 3 + temp) % 6].text;
                temp++;
            }

            Debug.LogFormat(
                "[Tribal Council #{0}] Since your closest ally is Stacy, the correct person to vote off is {1}.",
                moduleId, answer);
        }
        else
        {
            answer = intendedvote;
            Debug.LogFormat("[Tribal Council #{0}] The correct person to vote off is {1}.", moduleId, answer);
        }

        correctanswer = answer;
        correctanswer2 = answer2;
    }

    private void ButtonPress(KMSelectable button)
    {
        if (moduleSolved)
        {
            return;
        }

        button.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Debug.LogFormat("[Tribal Council #{0}] You voted for {1}.", moduleId,
            button.GetComponentInChildren<TextMesh>().text);
        if (button.GetComponentInChildren<TextMesh>().text != correctanswer2 &&
            button.GetComponentInChildren<TextMesh>().text != correctanswer)
        {
            strike = true;
        }

        if (strike == false)
        {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Tribal Council #{0}] Module Solved.", moduleId);
            foreach (var but in buttons)
            {
                but.GetComponentInChildren<TextMesh>().color = fontcolor[1];
            }

            parchmenttext.text = button.GetComponentInChildren<TextMesh>().text;
            parchmenttext.color = fontcolor[0];
            Audio.PlaySoundAtTransform("JeffProbst-TribeHasSpoken", transform);
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Tribal Council #{0}] You did not vote out the right person.", moduleId);
            strike = false;
        }
    }
    
#pragma warning disable 414
    private const string TwitchHelpMessage = "Use !{0} Bob, to submit the name Bob.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        var possibleNames = namesText.Select(x => x.text.ToLowerInvariant()).ToList();
        if (!possibleNames.Contains(command))
        {
            yield return string.Format("sendtochaterror Do you honestly expect me to know who {0} is?", command);
            yield break;
        }

        yield return null;
        buttons[possibleNames.IndexOf(command)].OnInteract();
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        buttons[namesText.Select(x => x.text.ToLowerInvariant()).ToList().IndexOf(correctanswer.ToLowerInvariant())].OnInteract();
        yield return true;
    }
}