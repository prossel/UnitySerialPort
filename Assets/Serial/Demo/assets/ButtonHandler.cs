/** Use this script on a GUIText gameObject, acting as a placholder, to transform it at runtime into a Button.
 * Configure the action(s) for the click event and you have an easy way to setup an interface
 * visually by placing elements on a screen.
 * 
 * Pierre Rossel, 2014-01-24
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ButtonHandler : MonoBehaviour
{

	public string onClickLoadScene;
	public string onClickSendMessage;
	public string onClickBroadcastMessage;
	public string onClickSendMessageUpwards;
	public GameObject MessageTarget;

	void Start ()
	{
		GetComponent<Button> ().onClick.AddListener (OnClick);
	}

	void OnClick ()
	{
		if (onClickLoadScene != null)
			SceneManager.LoadScene (onClickLoadScene);
		
		GameObject target = MessageTarget;
		if (target == null)
			target = gameObject;
		
		if (onClickSendMessage != "")
			target.SendMessage (onClickSendMessage, this);
		if (onClickBroadcastMessage != "")
			target.BroadcastMessage (onClickBroadcastMessage, this);
		if (onClickSendMessageUpwards != "")
			target.SendMessageUpwards (onClickSendMessageUpwards, this);
	}

	//	void OnGUI ()
	//	{
	//
	//		if (GUI.Button (rect, gameObject.GetComponent<GUIText>().text, guiStyle)) {
	//			if (onClickLoadScene != null)
	//				SceneManager.LoadScene (onClickLoadScene);
	//
	//			GameObject target = MessageTarget;
	//			if (target == null)
	//				target = gameObject;
	//
	//			if (onClickSendMessage != "")
	//				target.SendMessage (onClickSendMessage, this);
	//			if (onClickBroadcastMessage != "")
	//				target.BroadcastMessage (onClickBroadcastMessage, this);
	//			if (onClickSendMessageUpwards != "")
	//				target.SendMessageUpwards (onClickSendMessageUpwards, this);
	//		}
	//	}
}
