var WebPlayer = {
    states: {
		NA: 0,
        Opening: 1,
        Buffering: 2,
        ImageReady: 3,
        Prepared: 4,
        Playing: 5,
        Paused: 6,
        Stopped: 7,
        EndReached: 8,
        EncounteredError: 9,
        TimeChanged: 10,
        PositionChanged: 11,
    },
    players: [],

    WVPInitialize__deps: ["states","players"],
    WVPInitialize: function(autoplay, loop, muted) {
        var player = document.createElement('video');
        //player.crossOrigin = "anonymous";
        player.preload = "auto";
        //player.controls=true;
        player.autoplay = autoplay;
        player.loop = loop;
        player.muted = muted;
        player.playsInline=true;
        player.disablepictureinpicture=true;
        //document.body.appendChild(player);

		var playerState = {
			state: _states.NA,
			valueFloat: -1,
			valueLong: -1,
			valueString: undefined,
		};
		
		var playerData = {
			path: "",
            player: player,
			started: false,
			ready: false,
			playerState: {state: _states.NA, value: undefined},
            playerStates: []
        };
		
        _players.push(playerData);

		player.oncanplay = function () {
			playerData.ready = true;
		};

        player.onwaiting = function () {
			playerData.playerStates.push({state: _states.Buffering, valueFloat: 0, valueLong: -1, valueString: undefined});
        };

        player.onpause = function () {
			if (playerData.ready && !player.ended) {
				playerData.playerStates.push({state: _states.Paused, valueFloat: -1, valueLong: -1, valueString: undefined});
			}
        };

        player.onended = function () {
			playerData.playerStates.push({state: _states.EndReached, valueFloat: -1, valueLong: -1, valueString: undefined});
        };

        player.ontimeupdate = function() {
			if (playerData.ready) {
				playerData.playerStates.push({state: _states.PositionChanged, valueFloat: player.currentTime / player.duration, valueLong: -1, valueString: undefined});
				playerData.playerStates.push({state: _states.TimeChanged, valueFloat: -1, valueLong: player.currentTime * 1000, valueString: undefined});
			}
        };
		 
        player.onerror = function (message) {
			playerData.playerStates.push({state: _states.EncounteredError, valueFloat: -1, valueLong: -1, valueString: undefined});
            var errormessage = "Undefined error";

            switch (this.error.code) {
                case 1:
                    err = "Fetching process aborted by user";
                    break;
                case 2:
                    err = "Error occurred when downloading";
                    break;
                case 3:
                    err = "Error occurred when decoding";
                    break;
                case 4:
                    err = "Audio/Video not supported";
                    break;
            }

            console.log(err + " (errorcode=" + this.error.code + ")");
        };

		return _players.length - 1;
    },

    WVPUpdateTexture__deps: ["players"],
    WVPUpdateTexture: function(indx, textureId)
    {
        GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[textureId]);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MAG_FILTER, GLctx.LINEAR);
        GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, _players[indx].player);
		GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
    },

    WVPSetDataSource__deps: ["players"],
    WVPSetDataSource: function (indx, path)
    {
        _players[indx].path = UTF8ToString(path);
    },

    WVPSourcePlay__deps: ["players"],
    WVPSourcePlay: function (indx)
    {
		if (!_players[indx].started) {
			_players[indx].player.src = _players[indx].path;
			_players[indx].player.load();
			_players[indx].started = true;
		}
		
        _players[indx].player.play();
		return true;
    },

    WVPSourcePause__deps: ["players"],
    WVPSourcePause: function (indx)
    {
        _players[indx].player.pause();
    },

    WVPSourceStop__deps: ["players"],
    WVPSourceStop: function(indx)
    {
        if(_players[indx].started)
        {
            _players[indx].ready=false;
            _players[indx].started=false;
            var pl = _players[indx].player;
            pl.pause();
            _players[indx].playerStates.push({state: _states.Stopped, valueFloat:-1, valueLong: -1, valueString: undefined});
        }
    },

    WVPSourceRelease__deps: ["players"],
    WVPSourceRelease: function(indx)
    {
        var player = _players[indx].player;
        if(player.parentNode!=null)
        {
            player.parentNode.removeChild(vid);
        }
        _players[indx].player=null;
    },

    WVPSourceIsPlaying__deps:["players"],
    WVPSourceIsPlaying: function(indx)
    {
        var player = _players[indx].player;
        return !(player.paused || player.ended || player.seeking);
    },
    
    WVPSourceIsReady__deps: ["players"],
    WVPSourceIsReady: function(indx)
    {
        return _players[indx].ready;
    },

    WVPSourceDuration__deps: ["players"],
    WVPSourceDuration: function(indx)
    {
        return _players[indx].player.duration;
    },

    WVPSourceIsMute__deps: ["players"],
    WVPSourceIsMute: function(indx)
    {
        return _players[indx].player.muted;
    },
    
    WVPSourceSetMute__deps: ["players"],
    WVPSourceSetMute: function(indx, mute)
    {
        _players[indx].player.muted = mute;
    },

    WVPSourceSetLoop__deps: ["players"],
    WVPSourceSetLoop: function(indx, loop)
    {
        return _players[indx].player.loop = loop;
    },
    
    WVPSourceWidth__deps: ["players"],
    WVPSourceWidth: function(indx)
    {
        return _players[indx].player.videoWidth;
    },
    
    WVPSourceHeight__deps: ["players"],
    WVPSourceHeight: function(indx)
    {
        return _players[indx].player.videoHeight;
    },
    
    WVPSourceFrameTime__deps: ["players"],
    WVPSourceFrameTime: function(indx)
    {
        return _players[indx].player.currentTime;
    },
    
    WVPSourceSetFrameTime__deps: ["players"],
    WVPSourceSetFrameTime: function(indx, time)
    {
        _players[indx].player.currentTime = time;
    },

    WVPGetPlayerState__deps: ["states","players"],
    WVPGetPlayerState: function(indx)
    {
        var states = _players[indx].playerStates;
        if(states.length>0)
        {
            _players[indx].playerState = states.shift();
            return _players[indx].playerState.state;
        }
        return _states.Empty;
    },
};

autoAddDeps(WebPlayer, "states");
autoAddDeps(WebPlayer, "players");
mergeInto(LibraryManager.library, WebPlayer);