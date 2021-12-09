using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BrumososBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    public NavigationPoints navigation;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    StateMachineEngine stateMachine;
    LeafNode merodearSubFSM;
    string accion = "";
    string UItxt = "";

    #region variables Brumoso
    int salud = 100;
    int metales = 60;
    private int diaNacimiento;
    #endregion variables Brumoso

    #region percepcionesMerodear
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
    #endregion percepcionesMerodear

    #region Estados merodear
    private State patrullando;
    private State patrullandoAux;
    private State cazando;
    private State luchar;
    private State morir;
    private State huyendo;
    private State irAHogar;
    private State recargarMetales;
    #endregion Estados merodear

    #region variables Patrulla
    private Transform target;
    private bool patrullaje = false;
    [SerializeField] bool inRange;
    public Vector3[] destinos;
    private int currentPoint = 0;
    #endregion variables Patrulla
    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Salud: " + salud + "\nMetales: " + metales + "\n" + accion;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.2f));
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
        diaNacimiento = simManager.dias;

        behaviourTree = new BehaviourTreeEngine(BehaviourEngine.IsNotASubmachine);
        stateMachine = new StateMachineEngine(BehaviourEngine.IsASubmachine);
        createFSMMerodear();
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

    private void createFSMMerodear()
    {
        //Percepciones
        saludBajaAlo = stateMachine.CreatePerception<PushPerception>();
        saludCompletaAlo = stateMachine.CreatePerception<PushPerception>();
        metalesBajosAlo = stateMachine.CreatePerception<PushPerception>();
        probHuirAlo = stateMachine.CreatePerception<PushPerception>();
        metalesCompletosAlo = stateMachine.CreatePerception<PushPerception>();
        enemigosCercaAlo = stateMachine.CreatePerception<PushPerception>();
        enemigoAlcanzadoAlo = stateMachine.CreatePerception<PushPerception>();
        enemigoLejosAlo = stateMachine.CreatePerception<PushPerception>();
        esNocheAlo = stateMachine.CreatePerception<PushPerception>();

        timerAuxAlo = stateMachine.CreatePerception<TimerPerception>(1.0f);

        //Estados
         patrullando = stateMachine.CreateState("Patrullando", actPatrullar);
         patrullandoAux = stateMachine.CreateState("patrullandoAux", actPatrullarAux);
         cazando = stateMachine.CreateState("cazando", actCazar);
         luchar = stateMachine.CreateState("luchar", actLuchar);
         morir = stateMachine.CreateState("morir", actMorir);
         huyendo = stateMachine.CreateState("huyendo", actHuir);
         irAHogar = stateMachine.CreateState("irAHogar", actHogar);
         recargarMetales = stateMachine.CreateState("recargarMetales", actRecargarMetales);

        //Transiciones
        stateMachine.CreateTransition("Repatrullar", patrullando, timerAuxAlo, patrullandoAux);
        stateMachine.CreateTransition("Repatrulla aux", patrullandoAux, timerAuxAlo, patrullando);
        stateMachine.CreateTransition("Repatrulla au", patrullandoAux, metalesBajosAlo, recargarMetales);


        stateMachine.CreateTransition("Enemigo Detectado", patrullando, enemigosCercaAlo, cazando);
        stateMachine.CreateTransition("Enemigo Lejos", cazando, enemigoLejosAlo, patrullando);
        stateMachine.CreateTransition("Cazando", cazando, timerAuxAlo, cazando);
        stateMachine.CreateTransition("Enemigo Alcanzado", cazando, enemigoAlcanzadoAlo, luchar);
        stateMachine.CreateTransition("Luchando", luchar, timerAuxAlo, luchar);
        stateMachine.CreateTransition("Morir", luchar, saludCeroAlo, morir);
        stateMachine.CreateTransition("Intento Huir", luchar, probHuirAlo, huyendo);
        stateMachine.CreateTransition("Huir", huyendo, timerAuxAlo, huyendo);
        stateMachine.CreateTransition("Huida exitosa", huyendo, enemigoLejosAlo, irAHogar);
        stateMachine.CreateTransition("Huida fallida", huyendo, enemigoAlcanzadoAlo, luchar);
        stateMachine.CreateTransition("Vuelta a Patrullar", irAHogar, saludCompletaAlo, patrullando);
        stateMachine.CreateTransition("Camino a casa", irAHogar, timerAuxAlo, irAHogar);
        stateMachine.CreateTransition("Recargar Metales", patrullando, metalesBajosAlo, recargarMetales);
        stateMachine.CreateTransition("Recargando metales", recargarMetales, timerAuxAlo, recargarMetales);
        stateMachine.CreateTransition("Metales Recargados", recargarMetales, metalesCompletosAlo, patrullando);

        //Entrada y salida del arbol
        merodearSubFSM = behaviourTree.CreateSubBehaviour("Entrada Sub FSM", stateMachine, patrullando);
        stateMachine.CreateExitTransition("Hizo noche", patrullando, esNocheAlo, ReturnValues.Succeed);

    }
    private void createBT()
    {
        //Leaf nodes
        LeafNode esDiaLeafNode = behaviourTree.CreateLeafNode("EsDia", actDia, comprobarDiaBT);
        //La Sub FSM
        LeafNode esNocheLeafNode = behaviourTree.CreateLeafNode("EsNoche", actNoche, comprobarNocheBT);
        LeafNode dormirLeafNode = behaviourTree.CreateLeafNode("Dormir", actDormir, comprobarDormir);
        LeafNode edadLeafNode = behaviourTree.CreateLeafNode("Comprobar Edad", actEdad, comprobarEdadBT);
        LeafNode morirLeafNode = behaviourTree.CreateLeafNode("Morir", actMorir, comprobarMorir);

        //Sequence Node 1
        SequenceNode comprobarDia = behaviourTree.CreateSequenceNode("ComprobarDiaSequence", false);
        comprobarDia.AddChild(esDiaLeafNode);
        comprobarDia.AddChild(merodearSubFSM);

        //Sequence Node 2
        SequenceNode comprobarNoche = behaviourTree.CreateSequenceNode("ComprobarNocheSequence", false);
        comprobarNoche.AddChild(esNocheLeafNode);
        comprobarNoche.AddChild(dormirLeafNode);

        //Selector 1
        SelectorNode diaONocheSelector = behaviourTree.CreateSelectorNode("DiaONocheSelector");
        diaONocheSelector.AddChild(comprobarDia);
        diaONocheSelector.AddChild(comprobarNoche);

        //Sequence Node 3
        SequenceNode comprobarEdad = behaviourTree.CreateSequenceNode("ComprobarEdad", false);
        comprobarEdad.AddChild(edadLeafNode);
        comprobarEdad.AddChild(diaONocheSelector);

        //Selector 2
        SelectorNode baseSelector = behaviourTree.CreateSelectorNode("MorirSelector");
        baseSelector.AddChild(comprobarEdad);
        baseSelector.AddChild(morirLeafNode);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", baseSelector);
        behaviourTree.SetRootNode(rootNode);
    }

    #region BT
    private void actDia()
    {

    }
    private ReturnValues comprobarDiaBT()
    {
        Debug.Log("compruebo dia");
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

    }
    private ReturnValues comprobarNocheBT()
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
    private void actEdad()
    {

    }
    private ReturnValues comprobarEdadBT()
    {
        //Debug.Log("compruebo edad");
        if (simManager.dias - diaNacimiento <= 20)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }
    private ReturnValues comprobarMorir()
    {
        return ReturnValues.Succeed;
    }
    private void actDormir()
    {
        accion = "Durmiendo";
    }
    private ReturnValues comprobarDormir()
    {
        //Debug.Log("compruebo dormir");
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion BT

    #region FSM Merodear
    private void actPatrullar()
    {
        accion = "Patrullando";
        Debug.Log("Patrullando");
        //if (metales <= 10)
        //{
        //    metalesBajosAlo.Fire();
        //}
        metales -= 10;
        metalesBajosAlo.Fire();
        //if (metales<= 0)
        //{
        //    metales = 0;
        //}

    }
    private void actPatrullarAux()
    {
        if (metales<=80)
        {
            metalesBajosAlo.Fire();
        }
    }
    private void actCazar()
    {

    }
    private void actLuchar()
    {

    }
    private void actMorir()
    {

    }
    private void actHuir()
    {

    }
    private void actHogar()
    {

    }
    private void actRecargarMetales()
    {
        metales = 100;
        Debug.Log("Recargo metales");
        if (metales>= 80)
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
    #endregion FSM Merodear


}
