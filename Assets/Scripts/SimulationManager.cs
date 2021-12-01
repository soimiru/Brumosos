using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationManager : MonoBehaviour
{
    public Text textReloj, textDias;

    private int dias = 1;
    private int tiempoInicial;
    [Header ("Minutos que transcurren en tiempo de juego por cada segundo en la vida real")]
    public int minutosPorSegundo = 10;   //Minutos que transcurren en tiempo de juego por cada segundo en la vida real
    private float tiempoDelFrameConTimeScale = 0f;
    private float tiempoAMostrarEnSegundos = 0f;

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
        Time.timeScale = 10;
        //minutosPorSegundo = 10;
    }

    public void timex2()
    {
        Time.timeScale = 100;
        //minutosPorSegundo = 200;
    }
}


