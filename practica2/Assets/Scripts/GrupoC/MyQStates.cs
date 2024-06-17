using NavigationDJIA.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyQStates
{
    //Estados
    //1. Rango de expansión: 0, 1, 2 // 0 más cercano y 2 más lejano
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


    public override bool Equals(object obj)
    {
        if (obj is MyQStates other)
        {
            return rangeDistance == other.rangeDistance &&
                   orientation == other.orientation &&
                   cellState.SequenceEqual(other.cellState);
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 31 + rangeDistance.GetHashCode();
        hash = hash * 31 + orientation.GetHashCode();
        foreach (bool cell in cellState)
        {
            hash = hash * 31 + cell.GetHashCode();
        }
        return hash;
    }
}