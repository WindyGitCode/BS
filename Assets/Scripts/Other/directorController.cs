using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class directorController : MonoBehaviour
{
    private PlayableDirector director;
    private Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        director = GetComponent<PlayableDirector>();
        director.Play();
        Invoke("FinishInvoke", (float)director.duration);
    }
    void FinishInvoke()
    {
        EventMgr.Instance.Trigger(EventConst.FinishDirection);
        Destroy(camera.gameObject);
    }
}
