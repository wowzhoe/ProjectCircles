using UnityEngine;

public class Fps : PersistentMonoBehaviour<Fps>
{
    float timeA;
    public int fps;
    public int lastFPS;

    // Use this for initialization
    public void Start()
    {
        timeA = Time.timeSinceLevelLoad;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad - timeA <= 1)
        {
            fps++;
        }
        else
        {
            lastFPS = fps + 1;
            timeA = Time.timeSinceLevelLoad;
            fps = 0;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 75, 100, 20), "FPS: " + lastFPS);     
    }
}
