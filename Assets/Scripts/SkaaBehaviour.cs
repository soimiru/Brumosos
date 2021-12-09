using UnityEngine;
using UnityEngine.AI;

public class SkaaBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private NavigationPoints navPoints;
    public enum posiciones { FABRICA, CHOZASKAA, CALLE }
    public posiciones miPosicion;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    StateMachineEngine childFSM;
    StateMachineEngine adultFSM;
    LeafNode trabajarSubFsm;
    string accion = "";
    string UItxt = "";
    [SerializeField] float timer;

    #region variables Ska
    int salud = 100;
    int cansancio = 0;
    private int diaNacimiento;
    private bool adulto = false;
    int recursos = 0;

    #endregion variables Ska

    #region percepcionesTrabajar
    Perception recursosRecogidos;
    Perception recursosAgotados;
    Perception puestoTrabajo;
    Perception puestoRecogida;
    Perception timerAux;
    Perception estoyCansado;
    Perception auxRecogiendo;
    Perception tiempoAuxiliar;
    Perception seHaceNoche;
    #endregion percepcionesTrabajar
    private void OnGUI()
    {
        //TEXTO A MOSTRAR
        UItxt = "Cansancio: " + cansancio + "\nSalud: " + salud + "\nRecursos: " + recursos + "\n" + accion;

        //ESTILO DE LA CAJA DE TEXTO
        GUIStyle style = new GUIStyle();
        Texture2D debugTex = new Texture2D(1, 1);
        debugTex.SetPixel(0, 0, new Color(1f, 1f, 0.7f, 0.5f));
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

    private void Awake()
    {
        this.transform.localScale = new Vector3(0.5f, 1, 0.5f);
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        diaNacimiento = simManager.dias;
        navPoints = new NavigationPoints();

        behaviourTree = new BehaviourTreeEngine(BehaviourEngine.IsNotASubmachine);
        childFSM = new StateMachineEngine();
        adultFSM = new StateMachineEngine(BehaviourEngine.IsASubmachine);
    }

    // Start is called before the first frame update
    void Start()
    {
        createFSMChild();
        createFSMAdult();
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
            adultFSM.Update();
        }
        
        
    }
    private void createBT()
    {
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

        LeafNode timeDescansarLeafNode = behaviourTree.CreateLeafNode("TimerDescansar", actTimer, comprobarTimer);
        LeafNode timeTrabajarLeafNode = behaviourTree.CreateLeafNode("TimerTrabajar", actTimer, comprobarTimer);

        TimerDecoratorNode timerNodeDescansar = behaviourTree.CreateTimerNode("TimerNodeDescansar", timeDescansarLeafNode, 5);
        TimerDecoratorNode timerNodeTrabajar = behaviourTree.CreateTimerNode("TimerNodeTrabajar", timeTrabajarLeafNode, 2);

        //Nodo secuencia 1
        SequenceNode descansarSequenceNode = behaviourTree.CreateSequenceNode("DescansarSelectorNode", false);
        descansarSequenceNode.AddChild(cansadoLeafNode);
        descansarSequenceNode.AddChild(descansarLeafNode);
        descansarSequenceNode.AddChild(timerNodeDescansar);

        //Nodo secuencia 1**
        SequenceNode trabajarYEsperarSequenceNode = behaviourTree.CreateSequenceNode("TrabajarYEsperarSequenceNode", false);
        trabajarYEsperarSequenceNode.AddChild(trabajarSubFsm);

        //Nodo selector 1
        SelectorNode cansadoTrabajarSelectorNode = behaviourTree.CreateSelectorNode("CansadoTrabajarSequenceNode");
        cansadoTrabajarSelectorNode.AddChild(descansarSequenceNode);
        cansadoTrabajarSelectorNode.AddChild(trabajarYEsperarSequenceNode);


        //Nodo secuencia 2
        SequenceNode comprobarDiaSequenceNode = behaviourTree.CreateSequenceNode("ComprobarDiaSequenceNode", false);
        comprobarDiaSequenceNode.AddChild(esDiaLeafNode);
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

        //Nodo raíz
        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", baseSelectorNode);
        behaviourTree.SetRootNode(rootNode);

    }
    private void createFSMChild()
    {
        //Maquina de estados de cuando el agente es niño
        //Estados
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
    private void createFSMAdult()
    {
        //FSM de trabajar
        //Percepciones
        tiempoAuxiliar = adultFSM.CreatePerception<TimerPerception>(1.0f);
        recursosRecogidos = adultFSM.CreatePerception<PushPerception>();
        recursosAgotados = adultFSM.CreatePerception<PushPerception>();
        puestoTrabajo = adultFSM.CreatePerception<PushPerception>();
        puestoRecogida = adultFSM.CreatePerception<PushPerception>();
        estoyCansado = adultFSM.CreatePerception<PushPerception>();
        auxRecogiendo = adultFSM.CreatePerception<PushPerception>();
        timerAux = adultFSM.CreatePerception<TimerPerception>(0.5f);
        seHaceNoche = adultFSM.CreatePerception<PushPerception>();


        //Estados
        State irAPorRecursos = adultFSM.CreateState("Ir a por Recursos", irARecogerRecursosFSM);
        State recogiendoRecursos = adultFSM.CreateState("Recogiendo Recursos", recogerRecursosFSM);
        State irAUsarRecursos = adultFSM.CreateEntryState("Ir a usar Recursos", irAUsarRecursosFSM);
        State usandoRecursos = adultFSM.CreateState("Usando Recursos", usandoRecursosFSM);

        State auxRecogerRecursos = adultFSM.CreateState("Aux Recoger Recursos", auxRecogerRecursosFSM);

        //Transiciones
        adultFSM.CreateTransition("Recursos recogidos", recogiendoRecursos, recursosRecogidos, irAUsarRecursos);
        adultFSM.CreateTransition("Recursos gastados", usandoRecursos, recursosAgotados, irAPorRecursos);
        adultFSM.CreateTransition("Puesto recogida", irAPorRecursos, puestoRecogida, recogiendoRecursos);
        adultFSM.CreateTransition("Puesto trabajo", irAUsarRecursos, puestoTrabajo, usandoRecursos);
        adultFSM.CreateTransition("De ir a usar a ir a por recursos", irAUsarRecursos, recursosAgotados, irAPorRecursos);

        adultFSM.CreateTransition("Timer Aux 1", recogiendoRecursos, auxRecogiendo, auxRecogerRecursos);
        adultFSM.CreateTransition("Timer Aux 2", auxRecogerRecursos, tiempoAuxiliar, recogiendoRecursos);
        adultFSM.CreateTransition("Timer Aux 3", usandoRecursos, timerAux, usandoRecursos);
        //adultFSM.CreateTransition("Timer Aux 4", irAPorRecursos, timerAux, irAPorRecursos);

        trabajarSubFsm = behaviourTree.CreateSubBehaviour("Sub FSM Trabajar", adultFSM, irAUsarRecursos);
        adultFSM.CreateExitTransition("Vuelta a BT por cansancio", usandoRecursos, estoyCansado, ReturnValues.Succeed);
        adultFSM.CreateExitTransition("Vuelta a BT por cansancio 2", recogiendoRecursos, estoyCansado, ReturnValues.Succeed);
        adultFSM.CreateExitTransition("Vuelta a BT por noche", usandoRecursos, seHaceNoche, ReturnValues.Succeed);
        adultFSM.CreateExitTransition("Vuelta a BT por noche 2", recogiendoRecursos, seHaceNoche, ReturnValues.Succeed);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Inquisidor")
        {
            salud -= 10;
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
        if (simManager.dias == (diaNacimiento + 3))
        {
            childFSM.Fire("Crecer");
        }
    }

    #region FSM Child
    void estudiarAction()
    {
        //agent.SetDestination(new Vector3(-2.5f, 1f, 8f));
        agent.SetDestination(navPoints.goToEscuelaSka());    
        miPosicion = posiciones.FABRICA;
        accion = "Estudiando";
    }
    void dormirAction()
    {
        //agent.SetDestination(new Vector3(18f, 1f, -20f));
        agent.SetDestination(navPoints.goToChozaSkaa());
        miPosicion = posiciones.CHOZASKAA;
        accion = "Durmiendo";
    }
    void nacerAction()
    {
        //Debug.Log("I WOULD LIKE TO SEE THE BABY");
    }
    void crecerAction()
    {
        transform.localScale = new Vector3(1, 1, 1);
        adulto = true;
        createBT();
    }
    #endregion FSM Child

    #region AccionesBT Adulto
    private void actSalud()
    {
        
    }
    private ReturnValues comprobarSalud()
    {
        if ((diaNacimiento + 20) <= simManager.dias)
        {
            salud = 0;
        }
        if (salud > 0 )
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }
    private void actDia()
    {
        //Debug.Log("Compruebo Dia");
    }
    private ReturnValues comprobarDia()
    {
        if (simManager.ciclo == SimulationManager.cicloDNA.DIA)
        {
            //Debug.Log("Es de dia");
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Failed;
        }
    }

    private void actNoche()
    {
        //Debug.Log("Compruebo Noche");
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
        //Debug.Log("Compruebo cansancio");
    }
    private ReturnValues comprobarCansado()
    {
        if (cansancio >= 50)
        {
            //Debug.Log("Cansancio alto");
            return ReturnValues.Succeed;
        }
        else
        {
            //Debug.Log("Cansancio bajo");
            return ReturnValues.Failed;
        }
    }

    private void actDescansar()
    {
        //Programar la acción de descansar
        agent.SetDestination(new Vector3(-8.5f, 1f, 18.5f));
        accion = "Voy a descansar";
    }
    private ReturnValues comprobarDescansar()
    {
        if (this.transform.position.x == -8.5f || this.transform.position.z == 18.5f)
        {
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
        
        
    }
    private void actDormir()
    {
        //Programar la acción de dormir
        //Futuramente se cambiará al US
        //agent.SetDestination(new Vector3(15.5f, 1f, -18.5f));
        agent.SetDestination(navPoints.goToChozaSkaa());
        accion = "Durmiendo";
    }
    private ReturnValues comprobarDormir()
    {
        //Debug.Log("comprobacion de dormir");
        //if (this.transform.position.x == 15.5f && this.transform.position.z == -18.5f && simManager.ciclo == SimulationManager.cicloDNA.DIA)
        if(navPoints.comprobarChozaSkaa(this.transform.position))
        {
            miPosicion = posiciones.CHOZASKAA;
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
        
    }

    private void actTrabajar()
    {
        //agent.SetDestination(new Vector3(-19.5f, 1f, 19.5f));
        agent.SetDestination(navPoints.goToFabrica());
        cansancio += 20;
        //Debug.Log("Acabo de trabajar");
    }
    private ReturnValues comprobarTrabajar()
    {
        //if (this.transform.position.x == -19.5f || this.transform.position.z == 19.5f)
        if(navPoints.comprobarPosFabrica(this.transform.position))
        {
            miPosicion = posiciones.FABRICA;
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }

    private void actMorir()
    {
        //La unidad se muere
        Destroy(this.gameObject);
    }
    private ReturnValues comprobarMorir()
    {
        //Debug.Log("Me morí");
        return ReturnValues.Succeed;
    }
    private void actTimer()
    {
        cansancio -= 0;
    }
    private ReturnValues comprobarTimer()
    {
        return ReturnValues.Succeed;
    }
    private void actIrATrabajar()
    {
        accion = "Voy a trabajar";
        agent.SetDestination(new Vector3(-19.5f, 1f, 19.5f));
        agent.SetDestination(navPoints.goToFabrica());

    }
    private ReturnValues comprobarLlegada()
    {
        //if (this.transform.position.x == -19.5f || this.transform.position.z == 19.5f)
        if (navPoints.comprobarPosFabrica(this.transform.position))
        {
            miPosicion = posiciones.FABRICA;
            return ReturnValues.Succeed;
        }
        else
        {
            return ReturnValues.Running;
        }
    }
    #endregion accionesBT

    #region FSM Adulto
    private void irAUsarRecursosFSM()
    {
        if (recursos >=50)
        {
            accion = "Voy al puesto de trabajo";
            agent.SetDestination(new Vector3(-19.5f, 1f, 19.5f));
            puestoTrabajo.Fire();
        }
        else
        {
            recursosAgotados.Fire();
        }
        
    }
    private void usandoRecursosFSM()
    {
        //navigation.comprobarPosFabrica(this.transform.position)
        if (this.transform.position.x == -19.5f || this.transform.position.z == 19.5f)
        {
            if (simManager.ciclo == SimulationManager.cicloDNA.NOCHE)
            {
                seHaceNoche.Fire();
            }
            if (cansancio >= 100)
            {
                estoyCansado.Fire();
            }
            else
            {
                if (recursos <= 10)
                {
                    recursosAgotados.Fire();
                }
                else
                {
                    accion = "Usando recursos";
                    recursos -= Random.Range(3, 12);
                    cansancio += Random.Range(5, 10);
                }
            }
        }
    }
    private void irARecogerRecursosFSM()
    {
        accion = "Voy a por recursos";
        agent.SetDestination(new Vector3(-3.5f, 1.0f, 21.0f));
        puestoRecogida.Fire();
    }
    private void recogerRecursosFSM()
    {
        if (this.transform.position.x >= -4.0f && this.transform.position.x <= -3.0f && this.transform.position.z >= 20.0 && this.transform.position.z <= 22.0)
        {
            if (simManager.ciclo == SimulationManager.cicloDNA.NOCHE)
            {
                seHaceNoche.Fire();
            }
            if (cansancio >= 100)
            {
                estoyCansado.Fire();
            }
            else
            {
                if (recursos >= 80)
                {
                    recursosRecogidos.Fire();
                }
                else
                {
                    recursos += Random.Range(5, 20);
                    cansancio += Random.Range(2, 5);
                    accion = "Estoy recogiendo recursos";
                }
            } 
        }
        auxRecogiendo.Fire();
    }
    
    private void auxRecogerRecursosFSM()
    {
        
    }
    


    #endregion FSM Adulto
}
