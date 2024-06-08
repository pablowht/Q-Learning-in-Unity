using NavigationDJIA.World;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MyQStates
{
    //Estados
    //1. Rango de expansión: 0, 1, 2
    //2. Orientacion: 0, 1, 2, 3, 4, 5, 6, 7
    //3. Estado de la celda: Muros true/false
    //Numero de estados: 3 * 8 * 2^4 = 384
    public int rangeDistance;
    public int orientation;
    public bool[] cellState;

    public MyQStates(int rangeDistance, int orientation, bool[] cellState)
    {
        this.rangeDistance = rangeDistance;
        this.orientation = orientation;
        this.cellState = cellState;
    }
}