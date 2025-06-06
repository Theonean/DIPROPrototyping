using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.BlightProtocol.Scripts
{

	public class EndOfGameManager : MonoBehaviour
	{
		public static EndOfGameManager Instance;
		[Header("UI References")]
		[SerializeField] private CanvasGroup gameOverGroup;
		[SerializeField] private TextMeshProUGUI resourcesHarvestedText;
		[SerializeField] private TextMeshProUGUI resetText;
		[SerializeField] private HarvesterTrailTracker trailTracker;

		[Header("Timings & Curves")]
		[SerializeField] private float gameOverFadeDuration = 20f;
		[SerializeField] private float respawnAnimationDuration = 2f;
		[SerializeField] private AnimationCurve respawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

		private bool _isGameOver = false;
		private bool finishedGame = false;

		private int score = 0;

        public bool isPaused = false;
        Coroutine pauseFadeRoutine;
        public CanvasGroup pauseGroup;

        private void Awake()
        {
            // Ensure there's only one instance of the FrankenGameManager
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
		{
			// hide game-over UI at start
			gameOverGroup.alpha = 0f;

			// subscribe to the harvester's death event
			Harvester.Instance.health.died.AddListener(OnPlayerDied);
			EnemyDamageHandler.enemyTypeDestroyed.AddListener(PlayerKilledEnemy);

			isPaused = true;
			TogglePause();
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

			if(FrankenGameManager.Instance.m_GameState != FrankenGameManager.GameState.GAMEOVER)
			{
                // Toggle pause with spacebar
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePause();
                }
            }
		}

        public void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
            if (pauseFadeRoutine != null)
            {
                StopCoroutine(pauseFadeRoutine);
            }
			pauseGroup.interactable = isPaused;
			pauseGroup.blocksRaycasts = isPaused;
            pauseFadeRoutine = StartCoroutine(FadeUI(pauseGroup, isPaused, 0.5f));
        }

        public void PlayerFinishedGame()
        {
            finishedGame = true;

            // fade in the game-over overlay
            StartCoroutine(FadeUI(gameOverGroup, true, gameOverFadeDuration));

            if (isPaused)
                TogglePause();

			Time.timeScale = 0;

            resourcesHarvestedText.text =
                $"time needed: " + FrankenGameManager.Instance.m_TotalGameTime + "\n Score: " + score + "\n total crystals collected " + ItemManager.Instance.totalCrystalsCollected + "\n gas collected " + ItemManager.Instance.gas;

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
			if (isPaused)
				TogglePause();
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

			// create new player trail
			trailTracker.CreateNewPath();
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
