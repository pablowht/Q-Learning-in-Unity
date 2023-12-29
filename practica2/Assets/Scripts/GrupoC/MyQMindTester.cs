
using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;

public class MyQMindTester : IQMind
{
    public void Initialize(WorldInfo worldInfo)
    {

    }

    CellInfo IQMind.GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        throw new System.NotImplementedException();
    }
}
