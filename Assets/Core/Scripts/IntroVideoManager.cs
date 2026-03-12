using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Core.Scripts
{
    [RequireComponent(typeof(VideoPlayer))]
    public class IntroVideoManager : MonoBehaviour
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private string nextSceneName = "InvestigationScene";

        private const string HasPlayedKey = "HasPlayedIntro";
        
        // Set this to true from other scripts before loading IntroScene to force a replay
        public static bool ForceReplay = false;

        private void Awake()
        {
            if (videoPlayer == null)
            {
                videoPlayer = GetComponent<VideoPlayer>();
            }

            // For prototyping: If no clip is assigned, use a sample video URL
            if (videoPlayer.clip == null && string.IsNullOrEmpty(videoPlayer.url))
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
            }

            // Ensure the video renders directly to the main camera
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            if (videoPlayer.targetCamera == null)
            {
                videoPlayer.targetCamera = Camera.main;
            }

            // Set audio output to direct
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioVolume(0, 1.0f);
        }

        private void Start()
        {
            // For testing purposes, uncomment to always play the video
            // PlayerPrefs.DeleteKey(HasPlayedKey);

            if (!ForceReplay && PlayerPrefs.GetInt(HasPlayedKey, 0) == 1)
            {
                // Already played and not forcing replay, skip to next scene immediately
                LoadNextScene();
            }
            else
            {
                // First time or forced replay, play video
                ForceReplay = false; // Reset the flag
                videoPlayer.loopPointReached += OnVideoFinished;
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += (vp) => vp.Play();
            }
        }

        private void Update()
        {
            bool shouldSkip = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    shouldSkip = true;
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
            {
                shouldSkip = true;
            }
#endif

            if (shouldSkip)
            {
                SkipVideo();
            }
        }

        public void SkipVideo()
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            OnVideoFinished(videoPlayer);
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            PlayerPrefs.SetInt(HasPlayedKey, 1);
            PlayerPrefs.Save();
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            // Check if the scene exists before loading to prevent errors during simple play testing
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning($"[IntroVideoManager] Could not load scene '{nextSceneName}'. Make sure it is added to the Build Settings.");
            }
        }
    }
}
