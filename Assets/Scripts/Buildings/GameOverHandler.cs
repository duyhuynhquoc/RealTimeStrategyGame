using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;

    [SerializeField]
    private List<UnitBase> bases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        UnitBase.ServerOnUnitBaseSpawned += ServerHandleUnitBaseSpawned;
        UnitBase.ServerOnUnitBaseDespawned += ServerHandleUnitBaseDespawned;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnUnitBaseSpawned -= ServerHandleUnitBaseSpawned;
        UnitBase.ServerOnUnitBaseDespawned -= ServerHandleUnitBaseDespawned;
    }

    [Server]
    private void ServerHandleUnitBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    [Server]
    private void ServerHandleUnitBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1)
        {
            return;
        }

        int playerId = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerId}");

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    [ClientRpc]
    private void RpcGameOver(string winnerName)
    {
        ClientOnGameOver?.Invoke(winnerName);
    }

    #endregion
}
