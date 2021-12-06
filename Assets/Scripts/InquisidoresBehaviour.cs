using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InquisidoresBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    string UItxt = "";

    #region variables Inquisidores
    int salud = 100;
    int metales = 100;
    private int diaNacimiento;
    #endregion variables Inquisidores


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
        LeafNode patrullarLeafNode1 = behaviourTree.CreateLeafNode("Patrullar1", actPatrullar1, compPatrullar1);
        LeafNode patrullarLeafNode2 = behaviourTree.CreateLeafNode("Patrullar2", actPatrullar2, compPatrullar2);
        LeafNode patrullarLeafNode3 = behaviourTree.CreateLeafNode("Patrullar3", actPatrullar3, compPatrullar3);

        LeafNode irAlMinisterio = behaviourTree.CreateLeafNode("IrAMinisterio", actIrAMinisterio, comprobarMinisterio);
        LeafNode recargarMetales = behaviourTree.CreateLeafNode("RecargarMetales", actRecargarMetales, comprobarMetalesRecargados);

        TimerDecoratorNode timerRecarga = behaviourTree.CreateTimerNode("TimerRecargaMetales", recargarMetales, 5);
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

        SequenceNode irMinisterioRecargaSequenceNode = behaviourTree.CreateSequenceNode("IRMinisterioYRecargar", false);
        irMinisterioRecargaSequenceNode.AddChild(irAlMinisterio);
        irMinisterioRecargaSequenceNode.AddChild(timerRecarga);

        SequenceNode recargarMetalesSequenceNode = behaviourTree.CreateSequenceNode("RecargarMetalesSequenceNode", false);
        recargarMetalesSequenceNode.AddChild(patrullarUntilFail);
        recargarMetalesSequenceNode.AddChild(irMinisterioRecargaSequenceNode);

        LoopDecoratorNode rootNode = behaviourTree.CreateLoopNode("RootNode", recargarMetalesSequenceNode);
        behaviourTree.SetRootNode(rootNode);
    }

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
}
