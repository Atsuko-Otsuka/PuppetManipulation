#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

/*
 * このコードは、TCA9548A I2Cマルチプレクサに接続された
 * 複数のBNO055センサーからデータを読み取ります。
 */

// --- マルチプレクサ設定 ---
#define TCAADDR 0x70       // TCA9548AのI2Cアドレス
#define NUM_SENSORS 8      // マルチプレクサのポート数 (最大8)

// --- BNO055 設定 ---
#define BNO055_SAMPLERATE_DELAY_MS (15) // ループごとのディレイ

// マルチプレクサのポートを切り替える関数
void tcaselect(uint8_t i) {
  if (i > 7) return;
  Wire.beginTransmission(TCAADDR);
  Wire.write(1 << i);
  Wire.endTransmission();
}

// センサーオブジェクトをポートの数だけ配列として作成
Adafruit_BNO055 bno_sensors[NUM_SENSORS];

// 各センサーが正常に初期化されたかを記録する配列
bool sensor_initialized[NUM_SENSORS];


void setup(void) {
  Serial.begin(115200);
  while (!Serial) {
    delay(10); // シリアルモニタが開くのを待つ
  }

  // Pico WのカスタムI2Cピンを設定
  // Wire.setSDA(4);
  // Wire.setSCL(5);
  // pinMode(4, INPUT_PULLUP);
  // pinMode(5, INPUT_PULLUP);
  Wire.begin(); // I2Cバスを初期化

  Wire.setClock(400000);

  // タイムアウトを延長 (クロックストレッチ対策)
  Wire.setTimeout(3000);

  Serial.println("Multi-BNO055 Test via TCA9548A");

  // すべてのポートをループして、BNO055を探し、初期化する
  for (uint8_t i = 0; i < NUM_SENSORS; i++) {
    tcaselect(i); // ポートiを選択
    Serial.print("Checking Port #");
    Serial.print(i);

    // センサーの初期化を試みる
    if (!bno_sensors[i].begin()) {
      Serial.println(" ... No BNO055 detected");
      sensor_initialized[i] = false;
    } else {
      Serial.println(" ... BNO055 Found!");
      bno_sensors[i].setExtCrystalUse(true); // 外部クリスタルを使用
      sensor_initialized[i] = true;
    }
  }
  Serial.println("Setup Complete.");
}


void loop(void) {
  // //データを一時保存する配列を作成
  // //(w, x, y, z) を持つ Quaternion 型の配列です
  // imu::Quaternion quats[NUM_SENSORS];
  
  // // --- フェーズ1: 一気に読み取る (I2C通信に集中) ---
  // for (uint8_t i = 0; i < NUM_SENSORS; i++) {
  //   if (sensor_initialized[i]) {
  //     tcaselect(i);
  //     // データ取得して配列に保存
  //     quats[i] = bno_sensors[i].getQuat();
  //   }
  // }

  // // --- フェーズ2: 一気に送信する (USB通信) ---
  // for (uint8_t i = 0; i < NUM_SENSORS; i++) {
  //   if (sensor_initialized[i]) {
  //     // 配列からデータを取り出して送信
  //     Serial.print(i);
  //     Serial.print(",");
  //     Serial.print(quats[i].w(), 4);
  //     Serial.print(",");
  //     Serial.print(quats[i].x(), 4);
  //     Serial.print(",");
  //     Serial.print(quats[i].y(), 4);
  //     Serial.print(",");
  //     Serial.println(quats[i].z(), 4);
  //   }
  // }

  // データを読み込んだらシリアル送信
  for (uint8_t i = 0; i < NUM_SENSORS; i++) {
    
    // setup()で初期化に成功したセンサーだけを読み取る
    if (sensor_initialized[i]) {
      
      tcaselect(i); // 読み取りたいセンサーのポートを選択

      // クォータニオンデータを取得
      imu::Quaternion quat = bno_sensors[i].getQuat();

      // シリアルに出力 (先頭にポート番号を追加)
      Serial.print(i); // ポート番号
      Serial.print(",");
      Serial.print(quat.w(), 4);
      Serial.print(",");
      Serial.print(quat.x(), 4);
      Serial.print(",");
      Serial.print(quat.y(), 4);
      Serial.print(",");
      Serial.println(quat.z(), 4);
    }
  }

  // 少し待機
  delay(BNO055_SAMPLERATE_DELAY_MS);
}