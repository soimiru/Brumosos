using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    [Header("UI")]
    public Text textReloj; public Text textDias; public Text textCiclo;

    public int dias = 1;
    private int tiempoInicial;

    [Header ("Minutos que transcurren en tiempo de juego por cada segundo en la vida real")]
    public int minutosPorSegundo = 10;   //Minutos que transcurren en tiempo de juego por cada segundo en la vida real
    private float tiempoDelFrameConTimeScale = 0f;
    public float tiempoAMostrarEnSegundos = 0f;

    [Header("Estado del día")]
    public Light luz;
    public enum cicloDNA { DIA, NOCHE, AMANECER}
    public cicloDNA ciclo;

    // Start is called before the first frame update
    void Start()
    {
        tiempoAMostrarEnSegundos = tiempoInicial;
        Time.timeScale = minutosPorSegundo;
        //canvasUI = gameObject.transform.Find("MainCanvas").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
        //tiempoDelFrameConTimeScale = Time.deltaTime * minutosPorSegundo;
        tiempoDelFrameConTimeScale = Time.deltaTime * Time.timeScale;

        tiempoAMostrarEnSegundos += tiempoDelFrameConTimeScale;

        ActualizarReloj(tiempoAMostrarEnSegundos);
        if (tiempoAMostrarEnSegundos > 1440) {
            dias++;
            ActualizarDias();
            tiempoAMostrarEnSegundos = 0;
        }
        if (tiempoAMostrarEnSegundos > 360 && tiempoAMostrarEnSegundos < 1080)
        {
            textCiclo.text = "DIA";
            ciclo = cicloDNA.DIA;
            luz.color = new Color(1, 0.9568627f, 0.8392157f, 1);
        }
        if (tiempoAMostrarEnSegundos > 1080 && tiempoAMostrarEnSegundos < 1440 || tiempoAMostrarEnSegundos > 0 && tiempoAMostrarEnSegundos < 240)
        {
            textCiclo.text = "NOCHE";
            ciclo = cicloDNA.NOCHE;
            luz.color = new Color(0.2396831f, 0.2193396f, 0.2924528f, 1); 
        }
        if (tiempoAMostrarEnSegundos > 240 && tiempoAMostrarEnSegundos < 360)
        {
            textCiclo.text = "AMANECER";
            ciclo = cicloDNA.AMANECER;
            luz.color = new Color(0.6792453f, 0.4388201f, 0.3556426f, 1);
        }
    }

    public void ActualizarReloj(float tiempoEnSegundos)
    {

        int minutos = 0;
        int segundos = 0;
        // int milisegundos = 0;
        string textoDelReloj;

        if (tiempoEnSegundos < 0) tiempoEnSegundos = 0;
        minutos = (int)tiempoEnSegundos / 60;
        segundos = (int)tiempoEnSegundos % 60;

        //milisegundos = (int)tiempoEnSegundos / 1000;

        textoDelReloj = minutos.ToString("00") + ":" + segundos.ToString("00"); //+ ":" + milisegundos.ToString("00");

        textReloj.text = textoDelReloj;
    }

    public void ActualizarDias() {
        textDias.text = "Día " + dias.ToString();
    }

    public void timex1()
    {
        Time.timeScale = 1;
        //minutosPorSegundo = 10;
    }

    public void timex2()
    {
        Time.timeScale = 100;
        //minutosPorSegundo = 200;
    }
}


