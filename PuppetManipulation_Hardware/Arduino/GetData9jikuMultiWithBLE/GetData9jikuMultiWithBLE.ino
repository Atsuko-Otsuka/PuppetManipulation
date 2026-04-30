//Server Code
#include <BLEDevice.h>
#include <BLEUtils.h> // BLEのユーティリティ（UUID変換など）
#include <BLEServer.h> // BLEサーバー（データ送信側）としての機能

#include <Wire.h> // I2C通信用
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h> // クォータニオン計算用

// --- 通信の識別子（UUID）の設定 ---
#define SERVICE_UUID        "9d8c03f6-a988-4647-9474-dedaedf309e0" 
#define CHARACTERISTIC_UUID "bfbadb76-f1d7-4b84-9e66-82ac847e21fa" 

// --- マルチプレクサ設定 ---
#define TCAADDR 0x70       // TCA9548AのI2Cアドレス
#define NUM_SENSORS 8      // マルチプレクサのポート数 (最大8)

// --- BNO055 設定 ---
#define BNO055_SAMPLERATE_DELAY_MS (20) // 各センサーの読み取り間隔
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

//Adafruit_BNO055 bno = Adafruit_BNO055(55, 0x28);

// --- Bluetooth 設定 ---
BLECharacteristic *pCharacteristic;
bool deviceConnected = false;

// 接続状態を管理するコールバック
class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      deviceConnected = true;
    };
    void onDisconnect(BLEServer* pServer) {
      deviceConnected = false;
      // 切断時にアドバタイズを再開（再接続しやすくするため）
      BLEDevice::startAdvertising();
    }
};

void setup() {
  Serial.begin(115200);
  Serial.println("Starting BLE work!");

  Wire.begin();
  Wire.setClock(400000); // I2C通信モード
  Wire.setTimeout(3000); // クロックストレッチ対策

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
  delay(1000);

  // BLE初期化
  BLEDevice::init("XIAO_ESP32C6");
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  BLEService *pService = pServer->createService(SERVICE_UUID);

  // グローバル変数 pCharacteristic に代入
  pCharacteristic = pService->createCharacteristic(
                      CHARACTERISTIC_UUID,
                      BLECharacteristic::PROPERTY_READ |
                      BLECharacteristic::PROPERTY_WRITE |
                      BLECharacteristic::PROPERTY_NOTIFY
                    );

  pCharacteristic->setValue("Hello World");
  pService->start();
  // BLEAdvertising *pAdvertising = pServer->getAdvertising();  // this still is working for backward compatibility
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();
  Serial.println("Setup Complete.");
}

void loop() {
  if (deviceConnected)
  {
    for (uint8_t i = 0; i < NUM_SENSORS; i++) 
    {
      if (sensor_initialized[i])
      {
        tcaselect(i); // 読み取りたいセンサーのポートを選択
        imu::Quaternion quat = bno_sensors[i].getQuat(); // クォータニオンデータを取得
        // データの作成
        String data = String(i) + ","
                      + String(quat.w(), 3) + ","
                      + String(quat.x(), 3) + ","
                      + String(quat.y(), 3) + ","
                      + String(quat.z(), 3);
        pCharacteristic->setValue(data.c_str());
        pCharacteristic->notify();
        Serial.println(data);
      }
      delay(BNO055_SAMPLERATE_DELAY_MS);
    }
  }
  delay(BNO055_SAMPLERATE_DELAY_MS);
}