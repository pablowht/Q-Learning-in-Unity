using NavigationDJIA.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MyQTable
{
    //Quizás hay que pasar la clase MyQState aquí mediante un struct para poder utilizar GetHashCode y Equals
    public Dictionary<MyQStates, float[]> qTable = new();
    //public List<float> qTable;

    private string qTableName;
    private string qTablePath;

    private int numStates = 384;
    private int numActions = 4;
    private int qTableSize;

    public MyQTable(int numTable, int numEpocas, int bestRewardTable)
    {
        //Nombre Tabla
        qTableName = string.Format("Q{0}-{1}-{2}", numTable, numEpocas, bestRewardTable); ; //Q2-20000-130
        qTablePath = Application.dataPath + "/Scripts/QTables/" + qTableName + ".csv";
        
        //Datos Tabla
        qTableSize = numStates * numActions;
        qTable = new Dictionary<MyQStates, float[]>(qTableSize);

        InitializeQTable();
        //

    }

    private void InitializeQTable()
    {
        List<bool[]> cellStateCombinations = GenerateAllBooleanCombinations(4);

        for (int rangeDistance = 0; rangeDistance <= 2; rangeDistance++)
        {
            for (int rangeOrientation = 0; rangeOrientation <= 7; rangeOrientation++)
            {
                foreach (bool[] cellState in cellStateCombinations)
                {
                    MyQStates state = new(rangeDistance, rangeOrientation, cellState);
                    qTable[state] = new float[numActions];
                }
            }
        }
    }

    private List<bool[]> GenerateAllBooleanCombinations(int length)
    {
        List<bool[]> combinations = new List<bool[]>();
        int totalCombinations = (int)Math.Pow(2, length);

        for (int i = 0; i < totalCombinations; i++)
        {
            bool[] combination = new bool[length];
            for (int j = 0; j < length; j++)
            {
                combination[j] = (i & (1 << j)) != 0;
            }
            combinations.Add(combination);
        }

        return combinations;
    }

    public void SaveTableToCSV()
    {
        Debug.Log("GUARDANDO");
        StringBuilder csvTable = new StringBuilder();
        //Cabecera de la tabla
        csvTable.AppendLine("RangeDistance;RangeOrientation;CellState;Norte;Este;Sur;Oeste");

        foreach (var kvp in qTable)
        {
            MyQStates state = kvp.Key;
            float[] qValues = kvp.Value;
            string cellStateStr = string.Join(",", state.cellState.Select(b => b.ToString()));

            csvTable.AppendLine($"{state.rangeDistance};{state.orientation};{cellStateStr};{qValues[0]};{qValues[1]};{qValues[2]};{qValues[3]}");
        }

        File.WriteAllText(qTablePath, csvTable.ToString());
    }
}
