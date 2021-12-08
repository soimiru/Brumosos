using UnityEngine;
using UnityEngine.AI;

public class BrumososBehaviour : MonoBehaviour
{
    public NavMeshAgent agent;
    public NavigationPoints navigation;
    private SimulationManager simManager;
    private BehaviourTreeEngine behaviourTree;
    StateMachineEngine merodearFSM;
    LeafNode merodearSubFSM;
    string accion = "";
    string UItxt = "";

    #region variables Brumoso
    int salud = 100;
    int metales = 100;
    private int diaNacimiento;
    #endregion variables Brumoso

    #region percepcionesMerodear
    Perception saludBaja;
    Perception saludCompleta;
    Perception metalesBajos;
    Perception metalesCompletos;
    Perception enemigosCerca;
    Perception enemigoAlcanzado;
    Perception enemigoLejos;

    #endregion percepcionesMerodear
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
        merodearFSM = new StateMachineEngine(BehaviourEngine.IsASubmachine);
    }
    void Start()
    {
        createFSMMerodear();
        createBT();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void createFSMMerodear()
    {
        //Percepciones
        saludBaja = merodearFSM.CreatePerception<PushPerception>();
        saludCompleta = merodearFSM.CreatePerception<PushPerception>();
        metalesBajos = merodearFSM.CreatePerception<PushPerception>();
        metalesCompletos = merodearFSM.CreatePerception<PushPerception>();
        enemigosCerca = merodearFSM.CreatePerception<PushPerception>();
        enemigoAlcanzado = merodearFSM.CreatePerception<PushPerception>();
        enemigoLejos = merodearFSM.CreatePerception<PushPerception>();

        //Estados


        //Transiciones
    }
    private void createBT()
    {

    }

    #region BT

    #endregion BT

    #region FSM Merodear

    #endregion FSM Merodear


}
