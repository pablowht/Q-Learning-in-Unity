using NavigationDJIA.World;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MyQTable : MonoBehaviour
{
    //Tabla Q
    public float[,] tableQ { get; set; }
    private int numeroFilas { get; set; }
    private int numeroColumnas { get; set; }

    private string filePath = "Assets/Scripts/GrupoC/TablaQ.csv";


    public void InitializeTable()
    {
        this.numeroFilas = 16;
        this.numeroColumnas = 4;
        //Cálculo del grid 
        this.tableQ = new float[numeroFilas, numeroColumnas]; //Numero de acciones posibles (norte, sur, este, oeste)
        
        for (int i = 0; i < this.numeroFilas; i++)
        {
            for (int j = 0; j < this.numeroColumnas; j++)
            {
                this.tableQ[i, j] = 0.0f;
            }
        }
    }

    public void UpdateTable(int row, int col, float valorQ)
    {
        this.tableQ[row, col] = valorQ;
    }

    public float GetQValue(int row, int col)
    {
            return this.tableQ[row, col];
    }

    public float GetMaxQValue(int row)
    {
        float maxQ = this.tableQ[0, 0];

        for (int i = 1; i < this.numeroColumnas; i++)
        {
            if (this.tableQ[row, i] > maxQ)
            {
                maxQ = this.tableQ[row, i];
            }
        }

        return maxQ;
    }
}
