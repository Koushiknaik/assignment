using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slots_Selection : MonoBehaviour
{
    [SerializeField] float stoptime;
    public int selectingpoint;
    [SerializeField] int increservalue;
    [SerializeField] Transform slotsobject;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SlotsSpinStart()
    {
        selectingpoint = Random.Range(0, 3);
        transform.localPosition = new Vector3(transform.localPosition.x, -166 + (selectingpoint * 84), transform.localPosition.z);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SlotsSpinStart();
        }
    }
}
