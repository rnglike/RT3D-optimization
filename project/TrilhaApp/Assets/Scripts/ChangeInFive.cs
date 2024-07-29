using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChangeInFive : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChangeScene());   
    }

    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(8);
        SceneManager.LoadSceneAsync(1);
    }
}
