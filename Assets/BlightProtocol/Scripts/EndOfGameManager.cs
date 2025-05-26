using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.BlightProtocol.Scripts
{

	public class EndOfGameManager : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private CanvasGroup gameOverGroup;
		[SerializeField] private TextMeshProUGUI resourcesHarvestedText;
		[SerializeField] private TextMeshProUGUI resetText;

		[Header("Timings & Curves")]
		[SerializeField] private float gameOverFadeDuration = 20f;
		[SerializeField] private float respawnAnimationDuration = 2f;
		[SerializeField] private AnimationCurve respawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

		private bool _isGameOver = false;
		private bool finishedGame = false;

		private int score = 0;

		private void Start()
		{
			// hide game-over UI at start
			gameOverGroup.alpha = 0f;

			// subscribe to the harvester's death event
			Harvester.Instance.health.died.AddListener(OnPlayerDied);
			EnemyDamageHandler.enemyTypeDestroyed.AddListener(PlayerKilledEnemy);
		}

		private void Update()
		{
			// if we're in game‐over and player hits R, respawn
			if ((_isGameOver || finishedGame) && Input.GetKeyDown(KeyCode.R))
			{
				if (_isGameOver)
				{
					StopAllCoroutines();
					StartCoroutine(RespawnPlayer());
				}
				else if(finishedGame)
				{
                    SceneManager.LoadScene("1_MainScene");
                }
			}
			else if (finishedGame && Input.GetKeyDown(KeyCode.E))
			{
				SceneManager.LoadScene("0_MainMenu");
			}
		}

		public void PlayerFinishedGame()
        {
            finishedGame = true;

            // fade in the game-over overlay
            StartCoroutine(FadeUI(gameOverGroup, true, gameOverFadeDuration));

            if (FrankenGameManager.Instance.isPaused)
                FrankenGameManager.Instance.TogglePause();

			Time.timeScale = 0;

            resourcesHarvestedText.text =
                $"time needed: " + FrankenGameManager.Instance.m_TotalGameTime + "\n Score: " + score + " \n total crystals collected " + ItemManager.Instance.totalCrystalsCollected + "\n gas collected " + ItemManager.Instance.gas;

            resetText.text =
                $"press 'R' to replay or 'e' to exit";
        }

		private void OnPlayerDied()
		{
			_isGameOver = true;

			// disable player controls
			PlayerCore.Instance.enabled = false;

			// update UI texts
			var dm = DifficultyManager.Instance;
			resourcesHarvestedText.text =
				$"reached region {dm.maxDifficultyReached + 1} / {dm.maximumDifficultyRegions + 1}";

			resetText.text =
				$"press 'R' to respawn in region {Harvester.Instance.respawnPointDifficultyRegion + 1}";

			// fade in the game-over overlay
			StartCoroutine(FadeUI(gameOverGroup, true, gameOverFadeDuration));

			// ensure the game is unpaused so the fade runs on unscaled time
			if (FrankenGameManager.Instance.isPaused)
				FrankenGameManager.Instance.TogglePause();
		}

		private IEnumerator FadeUI(CanvasGroup cg, bool fadeIn, float duration)
		{
			float elapsed = fadeIn ? cg.alpha * duration : (1f - cg.alpha) * duration;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				cg.alpha = fadeIn
					? elapsed / duration
					: 1f - (elapsed / duration);
				yield return null;
			}
			cg.alpha = fadeIn ? 1f : 0f;
		}

		private IEnumerator RespawnPlayer()
		{
			Vector3 target = Harvester.Instance.respawnPoint;
			Vector3 start = new Vector3(target.x, 200f, target.z);
			float t = 0f;

			// fade the gameOverGroup out while moving the harvester down
			while (t < respawnAnimationDuration)
			{
				float norm = respawnCurve.Evaluate(t / respawnAnimationDuration);
				gameOverGroup.alpha = Mathf.Lerp(1f, 0f, norm);
				Harvester.Instance.transform.position =
					Vector3.Lerp(start, target, norm);
				t += Time.deltaTime;
				yield return null;
			}

			// reset state
			gameOverGroup.alpha = 0f;
			_isGameOver = false;
			FrankenGameManager.Instance.ResetAfterRespawn();

			// re-enable player
			PlayerCore.Instance.enabled = true;
			CameraTracker.Instance.objectToTrack = PlayerCore.Instance.gameObject;
        }
        private void PlayerKilledEnemy(EnemyType type)
		{ 
			switch(type)
			{
				case EnemyType.REGULAR:
					score += 2;
					break;
                case EnemyType.CHARGER:
                    score += 5;
                    break;
                case EnemyType.CRABTANK:
                    score += 10;
                    break;
            }
        }
    }
}
