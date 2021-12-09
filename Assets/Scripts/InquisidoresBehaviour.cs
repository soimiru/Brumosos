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
    string accion = "";
    string UItxt = "";

    #region variables Inquisidores
    private int salud = 100;
    private int metales = 100;
    private int diaNacimiento;
    #endregion variables Inquisidores

    #region variables Patrulla
    public Transform targetAlomantico;
    private Transform targetSka;
    private bool patrullando = false;
    private bool cazando = false;
    public float attackRadius = 30.0f;
    private int currentPoint = 0;
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
    //private State instanciarNuevoInquisidor;
    private State aux;
    private State cazarAux;
    private State golpearAux;
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
    private Perception timerCaza;
    private Perception golpearAuxP;

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
        timerCaza = stateMachine.CreatePerception<TimerPerception>(0.25f);
        golpearAuxP = stateMachine.CreatePerception<PushPerception>();



        //Estados
        patrullar = stateMachine.CreateEntryState("Patrullar", fsmPatrullar);
        golpear = stateMachine.CreateState("Golpear", fsmGolpear);
        cazar = stateMachine.CreateState("Cazar", fsmCazar);
        luchar = stateMachine.CreateState("Luchar", fsmLuchar);
        morir = stateMachine.CreateState("Morir", fsmMorir);
        //instanciarNuevoInquisidor = stateMachine.CreateState("instanciarInquisidor", instanciarInquisidor);
        aux = stateMachine.CreateState("Aux", fsmAux);
        cazarAux = stateMachine.CreateState("CazarAux", fsmAux);
        golpearAux = stateMachine.CreateState("GolpearAux", fsmGolpearAux);

        //Transiciones
        stateMachine.CreateTransition("Ska Detectado", patrullar, skaDescansandoDetectado, golpear);
        stateMachine.CreateTransition("Ska Detectado en aux", aux, skaDescansandoDetectado, golpear);
        stateMachine.CreateTransition("Ska Golpeado", golpear, skaGolpeado, golpearAux);
        stateMachine.CreateTransition("De camino a golpear", golpear, timerAux, golpear);
        stateMachine.CreateTransition("Enemigo detectado de camino a Ska", golpear, enemigoDetectado, cazar);
        stateMachine.CreateTransition("De vuelta a patrullar", golpearAux, golpearAuxP, patrullar);
        stateMachine.CreateTransition("De vuelta a patrullar me encontré un Enemigo", golpearAux, enemigoDetectado, cazar);
        stateMachine.CreateTransition("Comprobación si he vuelto a patrullar", golpearAux, timerAux, golpearAux);
        stateMachine.CreateTransition("Repatrullar", patrullar, patrullaCompleta, aux);
        stateMachine.CreateTransition("Timer Aux", aux, timerCaza, patrullar);


        stateMachine.CreateTransition("Enemigo Detectado", patrullar, enemigoDetectado, cazar);
        stateMachine.CreateTransition("Enemigo Detectado en aux", aux, enemigoDetectado, cazar);
        stateMachine.CreateTransition("Enemigo Perdido", cazar, enemigoPerdido, patrullar);

        stateMachine.CreateTransition("Cazando", cazar, timerCaza, cazar);
        //stateMachine.CreateTransition("CazandoAux", cazarAux, timerCaza, cazar);

        stateMachine.CreateTransition("Enemigo Alcanzado", cazar, enemigoAlcanzado, luchar);
        stateMachine.CreateTransition("Enemigo Fuera Rango", luchar, enemigoFueraDeRango, patrullar);
        stateMachine.CreateTransition("Luchando", luchar, timerAux, luchar);
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


        LeafNode irAlMinisterio = behaviourTree.CreateLeafNode("IrAMinisterio", actIrAMinisterio, comprobarMinisterio);
        LeafNode recargarMetales = behaviourTree.CreateLeafNode("RecargarMetales", actRecargarMetales, comprobarMetalesRecargados);

        TimerDecoratorNode timerRecarga = behaviourTree.CreateTimerNode("TimerRecargaMetales", recargarMetales, 5);


        //Sequence node comprobar metales
        SequenceNode comprobarMetalesSequenceNode = behaviourTree.CreateSequenceNode("ComprobarMetalesSequenceNode", false);
        comprobarMetalesSequenceNode.AddChild(tengoMetalesLeafNode);
        comprobarMetalesSequenceNode.AddChild(subFSM);

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
        //Debug.Log(metales);
        if (metales >= 10)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    
    private void actIrAMinisterio()
    {
        accion = "Volviendo al ministerio";
        agent.SetDestination(new Vector3(-21.5f, 1f, -13f));
    }
    private ReturnValues comprobarMinisterio()
    {
        if (this.transform.position.x >= -22.0 && this.transform.position.x >= -21.0 && this.transform.position.z >= -13.5 && this.transform.position.z <= -12.5)
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
        accion = "Recargando metales";
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
        patrullando = true;
        cazando = false;

        if (!inRange && this.transform.position.x >= agent.destination.x - 0.5f && this.transform.position.x <= agent.destination.x + 0.5f && this.transform.position.z >= agent.destination.z - 0.5f && this.transform.position.z <= agent.destination.z + 0.5f || first == true)
        {
            first = false;
            updateCurrentPoint();
            Vector3 newPos = destinos[currentPoint];
            agent.SetDestination(newPos);
            metales -= 5;
        }
        accion = "Patrullando";
        patrullaCompleta.Fire();
    }
    private void fsmGolpear()
    {
        patrullando = false;
        cazando = true;
        accion = "Cazando a un Skaa";
        float distTo = Vector3.Distance(transform.position, targetSka.position);
        transform.LookAt(targetSka);
        Vector3 moveTo = Vector3.MoveTowards(transform.position, targetSka.position, 180f);
        agent.SetDestination(moveTo);
        //Le da un palo
        if (distTo < 2)
        {
            updateCurrentPoint();
            Vector3 newPos = destinos[currentPoint];
            agent.SetDestination(newPos);
            metales -= 5;
            skaGolpeado.Fire();
        }

    }
    private void fsmCazar()
    {
        patrullando = false;
        cazando = true;
        //Cazo a un brumoso
        accion = "Cazando alomántico";
        float distTo = Vector3.Distance(transform.position, targetAlomantico.position);
        transform.LookAt(targetAlomantico);
        Vector3 moveTo = Vector3.MoveTowards(transform.position, targetAlomantico.position, 180f);
        agent.SetDestination(moveTo);
        if (distTo < 2)
        {
            enemigoAlcanzado.Fire();
        }
       

    }
    private void fsmLuchar()
    {
        
        patrullando = false;
        cazando = false;
        accion = "Luchando";
        //Lucho
        if (targetAlomantico == null)
        {
            first = true;
            enemigoDerrotado.Fire();
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
            if (salud <= 0)
            {
                salud = 0;
            }
        }
        if (salud <= 0)
        {
            luchaPerdida.Fire();
        }
    }
    private void fsmMorir()
    {
        Destroy(this.gameObject);

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
    private void fsmAux()
    {
        if (metales <= 0)
        {
            metalesBajos.Fire();
        }
    }
    private void fsmGolpearAux()
    {
        patrullando = false;
        cazando = false;
        accion = "Skaa golpeado \nVolviendo a patrullar";
        if (!inRange && this.transform.position.x == agent.destination.x && this.transform.position.z == agent.destination.z || first == true)
        {
            first = true;
            golpearAuxP.Fire();
        }
    }

    private void instanciarInquisidor()
    {

    }
    #endregion Metodos FSM

    #region metodosColision
    private void OnTriggerEnter(Collider other)
    {
        if (patrullando == true)
        {
            if (other.tag == "Skaa")
            {
                targetSka = other.transform;
                skaDescansandoDetectado.Fire();
            }
            
        }
        if (other.tag == "Alomántico")
        {
            targetAlomantico = other.transform;
            enemigoDetectado.Fire();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (cazando == true)
        {
            if (other.tag == "Skaa")
            {
                //enemigoFueraDeRango.Fire();
            }
            else if (other.tag == "Alomántico")
            {
                enemigoPerdido.Fire();
            }
        }
        if (cazando == false && patrullando == false)
        {
            if (other.tag == "Alomántico")
            {
                first = true;
                enemigoFueraDeRango.Fire();
            }
        }
    }
    #endregion metodosColision
}


