using UnityEngine;

namespace Nfynt.WVP
{
	[System.Serializable]
	public struct PlayerConfigs
	{
		[UnityEngine.Tooltip("Default video path of StreamingAssets is assumed if the path doesn't contain :/. Otherwise provide full system path or URL.")]
		public string VideoSrcPath;
		[UnityEngine.Tooltip("Target RenderTexture which is used in scene.")]
		public UnityEngine.RenderTexture VideoTextureTarget;
		[UnityEngine.Tooltip("Unity AudioSource component. If null then a new audio source is attached to this object.")]
		public UnityEngine.AudioSource AudioSource;
		[UnityEngine.Tooltip("Autoplay video on start. Note the audio source is muted on browser in this mode.")]
		public bool PlayOnAwake;
		[UnityEngine.Tooltip("True by default on web build. Useful for editor testing.")]
		public bool MuteAsDefault;
		[UnityEngine.Tooltip("Player video in loop")]
		public bool LoopPlayer;
		[Tooltip("Mapping for video texture to unity RTT")]
		public UnityEngine.Video.VideoAspectRatio VideoAspectRatio;
	};

	internal interface IVideoPlayer
    {
		public void InitializePlayer(PlayerConfigs config);
		public void ReleasePlayer();
        public bool PlayStopVideo(PlayerConfigs config, bool play);
        public bool PauseResumeVideo(bool pause);
        public bool MuteUnmuteVideo(bool mute);
		public bool IsPlaying();
		public bool IsMuted();
		public double VideoDuration();
		public double CurrFrameTime();
		public bool SetFrameTime(double timeInSec);
		public Vector2 FrameSize();
    }
}