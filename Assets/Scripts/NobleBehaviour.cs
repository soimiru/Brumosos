using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NobleBehaviour : MonoBehaviour
{
    private int diaNacimiento;
    float cansancio = 0;
    int salud = 100;
    int ebriedad = 0;
    float ganasReproducirse = 1;
    string accion = "";
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
    LeafNode fiestaNode;
    UtilitySystemEngine partyUS = new UtilitySystemEngine(BehaviourEngine.IsASubmachine);    //true porque es submáquina
    StateMachineEngine partyFSM = new StateMachineEngine(BehaviourEngine.IsASubmachine);
    State irALaFiestaState;
    Perception heLlegadoALaFiesta;
    Perception ebriedadBaja;
    Perception ebriedadMedia;
    Perception cansancioAlto;
    Perception nobleCerca;
    Perception esDeDia;
    Perception timer10;
    Perception timer10b;
    Perception timer15;
    Perception timer20;
    Perception aux;




    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Cansancio: " + cansancio + "\nSalud: " + salud+ "\nEbriedad: " + ebriedad+ "\n" + accion;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(0, 0, new Color(1f, 1f,1f, 0.2f));
        style.normal.background = debugTex;
        style.fontSize = 30;

        //TAMAÑO Y POSICION
        Rect rect = new Rect(0, 0, 300, 200);
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

            //partyUS.Update();
            partyFSM.Update();
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

    int getGanasReproducirse() {
        if (ganasReproducirse > 0.8)
        {
            return 1;
        }
        else {
            return 0;
        }
    }

    void createUSParty(){

        //EBRIEDAD
        Factor ebriedadVariables = new LeafVariable(()=> ebriedad, 0, 50); 
        Factor ebriedadExp = new ExpCurve(ebriedadVariables, 0.5f); //CURVA EXPONENCIAL
        partyUS.CreateUtilityAction("beber", beberAction, ebriedadExp);


        //CANSANCIO^+EBRIEDAD^ = IRSE
        Factor cansancioVariables = new LeafVariable(() => cansancio, 0, 100);
        Factor cansancioExp = new ExpCurve(cansancioVariables, -0.5f);  //CURVA EXPONENCIAL
        List<Factor> cansancioEbriedadIRSE = new List<Factor>();    //FACTORES
        cansancioEbriedadIRSE.Add(ebriedadExp);
        cansancioEbriedadIRSE.Add(cansancioExp);
        List<float> pesosCEI = new List<float>();   //PESOS
        pesosCEI.Add(0.3f);
        pesosCEI.Add(0.7f);
        Factor cansEbrIRSESum = new WeightedSumFusion(cansancioEbriedadIRSE, pesosCEI); //FUSION PONDERADA
        partyUS.CreateUtilityAction("irse", irseAction, cansEbrIRSESum);    //ACCIÓN


        //CANSANCIOv+EBRIEDAD^ = BAILAR
        //Nos sirven las variables de arriba?
        List<float> pesosCEB = new List<float>();   //PESOS
        pesosCEB.Add(0.8f);
        pesosCEB.Add(0.2f);
        Factor cansEbrBAILARSum = new WeightedSumFusion(cansancioEbriedadIRSE, pesosCEB);
        partyUS.CreateUtilityAction("bailar", bailarAction, cansEbrBAILARSum);


        //EBRIEDAD^+GANASREPRODUCIRSE^ = REPRODUCIRSE
        Factor reproducirseVariables = new LeafVariable(() => getGanasReproducirse(), 0, 1);
        Factor reproLinear = new LinearCurve(reproducirseVariables);
        List<Factor> reproEbriedadREP = new List<Factor>(); //FACTORES
        reproEbriedadREP.Add(reproLinear);
        reproEbriedadREP.Add(ebriedadExp);
        List<float> pesosRepro = new List<float>(); //PESOS
        pesosRepro.Add(0.9f);
        pesosRepro.Add(0.1f);
        Factor reproSum = new WeightedSumFusion(reproEbriedadREP, pesosRepro);
        partyUS.CreateUtilityAction("reproducirse", reproducirseAction, reproSum);  //ACCIÓN

        
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
        //fiestaNode arriba para poder usarlo en el SU
        LeafNode merodearNode;
        

        esDeNocheNode = adultBT.CreateLeafNode("esDeNocheNode", actionNoche, comprobarNoche);
        estoyCansadoNode = adultBT.CreateLeafNode("estoyCansado", actionCansado, comprobarCansancio);
        dormirNode = adultBT.CreateLeafNode("dormir", actionDormir, resultadoDormir);
        hayAlgunaFiestaNode = adultBT.CreateLeafNode("hayAlgunaFiesta", actionFiesta, comprobarFiesta);
        //fiestaNode = adultBT.CreateLeafNode("fiesta", fiestaSU, resultadoFiesta);
        merodearNode = adultBT.CreateLeafNode("merodear", merodearFSM, resultadoMerodear);

        cansancioDormirSequence = adultBT.CreateSequenceNode("cansancioDormir", false);
        cansancioDormirSequence.AddChild(estoyCansadoNode);
        cansancioDormirSequence.AddChild(dormirNode);

        fiestaSequence = adultBT.CreateSequenceNode("fiestaSequence", false);
        fiestaSequence.AddChild(hayAlgunaFiestaNode);
        //fiestaNode = adultBT.CreateSubBehaviour("FiestaUS", partyFSM, irALaFiestaState);   //TRANSICION?
        fiestaSequence.AddChild(fiestaNode);
        


        //BehaviourTreeStatusPerception perceptionUSBT = partyUS.CreatePerception<BehaviourTreeStatusPerception>(adultBT, ReturnValues.Succeed);
        //adultBT.CreateExitTransition("e", fiestaNode.StateNode, perceptionUSBT, entradaState);
        

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

    void createPartyFSM() {
        //ESTADOS
        irALaFiestaState = partyFSM.CreateEntryState("IrALaFiesta", irALaFiesta);
        State neutralFiestaState = partyFSM.CreateState("Fiesta", estarEnLaFiesta);  //Estado neutral donde se comprueban las cosas?
        State beberState = partyFSM.CreateState("Beber", beberAction);
        State bailarState = partyFSM.CreateState("Bailar", bailarAction);
        State reproducirseState = partyFSM.CreateState("Reproducirse", reproducirseAction);
        State irseState = partyFSM.CreateState("Irse", irseAction);

        //PERCEPCIONES
        heLlegadoALaFiesta = partyFSM.CreatePerception<ValuePerception>(() => navPoints.comprobarPosMansionNoble(this.transform.position));
        ebriedadBaja = partyFSM.CreatePerception<ValuePerception>(() => ebriedad <= 20);
        ebriedadMedia = partyFSM.CreatePerception<ValuePerception>(() => ebriedad > 20 && cansancio < 50);
        cansancioAlto = partyFSM.CreatePerception<ValuePerception>(() => cansancio > 80);
        //heLlegadoALaFiesta = partyFSM.CreatePerception<PushPerception>();
        //ebriedadBaja = partyFSM.CreatePerception<PushPerception>();
        //ebriedadMedia = partyFSM.CreatePerception<PushPerception>();
        //cansancioAlto = partyFSM.CreatePerception<PushPerception>();

        aux = partyFSM.CreatePerception<PushPerception>();
        esDeDia = partyFSM.CreatePerception<PushPerception>();
        nobleCerca = partyFSM.CreatePerception<PushPerception>();
        timer10 = partyFSM.CreatePerception<TimerPerception>(10);
        timer15 = partyFSM.CreatePerception<TimerPerception>(15);
        timer20 = partyFSM.CreatePerception<TimerPerception>(20);

        //TRANSICIONES
        partyFSM.CreateTransition("NeutroFiesta", irALaFiestaState, heLlegadoALaFiesta, neutralFiestaState);
        partyFSM.CreateTransition("ABeber", neutralFiestaState, ebriedadBaja, beberState);
        partyFSM.CreateTransition("VolverNeutroBeber", beberState, timer10, neutralFiestaState);
        partyFSM.CreateTransition("ABailar", neutralFiestaState, ebriedadMedia, bailarState);
        partyFSM.CreateTransition("VolverNeutroBailar", bailarState, timer15, neutralFiestaState);
        partyFSM.CreateTransition("A Reproducirse", neutralFiestaState, nobleCerca, reproducirseState);
        partyFSM.CreateTransition("VolverNeutroReprod", reproducirseState, timer20, neutralFiestaState);
        partyFSM.CreateTransition("IrseDia", neutralFiestaState, esDeDia, irseState);
        partyFSM.CreateTransition("IrseCansado", neutralFiestaState, cansancioAlto, irseState);

        fiestaNode = adultBT.CreateSubBehaviour("FiestaUS", partyFSM, irALaFiestaState);
        partyFSM.CreateExitTransition("Volver al BT", irseState, aux, ReturnValues.Succeed);
    }

    #region METODOS CHILD
    void estudiarAction()
    {
        agent.SetDestination(new Vector3(-19.5f, 1f, 14f));
        accion = "Estudiando";
    }
    void dormirAction()
    {
        agent.SetDestination(new Vector3(19f, 1f, 19f));
        accion = "Durmiendo";
    }
    void nacerAction()
    {
        
    }
    void crecerAction()
    {
        transform.localScale = new Vector3(1, 1, 1);
        adulto = true;
        //Debug.Log("He cresido");
        createPartyFSM();
        createUSParty();
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
            accion = "He llegado a la fábrica";
            miPosicion = posiciones.FABRICA;
            return ReturnValues.Succeed;
        }
        else {
            accion = "Estoy yendo a la fábrica";
            return ReturnValues.Running;
        }
    }

    private void actionIrFabrica()
    {
        agent.SetDestination(navPoints.goToFabrica());
    }

    //TRABAJAR
    void trabajar() {
        accion = "Trabajando";
        cansancio += 0.1f;
    }
    private ReturnValues haTrabajado()
    {
        return ReturnValues.Succeed;
    }

    //DESCANSAR
    void descansar()
    {
        accion = "Descansando de trabajar";
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
            accion = "Yendo a dormir";
            agent.SetDestination(navPoints.goToMansionNoble());
            miPosicion = posiciones.MANSIONNOBLE;
        }
        else
        {
            accion = "Durmiendo";
            cansancio -= 20;
            ebriedad = 0;
        }

    }

    //COMPROBAR FIESTA
    private ReturnValues comprobarFiesta()
    {
        if (simManager.ComprobarFiesta())
        {
            Debug.Log("HAY UNA FIESTA!!!");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }

    }
    private void actionFiesta() {
        
    }

    //FIESTA
    private ReturnValues resultadoFiesta()
    {
        return ReturnValues.Succeed;
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
            accion = "Yendo a merodear";
            agent.SetDestination(new Vector3(-7f, 1f, -2f));
            miPosicion = posiciones.CALLE;
        }
        else {
            accion = "Merodeando";
        }
        
    }


    #endregion

    #region PARTY

    void irALaFiesta() {
        agent.SetDestination(navPoints.goToMansionNoble());
        miPosicion = posiciones.MANSIONNOBLE;
        accion = "Yendo a la fiesta";
        if (navPoints.comprobarPosMansionNoble(this.transform.position) && miPosicion != posiciones.MANSIONNOBLE)
        {
            miPosicion = posiciones.MANSIONNOBLE;
            heLlegadoALaFiesta.Fire();
        }
    }

    void estarEnLaFiesta() {
        accion = "De fiesta";
        
        //if (ebriedad <= 20) {
        //    ebriedadBaja.Fire();
        //}
        //if (ebriedad > 20 && cansancio < 50) {
        //    ebriedadMedia.Fire();
        //}
        //if (cansancio > 80) {
        //    cansancioAlto.Fire();
        //}
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA) {
            esDeDia.Fire();
        }
        //timer10b.Fire();
    }

    void beberAction() {
        ebriedad += 10;
        accion = "Bebiendo";
        //timer10.Fire();
    }
    void bailarAction() {
        accion = "Bailando";
        cansancio += 10;
        ebriedad += 5;
        //timer15.Fire();
    }
    void reproducirseAction() {
        accion = "Reproduciendo";
        //timer20.Fire();
    }
    void irseAction()
    {
        aux.Fire();
        accion = "Me voy de la fiesta";
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
