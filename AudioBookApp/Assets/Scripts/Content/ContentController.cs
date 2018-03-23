using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterTextItem
{
    CharacterText_Left,
    CharacterText_Right,
};

public class ContentController : MonoBehaviour {

	public GameObject contentPanel;
	public GameObject itemPrefab;
    private bool characterText_Left = true;
	// Use this for initialization
	void Start () {
        /*
        bool characterText_Left = true;
        //string itemToSelect;
        CharacterTextItem itemToSelect;
		for (int i = 0; i < 10; i++) {
            itemToSelect = (characterText_Left) ? CharacterTextItem.CharacterText_Left : CharacterTextItem.CharacterText_Right;

            GameObject item = Instantiate (Resources.Load("00_Prefabs/"+itemToSelect.ToString(), typeof(GameObject))) as GameObject;
			item.transform.SetParent(contentPanel.transform, false);
            characterText_Left = !characterText_Left;

        }
        */
	}
	
	// Update is called once per frame
	void Update () {


        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayNewCharacterDialog();
        }
	}

    void DisplayNewCharacterDialog()
    {
        CharacterTextItem itemToSelect;
        itemToSelect = (characterText_Left) ? CharacterTextItem.CharacterText_Left : CharacterTextItem.CharacterText_Right;
        GameObject item = Instantiate(Resources.Load("00_Prefabs/" + itemToSelect.ToString(), typeof(GameObject))) as GameObject;
        item.transform.SetParent(contentPanel.transform, false);
        characterText_Left = !characterText_Left;
    }
}
