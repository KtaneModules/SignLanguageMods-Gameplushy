using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Entry
{
    private string word;
    private int solutionIndex;

    public string Word { get { return word; } }
    public int SolutionIndex { get { return solutionIndex; } }

    public Entry(string w,int i)
    {
        word = w.ToUpperInvariant();
        solutionIndex = i;
    }

}