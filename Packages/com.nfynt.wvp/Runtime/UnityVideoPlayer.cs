using UnityEngine;
using UnityEngine.Video;

namespace Nfynt.WVP
{
    [RequireComponent(typeof(VideoPlayer))]
    internal class UnityVideoPlayer : MonoBehaviour, IVideoPlayer
    {
        private VideoPlayer m_player = null;
        private const int AudioTrack = 0;

        public void Awake()
        {
            m_player = gameObject.GetComponent<VideoPlayer>() ?? gameObject.AddComponent<VideoPlayer>();
            m_player.source = VideoSource.Url;
            m_player.playOnAwake = false;
            m_player.isLooping = false;
            m_player.renderMode = VideoRenderMode.RenderTexture;
            m_player.aspectRatio = VideoAspectRatio.FitInside;
        }
        private void UpdateConfig(PlayerConfigs config)
		{
			m_player.source = VideoSource.Url;
			m_player.url = config.VideoSrcPath;
			m_player.targetTexture = config.VideoTextureTarget;
			m_player.audioOutputMode = VideoAudioOutputMode.AudioSource;
			config.AudioSource.mute = config.MuteAsDefault;
			m_player.SetTargetAudioSource(AudioTrack, config.AudioSource);
			m_player.isLooping = config.LoopPlayer;
			m_player.playOnAwake = config.PlayOnAwake;
			m_player.aspectRatio = config.VideoAspectRatio;
            Debug.Log("Video aspect: " + m_player.aspectRatio);
		}
        public void InitializePlayer(PlayerConfigs config)
		{
            UpdateConfig(config);
			if (config.PlayOnAwake)
                m_player.Play();
        }
        public void ReleasePlayer() { }
        public bool PlayStopVideo(PlayerConfigs config, bool play)
        {
            if (m_player == null) return false;
            if (play)
			{
				UpdateConfig(config);
				m_player.Play();
            }
            else
                m_player.Stop();
            return true;
        }
        public bool PauseResumeVideo(bool pause)
        {
            if (m_player == null) return false;
            if (pause)
                m_player.Pause();
            else
                m_player.Play();
            return true; 
        }
        public bool MuteUnmuteVideo(bool mute)
        {
            if (m_player == null) return false;
            AudioSource audSrc = m_player.GetTargetAudioSource(AudioTrack);
            if (audSrc==null) return false;

            audSrc.mute = mute;
            return true; 
        }
        public bool IsPlaying()
		{
            if (m_player == null) return false;
            return m_player.isPlaying;
		}
        public bool IsMuted()
        {
            if(m_player==null) return false;
            return m_player.GetTargetAudioSource(AudioTrack).mute;
        }
        public double VideoDuration()
        {
            if(m_player == null) return 0;
            if (m_player.source == VideoSource.VideoClip) return m_player.clip.length;
            if(!m_player.isPrepared) return 0;
            double time = m_player.frameCount / m_player.frameRate;
            return time;
        }
        public double CurrFrameTime()
        {
            if(m_player== null) return 0;
            return m_player.time;
		}
		public bool SetFrameTime(double timeInSec)
        {
            if(m_player==null) return false;
            m_player.time = timeInSec;
            return true;
        }
		public Vector2 FrameSize()
		{
			if (m_player==null) return Vector2.zero;
			return new Vector2(m_player.width, m_player.height);
		}

	}
}