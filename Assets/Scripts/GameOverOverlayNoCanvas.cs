using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverOverlayController : MonoBehaviour
{
    [Header("Controller Settings")]
    public string horizontalAxis = "Horizontal";
    public string jumpButton = "Jump";
    public float controllerDeadZone = 0.2f;
    [Header("Restart")]
    public string restartSceneName = "Dungeon_Level_2";

    [Header("Optional Prompt")]
    public GameObject pressSpacePrompt;
    public float promptDelay = 0.5f;

    private bool canRestart;

    private void OnEnable()
    {
        canRestart = false;

        if (pressSpacePrompt != null)
            pressSpacePrompt.SetActive(false);

        StartCoroutine(EnableRestartAfterDelay());
    }

    private IEnumerator EnableRestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(promptDelay);

        if (pressSpacePrompt != null)
            pressSpacePrompt.SetActive(true);

        canRestart = true;
    }

    private void Update()
    {
        if (!canRestart) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown(jumpButton))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(restartSceneName, LoadSceneMode.Single);
        }
    }
}