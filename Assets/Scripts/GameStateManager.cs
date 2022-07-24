using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static HSD_Utils;

public static class GameStateManager
{
    public static GameState CurrGamestate;

    public static void ChangeState(GameState state) {
        //Debug.Log(state);
        CurrGamestate = state;
    }
}
