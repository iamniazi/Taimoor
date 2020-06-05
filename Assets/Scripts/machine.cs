using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class machine : MonoBehaviour {
    private MySight checkMyVision;
    private NavMeshAgent agent;
    private Transform playerTransform;

    private Transform patrolDestination;

    private Health playerHealth;

    public float maxDamage = 10f;

    // Enums to keep states
    public enum ENEMY_STATES { patrol, chase, attack }

    // We need a property to get the current state
    [SerializeField]
    private ENEMY_STATES currentState;
    public ENEMY_STATES CurrentState {
        get { return currentState; }
        set {
            currentState = value;
            StopAllCoroutines();
            switch (currentState) {
                case ENEMY_STATES.patrol:
                    StartCoroutine (EnemyPatrol ());
                    break;
                case ENEMY_STATES.chase:
                    StartCoroutine (EnemyChase ());
                    break;
                case ENEMY_STATES.attack:
                    StartCoroutine (EnemyAttack ());
                    break;
            }
        }
    }

    private void Awake () {
        checkMyVision = GetComponent<MySight> ();
        agent = GetComponent<NavMeshAgent> ();

    }
    // Start is called before the first frame update
    void Start () {

        GameObject[] destinations = GameObject.FindGameObjectsWithTag ("Destination");

        patrolDestination = destinations[Random.Range (0, destinations.Length)].GetComponent<Transform> ();

        CurrentState = ENEMY_STATES.chase;
    }

    public IEnumerator EnemyPatrol () {

        while (currentState == ENEMY_STATES.patrol) {

            checkMyVision.sensitivity = MySight.Sensitivity.HIGH;
            agent.isStopped = false;
            agent.SetDestination (patrolDestination.position);
            while (agent.pathPending)
                yield return null;
            if (checkMyVision.targetInSight) {
                agent.isStopped = true;
                CurrentState = ENEMY_STATES.chase;
                yield break;
            }

            yield return null;
        }
        
    }
    public IEnumerator EnemyChase () {
       
        while (currentState == ENEMY_STATES.chase)
        {
            checkMyVision.sensitivity = MySight.Sensitivity.LOW ;
            agent.acceleration = 1000;

            agent.isStopped = false;
            agent.SetDestination(checkMyVision.lastknownSight);
            while (agent.pathPending)
            {
                yield return null;
            }
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;
                if (!checkMyVision.targetInSight)
                    CurrentState = ENEMY_STATES.patrol;
                else
                {
                  CurrentState = ENEMY_STATES.attack;
                }
                yield break;
            }
            yield return null;
        }
        
    }
    public IEnumerator EnemyAttack () {
       
        while (currentState == ENEMY_STATES.attack) {
            agent.isStopped = false;
            agent.SetDestination (playerTransform.position);
            while (agent.pathPending) {
                yield return null;
            }
            if (agent.remainingDistance > agent.stoppingDistance) {
                CurrentState = ENEMY_STATES.chase;
                yield break;
            } else {
                // Do something
                playerHealth.healthpoints -= maxDamage * Time.deltaTime;
            }
            yield return null;
        }
        
        
        yield break;
    }

    // Update is called once per frame
    void Update () {

    }
}