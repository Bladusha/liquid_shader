using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nfynt.WVP
{
    internal class WebVideoPlayer : MonoBehaviour, IVideoPlayer
    {
        private bool m_playerActive = false;
        private int m_playerIndx = -1;
        private RenderTexture m_targetTex;
        private Texture2D m_targetTex2D;
        private UnityEngine.Video.VideoAspectRatio m_videoAspectRatio;
        private Rect m_sampleRect = new Rect(0,0,1,1);

		public static string ValidVideoPath(string path)
		{
			if (path.Contains("://"))
			{
				//web url: https://; rtsp://; file:// etc
				//Todo implement accessibility test?
				return path;
			}
#if UNITY_EDITOR
			if (System.IO.File.Exists(path)) return path;
			path = System.IO.Path.Combine(Application.streamingAssetsPath, path);
			if (System.IO.File.Exists(path))
			{
				path = "file://" + path;
				return path;
			}
#elif UNITY_WEBGL
			path = System.IO.Path.Combine(Application.streamingAssetsPath, path);
			return path;
#endif
			return "";
		}

		private void UpdateConfig(PlayerConfigs config, string url)
		{
			m_targetTex = config.VideoTextureTarget;
			m_videoAspectRatio = config.VideoAspectRatio;
            if (!string.IsNullOrEmpty(url))
                WVPNative.WVPSetDataSource(m_playerIndx, url);
            WVPNative.WVPSourceSetMute(m_playerIndx, config.MuteAsDefault);
			WVPNative.WVPSourceSetLoop(m_playerIndx, config.LoopPlayer);
		}
        public void InitializePlayer(PlayerConfigs config)
        {
            m_playerIndx = WVPNative.WVPInitialize(config.PlayOnAwake, config.LoopPlayer, config.MuteAsDefault);
            UpdateConfig(config, config.VideoSrcPath);
            Debug.Log("NVideoPlayer initialized. Indx: " + m_playerIndx);
            if (config.PlayOnAwake)
            {
                PlayStopVideo(config, true);
            }
        }
    
        public void ReleasePlayer()
		{
            Debug.Log("Releasing video player indx: " + m_playerIndx);
            WVPNative.WVPSourceRelease(m_playerIndx);
		}
        public bool PlayStopVideo(PlayerConfigs config, bool play) 
        {
            m_playerActive = play;
            if (play)
            {
                UpdateConfig(config, ValidVideoPath(config.VideoSrcPath));
                WVPNative.WVPSourcePlay(m_playerIndx);
                StartCoroutine(TextureUpdateLoop());
            }
            else
            {
                StopCoroutine(TextureUpdateLoop());
                WVPNative.WVPSourceStop(m_playerIndx);
            }
            return true; 
        }
        public bool PauseResumeVideo(bool pause) 
        {
            if (pause)
                WVPNative.WVPSourcePause(m_playerIndx);
            else
                WVPNative.WVPSourcePlay(m_playerIndx);
            return true; 
        }
        public bool MuteUnmuteVideo(bool mute) 
        {
            WVPNative.WVPSourceSetMute(m_playerIndx, mute);
            return true;
        }

        public bool IsPlaying() => WVPNative.WVPSourceIsPlaying(m_playerIndx);
        public bool IsMuted() => WVPNative.WVPSourceIsMute(m_playerIndx);
        public double VideoDuration()
        {
            if(m_playerIndx<0) return 0;
            return WVPNative.WVPSourceDuration(m_playerIndx);
        }
        public double CurrFrameTime()
        {
            if(!m_playerActive) return 0;
            return WVPNative.WVPSourceFrameTime(m_playerIndx);
        }

		public bool SetFrameTime(double timeInSec)
		{
			if (m_playerIndx < 0) return false;
			WVPNative.WVPSourceSetFrameTime(m_playerIndx,timeInSec);
			return true;
		}
		public Vector2 FrameSize()
        {
            if(!m_playerActive) return Vector2.zero;
            return new Vector2(WVPNative.WVPSourceWidth(m_playerIndx),WVPNative.WVPSourceHeight(m_playerIndx));
        }

        private IEnumerator TextureUpdateLoop()
        {
            while (!WVPNative.WVPSourceIsReady(m_playerIndx))
			{
                yield return new WaitForEndOfFrame();
                Debug.Log("Loading video...");
			}

            int width = WVPNative.WVPSourceWidth(m_playerIndx);
            int height = WVPNative.WVPSourceHeight(m_playerIndx);
            Debug.Log("Vid Res: " + width + "x" + height);

            if (m_targetTex2D == null || m_targetTex2D.width != width || m_targetTex2D.height != height)
            {
                m_targetTex2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
                m_targetTex2D.filterMode = FilterMode.Bilinear;
                m_targetTex2D.Apply();
            }
            
            UpdateSampleRect(width, height, m_targetTex.width, m_targetTex.height, m_videoAspectRatio);

            while (m_playerActive)
            {
                yield return new WaitForEndOfFrame();
                if (IsPlaying())
                {
                    WVPNative.WVPUpdateTexture(m_playerIndx, m_targetTex2D.GetNativeTexturePtr());
                    RenderTexture at = RenderTexture.active;
                    //RenderTexture.active = m_targetTex;
                    //Graphics.Blit(m_targetTex2D, m_targetTex);
                    NfyntGPUUpdateRTT(m_targetTex2D, m_targetTex);
                    RenderTexture.active = at;
                }
			}
		}

        private void NfyntGPUUpdateRTT(Texture2D src, RenderTexture dstTex)
        {
			//Set the RTT for rendering
			Graphics.SetRenderTarget(dstTex);
            //RenderTexture.active = dstTex;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, 1, 1, 0);
            GL.Clear(true,true,new Color(0,0,0,0));
			Graphics.DrawTexture(m_sampleRect, src);
            GL.PopMatrix();
        }

        private void UpdateSampleRect(float srcWidth, float srcHeight, float dstWidth, float dstHeight, UnityEngine.Video.VideoAspectRatio arMode)
		{
			if (arMode == UnityEngine.Video.VideoAspectRatio.NoScaling)
			{
				m_sampleRect = new Rect(0, 0, srcWidth / dstWidth, srcHeight / dstHeight);
			}
			else if (arMode == UnityEngine.Video.VideoAspectRatio.FitVertically)
			{
				float srcAR = srcWidth / srcHeight;
                m_sampleRect = new Rect(0, 0, (dstHeight * srcAR) / dstWidth, 1);
				//Center the texture
				m_sampleRect = new Rect((1.0f - m_sampleRect.width) / 2.0f, (1.0f - m_sampleRect.height) / 2.0f, m_sampleRect.width, m_sampleRect.height);
			}
			else if (arMode == UnityEngine.Video.VideoAspectRatio.FitHorizontally)
			{
				float srcAR = srcWidth / srcHeight;
                m_sampleRect = new Rect(0, 0, 1, (dstWidth / srcAR) / dstHeight);
				//Center the texture
				m_sampleRect = new Rect((1.0f - m_sampleRect.width) / 2.0f, (1.0f - m_sampleRect.height) / 2.0f, m_sampleRect.width, m_sampleRect.height);
			}
			else if (arMode == UnityEngine.Video.VideoAspectRatio.FitInside)
			{
                if(srcWidth/dstWidth > srcHeight / dstHeight)   //Fit horizontally
				{
					float srcAR = srcWidth / srcHeight;
					m_sampleRect = new Rect(0, 0, 1, (dstWidth / srcAR) / dstHeight);
                }
                else //Fit vertically
				{
					float srcAR = srcWidth / srcHeight;
					m_sampleRect = new Rect(0, 0, (dstHeight * srcAR) / dstWidth, 1);
				}
                //Center the texture
				m_sampleRect = new Rect((1.0f - m_sampleRect.width) / 2.0f, (1.0f - m_sampleRect.height) / 2.0f, m_sampleRect.width, m_sampleRect.height);
			}
			else if (arMode == UnityEngine.Video.VideoAspectRatio.FitOutside)
			{
				if (srcWidth / dstWidth > srcHeight / dstHeight)   //Fit vertically so there's no border
				{
					float srcAR = srcWidth / srcHeight;
					m_sampleRect = new Rect(0, 0, (dstHeight * srcAR) / dstWidth, 1);
				}
				else //Fit horizontally so there's no border
				{
					float srcAR = srcWidth / srcHeight;
					m_sampleRect = new Rect(0, 0, 1, (dstWidth / srcAR) / dstHeight);
				}
				//Center the texture
				m_sampleRect = new Rect((1.0f - m_sampleRect.width) / 2.0f, (1.0f - m_sampleRect.height) / 2.0f, m_sampleRect.width, m_sampleRect.height);
			}
			else  //stretch mode
                m_sampleRect = new Rect(0,0,1,1);
            //Debug.Log("Sample rect: " + m_sampleRect);
		}
	}
}