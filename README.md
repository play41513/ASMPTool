# ASMPTool

📝 版本紀錄（Changelog）

[v1.1] - 2025-04-08
1. 可測試更多設定完的機種。
2. 登入畫面選取測試站 :讀取NAS端設定檔，顯示可測試的工站，供使用者選取。
3. 選取完工站後按下"登入"，會自動從NAS複製需要的參數檔及Tool至程式資料夾。
4. 複製前會檢查複製的當前資料夾是否有相同的checksum*.ini檔，如果有的話不進行複製，反之複製覆蓋。

🔧 修正NAS連線機制，沒有連線按下登入時會跳出警示。

[v1.0] - 2025-03-31
初始版本上線

基本功能：
OE-DI01使用，可供OE-DI01燒錄FW、功能測試
