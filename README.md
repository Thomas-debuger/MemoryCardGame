# 記憶翻牌遊戲

## 遊戲說明
這是一個使用 **Windows Forms C#** 開發的撲克牌記憶翻牌遊戲。
玩家需要翻開配對的撲克牌，將全部 8 對牌配對完成。

## 遊戲規則
1. 每次可以翻開 2 張牌
2. 若兩張牌的**點數相同**，即配對成功（牌會變綠色保持翻開）
3. 若不相同，牌會自動翻回背面
4. 以最少步數、最短時間完成所有配對為目標

## 功能特色
- ✅ **顯示牌面圖片**：繪製撲克牌花色（♠♥♦♣）與點數，包含圓角設計
- ✅ **音效播放**：
  - 翻牌音效（SystemSounds.Asterisk）
  - 配對成功音效（SystemSounds.Exclamation）  
  - 全部完成音效（SystemSounds.Hand）
- ✅ **步數計數**：追蹤玩家翻牌次數
- ✅ **計時器**：即時顯示遊戲用時
- ✅ **配對進度**：顯示目前完成配對數 / 總配對數
- ✅ **重新開始**：可隨時重置並洗牌

## 專案結構
```
MemoryCardGame/
├── MemoryCardGame.csproj   ← 專案設定檔
├── Program.cs              ← 程式進入點
├── Form1.cs                ← 主要遊戲邏輯與 UI
└── Form1.Designer.cs       ← 表單設計器檔案
```

## 如何開啟與執行

### 方法一：Visual Studio
1. 開啟 Visual Studio 2022
2. 「開啟專案或方案」→ 選擇 `MemoryCardGame.csproj`
3. 按 `F5` 執行

### 方法二：命令列
```bash
dotnet run --project MemoryCardGame.csproj
```

## 需求
- .NET 6.0 (或以上) + Windows
- Visual Studio 2022（建議）

## 畫面說明
- **深藍色背景**：高質感遊戲風格
- **牌背**：深藍漸層 + 格紋花樣
- **牌面**：白色底，紅色（♥♦）/ 黑色（♠♣）花色
- **配對成功**：牌面變為綠色
- **選中牌**：黃色外框高亮
