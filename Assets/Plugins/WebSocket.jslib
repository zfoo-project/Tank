var WebSocketLibrary =
    {
        $webSocketBridge:
            {
                ws: null,

                /* Event listeners */
                onOpen: null,
                onMessage: null,
                onError: null,
                onClose: null

            },

        /**
         * Set onOpen callback
         *
         * @param callback Reference to C# static function
         */
        WebSocketSetOnOpen: function(callback)
        {
            webSocketBridge.onOpen = callback;
        },

        /**
         * Set onMessage callback
         *
         * @param callback Reference to C# static function
         */
        WebSocketSetOnMessage: function(callback)
        {
            webSocketBridge.onMessage = callback;
        },

        /**[JSLIB WebSocket] Connected
         * Set onError callback
         *
         * @param callback Reference to C# static function
         */
        WebSocketSetOnError: function(callback)
        {
            webSocketBridge.onError = callback;
        },

        /**
         * Set onClose callback
         *
         * @param callback Reference to C# static function
         */
        WebSocketSetOnClose: function(callback)
        {
            webSocketBridge.onClose = callback;
        },

        /**
         * Connect WebSocket to the server
         *
         */
        WebSocketConnect: function(url)
        {
            var urlStr = Pointer_stringify(url);
            console.log("JSLIB WebSocket Connect url " + urlStr);
            webSocketBridge.ws = new WebSocket(urlStr);
            webSocketBridge.ws.binaryType = 'arraybuffer';
            webSocketBridge.ws.onopen = function()
            {
                console.log("[JSLIB WebSocket] Connected " + urlStr);
                if (webSocketBridge.onOpen)
                {
                    Runtime.dynCall('v', webSocketBridge.onOpen, 0);
                }
            };

            webSocketBridge.ws.onmessage = function(eve)
            {
                if (webSocketBridge.onMessage === null)
                {
                    return;
                }

                if (eve.data instanceof ArrayBuffer)
                {
                    var dataBuffer = new Uint8Array(eve.data);
                    var buffer = _malloc(dataBuffer.length);
                    HEAPU8.set(dataBuffer, buffer);
                    try
                    {
                        Runtime.dynCall('vii', webSocketBridge.onMessage, [ buffer, dataBuffer.length ]);
                    }
                    finally
                    {
                        _free(buffer);
                    }
                }
                else
                {
                    console.log("[JSLIB WebSocket] not support message type: " + (typeof eve.data));
                }
            };

            webSocketBridge.ws.onerror = function(eve)
            {
                console.log("[JSLIB WebSocket] Error occured.");
                if (webSocketBridge.onError)
                {
                    Runtime.dynCall('v', webSocketBridge.onError, 0);
                }
            };

            webSocketBridge.ws.onclose = function(eve)
            {
                console.log("[JSLIB WebSocket] Closed, Code: " + eve.code + ", Reason: " + eve.reason);
                if (webSocketBridge.onClose)
                {
                    Runtime.dynCall('v', webSocketBridge.onClose, 0);
                }

                webSocketBridge.ws = null;
            };
            return;
        },

        /**
         * Close WebSocket connection
         *
         * @param code Close status code
         * @param reasonPtr Pointer to reason string
         */
        WebSocketClose: function()
        {
            if (webSocketBridge.ws === null)
            {
                return;
            }
            if (webSocketBridge.ws.readyState === 0 || webSocketBridge.ws.readyState === 1) {
                webSocketBridge.ws.close();
                return;
            }
        },

        /**
         * Send message over WebSocket
         *
         * @param bufferPtr Pointer to the message buffer
         * @param length Length of the message in the buffer
         */
        WebSocketSend: function(bufferPtr, length)
        {
            if (webSocketBridge.ws === null)
            {
                console.log("JSLIB Websocket是空的，没有建立连接");
                return;
            }

            if (webSocketBridge.ws.readyState !== 1)
            {
                console.log("JSLIB websocket无法连接");
                return;
            }

            try {
                var buff = HEAPU8.subarray(bufferPtr, bufferPtr + length);
                webSocketBridge.ws.send(buff);
            } catch (excepion) {
                console.log(excepion);
            }
        }
    };

autoAddDeps(WebSocketLibrary, '$webSocketBridge');
mergeInto(LibraryManager.library, WebSocketLibrary);
