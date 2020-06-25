# KomeTubeR
讀取Youtube影片聊天室留言 

![](https://raw.githubusercontent.com/dghkd/KomeTubeR/master/preview1.png)

## 限制:  
1. 直播存檔影片需要等Youtube可顯示聊天室重播後才可讀取  
2. 影片必須啟用聊天室功能  
3. 已刪除的留言無法讀取  
4. 會員專用表情符號無法顯示  

## 操作方法:  
1. 左上角輸入直播影片網址  
2. 點擊開始  
開始後便會持續讀取聊天室留言並顯示於下方  

## 命令列參數:  
-url [Youtube video url]: 輸入想要讀取留言的影片網址    
-o [file path]: 輸出檔案名稱 EX: file.csv 或絕對路徑 C:\file.csv  
-close: 讀取留言完畢後自動關閉程式   
-hide: 隱藏視窗  

若要啟用自動模式必須同時輸入-url與-o參數  
-hide與-close為選項，非必要參數  
若輸入-hide則讀取留言完畢後也會自動關閉程式  

## 程式方法  
1. 從HTML內容中解析出continuation參數 (window["ytInitialData"]的值)  
2. 利用continuation參數取得聊天室留言以及下一次的continuation參數 (https://www.youtube.com/live_chat/get_live_chat?continuation={continuation.Continuation}&pbj=1)  
3. 循環利用continuation參數取得新留言  
