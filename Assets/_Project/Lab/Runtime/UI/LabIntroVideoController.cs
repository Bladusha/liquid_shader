using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Управляет воспроизведением видео для вводного меню.
/// Вынесен из LabIntroMenuController для уменьшения связности.
/// </summary>
public class LabIntroVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private string videoFileName = "LabIntro.mp4";
    [SerializeField] private VideoClip editorVideoClip;

    [Header("Stall Recovery")]
    [SerializeField] private bool useManualVideoClockWhenStalled = true;
    [SerializeField] private float stallThresholdSeconds = 0.35f;

    private VideoPlayer videoPlayer;
    private RenderTexture videoTexture;
    private bool videoPausedByUser;
    private bool usingManualVideoClock;
    private double lastObservedVideoTime;
    private long lastObservedVideoFrame = -1;
    private long manualVideoFrame;
    private double manualFrameAccumulator;
    private float stalledPlaybackSeconds;
    private bool videoIsPrepared;

    public event Action PlaybackStarted;
    public event Action PlaybackFinished;
    public event Action<string> ErrorOccurred;

    public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;
    public bool IsPaused => videoPausedByUser;
    public bool IsPrepared => videoIsPrepared;
    public VideoPlayer Player => videoPlayer;

    public void Initialize(VideoPlayer player)
    {
        if (player == null)
        {
            Debug.LogWarning("LabIntroVideoController.Initialize: VideoPlayer is null.", this);
            return;
        }

        videoPlayer = player;
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.errorReceived += HandleVideoError;
        videoPlayer.loopPointReached += HandleVideoFinished;
    }

    public void SetVideoFileName(string fileName)
    {
        videoFileName = NormalizeVideoFileName(fileName);
    }

    public void SetEditorVideoClip(VideoClip clip)
    {
        editorVideoClip = clip;
    }

    public void Prepare(Action onReady = null)
    {
        if (videoPlayer == null)
        {
            return;
        }

        CleanupTexture();
        videoIsPrepared = false;
        usingManualVideoClock = false;
        stalledPlaybackSeconds = 0f;
        lastObservedVideoTime = 0d;
        lastObservedVideoFrame = -1;
        manualVideoFrame = 0;
        manualFrameAccumulator = 0d;
        videoPausedByUser = false;

        string resolved = ResolveVideoPath(videoFileName);
        if (!string.IsNullOrEmpty(resolved))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = resolved;
        }
        else if (editorVideoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = editorVideoClip;
        }
        else
        {
            ErrorOccurred?.Invoke("Видеофайл не найден: " + videoFileName);
            return;
        }

        EnsureTexture();
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += HandlePrepared;
    }

    private void HandlePrepared(VideoPlayer source)
    {
        videoPlayer.prepareCompleted -= HandlePrepared;
        videoIsPrepared = true;
        videoPlayer.targetTexture = videoTexture;
        PlaybackStarted?.Invoke();
    }

    public void Play()
    {
        if (videoPlayer == null)
        {
            return;
        }
        videoPausedByUser = false;
        if (videoIsPrepared)
        {
            videoPlayer.Play();
        }
    }

    public void Pause()
    {
        if (videoPlayer == null)
        {
            return;
        }
        videoPausedByUser = true;
        videoPlayer.Pause();
    }

    public void TogglePause()
    {
        if (videoPausedByUser)
        {
            Play();
        }
        else
        {
            Pause();
        }
    }

    public void Stop()
    {
        if (videoPlayer == null)
        {
            return;
        }
        videoPlayer.Stop();
        videoPausedByUser = false;
        usingManualVideoClock = false;
        stalledPlaybackSeconds = 0f;
    }

    public void UpdatePlayback()
    {
        if (videoPlayer == null || !videoPlayer.isPlaying || videoPausedByUser)
        {
            return;
        }

        UpdateManualClock();
    }

    private void UpdateManualClock()
    {
        if (!useManualVideoClockWhenStalled)
        {
            return;
        }

        double currentTime = videoPlayer.time;
        long currentFrame = videoPlayer.frame;
        bool hasFrameInfo = videoPlayer.frameCount > 0;
        bool timeAdvanced = Mathf.Abs((float)(currentTime - lastObservedVideoTime)) > 0.0001f;
        bool frameAdvanced = currentFrame >= 0 && currentFrame != lastObservedVideoFrame;
        bool visualAdvanced = hasFrameInfo ? frameAdvanced : timeAdvanced;

        if (!usingManualVideoClock && visualAdvanced)
        {
            lastObservedVideoTime = currentTime;
            if (currentFrame >= 0)
            {
                lastObservedVideoFrame = currentFrame;
                manualVideoFrame = currentFrame;
                manualFrameAccumulator = currentFrame;
            }
            stalledPlaybackSeconds = 0f;
            return;
        }

        stalledPlaybackSeconds += Time.unscaledDeltaTime;
        if (!usingManualVideoClock && stalledPlaybackSeconds < stallThresholdSeconds)
        {
            return;
        }

        usingManualVideoClock = true;
        AdvanceManualClock(currentTime);
    }

    private void AdvanceManualClock(double currentTime)
    {
        double frameRate = videoPlayer.frameRate > 0d ? videoPlayer.frameRate : 30d;
        long frameCount = videoPlayer.frameCount > 0 ? (long)videoPlayer.frameCount : 0;

        manualFrameAccumulator = Math.Max(manualFrameAccumulator, manualVideoFrame)
            + Time.unscaledDeltaTime * frameRate;

        long nextFrame = (long)manualFrameAccumulator;
        if (frameCount > 0 && nextFrame >= frameCount)
        {
            if (videoPlayer.isLooping)
            {
                nextFrame %= frameCount;
                manualFrameAccumulator -= frameCount;
            }
            else
            {
                nextFrame = frameCount - 1;
                videoPlayer.Pause();
                HandleVideoFinished(videoPlayer);
                return;
            }
        }

        manualVideoFrame = nextFrame;
        videoPlayer.frame = manualVideoFrame;
        lastObservedVideoTime = currentTime + Time.unscaledDeltaTime;
    }

    public bool IsAtEnd()
    {
        return videoPlayer != null &&
            !videoPlayer.isLooping &&
            videoPlayer.frameCount > 0 &&
            videoPlayer.frame >= (long)videoPlayer.frameCount - 1;
    }

    private void EnsureTexture()
    {
        if (videoTexture != null)
        {
            return;
        }

        int width = (int)videoPlayer.width > 0 ? (int)videoPlayer.width : 1920;
        int height = (int)videoPlayer.height > 0 ? (int)videoPlayer.height : 1080;
        videoTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        videoTexture.Create();
    }

    private void CleanupTexture()
    {
        if (videoPlayer != null)
        {
            videoPlayer.targetTexture = null;
        }

        if (videoTexture != null)
        {
            videoTexture.Release();
            Destroy(videoTexture);
            videoTexture = null;
        }
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        ErrorOccurred?.Invoke(message);
    }

    private void HandleVideoFinished(VideoPlayer source)
    {
        PlaybackFinished?.Invoke();
    }

    public void Cleanup()
    {
        Stop();
        CleanupTexture();
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived -= HandleVideoError;
            videoPlayer.loopPointReached -= HandleVideoFinished;
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    // --- Static helpers ---

    public static string ResolveVideoPath(string fileName)
    {
        string normalized = NormalizeVideoFileName(fileName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        string streamingAssetsFile = Path.Combine(Application.streamingAssetsPath, normalized).Replace('\\', '/');
        if (!File.Exists(streamingAssetsFile))
        {
            return string.Empty;
        }

        return new Uri(streamingAssetsFile).AbsoluteUri;
    }

    public static string NormalizeVideoFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "LabIntro.mp4";
        }

        string normalized = Path.GetFileName(fileName.Trim());
        return string.IsNullOrWhiteSpace(normalized) ? "LabIntro.mp4" : normalized;
    }
}
