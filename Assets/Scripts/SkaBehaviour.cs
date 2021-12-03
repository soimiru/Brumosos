using UnityEngine;
using UnityEngine.AI;

public class SkaBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private int diaNacimiento;

    private enum estados { ESTUDIAR, DORMIR };

    StateMachineEngine childFSM = new StateMachineEngine();

    private void Awake()
    {
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        diaNacimiento = simManager.dias;
    }

    // Start is called before the first frame update
    void Start()
    {
        //State nacerState = childFSM.CreateEntryState("Nacer", nacerAction);
        State estudiarState = childFSM.CreateEntryState("Estudiar", estudiarAction);
        State dormirState = childFSM.CreateState("Dormir", dormirAction);

        //Percepciones
        Perception nacimiento = childFSM.CreatePerception<TimerPerception>(1);
        Perception hacerNoche = childFSM.CreatePerception<PushPerception>();
        Perception hacerDia = childFSM.CreatePerception<PushPerception>();

        //Transiciones
        //childFSM.CreateTransition("AMimir", nacerState, nacimiento, dormirState);
        childFSM.CreateTransition("Dormir", estudiarState, hacerNoche, dormirState);
        childFSM.CreateTransition("Estudiar", dormirState, hacerDia, estudiarState);
    }

    // Update is called once per frame
    void Update()
    {
        FSMChild();
        if (diaNacimiento + 10 == simManager.dias)
        {
            crecerAction();
        }
    }

    void FSMChild()
    {
        switch (simManager.ciclo)
        {
            case SimulationManager.cicloDNA.DIA:
                childFSM.Fire("Estudiar");
                break;
            case SimulationManager.cicloDNA.NOCHE:
                childFSM.Fire("Dormir");
                break;
        }
    }

    void estudiarAction()
    {
        //agent.SetDestination(new Vector3(12f, 1f, -20f));
        agent.SetDestination(new Vector3(-2.5f, 1f, 8f));
        Debug.Log("ESTUDIANDO");
    }
    void dormirAction()
    {
        agent.SetDestination(new Vector3(18f, 1f, -20f));
        Debug.Log("DURMIENDO");
    }
    void nacerAction()
    {
        Debug.Log("I WOULD LIKE TO SEE THE BABY");
    }
    void crecerAction()
    {
        transform.localScale = new Vector3(1, 1, 1);
        Debug.Log("He cresido");
    }
}
