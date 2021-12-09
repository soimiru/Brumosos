using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NobleBehaviour : MonoBehaviour
{
    [Header("Variables NOBLES")]
    int diaNacimiento;
    int cansancio = 0;
    int salud = 100;
    int ebriedad = 0;
    float ganasReproducirse = 1;
    string accion = "";
    string UItxt = "";
    bool fiesta = false;
    bool adulto = false;
    enum posiciones { FABRICA, MANSIONNOBLE, CALLE }
    posiciones miPosicion;
    
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private NavigationPoints navPoints;

    #region variables Patrulla
    private Transform target;
    private bool patrullando = false;
    private bool first = true;
    private int currentPoint = 0;
    public Vector3[] destinos;
    #endregion variables Patrulla

    [Header("Behaviour Engines")]
    StateMachineEngine childFSM = new StateMachineEngine();
    BehaviourTreeEngine adultBT = new BehaviourTreeEngine();
    LeafNode fiestaNode;
    LeafNode merodearFSMNode;
    UtilitySystemEngine partyUS = new UtilitySystemEngine(BehaviourEngine.IsASubmachine);   
    StateMachineEngine partyFSM = new StateMachineEngine(BehaviourEngine.IsASubmachine);
    StateMachineEngine merodearFSM = new StateMachineEngine(BehaviourEngine.IsASubmachine);
    State irALaFiestaState;
    Perception heLlegadoALaFiesta;
    Perception ebriedadBaja;
    Perception ebriedadMedia;
    Perception cansancioAlto;
    Perception nobleCerca;
    Perception esDeDia;
    Perception timer10;
    Perception timer15;
    Perception timer20;
    Perception aux;

    //Perceptions Merodear
    Perception seHaceDia;
    Perception encuentroSkaa;
    Perception timerAux;
    Perception reproduccionCompleta;
    Perception estoyCasa;

    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Cansancio: " + cansancio + "\nSalud: " + salud+ "\nEbriedad: " + ebriedad+ "\n" + accion;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(1, 1, new Color(1f, 0.7f,0.7f, 0.5f));
        debugTex.Apply();
        style.normal.background = debugTex;
        style.fontSize = 30;

        //TAMAÑO Y POSICION
        Rect rect = new Rect(0, 0, 330, 140);
        Vector3 offset = new Vector3(0f, 0.5f, 0f); //Altura respecto al objetivo
        Vector3 point = Camera.main.WorldToScreenPoint(this.transform.position + offset);
        rect.x = point.x - 150;
        rect.y = Screen.height - point.y - rect.height; // Esquina inferior izquierda

        //MOSTRAR POR PANTALLA
        GUI.Label(rect, UItxt, style);
    }

    private void Awake()
    {
        this.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        navPoints = new NavigationPoints();
        diaNacimiento = simManager.dias;
    }

    void Start()
    {
        createFSMChild();
    }

    void Update()
    {
        if (!adulto) {
            FSMChild();
            if (diaNacimiento + 3 == simManager.dias)
            {
                crecerAction();
            }
        }
        if (adulto) {
            partyFSM.Update();
            merodearFSM.Update();
            adultBT.Update();
        }
        
    }

    #region FSM CHILD
    void createFSMChild() {
        //ESTADOS
        State estudiarState = childFSM.CreateEntryState("Estudiar", estudiarAction);
        State dormirState = childFSM.CreateState("Dormir", dormirAction);

        //PERCEPCIONES
        Perception nacimiento = childFSM.CreatePerception<TimerPerception>(1);
        Perception hacerNoche = childFSM.CreatePerception<PushPerception>();
        Perception hacerDia = childFSM.CreatePerception<PushPerception>();

        //TRANSICIONES
        childFSM.CreateTransition("Dormir", estudiarState, hacerNoche, dormirState);
        childFSM.CreateTransition("Estudiar", dormirState, hacerDia, estudiarState);
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

    #endregion

    #region US FIESTA -no utilizado-
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
    #endregion

    #region BT ADULTO
    void createBTAdult() {

        //DÍA
        SequenceNode diaSequence;
        SelectorNode descansoTrabajoSelector;
        SequenceNode comprobarDescanso;
        SequenceNode trabajarYTimer;

        LeafNode esDeDiaNode;
        LeafNode irALaFabricaNode;
        LeafNode trabajarNode;
        LeafNode descansarNode;
        LeafNode comprobarDescansoNode;

        LeafNode timeDescansarLeafNode = adultBT.CreateLeafNode("TimerDescansar", actTimer, comprobarTimer);
        LeafNode timeTrabajarLeafNode = adultBT.CreateLeafNode("TimerTrabajar", actTimer, comprobarTimer);

        TimerDecoratorNode timerNodeDescansar = adultBT.CreateTimerNode("TimerNodeDescansar", timeDescansarLeafNode, 2);
        TimerDecoratorNode timerNodeTrabajar = adultBT.CreateTimerNode("TimerNodeTrabajar", timeTrabajarLeafNode, 2);


        esDeDiaNode = adultBT.CreateLeafNode("esDeDiaNode", actionDia, comprobarDia);
        irALaFabricaNode = adultBT.CreateLeafNode("irALaFabrica", actionIrFabrica, comprobarIrFabrica);
        trabajarNode = adultBT.CreateLeafNode("trabajarNode", trabajar, haTrabajado);
        descansarNode = adultBT.CreateLeafNode("descansarNode", descansar, haDescansado);
        comprobarDescansoNode = adultBT.CreateLeafNode("comprobarDescansoNode", actComprobarDescanso, compDescanso);

        trabajarYTimer = adultBT.CreateSequenceNode("TrabajarYTimer", false);
        trabajarYTimer.AddChild(trabajarNode);
        trabajarYTimer.AddChild(timerNodeTrabajar);

        comprobarDescanso = adultBT.CreateSequenceNode("ComprobarDescansoSequence", false);
        comprobarDescanso.AddChild(comprobarDescansoNode);
        comprobarDescanso.AddChild(descansarNode);
        comprobarDescanso.AddChild(timerNodeDescansar);

        descansoTrabajoSelector = adultBT.CreateSelectorNode("descansoTrabajoSelector");
        descansoTrabajoSelector.AddChild(comprobarDescanso);
        descansoTrabajoSelector.AddChild(trabajarYTimer);
        

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
        LeafNode merodearNode;

        esDeNocheNode = adultBT.CreateLeafNode("esDeNocheNode", actionNoche, comprobarNoche);
        estoyCansadoNode = adultBT.CreateLeafNode("estoyCansado", actionCansado, comprobarCansancio);
        dormirNode = adultBT.CreateLeafNode("dormir", actionDormir, resultadoDormir);
        hayAlgunaFiestaNode = adultBT.CreateLeafNode("hayAlgunaFiesta", actionFiesta, comprobarFiesta);
        merodearNode = adultBT.CreateLeafNode("merodear", merodearFSMact, resultadoMerodear);

        cansancioDormirSequence = adultBT.CreateSequenceNode("cansancioDormir", false);
        cansancioDormirSequence.AddChild(estoyCansadoNode);
        cansancioDormirSequence.AddChild(dormirNode);

        fiestaSequence = adultBT.CreateSequenceNode("fiestaSequence", false);
        fiestaSequence.AddChild(hayAlgunaFiestaNode);
        fiestaSequence.AddChild(fiestaNode);

        nocheSelector = adultBT.CreateSelectorNode("nocheSelector");
        nocheSelector.AddChild(cansancioDormirSequence);
        nocheSelector.AddChild(fiestaSequence);
        nocheSelector.AddChild(merodearFSMNode);

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
    #endregion

    #region FSM FIESTA
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

        aux = partyFSM.CreatePerception<PushPerception>();
        esDeDia = partyFSM.CreatePerception<PushPerception>();
        nobleCerca = partyFSM.CreatePerception<PushPerception>();
        timer10 = partyFSM.CreatePerception<TimerPerception>(1);
        timer15 = partyFSM.CreatePerception<TimerPerception>(1);
        timer20 = partyFSM.CreatePerception<TimerPerception>(2);

        //TRANSICIONES
        partyFSM.CreateTransition("NeutroFiesta", irALaFiestaState, heLlegadoALaFiesta, neutralFiestaState);
        partyFSM.CreateTransition("ABeber", neutralFiestaState, ebriedadBaja, beberState);
        partyFSM.CreateTransition("VolverNeutroBeber", beberState, timer10, neutralFiestaState);
        partyFSM.CreateTransition("ABailar", neutralFiestaState, ebriedadMedia, bailarState);
        partyFSM.CreateTransition("VolverNeutroBailar", bailarState, timer15, neutralFiestaState);
        partyFSM.CreateTransition("A Reproducirse", neutralFiestaState, nobleCerca, reproducirseState);
        partyFSM.CreateTransition("A ReproducirseBeber", beberState, nobleCerca, reproducirseState);
        partyFSM.CreateTransition("A ReproducirseBailar", bailarState, nobleCerca, reproducirseState);
        partyFSM.CreateTransition("VolverNeutroReprod", reproducirseState, timer20, neutralFiestaState);
        partyFSM.CreateTransition("IrseDia", neutralFiestaState, esDeDia, irseState);
        partyFSM.CreateTransition("IrseCansado", neutralFiestaState, cansancioAlto, irseState);

        fiestaNode = adultBT.CreateSubBehaviour("FiestaUS", partyFSM, irALaFiestaState);
        partyFSM.CreateExitTransition("Volver al BT", irseState, aux, ReturnValues.Succeed);
    }

    #endregion

    #region METODOS CHILD
    void estudiarAction()
    {
        agent.SetDestination(navPoints.goToFabrica());
        accion = "Estudiando";
    }
    void dormirAction()
    {
        agent.SetDestination(navPoints.goToMansionNoble());
        accion = "Durmiendo";
    }
    void nacerAction()
    {
        
    }
    void crecerAction()
    {
        transform.localScale = new Vector3(1, 1, 1);
        adulto = true;
        createMerodearFSM();
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
        cansancio += 10;
    }
    private ReturnValues haTrabajado()
    {
        return ReturnValues.Succeed;
    }

    //DESCANSAR
    void descansar()
    {
        accion = "Descansando de trabajar";
        cansancio -= 50;
    }

    private ReturnValues haDescansado()
    {
        return ReturnValues.Succeed;
    }

    void actComprobarDescanso()
    {

    }

    private ReturnValues compDescanso()
    {
        if (cansancio >= 60)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
        
    }
    private void actTimer()
    {
        cansancio -= 0;
    }
    private ReturnValues comprobarTimer()
    {
        return ReturnValues.Succeed;
    }

    #endregion

    #region METODOS NOCHE

    //COMPROBAR CANSANCIO
    private ReturnValues comprobarCansancio()
    {
        if (cansancio >= 15)
        {
            return ReturnValues.Succeed;
        }
        else {
            return ReturnValues.Failed;
        }
    }

    private void actionCansado()
    {
    }

    //DORMIR
    private ReturnValues resultadoDormir()
    {
        return ReturnValues.Succeed;
    }

    private void actionDormir()
    {
        if (miPosicion != posiciones.MANSIONNOBLE)
        {
            accion = "Yendo a dormir";
            agent.SetDestination(navPoints.goToMansionNoble());
            miPosicion = posiciones.MANSIONNOBLE;
        }
        else
        {
            accion = "Durmiendo";
            cansancio = 0;
            ebriedad = 0;
        }

    }

    //COMPROBAR FIESTA
    private ReturnValues comprobarFiesta()
    {
        if (simManager.ComprobarFiesta())
        {
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

    void merodearFSMact()
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

    private void OnCollisionStay(Collision collision)
    {
        if (fiesta == true && ganasReproducirse == 1)
        {
            nobleCerca.Fire();
        }
    }

    void irALaFiesta() {
        //agent.SetDestination(navPoints.goToMansionNoble());
        agent.SetDestination(new Vector3(12f, 1, 15f));
        miPosicion = posiciones.MANSIONNOBLE;
        accion = "Yendo a la fiesta";
        if (navPoints.comprobarPosMansionNoble(this.transform.position) && miPosicion != posiciones.MANSIONNOBLE)
        {
            miPosicion = posiciones.MANSIONNOBLE;
            heLlegadoALaFiesta.Fire();
        }
    }

    void estarEnLaFiesta() {
        fiesta = true;
        accion = "De fiesta";
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA) {
            esDeDia.Fire();
        }
    }

    void beberAction() {
        ebriedad += 10;
        accion = "Bebiendo";
        if (ebriedad >= 30) {
            ganasReproducirse = 1;
        }
    }
    void bailarAction() {
        accion = "Bailando";
        cansancio += 5;
        ebriedad += 5;
        if (ebriedad >= 30)
        {
            ganasReproducirse = 1;
        }
    }
    void reproducirseAction() {
        accion = "Reproduciendo";
        Instantiate(this.transform.gameObject);
        ganasReproducirse = 0;
    }
    void irseAction()
    {
        aux.Fire();
        accion = "Me voy de la fiesta";
        fiesta = false;
    }
    #endregion

    #region METODOS GENERALES
    private ReturnValues comprobarMorir()
    {
        return ReturnValues.Succeed;
    }

    private void actionMorir(){
        //Destroy(this);
    }

    private ReturnValues comprobarSalud()
    {
        if ((diaNacimiento + 25) <= simManager.dias) {
            salud = 0;
        }
        if (salud <= 0)
        {
            Debug.Log("MY TIME HAS COME");
            Destroy(this);
            return ReturnValues.Failed;
        }
        else {
            return ReturnValues.Succeed;
        }
        
    }

    private void actionSalud() { }
    #endregion

    #region FSM Merodear
    private void createMerodearFSM()
    {
        //Percepciones
        seHaceDia = merodearFSM.CreatePerception<PushPerception>();
        encuentroSkaa = merodearFSM.CreatePerception<PushPerception>();
        reproduccionCompleta = merodearFSM.CreatePerception<PushPerception>();
        estoyCasa = merodearFSM.CreatePerception<PushPerception>();
        timerAux = merodearFSM.CreatePerception<TimerPerception>(0.5f);

        //Estados
        State buscandoSkaa = merodearFSM.CreateEntryState("Buscando Skaa", buscarSkaa);
        State reproducirseConSkaa = merodearFSM.CreateState("Reproducirse con Skaa", reproducirseConSkaaAct);
        State volverACasa = merodearFSM.CreateState("Volver a casa", volverACasaAct);

        //Transiciones
        merodearFSM.CreateTransition("ReBuscar Skaa", buscandoSkaa, timerAux, buscandoSkaa);
        merodearFSM.CreateTransition("Skaa encontrado", buscandoSkaa, encuentroSkaa, reproducirseConSkaa);
        merodearFSM.CreateTransition("A por el Skaa", reproducirseConSkaa, timerAux, reproducirseConSkaa);
        merodearFSM.CreateTransition("Reproduccion completa, para casa", reproducirseConSkaa, reproduccionCompleta, volverACasa);
        merodearFSM.CreateTransition("Se hizo de dia", buscandoSkaa, seHaceDia, volverACasa);

        merodearFSMNode = adultBT.CreateSubBehaviour("Entrada a sub FSM", merodearFSM, buscandoSkaa);
        merodearFSM.CreateExitTransition("Vuelta a casa", volverACasa, estoyCasa, ReturnValues.Succeed);
    }


    private void buscarSkaa()
    {
        patrullando = true;
        accion = "Buscando un Skaa";
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            seHaceDia.Fire();
        }
        if (this.transform.position.x >= agent.destination.x - 0.5f && this.transform.position.x <= agent.destination.x + 0.5f && this.transform.position.z >= agent.destination.z - 0.5f && this.transform.position.z <= agent.destination.z + 0.5f || first == true)
        {
            first = false;
            updateCurrentPoint();
            Vector3 newPos = destinos[currentPoint];
            agent.SetDestination(newPos);
        }
    }
    private void reproducirseConSkaaAct()
    {
        patrullando = false;
        accion = "Yendo a reproducirme";
        float distTo = Vector3.Distance(transform.position, target.position);
        transform.LookAt(target);
        Vector3 moveTo = Vector3.MoveTowards(transform.position, target.position, 180f);
        agent.SetDestination(moveTo);
        if (distTo < 2)
        {
            int hijo = UnityEngine.Random.Range(1, 4);
            if (hijo == 1)
            {
                simManager.InstanciarAlomantico();
            }
            else
            {
                simManager.InstanciarSkaa();
            }
        }
    }
    private void volverACasaAct()
    {
        patrullando = false;
        accion = "Volviendo a casa";
        agent.SetDestination(navPoints.goToMansionNoble());
        estoyCasa.Fire();
    }
    private void updateCurrentPoint()
    {
        
        currentPoint = UnityEngine.Random.Range(0, 6);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (patrullando == true)
        {
            if (other.tag == "Skaa")
            {
                encuentroSkaa.Fire();
            }
        }
    }




    #endregion FSM Merodear

    #endregion
}
