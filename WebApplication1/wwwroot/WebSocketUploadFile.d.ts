/**
 * 如果要实现断点续传，并且WebSocketUploadFile是一个新的实例化对象，可以通过设置tranId和serverReceived属性来实现
 * 如果是同一个WebSocketUploadFile对象，在传输过程中发生错误中断，只需要再次调用该对象的upload方法即可
 * */
export declare class WebSocketUploadFile {
    element: HTMLInputElement;
    serverUrl: string;
    /**事务id，相同的事务id，可以实现断点续传*/
    tranId: string;
    /**服务器已经接收的数量*/
    serverReceived: number;
    isUploading: boolean;
    onProgress: (sender: WebSocketUploadFile, totalBytes: any, sended: any) => void;
    onCompleted: (sender: WebSocketUploadFile) => void;
    onError: (sender: WebSocketUploadFile, err: any) => void;
    private webSocket;
    private file;
    private reader;
    private readedPosition;
    /**
     *
     * @param fileEle
     * @param tranId
     * @param serverUrl 服务器地址，如：http://www.test.com，如果为空，则以location.href为准
     */
    constructor(fileEle: HTMLInputElement, serverUrl?: string);
    private onSocketError;
    private initWebSocket;
    private sendBlock;
    private onerror;
    upload(): void;
}
