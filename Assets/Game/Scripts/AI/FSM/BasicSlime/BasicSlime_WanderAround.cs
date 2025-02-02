using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSlime_WanderAround : BasicSlime_BaseState
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Enable wandering, disable seeking
        fsm.wanderSteeringBehaviour.enabled = true;
        fsm.wanderSteeringBehaviour.gameObject.SetActive(true);
        fsm.seekSteeringBehaviour.enabled = false;
        fsm.seekSteeringBehaviour.gameObject.SetActive(false);

        fsm.slimeAgent.reachedGoal = false;
        fsm.slimeAgent.maxSpeed = fsm.wanderSpeed;

        fsm.slimeEnemy.SetEye(EnemyEye.NORMAL);

        fsm.slimeEnemy.Alerted = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        // If slime reached close to player, switch to chase
        if (Vector3.Distance(fsm.transform.position, fsm.GetPlayerPosition()) <= fsm.seekDistance)
        {
            fsm.slimeEnemy.Alerted = true;
            fsm.ChangeState(fsm.ChasePlayerStateName);
        }
    }
}
