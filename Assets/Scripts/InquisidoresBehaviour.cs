using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InquisidoresBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    private StateMachineEngine stateMachine;
    private LeafNode subFSM;
    string UItxt = "";

    #region variables Inquisidores
    private int salud = 100;
    private int metales = 100;
    private int diaNacimiento;
    #endregion variables Inquisidores

    #region variables Patrulla
    public Transform target;
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
    private State golpear;
    private State cazar;
    private State luchar;
    private State morir;
    private State aux;
    #endregion estados

    #region percepciones
    private Perception skaDescansandoDetectado;
    private Perception skaGolpeado;
    private Perception enemigoDetectado;
    private Perception enemigoAlcanzado;
    private Perception enemigoFueraDeRango;
    private Perception enemigoPerdido;
    private Perception luchaPerdida;
    private Perception enemigoDerrotado;
    private Perception metalesBajos;
    private Perception patrullaCompleta;
    private Perception timerAux;

    #endregion percepciones


    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Salud: " + salud + "\nMetales: " + metales;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.2f));
        style.normal.background = debugTex;
        style.fontSize = 30;

        //TAMA�O Y POSICION
        Rect rect = new Rect(0, 0, 300, 100);
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
        skaDescansandoDetectado = stateMachine.CreatePerception<PushPerception>();
        skaGolpeado = stateMachine.CreatePerception<PushPerception>();
        enemigoDetectado = stateMachine.CreatePerception<PushPerception>();
        enemigoAlcanzado = stateMachine.CreatePerception<PushPerception>();
        enemigoFueraDeRango = stateMachine.CreatePerception<PushPerception>();
        enemigoPerdido = stateMachine.CreatePerception<PushPerception>();
        luchaPerdida = stateMachine.CreatePerception<PushPerception>();
        enemigoDerrotado = stateMachine.CreatePerception<PushPerception>();
        metalesBajos = stateMachine.CreatePerception<PushPerception>();
        patrullaCompleta = stateMachine.CreatePerception<PushPerception>();
        timerAux = stateMachine.CreatePerception<TimerPerception>(0.5f);


        //Estados
        patrullar = stateMachine.CreateEntryState("Patrullar", fsmPatrullar);
        golpear = stateMachine.CreateState("Golpear", fsmGolpear);
        cazar = stateMachine.CreateState("Cazar", fsmCazar);
        luchar = stateMachine.CreateState("Luchar", fsmLuchar);
        morir = stateMachine.CreateState("Morir", fsmMorir);
        aux = stateMachine.CreateState("Aux", fsmAux);

        //Transiciones
        stateMachine.CreateTransition("Ska Detectado", patrullar, skaDescansandoDetectado, golpear);
        stateMachine.CreateTransition("Repatrullar", patrullar, patrullaCompleta, aux);
        stateMachine.CreateTransition("Timer Aux", aux, timerAux, patrullar);

        stateMachine.CreateTransition("Ska Golpeado", golpear, skaGolpeado, patrullar);
        stateMachine.CreateTransition("Enemigo Detectado", patrullar, enemigoDetectado, cazar);
        stateMachine.CreateTransition("Enemigo Perdido", cazar, enemigoPerdido, patrullar);
        stateMachine.CreateTransition("Enemigo Alcanzado", cazar, enemigoAlcanzado, luchar);
        stateMachine.CreateTransition("Enemigo Fuera Rango", luchar, enemigoFueraDeRango, cazar);
        stateMachine.CreateTransition("Enemigo Derrotado", luchar, enemigoDerrotado, patrullar);
        stateMachine.CreateTransition("Lucha Perdida", luchar, luchaPerdida, morir);


        //Entrada y salida de la FSM
        subFSM = behaviourTree.CreateSubBehaviour("Sub-FSM", stateMachine, patrullar);
        stateMachine.CreateExitTransition("Vuelta a BT", aux, metalesBajos, ReturnValues.Succeed);
    }
    private void createBT()
    {

        //Nodos hoja
        LeafNode tengoMetalesLeafNode = behaviourTree.CreateLeafNode("TengoMetales", actTengoMetales, compMetales);
        LeafNode patrullarLeafNode1 = behaviourTree.CreateLeafNode("Patrullar1", actPatrullar1, compPatrullar1);
        LeafNode patrullarLeafNode2 = behaviourTree.CreateLeafNode("Patrullar2", actPatrullar2, compPatrullar2);
        LeafNode patrullarLeafNode3 = behaviourTree.CreateLeafNode("Patrullar3", actPatrullar3, compPatrullar3);

        LeafNode irAlMinisterio = behaviourTree.CreateLeafNode("IrAMinisterio", actIrAMinisterio, comprobarMinisterio);
        LeafNode recargarMetales = behaviourTree.CreateLeafNode("RecargarMetales", actRecargarMetales, comprobarMetalesRecargados);

        TimerDecoratorNode timerRecarga = behaviourTree.CreateTimerNode("TimerRecargaMetales", recargarMetales, 5);
        //Sequence node aleatorio
        //SequenceNode patrullarSequenceNode = behaviourTree.CreateSequenceNode("PatrullarSequenceNode", true);
        //patrullarSequenceNode.AddChild(subFSM);

        //Sequence node comprobar metales
        SequenceNode comprobarMetalesSequenceNode = behaviourTree.CreateSequenceNode("ComprobarMetalesSequenceNode", false);
        comprobarMetalesSequenceNode.AddChild(tengoMetalesLeafNode);
        comprobarMetalesSequenceNode.AddChild(subFSM);
        //comprobarMetalesSequenceNode.AddChild(patrullarSequenceNode);

        LoopUntilFailDecoratorNode patrullarUntilFail = behaviourTree.CreateLoopUntilFailNode("PatrullarUntilFail", comprobarMetalesSequenceNode);

        SequenceNode irMinisterioRecargaSequenceNode = behaviourTree.CreateSequenceNode("IRMinisterioYRecargar", false);
        irMinisterioRecargaSequenceNode.AddChild(irAlMinisterio);
        irMinisterioRecargaSequenceNode.AddChild(timerRecarga);

        SequenceNode recargarMetalesSequenceNode = behaviourTree.CreateSequenceNode("RecargarMetalesSequenceNode", false);
        recargarMetalesSequenceNode.AddChild(patrullarUntilFail);
        recargarMetalesSequenceNode.AddChild(irMinisterioRecargaSequenceNode);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", recargarMetalesSequenceNode);
        behaviourTree.SetRootNode(rootNode);
    }

    #region Metodos BT
    private void actTengoMetales()
    {

    }
    private ReturnValues compMetales()
    {
        Debug.Log(metales);
        if (metales >= 10)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void actPatrullar1()
    {
        agent.SetDestination(new Vector3(7.5f, 1f, -16.5f));
        metales -= 35;
        if (metales < 0)
        {
            metales = 0;
        }
    }
    private ReturnValues compPatrullar1()
    {

        if (this.transform.position.x >= 7.3 && this.transform.position.x <= 8 && this.transform.position.z >= -17 && this.transform.position.z <= -16)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    private void actPatrullar2()
    {
        agent.SetDestination(new Vector3(15.5f, 1f, 7.5f));
        metales -= 35;
        if (metales < 0)
        {
            metales = 0;
        }
    }
    private ReturnValues compPatrullar2()
    {
        if (this.transform.position.x >= 15 && this.transform.position.x <= 16 && this.transform.position.z >= 7 && this.transform.position.z <= 8)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    private void actPatrullar3()
    {
        agent.SetDestination(new Vector3(-18f, 1f, 10f));
        metales -= 35;
        if (metales < 0)
        {
            metales = 0;
        }
    }
    private ReturnValues compPatrullar3()
    {
        if (this.transform.position.x >= -18.5 && this.transform.position.x <= -17.5 && this.transform.position.z >= 9.5 && this.transform.position.z <= 10.5)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    private void actIrAMinisterio()
    {
        agent.SetDestination(new Vector3(-21.5f, 1f, -13f));
    }
    private ReturnValues comprobarMinisterio()
    {
        if (this.transform.position.x == -21.5 && this.transform.position.z == -13)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    private void actRecargarMetales()
    {
        metales = 100;
    }
    private ReturnValues comprobarMetalesRecargados()
    {
        if (metales == 100)
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
        //Estoy patrullando
        //float distTo = Vector3.Distance(transform.position, target.position);
        //Debug.Log(currentPoint);
        float distTo = 200;

        if (distTo <= attackRadius)
        {
            timer += Time.deltaTime;
            if (timer > maxTime)
            {
                inRange = true;
                enemigoDetectado.Fire();
            }
            else
            {
                inRange = false;
            }
        }

        if (!inRange && this.transform.position.x == agent.destination.x && this.transform.position.z == agent.destination.z || first == true)
        {

            first = false;
            updateCurrentPoint();
            Vector3 newPos = destinos[currentPoint];
            agent.SetDestination(newPos);
            Debug.Log(destinos[currentPoint]);
            Debug.Log(agent.destination);
            metales -= 20;
            
            
        }
        patrullaCompleta.Fire();
    }
    private void fsmGolpear()
    {
        //Golpeeo a un ska
    }
    private void fsmCazar()
    {
        //Cazo a un brumoso
        float distTo = Vector3.Distance(transform.position, target.position);

        transform.LookAt(target);
        Vector3 moveTo = Vector3.MoveTowards(transform.position, target.position, 180f);
        agent.SetDestination(moveTo);

        if (distTo > attackRadius)
        {
            enemigoFueraDeRango.Fire();
        }

    }
    private void fsmLuchar()
    {
        //Lucho
        if (salud <= 0)
        {
            luchaPerdida.Fire();
        }
    }
    private void fsmMorir()
    {
        //Muero
    }

    #endregion Metodos FSM

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
    private void fsmAux()
    {
        Debug.Log("estoy en aux");
        if (metales <= 0)
        {
            metalesBajos.Fire();
        }
    }
}


