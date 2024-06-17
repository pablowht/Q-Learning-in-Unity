using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class MyQTable
{
    public Dictionary<MyQStates, float[]> qTableDictionary = new();

    private string qTableName;
    private string qTablePath;

    private int numStates = 384;
    private int numActions = 4;
    private int qTableSize;

    public MyQTable()
    {
        qTableSize = numStates * numActions;
        qTableDictionary = new Dictionary<MyQStates, float[]>(qTableSize);
    }

    public void InitializeQTable()
    {
        List<bool[]> cellStateCombinations = GenerateAllBooleanCombinations(4);

        for (int rangeDistance = 0; rangeDistance <= 2; rangeDistance++)
        {
            for (int rangeOrientation = 0; rangeOrientation <= 7; rangeOrientation++)
            {
                foreach (bool[] cellState in cellStateCombinations)
                {
                    MyQStates state = new(rangeDistance, rangeOrientation, cellState);
                    float[] qValues = new float[numActions];
                    for (int i = 0; i < numActions; i++)
                    {
                        if (!cellState[i])
                        {
                            qValues[i] = -100.0f;
                        }
                        else
                        {
                            qValues[i] = 0.0f;
                        }
                    }
                    qTableDictionary[state] = qValues;
                }
            }
        }
    }

    public float GetBestQValue(MyQStates state, float[] qValues)
    {
        float highestQValue = 0;

        foreach (var qValue in qValues)
        {
            if (qValue < highestQValue)
            {
                highestQValue = qValue;
            }
        }

        return highestQValue;
    }

    public int GetBestAction(MyQStates currentState)
    {
        //int action;
        int highestAction = 0;
        float highestValue = float.NegativeInfinity;

        if(qTableDictionary.TryGetValue(currentState, out float[] qValues))
        {
            for (int i = 0; i < qValues.Length; i++)
            {
                if (qValues[i] > highestValue)
                {
                    highestValue = qValues[i];
                    highestAction = i;
                }
            }
        }

        return highestAction;
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

    public void SaveTableToCSV(int numTable, int nEpisode, float bestRewardNew)
    {
        //Nombre Tabla
        qTableName = string.Format("Q{0}-{1}-{2}", numTable, nEpisode, bestRewardNew); ; //Q2-20000-130
        qTablePath = Application.dataPath + "/Scripts/QTables/" + qTableName + ".csv";

        Debug.Log("GUARDANDO");
        StringBuilder csvTable = new StringBuilder();
        //Cabecera de la tabla
        csvTable.AppendLine("RangeDistance;RangeOrientation;CellState;Norte;Este;Sur;Oeste");

        foreach (var kvp in qTableDictionary)
        {
            MyQStates state = kvp.Key;
            float[] qValues = kvp.Value;
            string cellStateStr = string.Join(",", state.cellState.Select(b => b.ToString()));

            csvTable.AppendLine($"{state.rangeDistance};{state.orientation};{cellStateStr};{qValues[0]};{qValues[1]};{qValues[2]};{qValues[3]}");
        }

        File.WriteAllText(qTablePath, csvTable.ToString());
    }

    public Dictionary<MyQStates, float[]> LoadTable(string newestQTablePath)
    {

        string[] rows = File.ReadAllLines(newestQTablePath);

        foreach (string row in rows.Skip(1))
        {
            string[] celda = row.Split(";");
            int rangeDistance = int.Parse(celda[0]);
            int orientation = int.Parse(celda[1]);
            bool[] cellState = celda[2].Split(",").Select(bool.Parse).ToArray();
            float[] qValues = celda.Skip(3).Select(float.Parse).ToArray();

            MyQStates state = new MyQStates(rangeDistance, orientation, cellState);
            qTableDictionary[state] = qValues;
        }
        Debug.Log("Tabla Q cargada: " + newestQTablePath);
        return qTableDictionary;
    }

    public string ReturnNewestTable(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] files = directoryInfo.GetFiles("*.csv");

        FileInfo latestFile = files.OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
        
        return latestFile?.FullName;
    }
}
