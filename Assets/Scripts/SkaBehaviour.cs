using UnityEngine;
using UnityEngine.AI;

public class SkaBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    StateMachineEngine childFSM = new StateMachineEngine();

    #region variables Ska
    int salud = 100;
    int cansancio = 50;
    private int diaNacimiento;
    private bool adulto = false;

    #endregion variables Ska

    private void Awake()
    {
        
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        diaNacimiento = simManager.dias;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //Maquina de estados de cuando el agente es ni�o

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
        LeafNode tengoSaludLeafNode = behaviourTree.CreateLeafNode("TengoSalud", actSalud, comprobarSalud); 
        LeafNode esDiaLeafNode = behaviourTree.CreateLeafNode("EsDia", actDia, comprobarDia);
        LeafNode cansadoLeafNode = behaviourTree.CreateLeafNode("Cansado", actCansado, comprobarCansado); //Descansar Selector
        LeafNode descansarLeafNode = behaviourTree.CreateLeafNode("Descansar", actDescansar, comprobarDescansar); //Descansar Selector
        LeafNode esNocheLeafNode = behaviourTree.CreateLeafNode("EsNoche", actNoche, comprobarNoche);
        LeafNode dormirLeafNode = behaviourTree.CreateLeafNode("Dormir", actDormir, comprobarDormir);
        LeafNode trabajarLeafNode = behaviourTree.CreateLeafNode("Trabajar", actTrabajar, comprobarTrabajar);
        LeafNode morirLeafNode = behaviourTree.CreateLeafNode("Morir", actMorir, comprobarMorir);
        LeafNode irATrabajar = behaviourTree.CreateLeafNode("IrAlTrabajo", actIrATrabajar, comprobarLlegada);

        LeafNode timeDescansarLeafNode = behaviourTree.CreateLeafNode("TimerTrabajar", actTimer, comprobarTimer);

        TimerDecoratorNode timerNodeTrabajar = behaviourTree.CreateTimerNode("TimerNodeTrabajar", timeDescansarLeafNode, 1000);

        //Nodo secuencia 1
        SequenceNode descansarSequenceNode = behaviourTree.CreateSequenceNode("DescansarSelectorNode", false);
        descansarSequenceNode.AddChild(cansadoLeafNode);
        descansarSequenceNode.AddChild(descansarLeafNode);
        descansarSequenceNode.AddChild(timeDescansarLeafNode);

        //Nodo selector 1
        SelectorNode cansadoTrabajarSelectorNode = behaviourTree.CreateSelectorNode("CansadoTrabajarSequenceNode");
        cansadoTrabajarSelectorNode.AddChild(descansarSequenceNode);
        cansadoTrabajarSelectorNode.AddChild(trabajarLeafNode);

        //Nodo secuencia 2
        SequenceNode comprobarDiaSequenceNode = behaviourTree.CreateSequenceNode("ComprobarDiaSequenceNode", false);
        comprobarDiaSequenceNode.AddChild(esDiaLeafNode);
        comprobarDiaSequenceNode.AddChild(irATrabajar);
        comprobarDiaSequenceNode.AddChild(cansadoTrabajarSelectorNode);

        //Nodo secuencia 3
        SequenceNode comprobarNocheSequenceNode = behaviourTree.CreateSequenceNode("ComprobarNocheSequenceNode", false);
        comprobarNocheSequenceNode.AddChild(esNocheLeafNode);
        comprobarNocheSequenceNode.AddChild(dormirLeafNode);

        //Nodo selector 2
        SelectorNode esDiaONocheSelectorNode = behaviourTree.CreateSelectorNode("esDiaONocheSelectorNode");
        esDiaONocheSelectorNode.AddChild(comprobarDiaSequenceNode);
        esDiaONocheSelectorNode.AddChild(comprobarNocheSequenceNode);

        //Nodo secuencia 4
        SequenceNode tengoSaludSequenceNode = behaviourTree.CreateSequenceNode("TengoSaludSequenceNode", false);
        tengoSaludSequenceNode.AddChild(tengoSaludLeafNode);
        tengoSaludSequenceNode.AddChild(esDiaONocheSelectorNode);

        //Nodo selector 3
        SelectorNode baseSelectorNode = behaviourTree.CreateSelectorNode("BaseSelectorNode");
        baseSelectorNode.AddChild(tengoSaludSequenceNode);
        baseSelectorNode.AddChild(morirLeafNode);

        //Nodo ra�z
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

    #region FSM Child
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
    #endregion FSM Child

    #region accionesBT
    private void actSalud()
    {
        
    }
    private ReturnValues comprobarSalud()
    {
        Debug.Log("Compruebo Salud");
        if (salud > 0 )
        {
            Debug.Log("Salud bien");
            return ReturnValues.Succeed;
        }
        else
        {
            Debug.Log("Salud mal");
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
        if (simManager.ciclo == SimulationManager.cicloDNA.NOCHE || simManager.ciclo == SimulationManager.cicloDNA.AMANECER)
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
        //Programar la acci�n de descansar
        agent.SetDestination(new Vector3(-8.5f, 1f, 18.5f));
        cansancio -= 30;
        if (cansancio > 0)
        {
            cansancio = 0;
        }
        Debug.Log("ACABO DE DESCANSAR");
    }
    private ReturnValues comprobarDescansar()
    {
        if (this.transform.position.x == -8.5f || this.transform.position.z == 18.5f)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            Debug.Log(this.transform.position);
            Debug.Log("Voy de camino a descansar");
            return ReturnValues.Running;
        }
        
        
    }
    private void actDormir()
    {
        //Programar la acci�n de dormir
        //Futuramente se cambiar� al US
        agent.SetDestination(new Vector3(15.5f, 1f, -18.5f));
        Debug.Log("Durmiendo");
    }
    private ReturnValues comprobarDormir()
    {
        if (this.transform.position.x == 15.5f || this.transform.position.z == -18.5f)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
        
    }

    private void actTrabajar()
    {
        agent.SetDestination(new Vector3(-19.5f, 1f, 19.5f));
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
        //Debug.Log("Me mor�");
        return ReturnValues.Succeed;
    }
    private void actTimer()
    {
        Debug.Log("Timer descansar");
    }
    private ReturnValues comprobarTimer()
    {
        
        return ReturnValues.Succeed;
    }
    private void actIrATrabajar()
    {
        agent.SetDestination(new Vector3(-19.5f, 1f, 19.5f));
    }
    private ReturnValues comprobarLlegada()
    {
        if (this.transform.position.x == -19.5f || this.transform.position.z == 19.5f)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion accionesBT
}
