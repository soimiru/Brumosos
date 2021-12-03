using UnityEngine;
using UnityEngine.AI;

public class SkaBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private int diaNacimiento;
    private bool adulto = false;
    private BehaviourTreeEngine behaviourTree;
    private enum estados { ESTUDIAR, DORMIR };
    int cansancio = 50;
    int salud = 100;
    StateMachineEngine childFSM = new StateMachineEngine();

    private void Awake()
    {
        
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        diaNacimiento = simManager.dias;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Maquina de estados de cuando el agente es niño

        //Estados
        //State nacerState = childFSM.CreateEntryState("Nacer", nacerAction);
        State estudiarState = childFSM.CreateEntryState("Estudiar", estudiarAction);
        State dormirState = childFSM.CreateState("Dormir", dormirAction);
        State crecerState = childFSM.CreateState("CrecerA", crecerAction);

        //Percepciones
        Perception nacimiento = childFSM.CreatePerception<TimerPerception>(1);
        Perception hacerNoche = childFSM.CreatePerception<PushPerception>();
        Perception hacerDia = childFSM.CreatePerception<PushPerception>();
        Perception crecer = childFSM.CreatePerception<PushPerception>();

        //Transiciones
        //childFSM.CreateTransition("AMimir", nacerState, nacimiento, dormirState);
        childFSM.CreateTransition("Dormir", estudiarState, hacerNoche, dormirState);
        childFSM.CreateTransition("Estudiar", dormirState, hacerDia, estudiarState);
        childFSM.CreateTransition("Crecer", dormirState, crecer, crecerState);

    }

    // Update is called once per frame
    void Update()
    {
        if (adulto == false)
        {
            FSMChild();

        }
        else
        {
            behaviourTree.Update();
        }
        
        
    }
    private void createBT()
    {
        behaviourTree = new BehaviourTreeEngine(false);

        //Nodos hoja
        LeafNode tengoSalud = behaviourTree.CreateLeafNode("TengoSalud", actSalud, comprobarSalud); 
        LeafNode esDia = behaviourTree.CreateLeafNode("EsDia", actDia, comprobarDia);
        LeafNode cansado = behaviourTree.CreateLeafNode("Cansado", actCansado, comprobarCansado); //Descansar Selector
        LeafNode descansar = behaviourTree.CreateLeafNode("Descansar", actDescansar, comprobarDescansar); //Descansar Selector
        LeafNode esNoche = behaviourTree.CreateLeafNode("EsNoche", actNoche, comprobarNoche);
        LeafNode dormir = behaviourTree.CreateLeafNode("Dormir", actDormir, comprobarDormir);
        LeafNode trabajar = behaviourTree.CreateLeafNode("Trabajar", actTrabajar, comprobarTrabajar);
        LeafNode morir = behaviourTree.CreateLeafNode("Morir", actMorir, comprobarMorir);

        SequenceNode descansarSequenceNode = behaviourTree.CreateSequenceNode("DescansarSelectorNode", false);
        descansarSequenceNode.AddChild(cansado);
        descansarSequenceNode.AddChild(descansar);

        SelectorNode cansadoTrabajarSelectorNode = behaviourTree.CreateSelectorNode("CansadoTrabajarSequenceNode");
        cansadoTrabajarSelectorNode.AddChild(descansarSequenceNode);
        cansadoTrabajarSelectorNode.AddChild(trabajar);

        SequenceNode comprobarDiaSequenceNode = behaviourTree.CreateSequenceNode("ComprobarDiaSequenceNode", false);
        comprobarDiaSequenceNode.AddChild(esDia);
        comprobarDiaSequenceNode.AddChild(cansadoTrabajarSelectorNode);

        SequenceNode comprobarNocheSequenceNode = behaviourTree.CreateSequenceNode("ComprobarNocheSequenceNode", false);
        comprobarNocheSequenceNode.AddChild(esNoche);
        comprobarNocheSequenceNode.AddChild(dormir);

        SelectorNode esDiaONocheSelectorNode = behaviourTree.CreateSelectorNode("esDiaONocheSelectorNode");
        esDiaONocheSelectorNode.AddChild(comprobarDiaSequenceNode);
        esDiaONocheSelectorNode.AddChild(comprobarNocheSequenceNode);

        SequenceNode tengoSaludSequenceNode = behaviourTree.CreateSequenceNode("TengoSaludSequenceNode", false);
        tengoSaludSequenceNode.AddChild(tengoSalud);
        tengoSaludSequenceNode.AddChild(esDiaONocheSelectorNode);

        SelectorNode baseSelectorNode = behaviourTree.CreateSelectorNode("BaseSelectorNode");
        baseSelectorNode.AddChild(tengoSaludSequenceNode);
        baseSelectorNode.AddChild(morir);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", baseSelectorNode);
        behaviourTree.SetRootNode(rootNode);

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
        if (simManager.dias == (diaNacimiento + 10))
        {
            childFSM.Fire("Crecer");
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
        adulto = true;
        createBT();
        //Debug.Log("He cresido");
    }

    #region accionesBT
    private void actSalud()
    {
        Debug.Log("Compruebo Salud");
    }
    private ReturnValues comprobarSalud()
    {
        if (salud > 0 )
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }
    private void actDia()
    {
        Debug.Log("Compruebo Dia");
    }
    private ReturnValues comprobarDia()
    {
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void actNoche()
    {
        Debug.Log("Compruebo Noche");
    }
    private ReturnValues comprobarNoche()
    {
        if (simManager.ciclo == SimulationManager.cicloDNA.NOCHE)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void actCansado()
    {
        Debug.Log("Compruebo cansancio");
    }
    private ReturnValues comprobarCansado()
    {
        if (cansancio >= 50)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void actDescansar()
    {
        //Programar la acción de descansar
        cansancio = 0;
        Debug.Log("ACABO DE DESCANSAR");
    }
    private ReturnValues comprobarDescansar()
    {
        return ReturnValues.Succeed;
    }
    private void actDormir()
    {
        //Programar la acción de dormir
        //Futuramente se cambiará al US
        Debug.Log("Durmiendo");
    }
    private ReturnValues comprobarDormir()
    {
        return ReturnValues.Succeed;
    }

    private void actTrabajar()
    {
        cansancio += 20;
        Debug.Log("Acabo de trabajar");
    }
    private ReturnValues comprobarTrabajar()
    {
        return ReturnValues.Succeed;
    }

    private void actMorir()
    {
        //La unidad se muere
    }
    private ReturnValues comprobarMorir()
    {
        Debug.Log("Me morí");
        return ReturnValues.Succeed;
    }


    #endregion accionesBT
}
