using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key
{
    public static string Up(int UserIdx)
    {
        if (UserIdx == 0)
            return "Up1";
        else
            return "Up2";
    }

    public static string Left(int UserIdx)
    {
        if (UserIdx == 0)
            return "Left1";
        else
            return "Left2";
    }

    public static string Right(int UserIdx)
    {
        if (UserIdx == 0)
            return "Right1";
        else
            return "Right2";
    }

    public static string Down(int UserIdx)
    {
        if (UserIdx == 0)
            return "Down1";
        else
            return "Down2";
    }

    public static string TabPlayer(int UserIdx)
    {
        if (UserIdx == 0)
            return "Tab1";
        else
            return "Tab2";
    }

    public static string Pass(int UserIdx)
    {
        if (UserIdx == 0)
            return "Pass/Steal1";
        else
            return "Pass/Steal2";
    }

    public static string Shoot(int UserIdx)
    {
        if (UserIdx == 0)
            return "Shoot/Block1";
        else
            return "Shoot/Block2";
    }

    public static string Steal(int UserIdx)
    {
        return Pass(UserIdx);
    }

    public static string Block(int UserIdx)
    {
        return Shoot(UserIdx);
    }
}
