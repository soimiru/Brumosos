using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AlomantesMAL : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    private StateMachineEngine stateMachine;
    private LeafNode subFSM;
    string accion = "";
    string UItxt = "";

    #region variables Inquisidores
    private int salud = 100;
    private int metales = 100;
    private int diaNacimiento;
    #endregion variables Inquisidores

    #region variables Patrulla
    //public Transform target;
    private Transform targetSka;
    private bool patrullando = false;
    public float attackRadius = 30.0f;
    public Transform[] destinations;
    private int currentPoint = 0;
    [SerializeField] float timer;
    [SerializeField] float maxTime;
    [SerializeField] bool inRange;
    public Vector3[] destinos;
    private bool first = true;
    #endregion variables Patrulla

    #region estados
    private State patrullar;
    private State cazar;
    private State luchar;
    private State morir;
    private State huir;
    private State irAHogar;
    private State recargarMetales;
    #endregion estados

    #region percepciones
    private Perception saludBajaAlo;
    private Perception saludCompletaAlo;
    private Perception saludCeroAlo;
    private Perception probHuirAlo;
    private Perception metalesBajosAlo;
    private Perception metalesCompletosAlo;
    private Perception enemigosCercaAlo;
    private Perception enemigoAlcanzadoAlo;
    private Perception enemigoLejosAlo;
    private Perception timerAuxAlo;
    private Perception esNocheAlo;

    #endregion percepciones


    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Salud: " + salud + "\nMetales: " + metales + "\n" + accion;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(0, 0, new Color(0.7f, 0.75f, 1f, 0.5f));
        debugTex.Apply();
        style.normal.background = debugTex;
        style.fontSize = 30;

        //TAMAÑO Y POSICION
        Rect rect = new Rect(0, 0, 330, 140);
        Vector3 offset = new Vector3(0f, 0.5f, 0f); // height above the target position
        Vector3 point = Camera.main.WorldToScreenPoint(this.transform.position + offset);
        rect.x = point.x - 150;
        rect.y = Screen.height - point.y - rect.height; // bottom left corner set to the 3D point

        //MOSTRAR POR PANTALLA
        GUI.Label(rect, UItxt, style); // display its name, or other string
    }
    // Start is called before the first frame update
    private void Awake()
    {
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
       
        diaNacimiento = simManager.dias;
        behaviourTree = new BehaviourTreeEngine(BehaviourEngine.IsNotASubmachine);
        stateMachine = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        createSubFSM();
        createBT();
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        behaviourTree.Update();
        stateMachine.Update();
    }

    private void createSubFSM()
    {
        //Percepciones
        saludBajaAlo = stateMachine.CreatePerception<PushPerception>();
        saludCompletaAlo = stateMachine.CreatePerception<PushPerception>();
        saludCeroAlo = stateMachine.CreatePerception<PushPerception>();
        probHuirAlo = stateMachine.CreatePerception<PushPerception>();
        metalesBajosAlo = stateMachine.CreatePerception<PushPerception>();
        metalesCompletosAlo = stateMachine.CreatePerception<PushPerception>();
        enemigosCercaAlo = stateMachine.CreatePerception<PushPerception>();
        enemigoAlcanzadoAlo = stateMachine.CreatePerception<PushPerception>();
        enemigoLejosAlo = stateMachine.CreatePerception<PushPerception>();
        esNocheAlo = stateMachine.CreatePerception<PushPerception>();
        timerAuxAlo = stateMachine.CreatePerception<TimerPerception>(0.25f);
       // timerCaza = stateMachine.CreatePerception<TimerPerception>(0.25f);
        //golpearAuxP = stateMachine.CreatePerception<PushPerception>();



        //Estados
        patrullar = stateMachine.CreateEntryState("Patrullar", fsmPatrullar);
        cazar = stateMachine.CreateState("Cazar", fsmCazar);
        luchar = stateMachine.CreateState("Luchar", fsmLuchar);
        morir = stateMachine.CreateState("Morir", fsmMorir);
        huir = stateMachine.CreateState("Huir", fsmHuir);
        irAHogar = stateMachine.CreateState("irAHogar", fsmIrAHogar);
        recargarMetales = stateMachine.CreateState("RecargarMetales", fsmRecargarMetales);


        //Transiciones
        stateMachine.CreateTransition("Repatrullar", patrullar, timerAuxAlo, patrullar);


        stateMachine.CreateTransition("Enemigo Detectado", patrullar, enemigosCercaAlo, cazar);
        stateMachine.CreateTransition("Enemigo Lejos", cazar, enemigoLejosAlo, patrullar);
        stateMachine.CreateTransition("Cazando", cazar, timerAuxAlo, cazar);
        stateMachine.CreateTransition("Enemigo Alcanzado", cazar, enemigoAlcanzadoAlo, luchar);
        stateMachine.CreateTransition("Luchando", luchar, timerAuxAlo, luchar);
        stateMachine.CreateTransition("Morir", luchar, saludCeroAlo, morir);
        stateMachine.CreateTransition("Intento Huir", luchar, probHuirAlo, huir);
        stateMachine.CreateTransition("Huir", huir, timerAuxAlo, huir);
        stateMachine.CreateTransition("Huida exitosa", huir, enemigoLejosAlo, irAHogar);
        stateMachine.CreateTransition("Huida fallida", huir, enemigoAlcanzadoAlo, luchar);
        stateMachine.CreateTransition("Vuelta a Patrullar", irAHogar, saludCompletaAlo, patrullar);
        stateMachine.CreateTransition("Camino a casa", irAHogar, timerAuxAlo, irAHogar);
        stateMachine.CreateTransition("Recargar Metales", patrullar, metalesBajosAlo, recargarMetales);
        stateMachine.CreateTransition("Recargando metales", recargarMetales, timerAuxAlo, recargarMetales);
        stateMachine.CreateTransition("Metales Recargados", recargarMetales, metalesCompletosAlo, patrullar);


        //Entrada y salida de la FSM
        subFSM = behaviourTree.CreateSubBehaviour("Sub-FSM", stateMachine, patrullar);
        stateMachine.CreateExitTransition("Vuelta a BT", patrullar, esNocheAlo, ReturnValues.Succeed);
    }
    private void createBT()
    {

        //Nodos hoja
        LeafNode esDiaLeafNode = behaviourTree.CreateLeafNode("EsDia", actEsDia, compEsDia);
        LeafNode irACasita = behaviourTree.CreateLeafNode("IrACasita", actIrACasita, comprobarCasita);
        LeafNode recargarMetales = behaviourTree.CreateLeafNode("Dormir", actDormir, comprobarDormir);

        TimerDecoratorNode timerRecarga = behaviourTree.CreateTimerNode("TimerRecargaMetales", recargarMetales, 2);


        //Sequence node comprobar metales
        SequenceNode comprobarMetalesSequenceNode = behaviourTree.CreateSequenceNode("ComprobarMetalesSequenceNode", false);
        comprobarMetalesSequenceNode.AddChild(esDiaLeafNode);
        comprobarMetalesSequenceNode.AddChild(subFSM);

        LoopUntilFailDecoratorNode patrullarUntilFail = behaviourTree.CreateLoopUntilFailNode("PatrullarUntilFail", comprobarMetalesSequenceNode);

        SequenceNode irMinisterioRecargaSequenceNode = behaviourTree.CreateSequenceNode("IRMinisterioYRecargar", false);
        irMinisterioRecargaSequenceNode.AddChild(irACasita);
        irMinisterioRecargaSequenceNode.AddChild(timerRecarga);

        SequenceNode recargarMetalesSequenceNode = behaviourTree.CreateSequenceNode("RecargarMetalesSequenceNode", false);
        recargarMetalesSequenceNode.AddChild(patrullarUntilFail);
        recargarMetalesSequenceNode.AddChild(irMinisterioRecargaSequenceNode);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", recargarMetalesSequenceNode);
        behaviourTree.SetRootNode(rootNode);
    }

    #region Metodos BT
    private void actEsDia()
    {

    }
    private ReturnValues compEsDia()
    {
        Debug.Log("Compruebo si es de dia");
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            Debug.Log("Es de noche");
            return ReturnValues.Failed;
        }
    }

    
    private void actIrACasita()
    {
        accion = "Volviendo a casa";
        agent.SetDestination(new Vector3(21.5f, 1f, -4.5f));
    }
    private ReturnValues comprobarCasita()
    {
        //if (this.transform.position.x >= 21.0 && this.transform.position.x <= 22.0 && this.transform.position.z >= -5.0 && this.transform.position.z <= -4.0)
        //{
        //    return ReturnValues.Succeed;
        //}
        //else
        //{
        //    return ReturnValues.Running;
        //}
        return ReturnValues.Succeed;
    }
    private void actDormir()
    {
        accion = "Durmiendo";
    }
    private ReturnValues comprobarDormir()
    {
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion Metodos BT

    #region Metodos FSM
    private void fsmPatrullar()
    {
        patrullando = true;
        accion = "Patrullando";
        if (simManager.ciclo == SimulationManager.cicloDNA.NOCHE)
        {
            esNocheAlo.Fire();
        }
        if (metales <= 0)
        {
            metalesBajosAlo.Fire();
        }
        if (!inRange && this.transform.position.x == agent.destination.x && this.transform.position.z == agent.destination.z || first == true)
        {
            first = false;
            updateCurrentPoint();
            Vector3 newPos = destinos[currentPoint];
            agent.SetDestination(newPos);
            metales -= 5;
        }
    }
    
    private void fsmCazar()
    {
        patrullando = false;
        accion = "Cazando un Inquisidor";
    }
    private void fsmLuchar()
    {
        patrullando = false;
        accion = "Luchando";
        //Lucho
        
    }
    private void fsmMorir()
    {
        //Muero
    }
    private void fsmHuir()
    {
        patrullando = false;
        accion = "Huyendo";
    }
    private void fsmIrAHogar()
    {
        patrullando = false;
        accion = "De camino a casa";
    }
    private void fsmRecargarMetales()
    {
        patrullando = false;
        accion = "Recargando metales";
        metales += 10;
        if (metales>= 90)
        {
            metalesCompletosAlo.Fire();
        }
    }
    private void updateCurrentPoint()
    {
        if (currentPoint == destinos.Length - 1)
        {
            currentPoint = 0;
        }
        else
        {
            currentPoint++;
        }
    }


    #endregion Metodos FSM

    #region metodosColision
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (patrullando == true)
    //    {
    //        if (other.tag == "Skaa")
    //        {
    //            targetSka = other.transform;
    //            skaDescansandoDetectado.Fire();
    //        }
    //        else if(other.tag == "Brumoso") 
    //        {
    //            enemigoDetectado.Fire();
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
        
    //}
    #endregion metodosColision
}


