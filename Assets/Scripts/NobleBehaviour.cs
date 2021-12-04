using System;
using UnityEngine;
using UnityEngine.AI;

public class NobleBehaviour : MonoBehaviour
{
    private int diaNacimiento;
    int cansancio = 50;
    int salud = 100;

    private enum estados { ESTUDIAR, DORMIR };
    private bool adulto = false;
    private SimulationManager simManager;
    public NavMeshAgent agent;
    private NavigationPoints navPoints;
    public enum posiciones { FABRICA, MANSIONNOBLE, CALLE }
    public posiciones miPosicion;

    StateMachineEngine childFSM = new StateMachineEngine();
    BehaviourTreeEngine adultBT = new BehaviourTreeEngine();

    private void Awake()
    {
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        navPoints = new NavigationPoints();
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
        if (!adulto) {
            FSMChild();
            if (diaNacimiento + 10 == simManager.dias)
            {
                crecerAction();
            }
        }
        if (adulto) { 
            //BTAdult();
            adultBT.Update();
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


    void createBTAdult() {
        
        SequenceNode diaSequence;
        SelectorNode descansoTrabajoSelector;

        LeafNode esDeDiaNode;
        LeafNode trabajarNode;
        LeafNode descansarNode;
      
        //DÍA
        esDeDiaNode = adultBT.CreateLeafNode("esDeDiaNode", actionDia, comprobarDia);
        trabajarNode = adultBT.CreateLeafNode("trabajarNode", trabajar, haTrabajado);
        descansarNode = adultBT.CreateLeafNode("descansarNode", descansar, haDescansado);
        
        descansoTrabajoSelector = adultBT.CreateSelectorNode("descansoTrabajoSelector");
        descansoTrabajoSelector.AddChild(trabajarNode);
        descansoTrabajoSelector.AddChild(descansarNode);

        diaSequence = adultBT.CreateSequenceNode("diaSequence", false);
        diaSequence.AddChild(esDeDiaNode);
        diaSequence.AddChild(descansoTrabajoSelector);

        //NOCHE
        SequenceNode nocheSequence;
        SelectorNode nocheSelector;
        SequenceNode fiestaSequence;

        LeafNode esDeNocheNode;
        LeafNode hayAlgunaFiestaNode;
        LeafNode fiestaNode;
        LeafNode merodearNode;
        LeafNode dormirNode;

        esDeNocheNode = adultBT.CreateLeafNode("esDeNocheNode", actionNoche, comprobarNoche);
        hayAlgunaFiestaNode = adultBT.CreateLeafNode("hayAlgunaFiesta", actionFiesta, comprobarFiesta);
        fiestaNode = adultBT.CreateLeafNode("fiesta", fiestaSU, resultadoFiesta);
        merodearNode = adultBT.CreateLeafNode("merodear", merodearFSM, resultadoMerodear);
        dormirNode = adultBT.CreateLeafNode("dormir", actionDormir, resultadoDormir);

        fiestaSequence = adultBT.CreateSequenceNode("fiestaSequence", false);
        fiestaSequence.AddChild(hayAlgunaFiestaNode);
        fiestaSequence.AddChild(fiestaNode);

        nocheSelector = adultBT.CreateSelectorNode("nocheSelector");
        nocheSelector.AddChild(fiestaSequence);
        nocheSelector.AddChild(merodearNode);
        nocheSelector.AddChild(dormirNode);

        nocheSequence = adultBT.CreateSequenceNode("nocheSequence", false);
        nocheSequence.AddChild(esDeNocheNode);
        nocheSequence.AddChild(nocheSelector);

        //BASE
        LeafNode tengoSaludNode;
        LeafNode morirNode;

        SelectorNode esDiaONocheSelectorNode;
        SequenceNode tengoSaludSequenceNode;
        SelectorNode baseSelectorNode;
        LoopDecoratorNode rootNode;
        

        tengoSaludNode = adultBT.CreateLeafNode("tengoSaludNode", actionSalud, comprobarSalud);
        morirNode = adultBT.CreateLeafNode("morirNode", actionMorir, comprobarMorir);

        esDiaONocheSelectorNode = adultBT.CreateSelectorNode("esDiaONocheSelectorNode");
        esDiaONocheSelectorNode.AddChild(diaSequence);
        esDiaONocheSelectorNode.AddChild(nocheSequence);

        tengoSaludSequenceNode = adultBT.CreateSequenceNode("TengoSaludSequenceNode", false);
        tengoSaludSequenceNode.AddChild(tengoSaludNode);
        tengoSaludSequenceNode.AddChild(esDiaONocheSelectorNode);

        baseSelectorNode = adultBT.CreateSelectorNode("BaseSelectorNode");
        baseSelectorNode.AddChild(tengoSaludSequenceNode);
        baseSelectorNode.AddChild(morirNode);

        rootNode = adultBT.CreateLoopNode("RootNode", baseSelectorNode, 1440 * 40);
        adultBT.SetRootNode(rootNode);

    }



    #region METODOS CHILD
    void estudiarAction()
    {
        //agent.SetDestination(new Vector3(12f, 1f, -20f));
        agent.SetDestination(new Vector3(-19.5f, 1f, 14f));
        Debug.Log("ESTUDIANDO");
    }
    void dormirAction()
    {
        agent.SetDestination(new Vector3(19f, 1f, 19f));
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
        //Debug.Log("He cresido");
        createBTAdult();
    }
    #endregion

    #region METODOS ADULT
    void actionDia() { }
    private ReturnValues comprobarDia() 
    {
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            return ReturnValues.Succeed;
        }
        else {
            return ReturnValues.Failed;
        }
    }
    void actionNoche() { }
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
    void trabajar() {
        
        if (miPosicion != posiciones.FABRICA)
        {
            agent.SetDestination(navPoints.goToFabrica());
            miPosicion = posiciones.FABRICA;
        }
        else {
            Debug.Log("NOBLE TRABAJANDO");
            cansancio += 1;
        }
    }
    private ReturnValues haTrabajado()
    {
        if (miPosicion == posiciones.FABRICA)
        {
            return ReturnValues.Succeed;
        }
        else {
            return ReturnValues.Failed;
        }
        
    }

    void descansar()
    {
        if (miPosicion != posiciones.FABRICA)
        {
            agent.SetDestination(navPoints.goToFabrica());
            miPosicion = posiciones.FABRICA;
        }
        else {
            Debug.Log("NOBLE DESCANSANDO");
            cansancio -= 1;
        }
    }

    private ReturnValues haDescansado()
    {
        if (miPosicion == posiciones.FABRICA)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private ReturnValues resultadoFiesta()
    {
        return ReturnValues.Succeed;
    }

    private void fiestaSU()
    {
        if (miPosicion != posiciones.MANSIONNOBLE)
        {
            agent.SetDestination(navPoints.goToMansionNoble());
            miPosicion = posiciones.MANSIONNOBLE;
        }
        else
        {
            Debug.Log("NOBLE FIESTA");
        }
    }

    private ReturnValues comprobarFiesta()
    {
        if (simManager.dias % 3 == 0)
        {
            return ReturnValues.Succeed;
        }
        else {
            return ReturnValues.Failed;
        }
        
    }
    private void actionFiesta() { }

    private ReturnValues resultadoMerodear()
    {
        return ReturnValues.Succeed;
    }

    void merodearFSM()
    {
        if (miPosicion != posiciones.CALLE)
        {
            agent.SetDestination(new Vector3(-7f, 1f, -2f));
            miPosicion = posiciones.CALLE;
        }
        else {
            Debug.Log("NOBLE MERODEANDO");
        }
        
    }

    private ReturnValues resultadoDormir()
    {
        return ReturnValues.Succeed;
    }

    private void actionDormir()
    {
        //agent.SetDestination(new Vector3(19.5f, 1f, 13f));
        if (miPosicion != posiciones.MANSIONNOBLE)
        {
            agent.SetDestination(navPoints.goToMansionNoble());
            miPosicion = posiciones.MANSIONNOBLE;
        }
        else {
            Debug.Log("NOBLE MIMIDO");
            cansancio -= 1;
        }
        
    }


    private ReturnValues comprobarMorir()
    {
        return ReturnValues.Succeed;
    }

    private void actionMorir(){ }

    private ReturnValues comprobarSalud()
    {
        return ReturnValues.Succeed;
    }

    private void actionSalud() { }
    #endregion
}
