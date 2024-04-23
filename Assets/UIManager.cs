using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    // slider
    public Slider slider;
    public icosphere icosphere;
    // Start is called before the first frame update
    void Start()
    {
        if (slider == null){
            slider = transform.GetChild(0).GetComponent<Slider>();
        }
        slider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void ValueChangeCheck()
    {
        Debug.Log(slider.value);
        icosphere.subdivisions = (int)slider.value;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
