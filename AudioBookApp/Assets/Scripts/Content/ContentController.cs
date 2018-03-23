using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterTextItem
{
    CharacterText_Left,
    CharacterText_Right,
};

public class ContentController : MonoBehaviour {

	public GameObject contentPanel;
    public GameObject contentViewPort;
    public float displayFadeinTime;

    private bool characterText_Left = true;
    private ScrollRect contentScrollRect;
    // Use this for initialization
    void Awake()
    {
        contentScrollRect = contentViewPort.GetComponent<ScrollRect>();
    }
	void Start () {
        if (displayFadeinTime == 0f)
            displayFadeinTime += 0.01f;
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void DisplayNewCharacterDialog()
    {
        CharacterTextItem itemToSelect;
        itemToSelect = (characterText_Left) ? CharacterTextItem.CharacterText_Left : CharacterTextItem.CharacterText_Right;
        GameObject item = Instantiate(Resources.Load("00_Prefabs/" + itemToSelect.ToString(), typeof(GameObject))) as GameObject;
        item.transform.SetParent(contentPanel.transform, false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());
        StartCoroutine("DisplayCharacterDialog", item);
        characterText_Left = !characterText_Left;
    }
    IEnumerator DisplayCharacterDialog(GameObject item)
    {
        CanvasGroup itemCanvasGroup = item.GetComponent<CanvasGroup>();
        float elapsedTime = 0f;
        float scrollPosition = contentScrollRect.verticalNormalizedPosition;
        while (itemCanvasGroup.alpha < 1)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime/displayFadeinTime);
            itemCanvasGroup.alpha = (float)System.Math.Round(alpha, 2);
            float scrollValue = Mathf.Lerp(scrollPosition, 0, elapsedTime / displayFadeinTime);
            contentScrollRect.verticalNormalizedPosition = scrollValue;
        }
        StartCoroutine("ScrollToBottom");
        yield return null;
    }
    IEnumerator ScrollToBottom()
    {
        float elapsedTime = 0f;
        float scrollPosition = contentScrollRect.verticalNormalizedPosition;
        while (scrollPosition > 0)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.05f);
            float scrollValue = Mathf.Lerp(scrollPosition, 0, elapsedTime / (displayFadeinTime*4));
            contentScrollRect.verticalNormalizedPosition = scrollValue;
        }
        Debug.Log("Fini");
        yield return null;
    }
}
