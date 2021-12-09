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
    bool pausa = false;

    [Header("Minutos que transcurren en tiempo de juego por cada segundo en la vida real")]
    public int minutosPorSegundo = 10;   //Minutos que transcurren en tiempo de juego por cada segundo en la vida real
    private float tiempoDelFrameConTimeScale = 0f;
    public float tiempoAMostrarEnSegundos = 0f;

    [Header("Estado del día")]
    public Light luz;
    public enum cicloDNA { DIA, NOCHE, AMANECER }
    public cicloDNA ciclo;

    [Header("Agentes")]
    private NavigationPoints navPoints;
    public GameObject skaa;
    public GameObject noble;
    public GameObject inquisidor;
    public GameObject alomantico;

    // Start is called before the first frame update
    void Start()
    {
        navPoints = new NavigationPoints();
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
            ++dias;
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

    public bool ComprobarFiesta() {
        if (dias % 3 == 0 && tiempoAMostrarEnSegundos > 1080)
        {
            return true;
        }
        else if (dias-1 % 3 == 0 && tiempoAMostrarEnSegundos < 360) {
            return true;
        }
        else {
            return false;
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
    public void timex10()
    {
        Time.timeScale = 10;
        //minutosPorSegundo = 10;
    }

    public void timex50()
    {
        Time.timeScale = 50;
        //minutosPorSegundo = 200;
    }

    public void Pause() {
        Time.timeScale = 0;
    }

    public void InstanciarSkaa() {
        Instantiate(skaa, navPoints.goToChozaSkaa(), new Quaternion(1, 1, 1, 1));
    }
    public void InstanciarNoble()
    {
        Instantiate(noble, navPoints.goToMansionNoble(), new Quaternion(1, 1, 1, 1));
    }
    public void InstanciarInquisidor()
    {
        Instantiate(inquisidor, navPoints.goToMinisterio(), new Quaternion(1, 1, 1, 1));
    }
    public void InstanciarAlomantico()
    {
        Instantiate(alomantico, navPoints.goToChozaSkaa(), new Quaternion(1, 1, 1, 1));
    }
}


