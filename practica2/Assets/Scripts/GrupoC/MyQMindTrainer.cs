using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;
//using Random = UnityEngine.Random;

namespace QMind
{
    public class MyQMindTrainer : IQMindTrainer
    {
        //
        public int CurrentEpisode { get; }
        public int CurrentStep { get; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; }
        public float ReturnAveraged { get; }
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        //Parámetros del algoritmo
        QMindTrainerParams qMindParams;
        WorldInfo world;
        INavigationAlgorithm navAlgorithm;

        //Tabla Q
        MyQTable qTable;
        int qTableNumber = 0;
        int qTableEpocas = 0;
        int qTableBestReward = 0;


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Debug.Log("MyQMindTrainer: initialized");
            world = worldInfo;

            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();

            qMindParams = qMindTrainerParams;
            navAlgorithm = navigationAlgorithm;

            //Inicializar tabla Q
            qTableNumber += 1;
            qTableEpocas = CurrentEpisode;
            //qTableBestReward += 1;
            qTable = new MyQTable(qTableNumber, qTableEpocas, qTableBestReward);
            qTable.InitializeQTable();

            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }

        MyQStates currentState;
        int distance;
        int orientation;
        bool[] cellState;

        int action;
        float QValue;
        float accionAleatoria = 0.1f;

        public void DoStep(bool train)
        {
            Debug.Log("MyQMindTrainer: DoStep");

            //Creamos el estado actual

            distance = DiscretizeDistance(AgentPosition.x, AgentPosition.y, OtherPosition.x, OtherPosition.y);
            orientation = DiscretizeOrientation(AgentPosition.x, AgentPosition.y, OtherPosition.x, OtherPosition.y);
            cellState = CalculateCellState(AgentPosition);

            currentState = new MyQStates(distance, orientation, cellState);

            //Seleccionamos una acción de forma balanceada
            if (accionAleatoria < qMindParams.epsilon)
            {
                action = UnityEngine.Random.Range(0, 4);
                accionAleatoria += 0.05f;
            }
            else
            {
                action = qTable.GetBestAction(currentState);
                accionAleatoria = UnityEngine.Random.Range(0.1f, qMindParams.epsilon);
            }

            if (qTable.qTableDictionary.TryGetValue(currentState, out float[] qValues))
            {
                QValue = qValues[action];
            }

        }

        private int DiscretizeDistance(int xAgent, int yAgent, int xEnemy, int yEnemy)
        {
            int manhattanDistance = Math.Abs(xAgent - xEnemy) + Math.Abs(yAgent - yEnemy);

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

        private int DiscretizeOrientation(int xAgent, int yAgent, int xEnemy, int yEnemy)
        {
            float deltaX = xAgent - xEnemy;
            float deltaY = yAgent - yEnemy;
            float angleRadians = Mathf.Atan2(deltaY, deltaX);

            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            int discreteOrientation = Mathf.FloorToInt(angleDegrees / 45);

            return discreteOrientation;
        }

        private bool[] CalculateCellState(CellInfo AgentCell)
        {
            bool[] contiguousCells = new bool[4];
            CellInfo norteCell = new CellInfo(AgentCell.x, AgentCell.y + 1);
            CellInfo esteCell = new CellInfo(AgentCell.x + 1, AgentCell.y);
            CellInfo surCell = new CellInfo(AgentCell.x, AgentCell.y - 1);
            CellInfo oesteCell = new CellInfo(AgentCell.x - 1, AgentCell.y);

            contiguousCells[0] = norteCell.Walkable;
            contiguousCells[1] = esteCell.Walkable;
            contiguousCells[2] = surCell.Walkable;
            contiguousCells[3] = oesteCell.Walkable;

            return contiguousCells;
        }

    }
}