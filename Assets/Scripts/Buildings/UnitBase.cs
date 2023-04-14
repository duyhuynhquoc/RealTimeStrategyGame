using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField]
    private Health health = null;

    public static event Action<UnitBase> ServerOnUnitBaseSpawned;
    public static event Action<UnitBase> ServerOnUnitBaseDespawned;

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;

        ServerOnUnitBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnUnitBaseDespawned?.Invoke(this);

        health.ServerOnDie -= ServerHandleDie;
    }

    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion
}
