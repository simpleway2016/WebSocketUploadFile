declare class WebSocketUploadFile {
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
    constructor(fileEle: HTMLInputElement, serverUrl?: string);
    private onSocketError;
    private initWebSocket;
    private sendBlock;
    private onerror;
    upload(): void;
}
