using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Inquisidores : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;

    #region variables Inquisidores
    int salud = 100;
    int metales = 100;
    private int diaNacimiento;
    #endregion variables Inquisidores
    // Start is called before the first frame update
    private void Awake()
    {
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        diaNacimiento = simManager.dias;
    }
    void Start()
    {
        createBT();
    }

    // Update is called once per frame
    void Update()
    {
        behaviourTree.Update();
    }

    private void createBT()
    {
        behaviourTree = new BehaviourTreeEngine(false);
        //Nodos hoja
        LeafNode tengoMetalesLeafNode = behaviourTree.CreateLeafNode("TengoMetales", actTengoMetales, compMetales);
        LeafNode patrullarLeafNode1 = behaviourTree.CreateLeafNode("Patrullar", actPatrullar1, compPatrullar1);
        LeafNode patrullarLeafNode2 = behaviourTree.CreateLeafNode("Patrullar", actPatrullar2, compPatrullar2);
        LeafNode patrullarLeafNode3 = behaviourTree.CreateLeafNode("Patrullar", actPatrullar3, compPatrullar3);

        LeafNode irAlMinisterio = behaviourTree.CreateLeafNode("IrAMinisterio", actIrAMinisterio, comprobarMinisterio);
        LeafNode recargarMetales = behaviourTree.CreateLeafNode("RecargarMetales", actRecargarMetales, comprobarMetalesRecargados);

        //Sequence node aleatorio
        SequenceNode patrullarSequenceNode = behaviourTree.CreateSequenceNode("PatrullarSequenceNode", true);
        patrullarSequenceNode.AddChild(patrullarLeafNode1);
        patrullarSequenceNode.AddChild(patrullarLeafNode2);
        patrullarSequenceNode.AddChild(patrullarLeafNode3);

        //Sequence node comprobar metales
        SequenceNode comprobarMetalesSequenceNode = behaviourTree.CreateSequenceNode("ComprobarMetalesSequenceNode", false);
        comprobarMetalesSequenceNode.AddChild(tengoMetalesLeafNode);
        comprobarMetalesSequenceNode.AddChild(patrullarSequenceNode);

        LoopUntilFailDecoratorNode patrullarUntilFail = behaviourTree.CreateLoopUntilFailNode("PatrullarUntilFail", comprobarMetalesSequenceNode);

        SequenceNode recargarMetalesSequenceNode = behaviourTree.CreateSequenceNode("RecargarMetalesSequenceNode", false);
        recargarMetalesSequenceNode.AddChild(patrullarUntilFail);
        recargarMetalesSequenceNode.AddChild(recargarMetales);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", recargarMetalesSequenceNode);
        behaviourTree.SetRootNode(rootNode);
    }

    private void actTengoMetales()
    {

    }
    private ReturnValues compMetales()
    {
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
        metales -= 20;
    }
    private ReturnValues compPatrullar1()
    {
        if (this.transform.position.x == 7.5 && this.transform.position.z == -16.5)
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
        metales -= 20;
    }
    private ReturnValues compPatrullar2()
    {
        if (this.transform.position.x == 15.5 && this.transform.position.z == 7.5)
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
        metales -= 20;
    }
    private ReturnValues compPatrullar3()
    {
        if (this.transform.position.x == -18 && this.transform.position.z == 10)
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
}
