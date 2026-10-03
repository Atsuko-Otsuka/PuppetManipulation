# Puppet Manipulation

ぬいぐるみを使用したVR空間内のアバター操作システム。Unityを使用し、センサーやマイコン制御によりリアルタイムのキャラクター操作・アニメーション制御を実現。

## 概要

このプロジェクトは以下の機能を提供します：

- **キャラクターアニメーション制御** - Animator システムを使用したスムーズなアニメーション遷移
- **UDP通信** - Arduino や Raspberry Pi Pico などのマイコンボードからのリアルタイムデータ受信
- **キャラクター方向制御** - トラッキングデータに基づくキャラクターの向き変更
- **UI エフェクト** - フェードイン/アウトなどのUI演出
- **IK ソリューション** - Final IK による逆運動学計算
- **画像処理** - OpenCV を使用した視覚情報の処理

## 技術仕様

- **言語構成**
  - C#: 90.2%
  - ShaderLab: 6.4%
  - ASP.NET: 2.6%
  - その他: 0.8%

- **主な依存ライブラリ**
  - Unity (メインフレームワーク)
  - OpenCV for Unity (画像処理)
  - Final IK (逆運動学)
  - System.IO.Ports (シリアル通信)

## プロジェクト構成

```
PuppetManipulation_Software/
├── Assets/
│   ├── Scripts/
│   │   ├── Serial/           # シリアル通信関連
│   │   │   └── SerialHandler.cs
│   │   ├── Puppet/           # キャラクター制御
│   │   │   └── ChangeDirection.cs
│   │   ├── Blackout.cs       # UI フェード制御
│   │   ├── RotateCard.cs     # カード回転
│   │   └── Santa.cs          # キャラクター主制御
│   ├── OpenCVForUnity/       # OpenCV ラッパー
│   ├── Plugins/
│   │   └── RootMotion/       # Final IK ライブラリ
│   └── その他リソース
```

## 主要な機能

### 1. キャラクター制御 (Santa.cs)
キーボード入力やランダムイベントに基づいてキャラクターのアニメーション状態を管理します。

- **Space キー**: 走行開始
- **J キー**: ジャンプ
- **A キー**: テストアクション

### 2. シリアル通信 (SerialHandler.cs)
マイコンボードからのセンサーデータをリアルタイムで受信します。

- 対応ボード: Arduino Uno、Raspberry Pi Pico など
- ボーレート: 115200 bps
- プラットフォーム対応: Windows (COM)、Linux (/dev/ttyUSB0)

### 3. キャラクター方向制御 (ChangeDirection.cs)
トラッキングデータに基づいてキャラクターの向きを動的に変更します。

- ヨー角度監視によるリアルタイム回転
- 閾値ベースの判定システム

### 4. UI エフェクト (Blackout.cs)
フェード効果などのUI演出を制御します。

## セットアップ

### 必要な環境
- Unity 2019 以上
- .NET Framework (シリアル通信用)
- シリアル通信可能なマイコンボード (オプション)

### インストール

1. このリポジトリをクローン
```bash
git clone https://github.com/Atsuko-Otsuka/PuppetManipulation.git
```

2. Unity でプロジェクトを開く
3. シーンを実行

### シリアル通信設定

SerialHandler.cs 内のポート設定を環境に合わせて変更してください：

```csharp
public string portName = "COM3";      // ポート名の変更
public int baudRate = 115200;         // ボーレート設定
```

## 使用方法

### 基本操作

1. シーンを実行
2. キーボード入力でキャラクターを操作
3. シリアル接続されている場合、センサーデータが自動的に反映される

## ライセンス

このプロジェクトのライセンス情報については、[LICENSE](LICENSE) をご確認ください。

## 貢献

バグ報告や機能改善の提案は、[Issues](https://github.com/Atsuko-Otsuka/PuppetManipulation/issues) でお願いします。

## 参考

- [OpenCV Documentation](https://docs.opencv.org/)
- [Final IK Documentation](https://root-motion.com/)
- [Unity Documentation](https://docs.unity3d.com/)
