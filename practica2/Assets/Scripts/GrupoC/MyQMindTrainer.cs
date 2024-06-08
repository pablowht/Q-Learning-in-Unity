
using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using UnityEngine;
using System;
using Components.QLearning;
using UnityEditor;
using Components;
using NavigationDJIA.Algorithms.AStar;
using UnityEngine.Assertions;
using Unity.VisualScripting;

public class MyQMindTrainer //: IQMindTrainer
{
    public Movable agent;
    public Movable other;

    public int CurrentEpisode { get; private set; }
    public int CurrentStep { get; private set; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; private set; } //Ultima recompensa
    public float ReturnAveraged { get; private set; } //Promedio de recompensas cogidas 

    //public event EventHandler OnEpisodeStarted;
    //public event EventHandler OnEpisodeFinished;

    private INavigationAlgorithm navigationAlgorithm;
    private WorldInfo worldInfo;
    private QMindTrainerParams qMindTrainerParams;

    private bool _started = false;
    public bool showSimulation = false;
    public bool train;
    public float agentSpeed = 1f;


    public string qLearningTrainerClass;

    private IQMindTrainer _qMindTrainer;
    private CellInfo _agentCell;
    private CellInfo _oponentCell;
    private WorldInfo _worldInfo;





    //Recompensas
    private float RecompensaPorPasoNOCazado;
    //Penalizaciones
    private float PenalizacionPorCazado;
    private float PenalizacionPorAlcanzarPared;
    private float PenalizacionPorSalirTablero;

    //Tabla Q
    private MyQTable tablaQ;


    public void Start()
    {
        Assert.IsNotNull(other);
        Assert.IsNotNull(agent);

        _worldInfo = WorldManager.Instance.WorldInfo;

        Type qMindTrainerType = System.Type.GetType(qLearningTrainerClass);
        Assert.IsNotNull(qMindTrainerType);
        _qMindTrainer = (IQMindTrainer)Activator.CreateInstance(qMindTrainerType);
        //_qMindTrainer.OnEpisodeStarted += EpisodeStarted;
        //_qMindTrainer.OnEpisodeFinished += EpisodeFinished;

        _qMindTrainer.Initialize(qMindTrainerParams, _worldInfo, new AStarNavigation());
    }

    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        this.qMindTrainerParams = qMindTrainerParams;
        this.worldInfo = worldInfo;
        this.navigationAlgorithm = navigationAlgorithm;
    }

    //private void EpisodeStarted(object sender, EventArgs e)
    //{
    //    //_agentCell = _qMindTrainer.AgentPosition;
    //    //_oponentCell = _qMindTrainer.OtherPosition;
    //    //state = randomState();

    //    agent.transform.position = _worldInfo.ToWorldPosition(_agentCell);
    //    other.transform.position = _worldInfo.ToWorldPosition(_oponentCell);
    //}

    //private void EpisodeFinished(object sender, EventArgs e)
    //{
    //    if (qMindTrainerParams.episodes == -1 || _qMindTrainer.CurrentEpisode >= qMindTrainerParams.episodes)
    //    {
    //        Debug.Log($"Max episodes reached, stopping simulation");
    //        EditorApplication.ExitPlaymode();
    //    }
    //}

    public void Update()
    {
        if (!showSimulation)
        {
            _qMindTrainer.DoStep(train);
        }
        else
        {
            if (!_started || (agent.DestinationReached && other.DestinationReached))
            {
                agent.speed = agentSpeed;
                other.speed = agentSpeed;

                _started = true;
                _qMindTrainer.DoStep(train);

                //_agentCell = _qMindTrainer.AgentPosition;
                //_oponentCell = _qMindTrainer.OtherPosition;

                agent.destination = _worldInfo.ToWorldPosition(_agentCell);
                other.destination = _worldInfo.ToWorldPosition(_oponentCell);
            }
        }
    }

    public void DoStep(bool train)
    {
        // Actualiza la posición del agente y del oponente
        _agentCell = _qMindTrainer.AgentPosition;
        _oponentCell = _qMindTrainer.OtherPosition;

        // Verifica si el agente ha sido alcanzado por el oponente
        if (_agentCell.Equals(_oponentCell))
        {
            // Penalización por ser alcanzado
            //_qMindTrainer.Return = PenalizacionPorCazado;

            // Puedes reiniciar el episodio o hacer alguna otra acción en caso de ser alcanzado
            // Puedes reiniciar la posición del agente, por ejemplo
            _qMindTrainer.Initialize(qMindTrainerParams, _worldInfo, new AStarNavigation());
            //OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // Verifica si el agente ha chocado con una pared
            //if (_worldInfo.IsWall(_agentCell.x, _agentCell.y))
            //{
            //    // Penalización por chocar con una pared
            //    _qMindTrainer.Return = PenalizacionPorAlcanzarPared;
            //}
            //
            //// Verifica si el agente ha salido del tablero
            //if (!IsLimit(_agentCell))
            //{
            //    // Penalización por salirse del tablero
            //    _qMindTrainer.Return = PenalizacionPorSalirTablero;
            //}
            //
            //// Pequeña recompensa por cada paso que no lleva a ser cazado
            //_qMindTrainer.Return += RecompensaPorPasoNOCazado;
        }

        /*
         Otras cosas que deberíamos poner:
        1. Actualizar la tabla Q: Calculando los nuevos valores de Q en función de las acciones
        tomadas y las recompensas
        2. Calcular als recomenpensas: Definir como asignar las recompensas
        3. Actualizar las posiciones del agente y del zombie según el aprendizaje
         */

        // Mueve los objetos en el mundo de Unity a las nuevas posiciones
        agent.transform.position = _worldInfo.ToWorldPosition(_agentCell);
        other.transform.position = _worldInfo.ToWorldPosition(_oponentCell);


    }

    private void updateWithReward(int row, int col)
    {
        //ELEFANTE
        //float recompensa = 100;

        //float newQvalue = (1 - qMindTrainerParams.alpha) * tablaQ.GetQValue(row, col) + qMindTrainerParams.alpha * (recompensa + qMindTrainerParams.gamma * tablaQ.GetMaxQValue(row));

        //tablaQ.UpdateTable(row, col, newQvalue);
    }

    private int selectAction()
    {
        int accion;
        
        if (qMindTrainerParams.epsilon > 0.5)
        {
            accion = UnityEngine.Random.Range(0, 4);
        }
        else
        {
            //ELEFANTE
            //Crear un random que dependiendo de su valor elija hacer una accion aleatoria o una buena columna
            accion = UnityEngine.Random.Range(0, 4);
        }
        
        return accion;
    }

    private void randomState()
    {
        return;
    }
}
