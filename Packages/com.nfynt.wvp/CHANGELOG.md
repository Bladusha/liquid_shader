# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


## [1.0.2] - 2024-07-15
### Fixed
- Fixed IsPlayer and IsMuted API in NVideoPlayer.
- Removed explicit `anonymous` CORS setup of video player.

## [1.0.1] - 2023-10-19
### Updated
- Sample UI to look a bit nice on PC and mobile web.

### Known Issues
- Missing implementations for IsPlaying, IsMuted, and FrameCount.

## [1.0.0] - 2023-10-19
### Added
- New API for setting play time in player.
- New API to get video duration.
- Added texture resampling option for respecting source video aspect ratio.

### Updated
- Updating of video player config before starting of Play call. Earlier it was only set at the initialization.
- Update sample scene to demo new APIs and texture resampling.


## [0.1.3] - 2023-10-16
### Added
- Added new API for getting presentation time of current frame and size (width and height).

### Updated
- Sample scene to avoid auto play on start.
- Clear render texture on stop of video player.

### Fixed
- Disable fullscreen player and picture-in-picture prompt for the video, especially on Safari ios10+.


## [0.1.2] - 2022-05-06
### Fixed
- Added missing UnityVideoRenderTexture to the package

## [0.1.1] - 2022-05-06
### Added
- Test sample scene to demo the setup and test in editor and build.

## [0.1.0] - 2022-05-05
### Added
- Web Video Player with in-editor playing support.

### Known-issues
- Autoplay on start requires video to be muted. This is mainly due to Autoplay blocking policy in browsers. For more information [read here](https://docs.unity3d.com/Manual/webgl-audio.html).
- Video player audio is plays in 2D mode on browser. There doesn't seem any easy to pipe the web audio to arbitrary sink i.e. Unity.