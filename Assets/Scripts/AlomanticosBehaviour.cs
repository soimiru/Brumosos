using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AlomanticosBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    private StateMachineEngine stateMachine;
    private LeafNode subFSM;
    string accion = "";
    string UItxt = "";

    #region variables Alomanticos
    private int salud = 100;
    private int metales = 100;
    private int diaNacimiento;
    #endregion variables Alomanticos

    #region variables Patrulla
    private Transform target;
    private bool patrullando = false;
    private bool cazando = false;
    public float attackRadius = 30.0f;
    private int currentPoint = 0;
    private int escondite = 0;
    [SerializeField] bool inRange;
    public Vector3[] destinos;
    public Vector3[] escondrijos;
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
    private Perception enemigoDerrotado;
    private Perception timerAuxAlo;
    private Perception timerPatrol;
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
        enemigoDerrotado = stateMachine.CreatePerception<PushPerception>();
        esNocheAlo = stateMachine.CreatePerception<PushPerception>();
        timerAuxAlo = stateMachine.CreatePerception<TimerPerception>(0.5f);
        timerPatrol = stateMachine.CreatePerception<TimerPerception>(0.2f);



        //Estados
        patrullar = stateMachine.CreateEntryState("Patrullar", fsmPatrullar);
        cazar = stateMachine.CreateState("Cazar", fsmCazar);
        luchar = stateMachine.CreateState("Luchar", fsmLuchar);
        morir = stateMachine.CreateState("Morir", fsmMorir);
        huir = stateMachine.CreateState("Huir", fsmHuir);
        irAHogar = stateMachine.CreateState("irAHogar", fsmIrAHogar);
        recargarMetales = stateMachine.CreateState("RecargarMetales", fsmRecargarMetales);


        //Transiciones
        stateMachine.CreateTransition("Repatrullar", patrullar, timerPatrol, patrullar);


        stateMachine.CreateTransition("Enemigo Detectado", patrullar, enemigosCercaAlo, cazar);
        stateMachine.CreateTransition("Enemigo Lejos", cazar, enemigoLejosAlo, patrullar);
        stateMachine.CreateTransition("Cazando", cazar, timerAuxAlo, cazar);
        stateMachine.CreateTransition("Enemigo Alcanzado", cazar, enemigoAlcanzadoAlo, luchar);
        stateMachine.CreateTransition("Luchando", luchar, timerAuxAlo, luchar);
        stateMachine.CreateTransition("Morir", luchar, saludCeroAlo, morir);
        stateMachine.CreateTransition("Intento Huir", luchar, probHuirAlo, huir);
        stateMachine.CreateTransition("Enemigo Vencido", luchar, enemigoDerrotado, patrullar);
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
        LeafNode irACasitaLeafNode = behaviourTree.CreateLeafNode("IrACasita", actIrACasita, comprobarCasita);
        LeafNode dormirLeafNode = behaviourTree.CreateLeafNode("Dormir", actDormir, comprobarDormir);

        TimerDecoratorNode timerRecarga = behaviourTree.CreateTimerNode("TimerRecargaMetales", dormirLeafNode, 2);


        //Sequence node comprobar metales
        SequenceNode comprobarDiaSequenceNode = behaviourTree.CreateSequenceNode("ComprobarDiaSequenceNode", false);
        comprobarDiaSequenceNode.AddChild(esDiaLeafNode);
        comprobarDiaSequenceNode.AddChild(subFSM);

        LoopUntilFailDecoratorNode patrullarUntilFail = behaviourTree.CreateLoopUntilFailNode("PatrullarUntilFail", comprobarDiaSequenceNode);

        SequenceNode irCasaADormirSequenceNode = behaviourTree.CreateSequenceNode("irCasaADormirSequenceNode", false);
        irCasaADormirSequenceNode.AddChild(irACasitaLeafNode);
        irCasaADormirSequenceNode.AddChild(timerRecarga);

        SequenceNode rootSequenceNode = behaviourTree.CreateSequenceNode("RecargarMetalesSequenceNode", false);
        rootSequenceNode.AddChild(patrullarUntilFail);
        rootSequenceNode.AddChild(irCasaADormirSequenceNode);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", rootSequenceNode);
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
        agent.SetDestination(new Vector3(22.5f, 1f, -23f));
    }
    private ReturnValues comprobarCasita()
    {
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
        agent.speed = 3.5F;
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
        cazando = true;
        accion = "Cazando a un Inquisidor";
        float distTo = Vector3.Distance(transform.position, target.position);
        transform.LookAt(target);
        Vector3 moveTo = Vector3.MoveTowards(transform.position, target.position, 180f);
        agent.SetDestination(moveTo);

        if (distTo < 2)
        {
            enemigoAlcanzadoAlo.Fire();
        }

    }
    private void fsmLuchar()
    {
        patrullando = false;
        accion = "Luchando";
        //Lucho
        if (target == null)
        {
            first = true;
            enemigoDerrotado.Fire();
        }
        if (salud < 20)
        {
            int huida = Random.Range(1, 6);
            if (huida >= 1)
            {
                escondite = Random.Range(0, 3);
                Vector3 newPos = escondrijos[escondite];
                agent.SetDestination(newPos);
                probHuirAlo.Fire();
            }
        }
        if (metales >= 5)
        {
            metales -= Random.Range(2, 5);
            salud -= Random.Range(2, 5);
            if (metales <= 0)
            {
                metales = 0;
            }
        }
        else
        {
            salud -= Random.Range(5, 8);
            if (salud <=0)
            {
                salud = 0;
            }
        }
        if (salud <= 0)
        {
            saludCeroAlo.Fire();
        }

    }
    private void fsmMorir()
    {
        //Muero
        Destroy(this.gameObject);
    }
    private void fsmHuir()
    {
        patrullando = false;
        accion = "Huyendo";
        agent.speed = 5;
        if (this.transform.position.x >= agent.destination.x -0.5f && this.transform.position.x <= agent.destination.x + 0.5f && this.transform.position.z >= agent.destination.z - 0.5f && this.transform.position.z <= agent.destination.z + 0.5f)
        {
            enemigoLejosAlo.Fire();
        }
        
    }
    private void fsmIrAHogar()
    {
        patrullando = false;
        accion = "Estoy a salvo";
        salud += 10;
        metales += 10;
        if (salud >= 100 && metales>= 100)
        {
            first = true;
            salud = 100;
            metales = 100;
            saludCompletaAlo.Fire();
        }
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
        currentPoint = Random.Range(0, destinos.Length - 1);
    }


    #endregion Metodos FSM

    #region metodosColision
    private void OnTriggerEnter(Collider other)
    {
        if (patrullando == true)
        {
            if (other.tag == "Inquisidor")
            {
                target = other.transform;
                enemigosCercaAlo.Fire();
            }
        }
    }

    #endregion metodosColision
}


