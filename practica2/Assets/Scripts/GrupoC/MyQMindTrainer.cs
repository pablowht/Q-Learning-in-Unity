
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

public class MyQMindTrainer : IQMindTrainer
{
    public Movable agent;
    public Movable other;

    public int CurrentEpisode { get; private set; }
    public int CurrentStep { get; private set; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; }
    public float ReturnAveraged { get; private set; }

    public string qLearningTrainerClass;
    private QMindTrainerParams qMindTrainerParams;
    private WorldInfo worldInfo;
    private INavigationAlgorithm navigationAlgorithm;

    private IQMindTrainer _qMindTrainer;
    private CellInfo _agentCell;
    private CellInfo _oponentCell;
    private WorldInfo _worldInfo;

    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;

   

    public void Start()
    {
        Assert.IsNotNull(other);
        Assert.IsNotNull(agent);

        _worldInfo = WorldManager.Instance.WorldInfo;

        Type qMindTrainerType = System.Type.GetType(qLearningTrainerClass);
        Assert.IsNotNull(qMindTrainerType);
        _qMindTrainer = (IQMindTrainer)Activator.CreateInstance(qMindTrainerType);
        _qMindTrainer.OnEpisodeStarted += EpisodeStarted;
        _qMindTrainer.OnEpisodeFinished += EpisodeFinished;

        _qMindTrainer.Initialize(qMindTrainerParams, _worldInfo, new AStarNavigation());
    }

    private void EpisodeStarted(object sender, EventArgs e)
    {
        _agentCell = _qMindTrainer.AgentPosition;
        _oponentCell = _qMindTrainer.OtherPosition;

        agent.transform.position = _worldInfo.ToWorldPosition(_agentCell);
        other.transform.position = _worldInfo.ToWorldPosition(_oponentCell);
    }

    private void EpisodeFinished(object sender, EventArgs e)
    {
        if (qMindTrainerParams.episodes == -1 || _qMindTrainer.CurrentEpisode >= qMindTrainerParams.episodes)
        {
            Debug.Log($"Max episodes reached, stopping simulation");
            EditorApplication.ExitPlaymode();
        }
    }

    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        this.qMindTrainerParams = qMindTrainerParams;
        this.worldInfo = worldInfo;
        this.navigationAlgorithm = navigationAlgorithm;

    }

    public void DoStep(bool train)
    {
        // Realiza un paso en el entrenamiento
        _qMindTrainer.DoStep(train);

        // Actualiza la posición del agente y del oponente
        _agentCell = _qMindTrainer.AgentPosition;
        _oponentCell = _qMindTrainer.OtherPosition;

        // Verifica si el agente ha sido alcanzado por el oponente
        if (_agentCell.Equals(_oponentCell))
        {
            // Penalización por ser alcanzado
            _qMindTrainer.Return = PenalizacionPorCazado;

            // Puedes reiniciar el episodio o hacer alguna otra acción en caso de ser alcanzado
            // Puedes reiniciar la posición del agente, por ejemplo
            _qMindTrainer.Initialize(qMindTrainerParams, _worldInfo, new AStarNavigation());
            OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            // Verifica si el agente ha chocado con una pared
            if (_worldInfo.IsWall(_agentCell.x, _agentCell.y))
            {
                // Penalización por chocar con una pared
                _qMindTrainer.Return = PenaltyForHittingWall;
            }

            // Verifica si el agente ha salido del tablero
            if (!IsInsideBoard(_agentCell))
            {
                // Penalización por salirse del tablero
                _qMindTrainer.Return = PenaltyForLeavingBoard;
            }

            // Puedes asignar una pequeña recompensa por cada paso que no lleva al atrapado
            _qMindTrainer.Return += RewardForNonCaughtStep;
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
}
