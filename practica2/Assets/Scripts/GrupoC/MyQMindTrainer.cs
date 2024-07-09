using System;
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind.Interfaces;
using UnityEngine;

namespace QMind
{
    //Clase para el Trainer
    public class MyQMindTrainer : IQMindTrainer
    {
        //Variables básicas del entrenamiento provenientes de la interfaz
        public int CurrentEpisode { get; private set; }
        public int CurrentStep { get; private set; }
        public CellInfo AgentPosition { get; private set; }
        public CellInfo OtherPosition { get; private set; }
        public float Return { get; private set; }
        public float ReturnAveraged { get; private set; }

        //Eventos que controlan los episodios
        public event EventHandler OnEpisodeStarted;
        public event EventHandler OnEpisodeFinished;

        //Parámetros del algoritmo
        QMindTrainerParams qMindParams;
        WorldInfo world;
        INavigationAlgorithm navAlgorithm;

        //Tabla Q
        MyQTable qTable;
        int qTableNumber = 0;

        //Metodo Initialize donde se inicializa el mundo, la posición de los personajes, los valores del algoritmo y se crea o carga la tabla Q
        public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
        {
            Debug.Log("MyQMindTrainer: initialized");
            world = worldInfo;

            //Posiciones aleatorias
            AgentPosition = worldInfo.RandomCell();
            OtherPosition = worldInfo.RandomCell();

            //Parámetros del algoritmo
            qMindParams = qMindTrainerParams;
            qMindParams.episodes = 300000;
            qMindParams.episodesBetweenSaves = 20000;
            qMindParams.epsilon = 0.85f;
            //qMindParams.alpha = 0.3f;
            //qMindParams.gamma = 0.7f;

            //Contador de pasos totales para la recompensa media
            totalSteps = 1;

            //Inicialización del mundo
            navAlgorithm = navigationAlgorithm;
            navAlgorithm.Initialize(world);

            //Creación de la tabla Q
            qTable = new MyQTable();
            qTable.InitializeQTable();

            //Si se desea partir de una tabla ya creada poner el número de esta para un correcto nombre y comentar: qTable.InitializeQTable();

            //qTableNumber = 21;
            //string pathTable = qTable.ReturnNewestTable(Application.dataPath + "/Scripts/GrupoC/QTables/");
            //qTable.LoadTable(pathTable);
        }

        //Variables para el DoStep

        //Variables de los estados
        MyQStates currentState;
        MyQStates nextState;
        int distance;
        int orientation;
        bool[] cellState;
        CellInfo nextStateCell;


        //Valores Q
        float QValueCurrent;
        float QValueNew;
        float QValueNextState;

        int action;
        float reward;
        int totalSteps;
        float accionAleatoria = 0.1f;

        float maxQValue;
        int episodeAux = 0;

        bool newEpisode = true;

        //Metodo DoStep
        public void DoStep(bool train)
        {
            //Si no hay ningun episodio comenzado se comienza
            if (newEpisode)
            {
                StartEpisode();
            }
            
            CurrentStep += 1;

            //Creamos el estado actual discretizando sus datos
            distance = DiscretizeDistance(-1);
            orientation = DiscretizeOrientation(-1, AgentPosition, OtherPosition);
            cellState = CalculateCellState(-1);

            currentState = new MyQStates(distance, orientation, cellState);

            //Se obtiene el array de los valores Q
            qTable.qTableDictionary.TryGetValue(currentState, out float[] qValuesCurrentState);

            //Seleccionamos una acción de forma balanceada (al principio favoreciendo la aleatoriedad)
            accionAleatoria = UnityEngine.Random.Range(0.0f, 1.0f);
            if (accionAleatoria < qMindParams.epsilon)
            {
                action = UnityEngine.Random.Range(0, 4);
                //accionAleatoria += 0.05f;
            }
            else
            {
                action = qTable.GetBestAction(currentState);
                //accionAleatoria = UnityEngine.Random.Range(0.1f, qMindParams.epsilon);
            }

            //Calculo del siguiente estado según la acción
            int distanceNext = DiscretizeDistance(action);
            int orientationNext = DiscretizeOrientation(action, AgentPosition, OtherPosition);
            bool[] cellStateNext = CalculateCellState(action);
            nextState = new MyQStates(distanceNext, orientationNext, cellStateNext);

            //Se obtiene el array de los valores Q
            qTable.qTableDictionary.TryGetValue(nextState, out float[] qValuesNextState);

            //Obtenemos el valor Q del estado actual según la acción
            QValueCurrent = qValuesCurrentState[action];

            //Obtenemos el mejor valor Q del estado siguiente
            QValueNextState = qTable.GetBestQValue(nextState, qValuesNextState);

            //Se calcula la posición de la celda siguiente según la acción
            nextStateCell = CalculateCellPosition(action);
            nextStateCell = world[nextStateCell.x, nextStateCell.y];

            //Calculo de recompensa
            reward = GetReward(nextState, currentState);
            //Recompensa Total
            Return += reward;
            //Recompensa Media
            ReturnAveraged = Return / totalSteps;
            totalSteps++;

            //Calculo del nuevo valor Q del estado actual
            QValueNew = (1 - qMindParams.alpha) * QValueCurrent + qMindParams.alpha * (reward + qMindParams.gamma * QValueNextState);

            //Variable para guardar el mejor no
            maxQValue = QValueNew > maxQValue ? QValueNew : maxQValue;

            //Se asigna el nuevo valor de Q
            qValuesCurrentState[action] = QValueNew;

            //Si la recompensa es negativa grande o el agente está en la misma posición del oponente se reinicia 
            if (AgentPosition == OtherPosition || reward <= -50.0f)
            {
                RestartEpisode();
            }

            //Si se ha llegado al máximo de episodios se guarda la tabla y se reinicia
            if (episodeAux == qMindParams.episodesBetweenSaves)
            {
                qTableNumber++;
                qTable.SaveTableToCSV(qTableNumber, CurrentEpisode, maxQValue);

                episodeAux = 0;

                RestartEpisode();
            }

            //Se mueve al oponente con el GetPath y al Agent a la siguiente celda
            if (navAlgorithm.GetPath(OtherPosition, AgentPosition, 200).Length > 0)
            {
                OtherPosition = navAlgorithm.GetPath(OtherPosition, AgentPosition, 200)[0];
            }
            AgentPosition = world[nextStateCell.x, nextStateCell.y];
        }

        #region Controlador de episodios
        private void StartEpisode()
        {
            //Contador de variables
            CurrentEpisode += 1;
            episodeAux += 1;
            CurrentStep = 0;

            newEpisode = false;

            //Modificación del epsilon según los episodios
            if (CurrentEpisode == 80000)
            {
                qMindParams.epsilon = 0.6f;
            }
            if (CurrentEpisode == 100000)
            {
                qMindParams.epsilon = 0.5f;
            }
            //if (CurrentEpisode == 140000)
            //{
            //    qMindParams.epsilon = 0.4f;
            //}
            //if (CurrentEpisode == 160000)
            //{
            //    qMindParams.epsilon = 0.3f;
            //}

            //Se pone al agente y oponente en una celda aleatoria
            AgentPosition = world.RandomCell();
            OtherPosition = world.RandomCell();

            //Se comienza el episodio
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }
        
        //Acaba el episodio y se reinicia
        private void RestartEpisode()
        {
            newEpisode = true;

            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        //Metodo para obtener una recompensa según el estado
        private float GetReward(MyQStates nextState, MyQStates currentState)
        {
            float reward = 0.0f;

            int initialDistance = Mathf.Abs(AgentPosition.x - OtherPosition.x) + Mathf.Abs(AgentPosition.y - OtherPosition.y);
            int newDistance = Mathf.Abs(nextStateCell.x - OtherPosition.x) + Mathf.Abs(nextStateCell.y - OtherPosition.y);

            //Recompensa negativa si va a una celda no transitable
            if (!nextStateCell.Walkable)
            {
                reward = -100.0f;
            }
            else if (newDistance > initialDistance)
            {
                reward = 10.0f; //Recompensa positiva si aumenta la distancia entre ambos
            }
            else if (newDistance < initialDistance)
            {
                reward = -1.0f; //Recompensa negativa si disminuye la distancia entre ambos
            }
            else if (newDistance == initialDistance)
            {
                reward = 0.0f; //Recompensa neutra si mantiene la distancia entre ambos
            }
            //Recompensa negativa si la celda a la que va es la del Oponente
            if (nextStateCell == OtherPosition)
            {
                reward = -100.0f;
            }

            
            return reward;
        }

        //Calcula la celda correspondiente a la accion
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

        #region Discretización del estado

        //Discretiza la distancia según los valores queridos
        private int DiscretizeDistance(int actionNextState)
        {
            int manhattanDistance = 0;
            //Si se pasa una acción que es -1 significa que es el estado actual
            if (actionNextState != -1)
            {
                //Según la acción se calcula una distancia manhattan según la celda debida
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
                //Caso de estado actual
                manhattanDistance = Math.Abs(AgentPosition.x - OtherPosition.x) + Math.Abs(AgentPosition.y - OtherPosition.y);
            }

            //Segun la distancia manhattan se devuelve un valor u otro con 0 el más pequeño y 2 la mayor distancia
            if (manhattanDistance <= 5)
            {
                return 0;
            }
            else if (manhattanDistance > 5 && manhattanDistance <= 15) 
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        //Discretiza la orientación entre personajes
        private int DiscretizeOrientation(int actionNextState, CellInfo agentPos, CellInfo otherPos)
        {
            float deltaX = 0;
            float deltaY = 0;

            //Si la acción es distinta a -1 es una celda de siguiente estado
            if (actionNextState != -1)
            {
                switch (actionNextState)
                {
                    //Norte
                    case 0:
                        deltaX = agentPos.x - otherPos.x;
                        deltaY = otherPos.y - (agentPos.y + 1);
                        break;
                    //Este
                    case 1:
                        deltaX = (agentPos.x + 1) - otherPos.x;
                        deltaY = otherPos.y - agentPos.y;
                        break;
                    //Sur
                    case 2:
                        deltaX = agentPos.x - otherPos.x;
                        deltaY = otherPos.y - (agentPos.y - 1);
                        break;
                    //Oeste
                    case 3:
                        deltaX = (agentPos.x - 1) - otherPos.x;
                        deltaY = otherPos.y - agentPos.y;
                        break;
                }
            }
            else
            {
                //Si la acción es -1 se calcula el valor del estado actual
                deltaX = agentPos.x - otherPos.x;
                deltaY = otherPos.y - agentPos.y;
            }

            //Se discretiza según el ángulo calculado por la tangente
            float angleRadians = Mathf.Atan2(deltaY, deltaX);

            float angleDegrees = angleRadians * Mathf.Rad2Deg;
            if (angleDegrees < 0)
            {
                angleDegrees += 360;
            }

            int discreteOrientation = Mathf.FloorToInt(angleDegrees / 45);

            return discreteOrientation;
        }

        //Se calcula el valor de las celdas colindantes al estado actual
        private bool[] CalculateCellState(int actionNextState)
        {
            bool[] contiguousCells = new bool[4];

            CellInfo norteCell = new(AgentPosition.x, AgentPosition.y + 1);
            CellInfo esteCell = new(AgentPosition.x + 1, AgentPosition.y);
            CellInfo surCell = new(AgentPosition.x, AgentPosition.y - 1);
            CellInfo oesteCell = new(AgentPosition.x - 1, AgentPosition.y);
            
            //Si la acción es distinta de -1 es una celda de estado siguiente
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

            //True/False si es Walkable
            contiguousCells[0] = norteCell.Walkable;
            contiguousCells[1] = esteCell.Walkable;
            contiguousCells[2] = surCell.Walkable;
            contiguousCells[3] = oesteCell.Walkable;

            return contiguousCells;
        }

        #endregion
    }
}