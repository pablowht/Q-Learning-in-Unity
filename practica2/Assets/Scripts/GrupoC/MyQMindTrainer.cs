using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;

namespace QMind
{
    public class MyQMindTrainer : IQMindTrainer
    {
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; private set; }
        public float ReturnAveraged { get; private set; }

        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        //Parámetros del algoritmo
        QMindTrainerParams qMindParams;
        WorldInfo world;
        INavigationAlgorithm navAlgorithm;

        //Tabla Q
        MyQTable qTable;
        int qTableNumber = 0;
        //int qTableEpocas = 0;
        //int qTableBestReward = 0;


        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Debug.Log("MyQMindTrainer: initialized");
            world = worldInfo;

            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();

            qMindParams = qMindTrainerParams;
            qMindParams.episodes = 300000;
            qMindParams.episodesBetweenSaves = 20000;
            qMindParams.epsilon = 0.85f;

            totalSteps = 1;

            navAlgorithm = navigationAlgorithm;
            navAlgorithm.Initialize(world);

            qTable = new MyQTable();
            //qTable.InitializeQTable();
            qTableNumber = 19;
            string pathTable = qTable.ReturnNewestTable(Application.dataPath + "/Scripts/QTables/");
            qTable.LoadTable(pathTable);
        }

        MyQStates currentState;
        MyQStates nextState;
        int distance;
        int orientation;
        bool[] cellState;

        int action;
        float QValueCurrent;
        float QValueNew;
        float QValueNextState;
        float reward;
        int totalSteps;
        float accionAleatoria = 0.1f;
        CellInfo nextStateCell;

        float maxQValue;
        int episodeAux = 0;

        bool newEpisode = true;

        public void DoStep(bool train)
        {

            if (newEpisode)
            {
                StartEpisode();
            }
            CurrentStep += 1;

            //Creamos el estado actual
            distance = DiscretizeDistance(-1);
            orientation = DiscretizeOrientation(-1);
            cellState = CalculateCellState(-1);

            currentState = new MyQStates(distance, orientation, cellState);

            qTable.qTableDictionary.TryGetValue(currentState, out float[] qValuesCurrentState);

            //Se inicializan las celdas no andables con recompensa -100
            //for (int i = 0; i < cellState.Length; i++)
            //{
            //    if (!cellState[i])
            //    {
            //        qValuesCurrentState[i] = -100; //Recompensa negativa muy alta
            //    }
            //}

            //Seleccionamos una acción de forma balanceada
            accionAleatoria = UnityEngine.Random.Range(0.0f, 1.0f);
            if (accionAleatoria < qMindParams.epsilon)
            {
                action = UnityEngine.Random.Range(0, 4);
                //accionAleatoria += 0.05f;
            }
            else
            {
                action = qTable.GetBestAction(currentState, 0);
                //accionAleatoria = UnityEngine.Random.Range(0.1f, qMindParams.epsilon);
            }

            //Siguiente estado
            int distanceNext = DiscretizeDistance(action);
            int orientationNext = DiscretizeOrientation(action);
            bool[] cellStateNext = CalculateCellState(action);

            nextState = new MyQStates(distanceNext, orientationNext, cellStateNext);
            qTable.qTableDictionary.TryGetValue(nextState, out float[] qValuesNextState);

            //Obtenemos el valor Q del estado actual
            QValueCurrent = qValuesCurrentState[action];
            //Obtenemos el valor Q del estado siguiente
            QValueNextState = qTable.GetBestQValue(nextState, qValuesNextState);

            nextStateCell = CalculateCellPosition(action);
            nextStateCell = world[nextStateCell.x, nextStateCell.y];

            //Calculo de recompensa
            reward = GetReward(nextState, qValuesNextState);
            //Recompensa Total
            Return += reward;
            //Recompensa Media
            ReturnAveraged = Return / totalSteps;
            totalSteps++;

            if(reward > -100.0f)
            {
                QValueNew = (1 - qMindParams.alpha) * QValueCurrent + qMindParams.alpha * (reward + qMindParams.gamma * QValueNextState);
            }
            else
            {
                QValueNew = reward;
            }
            //Debug.Log(QValueNew);

            maxQValue = QValueNew > maxQValue ? QValueNew : maxQValue;

            qValuesCurrentState[action] = QValueNew;

            if (AgentPosition == OtherPosition || reward <= -50.0f)
            {
                RestartEpisode();
            }

            if (episodeAux == qMindParams.episodesBetweenSaves)
            {
                qTableNumber++;
                qTable.SaveTableToCSV(qTableNumber, CurrentEpisode, maxQValue);

                episodeAux = 0;

                RestartEpisode();
            }

            if (navAlgorithm.GetPath(OtherPosition, AgentPosition, 200).Length > 0)
            {
                OtherPosition = navAlgorithm.GetPath(OtherPosition, AgentPosition, 200)[0];
            }
            AgentPosition = world[nextStateCell.x, nextStateCell.y];
        }
        
        private void RestartEpisode()
        {
            newEpisode = true;

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
        }

        private float GetReward(MyQStates state, float[] qValuesAction)
        {
            float reward = 0.0f;

            if (nextStateCell.Walkable)
            {
                if (state.rangeDistance == 2)
                {
                    if (state.orientation == 4)
                    {
                        reward = 100.0f;
                    }
                    else if (state.orientation == 3 || state.orientation == 5)
                    {
                        reward = 60.0f;
                    }
                    else
                    {
                        reward = 40.0f;
                    }
                }
                else if (state.rangeDistance == 1)
                {
                    if (state.orientation == 4)
                    {
                        reward = 50.0f;
                    }
                    else if (state.orientation == 3 || state.orientation == 5)
                    {
                        reward = 30.0f;
                    }
                    else
                    {
                        reward = 20.0f;
                    }
                }
                else if (state.rangeDistance == 0)
                {
                    if (state.orientation == 4)
                    {
                        reward = 15.0f;
                    }
                    else if (state.orientation == 3 || state.orientation == 5)
                    {
                        reward = 7.0f;
                    }
                    else
                    {
                        reward = 0.0f;
                    }
                }
            }
            else
            {
                reward = -100.0f;
            }

            if (nextStateCell == OtherPosition)
            {
                reward = -100.0f;
            }

            return reward;
        }

        //if (state.rangeDistance == 2)
        //{
        //    if (state.orientation == 4)
        //    {
        //        reward = 100.0f;
        //    }
        //    else if (state.orientation == 3 || state.orientation == 5)
        //    {
        //        reward = 60.0f;
        //    }
        //    else
        //    {
        //        reward = 40.0f;
        //    }
        //}
        //else if (state.rangeDistance == 1)
        //{
        //    if (state.orientation == 4)
        //    {
        //        reward = 50.0f;
        //    }
        //    else if (state.orientation == 3 || state.orientation == 5)
        //    {
        //        reward = 30.0f;
        //    }
        //    else
        //    {
        //        reward = 20.0f;
        //    }
        //}
        //else if (state.rangeDistance == 0)
        //{
        //    if (state.orientation == 4)
        //    {
        //        reward = 15.0f;
        //    }
        //    else if (state.orientation == 3 || state.orientation == 5)
        //    {
        //        reward = 7.0f;
        //    }
        //    else
        //    {
        //        reward = 0.0f;
        //    }
        //}
        //if (!nextStateCell.Walkable || nextStateCell == OtherPosition || AgentPosition == OtherPosition)
        //{
        //    reward = -100.0f;
        //}

        private CellInfo CalculateCellPosition(int action)
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

        private int DiscretizeDistance(int actionNextState)
        {
            int manhattanDistance = 0;
            if (actionNextState != -1)
            {
                switch (actionNextState)
                {
                    //Norte
                    case 0:
                        manhattanDistance = Math.Abs(AgentPosition.x - OtherPosition.x) + Math.Abs((AgentPosition.y + 1) - OtherPosition.y);
                        break;
                    //Este
                    case 1:
                        manhattanDistance = Math.Abs((AgentPosition.x + 1) - OtherPosition.x) + Math.Abs(AgentPosition.y - OtherPosition.y);
                        break;
                    //Sur
                    case 2:
                        manhattanDistance = Math.Abs(AgentPosition.x - OtherPosition.x) + Math.Abs((AgentPosition.y - 1) - OtherPosition.y);
                        break;
                    //Oeste
                    case 3:
                        manhattanDistance = Math.Abs((AgentPosition.x - 1) - OtherPosition.x) + Math.Abs(AgentPosition.y - OtherPosition.y);
                        break;
                }
            }
            else
            {
                manhattanDistance = Math.Abs(AgentPosition.x - OtherPosition.x) + Math.Abs(AgentPosition.y - OtherPosition.y);
            }

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

        private int DiscretizeOrientation(int actionNextState)
        {
            float deltaX = 0;
            float deltaY = 0;

            if (actionNextState != -1)
            {
                switch (actionNextState)
                {
                    //Norte
                    case 0:
                        deltaX = AgentPosition.x - OtherPosition.x;
                        deltaY = OtherPosition.y - (AgentPosition.y + 1);
                        break;
                    //Este
                    case 1:
                        deltaX = (AgentPosition.x + 1) - OtherPosition.x;
                        deltaY = OtherPosition.y - AgentPosition.y;
                        break;
                    //Sur
                    case 2:
                        deltaX = AgentPosition.x - OtherPosition.x;
                        deltaY = OtherPosition.y - (AgentPosition.y - 1);
                        break;
                    //Oeste
                    case 3:
                        deltaX = (AgentPosition.x - 1) - OtherPosition.x;
                        deltaY = OtherPosition.y - AgentPosition.y;
                        break;
                }
            }
            else
            {
                deltaX = AgentPosition.x - OtherPosition.x;
                deltaY = OtherPosition.y - AgentPosition.y;
            }

            float angleRadians = Mathf.Atan2(deltaY, deltaX);

            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            int discreteOrientation = Mathf.FloorToInt(angleDegrees / 45);

            return discreteOrientation;
        }

        //public bool FacingEachOther(CellInfo agentCell, CellInfo otherCell)
        //{
        //    int agentToPlayerOrientation = DiscretizeOrientation(-1, agentCell, otherCell);
        //    int playerToAgentOrientation = DiscretizeOrientation(-1, otherCell, agentCell);
        //    return (agentToPlayerOrientation + 4) % 8 == playerToAgentOrientation;
        //}

        private bool[] CalculateCellState(int actionNextState)
        {
            bool[] contiguousCells = new bool[4];

            CellInfo norteCell = new(AgentPosition.x, AgentPosition.y + 1);
            CellInfo esteCell = new(AgentPosition.x + 1, AgentPosition.y);
            CellInfo surCell = new(AgentPosition.x, AgentPosition.y - 1);
            CellInfo oesteCell = new(AgentPosition.x - 1, AgentPosition.y);
            

            if (actionNextState != -1)
            {
                switch (actionNextState)
                {
                    //Norte
                    case 0:
                        norteCell = new(norteCell.x, norteCell.y + 1);
                        esteCell = new(esteCell.x, esteCell.y + 1);
                        surCell = new(surCell.x, surCell.y + 1);
                        oesteCell = new(oesteCell.x, oesteCell.y + 1);
                        break;
                    //Este
                    case 1:
                        norteCell = new(norteCell.x + 1, norteCell.y);
                        esteCell = new(esteCell.x + 1, esteCell.y);
                        surCell = new(surCell.x + 1, surCell.y);
                        oesteCell = new(oesteCell.x + 1, oesteCell.y);
                        break;
                    //Sur
                    case 2:
                        norteCell = new(norteCell.x, norteCell.y - 1);
                        esteCell = new(esteCell.x, esteCell.y - 1);
                        surCell = new(surCell.x, surCell.y - 1);
                        oesteCell = new(oesteCell.x, oesteCell.y - 1);
                        break;
                    //Oeste
                    case 3:
                        norteCell = new(norteCell.x - 1, norteCell.y);
                        esteCell = new(esteCell.x - 1, esteCell.y);
                        surCell = new(surCell.x - 1, surCell.y);
                        oesteCell = new(oesteCell.x - 1, oesteCell.y);
                        break;
                }
            }

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



        private void StartEpisode()
        {
            CurrentEpisode += 1;
            episodeAux += 1;
            CurrentStep = 0;

            newEpisode = false;

            if (CurrentEpisode == 80000)
            {
                qMindParams.epsilon = 0.5f;
            }
            if (CurrentEpisode == 100000)
            {
                qMindParams.epsilon = 0.4f;
            }
            if (CurrentEpisode == 120000)
            {
                qMindParams.epsilon = 0.3f;
            }
            if (CurrentEpisode == 160000)
            {
                qMindParams.epsilon = 0.2f;
            }
            if (CurrentEpisode == 200000)
            {
                qMindParams.epsilon = 0.1f;
            }

            AgentPosition = world.RandomCell();


            OtherPosition = world.RandomCell();


            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}