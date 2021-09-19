using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderController : MonoBehaviour
{
    public GameObject starPanel;
    public ChangeLetters changeLetter;
    public List<Slider> sliders = new List<Slider>();
    public int pathCount = 0;
    public bool success = false;

    // Start is called before the first frame update
    void Start()
    {
      ShowSlider(pathCount);
    }

    // Update is called once per frame
    void Update()
    {
      if(success) return;

      if(pathCount < sliders.Count && sliders[pathCount].finish) {
        Debug.Log("Show Path");
        pathCount++;
        ShowSlider(pathCount);
      } else if(pathCount == sliders.Count) {
        Debug.Log("Finish");
        pathCount = 0;
        success = true;
        starPanel.SetActive(true);
        // changeLetter.On_OffStarPanel();
      }
    }

    void ShowSlider(int index) {
      for(int i = 0; i < sliders.Count; i++) {
        if(i == index) sliders[i].gameObject.SetActive(true);
        else sliders[i].gameObject.SetActive(false);
      }
    }
}
