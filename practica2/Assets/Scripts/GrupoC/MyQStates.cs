using NavigationDJIA.World;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MyQStates : MonoBehaviour
{
    // Mapa de recompensas para cada estado
    public Dictionary<CellInfo, float> RecompensasState { get; private set; }

    // Mapa de penalizaciones para cada estado
    public Dictionary<CellInfo, float> PenalizacionesState { get; private set; }

    // Mapa que indica si hay una pared en las posiciones cardinales de cada estado
    public Dictionary<CellInfo, bool> WallToNorth { get; private set; }
    public Dictionary<CellInfo, bool> WallToSouth { get; private set; }
    public Dictionary<CellInfo, bool> WallToEast { get; private set; }
    public Dictionary<CellInfo, bool> WallToWest { get; private set; }

    // Mapa que indica si una celda es transitable (no es un obstáculo ni está fuera del tablero)
    public Dictionary<CellInfo, bool> Walkable { get; private set; }
    // Mapa que almacena la distancia de Manhattan entre el agente y el enemigo para cada estado
    public Dictionary<CellInfo, int> ManhattanDistance { get; private set; }

    public int ManhattanDistanceCalculada;


    // Constructor
    public MyQStates()
    {
        RecompensasState = new Dictionary<CellInfo, float>();
        PenalizacionesState = new Dictionary<CellInfo, float>();
        WallToNorth = new Dictionary<CellInfo, bool>();
        WallToSouth = new Dictionary<CellInfo, bool>();
        WallToEast = new Dictionary<CellInfo, bool>();
        WallToWest = new Dictionary<CellInfo, bool>();
        Walkable = new Dictionary<CellInfo, bool>();
        ManhattanDistance = new Dictionary<CellInfo, int>();
    }

    // Agregar información de recompensa para un estado
    public void AddReward(CellInfo state, float reward)
    {
        RecompensasState[state] = reward;
    }

    // Agregar información de penalización para un estado
    public void AddPenalty(CellInfo state, float penalty)
    {
        PenalizacionesState[state] = penalty;
    }

    // Agregar información sobre las paredes en las posiciones cardinales para un estado
    public void AddWallsInCardinalDirections(CellInfo state, bool toNorth, bool toSouth, bool toEast, bool toWest)
    {
        WallToNorth[state] = toNorth;
        WallToSouth[state] = toSouth;
        WallToEast[state] = toEast;
        WallToWest[state] = toWest;
    }

    // Agregar información sobre si una celda es transitable
    public void AddWalkable(CellInfo state, bool isWalkable)
    {
        Walkable[state] = isWalkable;
    }

    // Agregar la distancia de Manhattan entre el agente y el enemigo
    public void AddManhattanDistance(CellInfo state)
    {
        ManhattanDistance[state] = ManhattanDistanceCalculada;
    }

    // Obtener recompensa para un estado
    public float GetReward(CellInfo state)
    {
        return RecompensasState.TryGetValue(state, out float reward) ? reward : 0f;
    }

    // Obtener penalización para un estado
    public float GetPenalty(CellInfo state)
    {
        return PenalizacionesState.TryGetValue(state, out float penalty) ? penalty : 0f;
    }

    // Verificar si hay una pared en la posición cardinal Norte para un estado
    public bool HasWallToNorth(CellInfo state)
    {
        return WallToNorth.TryGetValue(state, out bool hasWall) && hasWall;
    }

    // Verificar si hay una pared en la posición cardinal Sur para un estado
    public bool HasWallToSouth(CellInfo state)
    {
        return WallToSouth.TryGetValue(state, out bool hasWall) && hasWall;
    }

    // Verificar si hay una pared en la posición cardinal Este para un estado
    public bool HasWallToEast(CellInfo state)
    {
        return WallToEast.TryGetValue(state, out bool hasWall) && hasWall;
    }

    // Verificar si hay una pared en la posición cardinal Oeste para un estado
    public bool HasWallToWest(CellInfo state)
    {
        return WallToWest.TryGetValue(state, out bool hasWall) && hasWall;
    }

    // Verificar si una celda es transitable
    public bool IsWalkable(CellInfo state)
    {
        return Walkable.TryGetValue(state, out bool isWalkable) && isWalkable;
    }

    // Obtener la distancia de Manhattan entre el agente y el enemigo
    public int GetManhattanDistance(CellInfo state)
    {
        return ManhattanDistance.TryGetValue(state, out int distance) ? distance : 0;
    }

    public void CalculateManhattanDistance(CellInfo agentPos, CellInfo enemyPos)
    {
        ManhattanDistanceCalculada = Math.Abs(agentPos.x - enemyPos.x) + Math.Abs(agentPos.y - enemyPos.y);
    }
}