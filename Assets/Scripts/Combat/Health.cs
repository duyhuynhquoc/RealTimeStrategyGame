using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField]
    private int maxHealth = 100;

    [SerializeField]
    [SyncVar]
    private int currentHealth;

    private event Action ServerOnDie;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    [Server]
    public void DealDamage(int damage)
    {
        Debug.Log(damage);

        if (currentHealth == 0)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (currentHealth != 0)
        {
            return;
        }

        ServerOnDie?.Invoke();

        Debug.Log("We died!");
    }

    #endregion

    #region Client

    #endregion
}
