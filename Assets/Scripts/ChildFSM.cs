using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildFSM : MonoBehaviour
{
    private int diaNacimiento;

    private enum estados {ESTUDIAR, DORMIR};
    private SimulationManager simManager;

    private void Awake()
    {
        simManager = GameObject.Find("_SimulationManager").GetComponent(typeof(SimulationManager)) as SimulationManager;
        diaNacimiento = simManager.dias;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        FSMChild();
        if (diaNacimiento + 10 == simManager.dias) {
            crecer();
        }
    }

    void FSMChild() {
        switch (simManager.ciclo) {
            case SimulationManager.cicloDNA.DIA:
                estudiar();
                break;
            case SimulationManager.cicloDNA.NOCHE:
                dormir();
                break;
            case SimulationManager.cicloDNA.AMANECER:
                dormir();
                break;

        }
    }

    void estudiar() {
        Debug.Log("ESTUDIANDO");
    }
    void dormir() {
        Debug.Log("DURMIENDO");
    }
    void crecer() {
        Debug.Log("He cresido");
    }
}
