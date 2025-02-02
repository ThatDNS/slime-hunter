using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Leveltransition : MonoBehaviour
{
    public Animator transition;

    public float transitionTime = 1f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            LoadNextLevel();
        }

    }

    public void LoadNextLevel()
    {
      StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));  
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSecondsRealtime(transitionTime);
        SceneManager.LoadScene(levelIndex); 
    }
}
