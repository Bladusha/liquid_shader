using UnityEngine;
using UnityEngine.UI;
using Nfynt;
using System;

public class TestWVP : MonoBehaviour
{
	public NVideoPlayer VidPlayer = null;
	public Text TimerTxt = null;

	private bool m_isPlaying = false;
	private bool m_isMuted = false;

	private void Start()
	{
		m_isMuted = VidPlayer.Config.MuteAsDefault;
		m_isPlaying = VidPlayer.Config.PlayOnAwake;
	}

	private void LateUpdate()
	{
		if(m_isPlaying)
		{
			TimeSpan curr = TimeSpan.FromSeconds(VidPlayer.CurrentFrameTimeInSec);
			TimeSpan tot = TimeSpan.FromSeconds(VidPlayer.VideoLengthInSec);
			TimerTxt.text =  curr.ToString(@"hh\:mm\:ss\:ff")+ " / " + tot.ToString(@"hh\:mm\:ss\:ff");
		}
	}

	public void SetSourceUrl(InputField urlInp)
	{
		VidPlayer.Stop();
		VidPlayer.Config.VideoSrcPath = urlInp.text;
	}

	public void PlayPauseBtn(Button btn)
	{
		m_isPlaying = !m_isPlaying;
		if (m_isPlaying)
		{
			VidPlayer.Play();
			btn.GetComponentInChildren<Text>().text = "Pause";
		}
		else
		{
			VidPlayer.Pause();
			btn.GetComponentInChildren<Text>().text = "Play";
		}
	}

	public void StopBtn(Button playBtn)
	{
		playBtn.GetComponentInChildren<Text>().text = "Play";
		VidPlayer.Stop();
		m_isPlaying = false;
	}

	public void UpdateVideoAspect(Dropdown arDropdown)
	{
		VidPlayer.Config.VideoAspectRatio = (UnityEngine.Video.VideoAspectRatio)arDropdown.value;
		Debug.Log("New aspect type: "+VidPlayer.Config.VideoAspectRatio);
	}

	public void MuteUnmuteBtn(Button btn)
	{
		if (m_isMuted)
		{
			VidPlayer.Unmute();
			btn.GetComponentInChildren<Text>().text = "Mute";
		}
		else
		{
			VidPlayer.Mute();
			btn.GetComponentInChildren<Text>().text = "Unmute";
		}
		m_isMuted = !m_isMuted;
	}
}
