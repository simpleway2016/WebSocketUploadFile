/**
 * 如果要实现断点续传，并且WebSocketUploadFile是一个新的实例化对象，可以通过设置tranId和serverReceived属性来实现
 * 如果是同一个WebSocketUploadFile对象，在传输过程中发生错误中断，只需要再次调用该对象的upload方法即可
 * */
export class WebSocketUploadFile {
    serverUrl: string;
    /**事务id，相同的事务id，可以实现断点续传*/
    tranId = "";
    /**服务器已经接收的数量*/
    serverReceived = 0;
    isUploading = false;
    /**上传到服务器的附加信息*/
    state: string;
    /**服务器校验信息*/
    auth: string;

    onProgress: (sender: WebSocketUploadFile, totalBytes, serverReceived) => void;
    onCompleted: (sender: WebSocketUploadFile) => void;
    onError: (sender: WebSocketUploadFile, err) => void;

    private webSocket: WebSocket;
    private dataProvider: IDataProvider;
    
    private readedPosition = 0;
    private sendedFirstMesssage = false;

    /**
     * 
     * @param fileEle
     * @param serverUrl 服务器地址，如：http://www.test.com，如果为空，则以location.href为准
     */
    constructor(fileEle: any, serverUrl: string = undefined) {
        if (fileEle.data && fileEle.filename) {
            var obj = <ArrayBufferDataObject>fileEle;
            this.dataProvider = new ArrayBufferProvider(obj);
        }
        else {
            this.dataProvider = new InputFileProvider(fileEle);
        }
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
                filename: this.dataProvider.name,
                length: this.dataProvider.size,
                position: this.serverReceived,
                tranid: this.tranId,
                state: this.state,
                auth: this.auth
            }));
        };

        this.sendedFirstMesssage = false;


        this.webSocket.onmessage = (ev) => {
            if (ev.data.indexOf("{") == 0) {
                var err;
                eval("err=" + ev.data);
                this.onerror(err);
                return;
            }

            if (!this.sendedFirstMesssage) {
                this.sendedFirstMesssage = true;
                this.onFirstMessage(ev);
            }
            else {
               
                this.serverReceived = parseInt(ev.data);

                if (this.serverReceived == -1) {
                    var web = this.webSocket;                    
                    this.webSocket.onmessage = null;
                    this.webSocket.onerror = null;
                    this.webSocket.onclose = null;
                    this.webSocket = null;
                    web.close();
                    this.isUploading = false;
                }

                if (this.onProgress && this.serverReceived >= 0) {
                    this.onProgress(this, this.dataProvider.size, this.serverReceived);
                }
                if (this.serverReceived == -1) {
                    this.readedPosition = 0;
                    this.serverReceived = 0;
                    this.tranId = "";
                    if (this.onCompleted != null) {
                        this.onCompleted(this);
                    }
                }
            }
        };
    }

    private senddata() {
        this.dataProvider.read(this.readedPosition, 20480, (filedata, err) => {
            if (err) {
                this.onerror(err);
                return;
            }

            this.webSocket.send(filedata);

            this.readedPosition += filedata.byteLength;
            if (this.readedPosition >= this.dataProvider.size) {
                return;
            }

            this.senddata();
        });
    }

    private onFirstMessage(ev: MessageEvent) {
        this.tranId = ev.data;
        this.webSocket.binaryType = "arraybuffer";

        this.senddata();        
    }

    private onSocketError() {
        if (!this.webSocket)
            return;       
        this.onerror(new Error("websocket error"));
    }

    private onerror(err) {
        if (!this.webSocket)
            return;

        var web = this.webSocket;
        this.webSocket.onmessage = null;
        this.webSocket.onerror = null;
        this.webSocket.onclose = null;
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
        this.dataProvider.init();
        this.readedPosition = this.serverReceived;
        this.initWebSocket();
    }
}

interface ArrayBufferDataObject {
    data: ArrayBuffer;
    filename: string;
}

interface IDataProvider {
    name: string;
    size: number;
    init();
    read(position: number, length: number, callback: (data: ArrayBuffer,err)=>void);
}

class InputFileProvider implements IDataProvider {
    file: File;
    element: HTMLInputElement;
    reader: FileReader;
    name: string;
    size: number;

    constructor(fileEle: HTMLInputElement) {
        this.element = fileEle;
      
    }
    init() {
        this.file = this.element.files[0];
        this.name = this.file.name;
        this.size = this.file.size;
        this.reader = new FileReader();
    }
    read(position: number, length: number, callback: (data: ArrayBuffer,err) => void) {
        try {
            this.reader.onload = (ev) => {
                var filedata: ArrayBuffer = <ArrayBuffer>this.reader.result;
                if (callback) callback(filedata, undefined);
            };
            this.reader.onerror = (ev) => {
                if (callback) callback(undefined , new Error("read file error"));
            };
            var blob = this.file.slice(position, position + length);
            this.reader.readAsArrayBuffer(blob);
        }
        catch (e) {
            if (callback)
                callback(undefined, e);
        }
    }

}

class ArrayBufferProvider implements IDataProvider {
    obj: ArrayBufferDataObject;
    name: string;
    size: number;

    constructor(obj: ArrayBufferDataObject) {
        this.obj = obj;

    }
    init() {
        this.name = this.obj.filename;
        this.size = this.obj.data.byteLength;
    }
    read(position: number, length: number, callback: (data: ArrayBuffer, err) => void) {
        try {
            var toread = length;
            if (position + toread > this.obj.data.byteLength)
                toread = this.obj.data.byteLength - position;

            var ret = this.obj.data.slice(position, position + toread);

            if (callback)
                callback(ret, undefined);
        }
        catch (e) {
            if (callback)
                callback(undefined, e);
        }
    }

}