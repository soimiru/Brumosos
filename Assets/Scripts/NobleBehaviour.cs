using System;
using UnityEngine;
using UnityEngine.AI;

public class NobleBehaviour : MonoBehaviour
{
    private int diaNacimiento;
    int cansancio = 50;
    int salud = 100;
    string UItxt = "";

    private enum estados { ESTUDIAR, DORMIR };
    private bool adulto = false;
    private SimulationManager simManager;
    public NavMeshAgent agent;
    private NavigationPoints navPoints;
    public enum posiciones { FABRICA, MANSIONNOBLE, CALLE }
    public posiciones miPosicion;

    StateMachineEngine childFSM = new StateMachineEngine();
    BehaviourTreeEngine adultBT = new BehaviourTreeEngine();

    

    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Cansancio: " + cansancio + "\nSalud: " + salud;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(0, 0, new Color(1f, 1f,1f, 0.2f));
        style.normal.background = debugTex;
        style.fontSize = 30;

        //TAMAÑO Y POSICION
        Rect rect = new Rect(0, 0, 300, 100);
        Vector3 offset = new Vector3(0f, 0.5f, 0f); // height above the target position
        Vector3 point = Camera.main.WorldToScreenPoint(this.transform.position + offset);
        rect.x = point.x - 150;
        rect.y = Screen.height - point.y - rect.height; // bottom left corner set to the 3D point

        //MOSTRAR POR PANTALLA
        GUI.Label(rect, UItxt, style); // display its name, or other string
    }

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
        LeafNode irALaFabricaNode;
        LeafNode trabajarNode;
        LeafNode descansarNode;
      
        //DÍA
        esDeDiaNode = adultBT.CreateLeafNode("esDeDiaNode", actionDia, comprobarDia);
        irALaFabricaNode = adultBT.CreateLeafNode("irALaFabrica", actionIrFabrica, comprobarIrFabrica);
        trabajarNode = adultBT.CreateLeafNode("trabajarNode", trabajar, haTrabajado);
        descansarNode = adultBT.CreateLeafNode("descansarNode", descansar, haDescansado);
        
        descansoTrabajoSelector = adultBT.CreateSelectorNode("descansoTrabajoSelector");
        descansoTrabajoSelector.AddChild(trabajarNode);
        descansoTrabajoSelector.AddChild(descansarNode);

        diaSequence = adultBT.CreateSequenceNode("diaSequence", false);
        diaSequence.AddChild(esDeDiaNode);
        diaSequence.AddChild(irALaFabricaNode);
        diaSequence.AddChild(descansoTrabajoSelector);

        //NOCHE
        SequenceNode nocheSequence;
        SelectorNode nocheSelector;
        SequenceNode cansancioDormirSequence;
        SequenceNode fiestaSequence;

        LeafNode esDeNocheNode;
        LeafNode estoyCansadoNode;
        LeafNode dormirNode;
        LeafNode hayAlgunaFiestaNode;
        LeafNode fiestaNode;
        LeafNode merodearNode;
        

        esDeNocheNode = adultBT.CreateLeafNode("esDeNocheNode", actionNoche, comprobarNoche);
        estoyCansadoNode = adultBT.CreateLeafNode("estoyCansado", actionCansado, comprobarCansancio);
        dormirNode = adultBT.CreateLeafNode("dormir", actionDormir, resultadoDormir);
        hayAlgunaFiestaNode = adultBT.CreateLeafNode("hayAlgunaFiesta", actionFiesta, comprobarFiesta);
        fiestaNode = adultBT.CreateLeafNode("fiesta", fiestaSU, resultadoFiesta);
        merodearNode = adultBT.CreateLeafNode("merodear", merodearFSM, resultadoMerodear);

        cansancioDormirSequence = adultBT.CreateSequenceNode("cansancioDormir", false);
        cansancioDormirSequence.AddChild(estoyCansadoNode);
        cansancioDormirSequence.AddChild(dormirNode);

        fiestaSequence = adultBT.CreateSequenceNode("fiestaSequence", false);
        fiestaSequence.AddChild(hayAlgunaFiestaNode);
        fiestaSequence.AddChild(fiestaNode);

        nocheSelector = adultBT.CreateSelectorNode("nocheSelector");
        nocheSelector.AddChild(cansancioDormirSequence);
        nocheSelector.AddChild(fiestaSequence);
        nocheSelector.AddChild(merodearNode);
        

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
    #region COMPROBAR DIA/NOCHE
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
    #endregion

    #region METODOS DÍA

    //IR A LA FABRICA
    private ReturnValues comprobarIrFabrica()
    {
        if (navPoints.comprobarPosFabrica(this.transform.position))
        {
            Debug.Log("He llegado a la fábrica");
            miPosicion = posiciones.FABRICA;
            return ReturnValues.Succeed;
        }
        else {
            return ReturnValues.Running;
        }
    }

    private void actionIrFabrica()
    {
        agent.SetDestination(navPoints.goToFabrica());
    }

    //TRABAJAR
    void trabajar() {
        Debug.Log("NOBLE TRABAJANDO");
        cansancio += 2;
    }
    private ReturnValues haTrabajado()
    {
        return ReturnValues.Succeed;
    }

    //DESCANSAR
    void descansar()
    {
        Debug.Log("NOBLE DESCANSANDO");
        cansancio -= 1;
    }

    private ReturnValues haDescansado()
    {
        return ReturnValues.Succeed;
    }

    #endregion

    #region METODOS NOCHE

    //COMPROBAR CANSANCIO
    private ReturnValues comprobarCansancio()
    {
        if (cansancio >= 200)
        {
            return ReturnValues.Succeed;
        }
        else {
            return ReturnValues.Failed;
        }
    }

    private void actionCansado()
    {
        Debug.Log("Cansancio comprobado");
    }

    //DORMIR
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
        else
        {
            Debug.Log("NOBLE MIMIDO");
            cansancio -= 1;
        }

    }

    //COMPROBAR FIESTA
    private ReturnValues comprobarFiesta()
    {
        if (simManager.dias == 12 || simManager.dias == 15 || simManager.dias == 18)
        {
            Debug.Log("HAY UNA FIESTA!!!");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }

    }
    private void actionFiesta() { }

    //FIESTA
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

    //MERODEAR
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


    #endregion

    #region METODOS GENERALES
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

    #endregion
}
