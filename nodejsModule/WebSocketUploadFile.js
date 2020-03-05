"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * 如果要实现断点续传，并且WebSocketUploadFile是一个新的实例化对象，可以通过设置tranId和serverReceived属性来实现
 * 如果是同一个WebSocketUploadFile对象，在传输过程中发生错误中断，只需要再次调用该对象的upload方法即可
 * */
var WebSocketUploadFile = /** @class */ (function () {
    /**
     *
     * @param fileEle
     * @param tranId
     * @param serverUrl 服务器地址，如：http://www.test.com，如果为空，则以location.href为准
     */
    function WebSocketUploadFile(fileEle, serverUrl) {
        if (serverUrl === void 0) { serverUrl = undefined; }
        /**事务id，相同的事务id，可以实现断点续传*/
        this.tranId = "";
        /**服务器已经接收的数量*/
        this.serverReceived = 0;
        this.isUploading = false;
        this.readedPosition = 0;
        this.element = fileEle;
        if (!serverUrl) {
            if (location.port)
                serverUrl = location.protocol + "//" + location.hostname + ":" + location.port + "/";
            else
                serverUrl = location.protocol + "//" + location.hostname + "/";
        }
        else {
            if (serverUrl.indexOf("/") != serverUrl.length - 1)
                serverUrl += "/";
        }
        serverUrl = serverUrl.toLocaleLowerCase();
        if (serverUrl.indexOf("http://") == 0) {
            this.serverUrl = "ws://";
            serverUrl = serverUrl.replace("http://", "");
        }
        else if (serverUrl.indexOf("https://") == 0) {
            this.serverUrl = "wss://";
            serverUrl = serverUrl.replace("https://", "");
        }
        this.serverUrl += serverUrl;
        if (this.serverUrl.indexOf("?") > 0)
            this.serverUrl += "&WebSocketUploadFile=1";
        else
            this.serverUrl += "?WebSocketUploadFile=1";
        console.log("WebSocket Address:" + this.serverUrl);
    }
    WebSocketUploadFile.prototype.initWebSocket = function () {
        var _this = this;
        this.webSocket = new WebSocket(this.serverUrl);
        var originalType = this.webSocket.binaryType;
        this.reader = undefined;
        this.webSocket.onerror = function (ev) {
            _this.onSocketError();
        };
        this.webSocket.onclose = function () {
            _this.onSocketError();
        };
        this.webSocket.onopen = function () {
            _this.webSocket.send(JSON.stringify({
                filename: _this.file.name,
                length: _this.file.size,
                position: _this.serverReceived,
                tranid: _this.tranId,
                state: _this.state,
                auth: _this.auth
            }));
        };
        this.webSocket.onmessage = function (ev) {
            if (ev.data.indexOf("{") == 0) {
                var err;
                eval("err=" + ev.data);
                _this.onerror(err);
                return;
            }
            if (!_this.reader) {
                _this.onFirstMessage(ev);
            }
            else {
                _this.serverReceived = parseInt(ev.data);
                if (_this.serverReceived == -1) {
                    var web = _this.webSocket;
                    _this.webSocket.onmessage = null;
                    _this.webSocket.onerror = null;
                    _this.webSocket.onclose = null;
                    _this.webSocket = null;
                    web.close();
                    _this.isUploading = false;
                }
                if (_this.onProgress && _this.serverReceived >= 0) {
                    _this.onProgress(_this, _this.file.size, _this.serverReceived);
                }
                if (_this.serverReceived == -1) {
                    _this.readedPosition = 0;
                    _this.serverReceived = 0;
                    _this.tranId = "";
                    if (_this.onCompleted != null) {
                        _this.onCompleted(_this);
                    }
                }
            }
        };
    };
    WebSocketUploadFile.prototype.onFirstMessage = function (ev) {
        var _this = this;
        this.tranId = ev.data;
        this.webSocket.binaryType = "arraybuffer";
        this.reader = new FileReader();
        this.reader.onload = function (ev) {
            if (!_this.webSocket)
                return;
            var filedata = _this.reader.result;
            _this.webSocket.send(filedata);
            _this.readedPosition += filedata.byteLength;
            if (_this.readedPosition == _this.file.size) {
                return;
            }
            _this.sendBlock(_this.readedPosition, 20480);
        };
        this.reader.onerror = function (ev) {
            _this.onerror(new Error("read file error"));
        };
        this.sendBlock(this.readedPosition, 20480);
    };
    WebSocketUploadFile.prototype.sendBlock = function (start, len) {
        try {
            var blob = this.file.slice(start, start + len);
            this.reader.readAsArrayBuffer(blob);
        }
        catch (e) {
            this.onerror(e);
        }
    };
    WebSocketUploadFile.prototype.onSocketError = function () {
        if (!this.webSocket)
            return;
        this.onerror(new Error("websocket error"));
    };
    WebSocketUploadFile.prototype.onerror = function (err) {
        if (!this.webSocket)
            return;
        var web = this.webSocket;
        this.webSocket.onmessage = null;
        this.webSocket.onerror = null;
        this.webSocket.onclose = null;
        this.webSocket = null;
        if (web)
            web.close();
        if (this.reader) {
            var reader = this.reader;
            this.reader = null;
            reader.abort();
        }
        this.isUploading = false;
        if (this.onError) {
            this.onError(this, err);
        }
    };
    WebSocketUploadFile.prototype.upload = function () {
        if (this.isUploading)
            throw new Error("is uploading");
        this.isUploading = true;
        this.file = this.element.files[0];
        this.readedPosition = this.serverReceived;
        this.initWebSocket();
    };
    return WebSocketUploadFile;
}());
exports.WebSocketUploadFile = WebSocketUploadFile;
//# sourceMappingURL=WebSocketUploadFile.js.map