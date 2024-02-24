using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClickButtonPlay()
    {
        Application.LoadLevel("Game");
    }

    public void MyLoopListener(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        
    }
}
