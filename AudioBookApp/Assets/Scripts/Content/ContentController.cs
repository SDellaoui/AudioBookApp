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

    
    private ScrollRect contentScrollRect;
	private bool characterText_Left = true;

	//booleans for preventing display spam
	private bool m_isDisplayingNewCharacterDialog = false;
	private bool m_isCoroutineDisplayRunning = false;
	private bool m_isCoroutineScrollRunning = false;

    // Use this for initialization
    void Awake()
    {
		//get content scroll rect component
        contentScrollRect = contentViewPort.GetComponent<ScrollRect>();
    }
	void Start () {
		
        if (displayFadeinTime == 0f)
            displayFadeinTime += 0.01f;
	}
	
	// Update is called once per frame
	void Update () {
		//Allow Adding dialog only once at a time
		if(!m_isCoroutineScrollRunning && !m_isCoroutineDisplayRunning && m_isDisplayingNewCharacterDialog)
			m_isDisplayingNewCharacterDialog = false;
	}

    public void DisplayNewCharacterDialog(string text)
    {
		//prevent multiple display
		if(m_isDisplayingNewCharacterDialog)
			return;
		
		// alternate between left and right display
        CharacterTextItem itemToSelect;
        itemToSelect = (characterText_Left) ? CharacterTextItem.CharacterText_Left : CharacterTextItem.CharacterText_Right;

		//get item prefab
        GameObject item = Instantiate(Resources.Load("00_Prefabs/" + itemToSelect.ToString(), typeof(GameObject))) as GameObject;
        item.transform.Find("Text").GetComponent<Text>().text = text;
        item.transform.SetParent(contentPanel.transform, false);
		//ForceMode rebuild content height
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel.GetComponent<RectTransform>());

		//Use coroutine for fade display
        StartCoroutine("DisplayCharacterDialog", item);

		//avoid multiple adding
		m_isDisplayingNewCharacterDialog = true;

		//switch for the next dialog display
        characterText_Left = !characterText_Left;
    }


    IEnumerator DisplayCharacterDialog(GameObject item)
    {
		m_isCoroutineDisplayRunning = true;

        CanvasGroup itemCanvasGroup = item.GetComponent<CanvasGroup>();
        float elapsedTime = 0f;
        float scrollPosition = contentScrollRect.verticalNormalizedPosition;

        while (itemCanvasGroup.alpha < 1)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime/displayFadeinTime);
            itemCanvasGroup.alpha = (float)System.Math.Round(alpha, 2);

			if(itemCanvasGroup.alpha > 0.5f)
				StartCoroutine("ScrollToBottom");

        }
		StopCoroutine("DisplayCharacterDialog");
		m_isCoroutineDisplayRunning = false;
		yield return null; 
    }

    IEnumerator ScrollToBottom()
    {
		m_isCoroutineScrollRunning = true;
        float elapsedTime = 0f;
		float scrollPosition = contentScrollRect.verticalNormalizedPosition;
		float scrollValue = 1f;
		while (scrollValue > 0)
        {
            elapsedTime += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
            scrollValue = Mathf.Lerp(scrollPosition, 0, elapsedTime / displayFadeinTime);
			contentScrollRect.verticalNormalizedPosition = scrollValue;
			//contentScrollRect.verticalScrollbar.value = scrollValue;
        }
		StopCoroutine("ScrollToBottom");
		m_isCoroutineScrollRunning = false;
		yield return null;
    }
}
