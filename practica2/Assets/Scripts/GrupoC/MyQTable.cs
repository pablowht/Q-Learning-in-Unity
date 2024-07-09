using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

//Clase para la Tabla Q
public class MyQTable
{
    //Diccionario
    public Dictionary<MyQStates, float[]> qTableDictionary = new();

    //Nombre y Path de las tablas Q
    private string qTableName;
    private string qTablePath;

    //Variables para la construcción de las tablas
    private int numStates = 384;
    private int numActions = 4;
    private int qTableSize;

    //Constructor
    public MyQTable()
    {
        qTableSize = numStates * numActions;
        qTableDictionary = new Dictionary<MyQStates, float[]>(qTableSize);
    }

    //Inicialización de la tabla Q con valores 0 y -100 donde no sea Walkable
    public void InitializeQTable()
    {
        //Generación de una lista para almacenar los array de los booleanos
        List<bool[]> cellStateCombinations = GenerateCellsCombinations(4);


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
                        //Si la celda es falsa (!Walkable) valor de -100 sino 0
                        if (!cellState[i]) { qValues[i] = -100.0f; }
                        else { qValues[i] = 0.0f; }
                    }
                    qTableDictionary[state] = qValues;
                }
            }
        }
    }

    //Generación de una lista de array de bools
    private List<bool[]> GenerateCellsCombinations(int length)
    {
        //Lista de arrays de bool
        List<bool[]> cellCombs = new();
        int totalCombs = (int)Math.Pow(2, length);

        for (int i = 0; i < totalCombs; i++)
        {
            bool[] cells = new bool[length];
            for (int j = 0; j < length; j++)
            {
                //Condicion para obtener todas las posibles combinaciones
                cells[j] = (i & (1 << j)) != 0;
            }
            cellCombs.Add(cells);
        }
        return cellCombs;
    }

    //Devuelve el mejor valor de Q para el estado deseado
    public float GetBestQValue(MyQStates state, float[] qValues)
    {
        return qValues.Max();
    }

    //Devuelve un número representativo de la acción a tomar (0-Norte, 1-Este, 2-Sur, 3-Oeste)
    public int GetBestAction(MyQStates currentState)
    {
        int highestAction = 0;
        float highestValue = -100.0f;

        if (qTableDictionary.TryGetValue(currentState, out float[] qValues))
        {
            for (int i = 0; i < qValues.Length; i++)
            {
                //Se guarda el indice del mejor valor
                if (qValues[i] > highestValue)
                {
                    highestValue = qValues[i];
                    highestAction = i;
                }
            }
        }
        return highestAction;
    }

    #region Metodos CSV
    //Genera un archivo CSV y guarda la tabla
    public void SaveTableToCSV(int numTable, int nEpisode, float bestRewardNew)
    {
        //Nombre del fichero: Q2-20000-130
        qTableName = string.Format("Q{0}-{1}-{2}", numTable, nEpisode, bestRewardNew);
        qTablePath = Application.dataPath + "/Scripts/GrupoC/QTables/" + qTableName + ".csv";

        Debug.Log("GUARDANDO");

        //StringBuilder
        StringBuilder csvTable = new StringBuilder();

        //Cabecera de la tabla
        csvTable.AppendLine("RangeDistance;RangeOrientation;CellState;Norte;Este;Sur;Oeste");
        //Filas de la tabla
        foreach (var kvp in qTableDictionary)
        {
            MyQStates state = kvp.Key;
            float[] qValues = kvp.Value;
            string cellStateStr = string.Join(",", state.cellState.Select(b => b.ToString()));
            //Se appendea los valores a sus respectivas casillas
            csvTable.AppendLine($"{state.rangeDistance};{state.orientation};{cellStateStr};{qValues[0]};{qValues[1]};{qValues[2]};{qValues[3]}");
        }
        //Se guarda los datos en el fichero
        File.WriteAllText(qTablePath, csvTable.ToString());
    }

    //Se cargan los datos del fichero CSV en el diccionario
    public Dictionary<MyQStates, float[]> LoadTable(string newestQTablePath)
    {
        //Se crea un array de string con cada fila de la tabla
        string[] rows = File.ReadAllLines(newestQTablePath);

        //Se salta la primera fila que es la cabecera
        foreach (string row in rows.Skip(1))
        {
            //Se divide cada celda según los ;
            string[] celda = row.Split(";");

            //Se guarda cada elemento parseado sobre los respectivos valores para los estados
            int rangeDistance = int.Parse(celda[0]);
            int orientation = int.Parse(celda[1]);
            bool[] cellState = celda[2].Split(",").Select(bool.Parse).ToArray();
            float[] qValues = celda.Skip(3).Select(float.Parse).ToArray();

            //Se crean los estados con los datos de la tabla y se guarda en el diccionario
            MyQStates state = new MyQStates(rangeDistance, orientation, cellState);
            qTableDictionary[state] = qValues;
        }

        //Debug para saber que tabla está utilizando
        Debug.Log("Tabla Q cargada: " + newestQTablePath);
        return qTableDictionary;
    }

    //Método para obtener la última tabla de la carpeta
    public string ReturnNewestTable(string path)
    {
        DirectoryInfo directoryInfo = new(path);
        FileInfo[] files = directoryInfo.GetFiles("*.csv");

        FileInfo latestFile = files.OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
        
        return latestFile?.FullName;
    }
    #endregion
}
