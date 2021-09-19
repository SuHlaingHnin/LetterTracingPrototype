using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Slider : MonoBehaviour, IDragHandler
{
  public Image fillArea;
  public Transform start;
  public Transform end;

  public List<Vector2[]> polygons = new List<Vector2[]>();

  public bool onTrigger = false;
  public bool finish = false;
  public float pathDistance;
  public float currentDistance;

  void Start() {
    
    pathDistance = Vector3.Distance(transform.position, end.position);
    currentDistance = Vector3.Distance(transform.position, end.position);
  }

  public void OnDrag(PointerEventData eventData) {  
    currentDistance = Vector3.Distance(start.position, transform.position);

    Vector2 position = transform.localPosition;
    Vector2 newPos = position + eventData.delta;
    transform.localPosition = newPos;
    
    if(onTrigger && !finish) {      
      float fillAmount = Mathf.Clamp(currentDistance / pathDistance, 0, 1);  
      fillArea.fillAmount = fillAmount;
    }
  }

  void Update() {
    if(fillArea.fillAmount == 1) {
      finish = true;
    }
  }

  void OnTriggerStay2D(Collider2D other)
  {
    if(other.tag == "Fill") {
      onTrigger = true;
    }
  }

  void OnTriggerExit2D(Collider2D other)
  {
    if(other.tag == "Fill") {
      onTrigger = false;
    }

    if(other.name == end.name) {
      finish = true;

      fillArea.fillAmount = 1;
    }
  }
}
