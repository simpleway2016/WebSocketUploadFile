/**
 * 如果要实现断点续传，并且WebSocketUploadFile是一个新的实例化对象，可以通过设置tranId和serverReceived属性来实现
 * 如果是同一个WebSocketUploadFile对象，在传输过程中发生错误中断，只需要再次调用该对象的upload方法即可
 * */
export class WebSocketUploadFile {
    element: HTMLInputElement;
    serverUrl: string;
    /**事务id，相同的事务id，可以实现断点续传*/
    tranId = "";
    /**服务器已经接收的数量*/
    serverReceived = 0;
    isUploading = false;

    onProgress: (sender: WebSocketUploadFile, totalBytes, sended) => void;
    onCompleted: (sender: WebSocketUploadFile) => void;
    onError: (sender: WebSocketUploadFile, err) => void;

    private webSocket: WebSocket;
    private file: File;
    
    private reader: FileReader;
    private readedPosition = 0;

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
                position: this.serverReceived,
                tranid: this.tranId
            }));
        };
        var callBack = (ev: MessageEvent) => {
            this.serverReceived = parseInt(ev.data);

            if (this.serverReceived == -1) {
                var web = this.webSocket;
                this.webSocket = null;
                web.close();
                this.isUploading = false;
            }

            if (this.onProgress && this.serverReceived >= 0) {
                this.onProgress(this, this.file.size, this.serverReceived);
            }
            if (this.serverReceived == -1) {
                this.readedPosition = 0;
                this.serverReceived = 0;
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
        this.readedPosition = this.serverReceived;
        this.initWebSocket();
    }
}
