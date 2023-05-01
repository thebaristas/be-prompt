using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
  public void Restart()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }

  public void Quit()
  {
    Application.Quit();
  }

  public void LevelSelect()
  {
    SceneManager.LoadScene("MainMenu");
  }
}
