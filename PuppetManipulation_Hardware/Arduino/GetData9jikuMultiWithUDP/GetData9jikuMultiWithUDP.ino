#include <Wire.h> // I2C通信用
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h> // クォータニオン計算用
#include <WiFi.h> // Wi-fi
#include <WiFiUDP.h> // UDP通信

// --- マルチプレクサ設定 ---
#define TCAADDR 0x70       // TCA9548AのI2Cアドレス
#define NUM_SENSORS 8      // マルチプレクサのポート数 (最大8)

// --- BNO055 設定 ---
#define BNO055_SAMPLERATE_DELAY_MS (20) // 読み取り待機時間
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

const char* ssid     = "STUDIO"; // SSID
const char* password = "79772736ef699"; // Password
static WiFiUDP wifiUdp;
static const char *RemoteIpadr = "192.168.100.35"; // 送信先のIPアドレス
static const int RmoteUdpPort = 9000; // 送信先ポート
static const int LocalPort = 9001; // 受信ポート
//char WiFibuff[4];

void setup()
{
  Serial.begin(115200);

  // I2C開始
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
  delay(100);

  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(ssid);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected.");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
  wifiUdp.begin(LocalPort);
}

void loop(){
  if (WiFi.status() != WL_CONNECTED) {
    return; 
  }
  
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
      // --- UDP送信実行 ---
      wifiUdp.beginPacket(RemoteIpadr, RmoteUdpPort);
      wifiUdp.print(data); // パケットにデータを書き込む
      wifiUdp.endPacket(); // パケット送信完了
      
      // シリアルモニタでの確認用
      Serial.print("Sent: ");
      Serial.println(data);
      
      delay(10);
    }
  }
  delay(BNO055_SAMPLERATE_DELAY_MS);
}