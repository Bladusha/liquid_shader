using UnityEngine;
using UnityEngine.Events;
using Nfynt.WVP;

//http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4
namespace Nfynt
{
	public class NVideoPlayer : MonoBehaviour
	{
		[SerializeField, Tooltip("Video player configurations")]
		public PlayerConfigs Config = new PlayerConfigs()
		{
			PlayOnAwake = false,
			MuteAsDefault = false,
			LoopPlayer = false,
			VideoAspectRatio = UnityEngine.Video.VideoAspectRatio.Stretch
		};

		[Space(20)]

		public UnityEvent OnStarted;
		public UnityEvent OnStoped;
		public UnityEvent OnPaused;
		public UnityEvent OnResumed;

		public bool IsPlaying
		{
			get
			{
				if (m_videoPlayer == null) return false;
				return m_videoPlayer.IsPlaying();
			}
		}
		public bool IsMuted
		{
			get
			{
				if (m_videoPlayer == null) return false;
				return m_videoPlayer.IsMuted();
			}
		}
		public double CurrentFrameTimeInSec
		{
			get
			{
				if (m_videoPlayer == null) return 0;
				double time = m_videoPlayer.CurrFrameTime();
				return double.IsNaN(time) ? 0 : time;
			}
		}
		public double VideoLengthInSec
		{
			get
			{
				if (m_videoPlayer == null) return 0;
				double time = m_videoPlayer.VideoDuration();
				return double.IsNaN(time) ? 0 : time;
			}
		}
		public Vector2 FrameSize
		{
			get
			{
				if(m_videoPlayer==null || !m_videoPlayer.IsPlaying()) return Vector2.zero;
				return m_videoPlayer.FrameSize();
			}
		}
		private IVideoPlayer m_videoPlayer = null;
		private string m_videoSrc = "";
		private bool m_initialized = false;

		#region UnityFunctions
		private void Start()
		{
			GameObject player = new GameObject("_videoPlayer");
#if UNITY_EDITOR
			m_videoPlayer = player.AddComponent<UnityVideoPlayer>();
#elif UNITY_WEBGL
			m_videoPlayer = player.AddComponent<WebVideoPlayer>();
#endif
			player.transform.SetParent(transform);

			if (m_videoPlayer == null)
			{
				this.enabled = false;
				Debug.LogError("Current platform is not supported!");
				return;
			}
            if (Config.PlayOnAwake)
			{
				Debug.Log("Explicitly muting video player on start, to avoid playback error in browsers");
				Config.MuteAsDefault = true;
			}
            InitializeVideoPlayer();
			if (m_initialized && Config.PlayOnAwake && m_videoPlayer.IsPlaying())
			{
				OnStarted?.Invoke();
			}
		}

		private void OnDestroy()
		{
			ReleaseVideoPlayer();
		}

		#endregion //unity functions

		private void InitializeVideoPlayer()
		{
			m_videoSrc = WebVideoPlayer.ValidVideoPath(Config.VideoSrcPath);
			if (string.IsNullOrEmpty(m_videoSrc)) m_videoSrc = Config.VideoSrcPath;

			if (string.IsNullOrEmpty(m_videoSrc))
			{
				Debug.LogError("Invalid video path: " + Config.VideoSrcPath);
				this.enabled = false;
				return;
			}
			Debug.Log("Initializing NVideoPlayer with src: " + m_videoSrc);
			Config.VideoSrcPath = m_videoSrc;
			if (Config.AudioSource == null)
				Config.AudioSource = gameObject.AddComponent<AudioSource>();
            m_videoPlayer.InitializePlayer(Config);
			m_initialized = true;
		}
		public static void ResetRenderTexture(RenderTexture renderTexture)
		{
			RenderTexture rt = RenderTexture.active;
			RenderTexture.active = renderTexture;
			GL.Clear(true, true, Color.black);
			RenderTexture.active = rt;
		}

		#region WVPFunctions
		public void Play()
		{
			if (!m_initialized) InitializeVideoPlayer();
			m_videoPlayer?.PlayStopVideo(Config, true);
			OnStarted?.Invoke();
		}
		public void Stop()
		{
			m_videoPlayer?.PlayStopVideo(Config, false);
			OnStoped?.Invoke();
			ResetRenderTexture(Config.VideoTextureTarget);
		}
		public void Pause()
		{
			m_videoPlayer?.PauseResumeVideo(true);
			OnPaused?.Invoke();
		}
		public void Resume()
		{
			m_videoPlayer?.PauseResumeVideo(false);
			OnResumed?.Invoke();
		}
		public void Mute()
		{
			m_videoPlayer?.MuteUnmuteVideo(true);
		}
		public void Unmute()
		{
			m_videoPlayer?.MuteUnmuteVideo(false);
		}
		public void ReleaseVideoPlayer()
		{
			m_videoPlayer?.ReleasePlayer();
			m_initialized = false;
		}
		#endregion  //wvpfunctions
	}
}