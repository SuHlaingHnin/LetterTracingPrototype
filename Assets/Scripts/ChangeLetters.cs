using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagneticScrollView;

public class ChangeLetters : MonoBehaviour 
{
    public List<OrderController> letters;
    public Button btnNextLevel;
    public Button btnRight_Next;
    public Button btnLeft_Prev;
    public Text txtCurrentLetter;
    public Text txtCurrentLevel;
    public Text txtCharCount;
    public int m_index;
    public GameObject animObj;
    private enum E_Direction
    {
        Left, Right
    }
    public static ChangeLetters m_ChangeLetters;
    private string[] chars = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    void Awake()
    {
        m_ChangeLetters = this;
        animObj.SetActive(false);
        onClickChangeSelectedIndex(E_Direction.Right);
        
        // StartCoroutine(On_OffStarPanel());
    }
    void Update()
    {
        ChangeUIValues(m_index);
    }
    private void onClickChangeSelectedIndex(E_Direction direction)
    {
        switch (direction)
        {
            case E_Direction.Left:
                m_index--;
                break;
            case E_Direction.Right:
                m_index++;
                break;
            default:
                m_index--;
                break;
        }
        if (m_index >= 26)
        {
            m_index = 26;
        }
        else if (m_index <= 0)
        {
            m_index = 1;
        }
        ChangeUIValues(m_index);
    }
    public void ChangeUIValues(int index)
    {
        txtCurrentLevel.text = "Level " + index.ToString();
        txtCharCount.text = index.ToString() + "/26";
        string str = "\"" + chars[index - 1] + "\"";
        txtCurrentLetter.text = "Select Letter " + str;
    }
    public void IncreaeIndex()
    {
        onClickChangeSelectedIndex(E_Direction.Right);
        ShowStars();
    }
    public void DecreaseIndex()
    {
        onClickChangeSelectedIndex(E_Direction.Left);
        ShowStars();
    }

    public void ShowStars() {
      // Show stars for finish letters 
      
      if(m_index <= letters.Count && letters[m_index-1].success) {
        animObj.SetActive(true);           
        StartCoroutine(On_OffStarPanel());
      } else {
        animObj.SetActive(false);
      }
    }

    public IEnumerator On_OffStarPanel()
    {
        animObj.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        animObj.SetActive(true);
    }
}
