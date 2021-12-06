using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Inquisidores : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    private StateMachineEngine stateMachine;
    private LeafNode subFSM;

    #region variables Inquisidores
    private int salud = 100;
    private int metales = 100;
    private int diaNacimiento;
    #endregion variables Inquisidores

    #region estados
    private State patrullar;
    private State golpear;
    private State cazar;
    private State luchar;
    private State morir;
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

    #endregion percepciones

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


        //Estados
        patrullar = stateMachine.CreateEntryState("Patrullar", fsmPatrullar);
        golpear = stateMachine.CreateState("Golpear", fsmGolpear);
        cazar = stateMachine.CreateState("Cazar", fsmCazar);
        luchar = stateMachine.CreateState("Luchar", fsmLuchar);
        morir = stateMachine.CreateState("Morir", fsmMorir);

        //Transiciones
        stateMachine.CreateTransition("Ska Detectado", patrullar, skaDescansandoDetectado, golpear);
        stateMachine.CreateTransition("Ska Golpeado", golpear, skaGolpeado, patrullar);
        stateMachine.CreateTransition("Enemigo Detectado", patrullar, enemigoDetectado, cazar);
        stateMachine.CreateTransition("Enemigo Perdido", cazar, enemigoPerdido, patrullar);
        stateMachine.CreateTransition("Enemigo Alcanzado", cazar, enemigoAlcanzado, luchar);
        stateMachine.CreateTransition("Enemigo Fuera Rango", luchar, enemigoFueraDeRango, cazar);
        stateMachine.CreateTransition("Enemigo Derrotado", luchar, enemigoDerrotado, patrullar);
        stateMachine.CreateTransition("Lucha Perdida", luchar, luchaPerdida, morir);

        //Entrada y salida de la FSM
        subFSM = behaviourTree.CreateSubBehaviour("Sub-FSM", stateMachine, patrullar);
        stateMachine.CreateExitTransition("Vuelta a BT", patrullar, metalesBajos, ReturnValues.Succeed);
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
        agent.SetDestination(new Vector3(-18f, 1f, 10f));
        Debug.Log("patrullando");
    }
    private void fsmGolpear()
    {
        //Golpeeo a un ska
    }
    private void fsmCazar()
    {
        //Cazo a un brumoso
    }
    private void fsmLuchar()
    {
        //Lucho
        if (salud <=0)
        {
            luchaPerdida.Fire();
        }
    }
    private void fsmMorir()
    {
        //Muero
    }

    #endregion Metodos FSM
}
