
using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class MyQMindTester : IQMind
{
    WorldInfo world;
    MyQTable qTableTester;
    string qTablePath;

    public void Initialize(WorldInfo worldInfo)
    {
        world = worldInfo;
        
        qTableTester = new MyQTable();
        qTablePath = qTableTester.ReturnNewestTable(Application.dataPath + "/Scripts/QTables/");
        qTableTester.qTableDictionary = qTableTester.LoadTable(qTablePath);
    }

    MyQStates currentState;
    int bestAction;
    CellInfo nextCell;

    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        Debug.Log("QMind: GetNextStep");

        int distance = DiscretizeDistance(currentPosition, otherPosition);
        int orientation = DiscretizeOrientation(currentPosition, otherPosition);
        bool[] cells = CalculateCellState(currentPosition);

        currentState = new MyQStates(distance, orientation, cells);

        bestAction = qTableTester.GetBestAction(currentState);

        nextCell = ReturnCellAction(bestAction, currentPosition);
        nextCell = world[nextCell.x, nextCell.y];

        return nextCell;
    }

    private CellInfo ReturnCellAction(int action, CellInfo AgentPosition)
    {
        return action switch
        {
            //Norte
            0 => new CellInfo(AgentPosition.x, AgentPosition.y + 1),
            //Este
            1 => new CellInfo(AgentPosition.x + 1, AgentPosition.y),
            //Sur
            2 => new CellInfo(AgentPosition.x, AgentPosition.y - 1),
            //Oeste
            _ => new CellInfo(AgentPosition.x - 1, AgentPosition.y),
        };
    }

    private int DiscretizeDistance(CellInfo AgentPosition, CellInfo OtherPosition)
    {
        int manhattanDistance = Math.Abs(AgentPosition.x - OtherPosition.x) + Math.Abs(AgentPosition.y - OtherPosition.y);

        if (manhattanDistance <= 5)
        {
            return 0;
        }
        else if (manhattanDistance > 5 && manhattanDistance <= 20)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    private int DiscretizeOrientation(CellInfo AgentPosition, CellInfo OtherPosition)
    {
        float deltaX = AgentPosition.x - OtherPosition.x;
        float deltaY = OtherPosition.y - AgentPosition.y;

        float angleRadians = Mathf.Atan2(deltaY, deltaX);

        float angleDegrees = angleRadians * Mathf.Rad2Deg;
        if (angleDegrees < 0)
        {
            angleDegrees += 360;
        }

        int discreteOrientation = Mathf.FloorToInt(angleDegrees / 45);

        return discreteOrientation;
    }

    private bool[] CalculateCellState(CellInfo AgentPosition)
    {
        bool[] contiguousCells = new bool[4];

        CellInfo norteCell = new(AgentPosition.x, AgentPosition.y + 1);
        CellInfo esteCell = new(AgentPosition.x + 1, AgentPosition.y);
        CellInfo surCell = new(AgentPosition.x, AgentPosition.y - 1);
        CellInfo oesteCell = new(AgentPosition.x - 1, AgentPosition.y);

        norteCell = world[norteCell.x, norteCell.y];
        esteCell = world[esteCell.x, esteCell.y];
        surCell = world[surCell.x, surCell.y];
        oesteCell = world[oesteCell.x, oesteCell.y];

        contiguousCells[0] = norteCell.Walkable;
        contiguousCells[1] = esteCell.Walkable;
        contiguousCells[2] = surCell.Walkable;
        contiguousCells[3] = oesteCell.Walkable;

        return contiguousCells;
    }

}
