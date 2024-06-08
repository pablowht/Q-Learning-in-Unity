using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptPruebaTabla : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MyQTable qTable = new MyQTable(1, 20000, 130);
        qTable.SaveTableToCSV();
        //int numTable, int numEpocas, int bestRewardTable
    }
}
