export class WebSocketUploadFile {
    element: HTMLInputElement;
    serverUrl: string;
    
    isUploading = false;

    onProgress: (sender: WebSocketUploadFile, totalBytes, sended) => void;
    onCompleted: (sender: WebSocketUploadFile) => void;
    onError: (sender: WebSocketUploadFile, err) => void;

    private webSocket: WebSocket;
    private file: File;
    private position = 0;
    private reader: FileReader;
    private readedPosition = 0;
    private tranId = "";
    /**
     * 
     * @param fileEle
     * @param tranId
     * @param serverUrl 服务器地址，如：http://www.test.com，如果为空，则以location.href为准
     */
    constructor(fileEle: HTMLInputElement,serverUrl: string = undefined) {
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
        console.log(`WebSocket Address:${this.serverUrl}`);
    }

    private onSocketError() {
        if (!this.webSocket)
            return;
        this.webSocket = null;
        if (this.reader) {
            var reader = this.reader;
            this.reader = null;
            this.reader.abort();
        }
        this.onerror(new Error("websocket error"));
    }

    private initWebSocket() {
        this.webSocket = new WebSocket(this.serverUrl);
        var originalType = this.webSocket.binaryType;

        this.webSocket.onerror = (ev) => {
            this.onSocketError();          
        };
        this.webSocket.onclose = () => {
            this.onSocketError();
        };
        this.webSocket.onopen = () => {
            this.webSocket.send(JSON.stringify({
                filename : this.file.name,
                length: this.file.size,
                position: this.position,
                tranid: this.tranId
            }));
        };
        var callBack = (ev: MessageEvent) => {
            this.position = parseInt(ev.data);

            if (this.position == -1) {
                var web = this.webSocket;
                this.webSocket = null;
                web.close();
                this.isUploading = false;
            }

            if (this.onProgress && this.position >= 0) {
                this.onProgress(this, this.file.size, this.position);
            }
            if (this.position == -1) {
                this.readedPosition = 0;
                this.position = 0;
                this.tranId = "";
                if (this.onCompleted != null) {
                    this.onCompleted(this);
                }
            }
        };

        this.webSocket.onmessage = (ev) => {
            this.tranId = ev.data;
            this.webSocket.onmessage = callBack;
            this.webSocket.binaryType = "arraybuffer";

            this.reader = new FileReader();
            this.reader.onload = (ev) => {
                if (!this.webSocket)
                    return;

                var filedata: ArrayBuffer = <ArrayBuffer>this.reader.result;
                this.webSocket.send(filedata);

                this.readedPosition += filedata.byteLength;
                if (this.readedPosition == this.file.size) {
                    this.reader = null;
                    return;
                }
                this.sendBlock(this.readedPosition, 20480);
            };
            this.reader.onerror = (ev) => {
                this.onerror(new Error("read file error"));
            };
            this.sendBlock(this.readedPosition , 20480);           
        };
    }

    private sendBlock(start, len) {
        try {
            var blob = this.file.slice(start, start + len);
            this.reader.readAsArrayBuffer(blob);
        }
        catch (e) {
            this.onerror(e);
        }
    }

    private onerror(err) {
        var web = this.webSocket;
        this.webSocket = null;
        if (web)
            web.close();

        this.isUploading = false;
        if (this.onError) {
            this.onError(this, err);
        }
    }

    upload() {
        if (this.isUploading)
            throw new Error("is uploading");

        this.isUploading = true;
        this.file = this.element.files[0];
        this.initWebSocket();
    }
}
