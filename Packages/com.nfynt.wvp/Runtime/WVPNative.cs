using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Nfynt.WVP
{
    internal class WVPNative
    {
        const string PLUGIN ="__Internal";

        enum PlayerStates {
            NA= 0,
            Opening= 1,
            Buffering,
            ImageReady,
            Prepared,
            Playing,
            Paused,
            Stopped,
            EndReached,
            EncounteredError,
            TimeChanged,
            PositionChanged,
        };

#if UNITY_WEBGL

        [DllImport(PLUGIN)]
        public static extern int WVPInitialize(bool autoplay, bool loop, bool muted);
        [DllImport(PLUGIN)]
        public static extern void WVPUpdateTexture(int indx, IntPtr texId);
        [DllImport(PLUGIN)]
        public static extern void WVPSetDataSource(int indx, string path);
        [DllImport(PLUGIN)]
        public static extern bool WVPSourceIsReady(int indx);
        [DllImport(PLUGIN)]
        public static extern bool WVPSourcePlay(int indx);
        [DllImport(PLUGIN)]
        public static extern void WVPSourceStop(int indx);
        [DllImport(PLUGIN)]
        public static extern void WVPSourcePause(int indx);
        [DllImport(PLUGIN)]
        public static extern void WVPSourceRelease(int indx);
        [DllImport(PLUGIN)]
        public static extern bool WVPSourceIsPlaying(int indx);
        [DllImport(PLUGIN)]
        public static extern float WVPSourceDuration(int indx);
        [DllImport(PLUGIN)]
        public static extern bool WVPSourceIsMute(int indx);
        [DllImport(PLUGIN)]
        public static extern void WVPSourceSetMute(int indx, bool mute);
		[DllImport(PLUGIN)]
		public static extern void WVPSourceSetLoop(int indx, bool loop);
		[DllImport(PLUGIN)]
        public static extern int WVPSourceWidth(int indx);
        [DllImport(PLUGIN)]
        public static extern int WVPSourceHeight(int indx);
        [DllImport(PLUGIN)]
        public static extern double WVPSourceFrameTime(int indx);
		[DllImport(PLUGIN)]
		public static extern void WVPSourceSetFrameTime(int indx, double timeInSec);
#else
        public static int WVPInitialize(bool autoplay, bool loop, bool muted){return -1;}
        public static void WVPUpdateTexture(int indx, IntPtr texId){}
        public static void WVPSetDataSource(int indx, string path){}
        public static bool WVPSourceIsReady(int indx){return false;}
        public static bool WVPSourcePlay(int indx){return false;}
        public static void WVPSourceStop(int indx){}
        public static void WVPSourcePause(int indx){}
        public static void WVPSourceRelease(int indx){}
        public static bool WVPSourceIsPlaying(int indx){return false;}
        public static float WVPSourceDuration(int indx){return 0;}
        public static bool WVPSourceIsMute(int indx){return false;}
        public static void WVPSourceSetMute(int indx, bool mute){}
		public static void WVPSourceSetLoop(int indx, bool loop){}
        public static int WVPSourceWidth(int indx){return -1;}
        public static int WVPSourceHeight(int indx){return -1;}
        public static double WVPSourceFrameTime(int indx){return 0;}
		public static void WVPSourceSetFrameTime(int indx, double timeInSec){}
#endif
	}
}