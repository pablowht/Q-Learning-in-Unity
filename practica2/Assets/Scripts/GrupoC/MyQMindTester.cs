
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
        //escogemos la dirección de la última tabla guardada
        qTablePath = qTableTester.ReturnNewestTable(Application.dataPath + "/Scripts/GrupoC/QTables/"); 
        //cargamos la tabla en el diccionario
        qTableTester.qTableDictionary = qTableTester.LoadTable(qTablePath);
    }

    MyQStates currentState;
    int bestAction;
    CellInfo nextCell;

    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        Debug.Log("QMind: GetNextStep");

        //se discretizan los distintos valores del estado 
        int distance = DiscretizeDistance(currentPosition, otherPosition);
        int orientation = DiscretizeOrientation(currentPosition, otherPosition);
        bool[] cells = CalculateCellState(currentPosition);


        currentState = new MyQStates(distance, orientation, cells);

        //se escoge la mejor acción a realizar según el estado actual 
        bestAction = qTableTester.GetBestAction(currentState);

        //la siguiente celda será a la que lleve la acción escogida 
        nextCell = ReturnCellAction(bestAction, currentPosition);
        //se trasforma a coordenadas del mundo
        nextCell = world[nextCell.x, nextCell.y];

        //se comprueba que la siguiente celda sea andable para evitar malas celdas
        if (nextCell.Walkable)
        {
            //en caso de que sea andable se devuelve la celda nueva 
            return nextCell;
        }
        else
        {   // en caso de que no, se devuelve la celda en la que se estaba 
            return currentPosition;
        }

    }

    private CellInfo ReturnCellAction(int action, CellInfo AgentPosition)
    {
        // se crea un cellinfo dependiendo de la mejor acción elegida 
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

    #region Discretizaciones
    //método para discretizar la distancia, más explicado en MyQMindTrainer
    private int DiscretizeDistance(CellInfo AgentPosition, CellInfo OtherPosition)
    {
        int manhattanDistance = Math.Abs(AgentPosition.x - OtherPosition.x) + Math.Abs(AgentPosition.y - OtherPosition.y);

        if (manhattanDistance <= 5)
        {
            return 0;
        }
        else if (manhattanDistance > 5 && manhattanDistance <= 15) //ELEFANTE: he cambiando el 20 por un 15
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }
    //método para discretizar la orientación, más explicado en MyQMindTrainer
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
    //método para saber si las celdas colindantes son andables o no, más explicada en MyQMindTester 
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
    #endregion
}
