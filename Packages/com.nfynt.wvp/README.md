# Unity Web Video Player

[![npm version](https://badge.fury.io/js/com.nfynt.wvp.svg)](https://badge.fury.io/js/com.nfynt.wvp)
[!["Buy Me A Coffee"](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/nfynt)

A video player plugin for Unity WebGL applications. Unity's builtin VideoPlayer can struggle to work with WebGL builds especially on mobile browsers. This package uses Web-native video elements to read the video source and exposes basic video controls to Unity.
You can use video URLs either from `StreamingAssets` path or web servers (permissing CORS). While minimal, the plugin aims to be a complete video player for web across browsers and platforms. Key features include:
- Start, Stop, Pause, Resume, (Un)Mute, APIs along with respective callback events.
- Resampling of source video texture to preserve aspect ratio or arbitrarly stretch to match target texture size.
- Fetch and set video player timestamps.
- Supports both in-built and scriptable rendering pipeline. 


### Importing in Unity

To install this package in Unity you'll first have to add a new Scoped Registry. Head over to `Edit>Project Settings>Package Manager` and add the new registry details as follows - 
```
Name: npmjs
URL: https://registry.npmjs.org
Scope(s): com.nfynt
```

After this head to `Window>Package Manager` and select `My Registries` under the `Packages:` options. This will list all the available package on npm, find `Web Video Player` and select the latest version available and hit install 🚀.

<u>[Project Demo](https://nfynt.github.io/com.nfynt.wvp/)</u>

### Issues
Submit issues on [Github Repo](https://github.com/nfynt/com.nfynt.wvp/issues) with appropriate label as `bug\enhancement\help wanted`.

### Authors
```
Shubham
```