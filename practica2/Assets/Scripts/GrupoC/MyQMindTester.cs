
using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;

public class MyQMindTester : IQMind
{
    public void Initialize(WorldInfo worldInfo)
    {
        Debug.Log("QMindDummy: initialized");
    }

    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        Debug.Log("QMind: GetNextStep");
        return null;
    }
}
