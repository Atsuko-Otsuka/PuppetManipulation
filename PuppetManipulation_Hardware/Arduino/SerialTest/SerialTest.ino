/*
  ArduinoからUnityへ定期的にメッセージを送信するスケッチ
*/

void setup() {
  // Unity側のBaudRate設定（115200）と合わせる
  Serial.begin(115200);
}

void loop() {
  // Unityへメッセージを送信
  // ReadLine()で読み取るため、println()を使い改行コードを末尾に付加する
  Serial.println("Hello from Arduino!");
  
  // 1秒待機
  delay(1000);
}
