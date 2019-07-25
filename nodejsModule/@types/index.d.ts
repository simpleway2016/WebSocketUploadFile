export declare class WebSocketUploadFile {
    element: HTMLInputElement;
    serverUrl: string;
    isUploading: boolean;
    onProgress: (sender: WebSocketUploadFile, totalBytes: any, sended: any) => void;
    onCompleted: (sender: WebSocketUploadFile) => void;
    onError: (sender: WebSocketUploadFile, err: any) => void;
    private webSocket;
    private file;
    private position;
    private reader;
    private readedPosition;
    private tranId;
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
