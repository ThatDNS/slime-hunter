using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSlime_ChasePlayer : BasicSlime_BaseState
{
    public virtual void SetChaseTarget()
    {
        fsm.seekSteeringBehaviour.target = fsm.GetPlayerPosition();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetChaseTarget();
        fsm.slimeAgent.reachedGoal = false;
        fsm.slimeAgent.maxSpeed = fsm.chaseSpeed;

        // Enable seeking, disable wandering
        fsm.seekSteeringBehaviour.enabled = true;
        fsm.seekSteeringBehaviour.gameObject.SetActive(true);
        fsm.wanderSteeringBehaviour.enabled = false;
        fsm.wanderSteeringBehaviour.gameObject.SetActive(false);

        // Change slime material (color)
        if (fsm.slimeOuterMesh && fsm.slimeOuterMesh.materials.Length > 0)
        {
            Material[] mats = { fsm.chaseMat };
            fsm.slimeOuterMesh.materials = mats;
        }

        fsm.slimeEnemy.SetEye(EnemyEye.NORMAL);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // Player can move so keep adjusting target
        SetChaseTarget();

        float playerDistance = Vector3.Distance(fsm.slimeAgent.transform.position, fsm.GetPlayerPosition());
        // If player evaded, switch back to wander
        if (playerDistance > fsm.seekDistance)
        {
            fsm.ChangeState(fsm.WanderAroundStateName);
        }
        // If player is within attack radius, attack him
        else if (playerDistance < fsm.attackRadius)
        {
            fsm.ChangeState(fsm.AttackPlayerStateName);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Revert slime material (color)
        if (fsm.slimeOuterMesh && fsm.slimeOuterMesh.materials.Length > 0)
        {
            Material[] mats = { fsm.defaultMat };
            fsm.slimeOuterMesh.materials = mats;
        }
    }
}
