using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField]
    private NavMeshAgent agent = null;

    [SerializeField]
    private Targeter targeter = null;

    [SerializeField]
    private float chaseRange = 10f;

    private Camera mainCamera;

    #region Server

    [ServerCallback]
    private void Update()
    {
        // If the unit has a target, chase it
        Targetable target = targeter.GetTarget();
        if (target != null)
        {
            if (
                (target.transform.position - transform.position).sqrMagnitude
                > chaseRange * chaseRange
            )
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }
        }

        if (agent.hasPath && agent.remainingDistance < agent.stoppingDistance)
        {
            agent.ResetPath();
        }
    }

    [Command]
    public void CmdMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            return;
        }

        agent.SetDestination(hit.position);
    }

    #endregion
}
