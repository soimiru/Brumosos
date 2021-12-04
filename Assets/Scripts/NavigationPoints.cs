using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NavigationPoints : MonoBehaviour
{

    public Vector3 goToMansionNoble() {
        Vector3 newPos = new Vector3(Random.Range(9, 20), 1, Random.Range(11, 20));

        return newPos;
    }

    public Vector3 goToFabrica()
    {
        Vector3 newPos = new Vector3(Random.Range(-20, -8), 1, Random.Range(14, 20));

        return newPos;
    }

    public Vector3 goToChozaSkaa() {
        Vector3 newPos = new Vector3(Random.Range(12, 20), 1, Random.Range(-20, -17));

        return newPos;
    }

    public Vector3 goToMinisterio()
    {
        Vector3 newPos = new Vector3(Random.Range(-22, -18), 1, Random.Range(-14, -12));

        return newPos;
    }

    public bool comprobarPosMansionNoble(Vector3 pos)
    {
        if (pos.x >= 9 && pos.x <= 20)
        {
            if (pos.z >= 11 && pos.z <= 20)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public bool comprobarPosFabrica(Vector3 pos) {
        if (pos.x <= -8 && pos.x >= -20) {
            if (pos.z >= 14 && pos.z <= 20)
            {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }

    public bool comprobarChozaSkaa(Vector3 pos) {
        if (pos.x <= 20 && pos.x >= 12)
        {
            if (pos.z >= -20 && pos.z <= -17)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
