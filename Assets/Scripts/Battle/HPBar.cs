using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private GameObject health;

    private Image image;
    void Awake()
    {
        image = health.GetComponent<Image>();
    }
    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
        image.color = Color.green;
    }
    

    public IEnumerator SetHPSmooth(float newHp)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;
        
        while (curHp - newHp > Mathf.Epsilon)
        {
            if (curHp <= 0.2f)
            {
                image.color = Color.red;
            }
            else if (curHp <= 0.5f)
            {
                image.color = Color.yellow;
            }
            else
            {
               image.color = Color.green;
            }
            curHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHp, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);
    }
}
