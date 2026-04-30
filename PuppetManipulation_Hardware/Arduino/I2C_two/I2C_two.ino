#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h> 

/*
 * 2系統I2C通信を使用して2つのBNO055センサーからデータを読み取ります。
 */

#define BNO055_SAMPLERATE_DELAY_MS (10)

// センサーオブジェクトを2つ作成し、それぞれ異なるI2Cバスを指定
// 1つ目のセンサー (I2C0 - Wire)
Adafruit_BNO055 bno1 = Adafruit_BNO055(55, 0x28, &Wire); 
// 2つ目のセンサー (I2C1 - Wire1)
Adafruit_BNO055 bno2 = Adafruit_BNO055(56, 0x28, &Wire1); 

void setup(void)
{
  Serial.begin(115200); 

  // 1つ目のI2Cバス (Wire / I2C0) のカスタムピンを設定
  Wire.setSDA(16); // 例: GP20
  Wire.setSCL(17); // 例: GP21
  // ★ I2C0のピンの内部プルアップを有効化
  //pinMode(16, INPUT_PULLUP);
  //pinMode(17, INPUT_PULLUP);
  // Wire.begin() を呼び出すことで I2C0 を初期化
  Wire.begin();
  Wire.setClock(400000);
  // タイムアウトを延長 (クロックストレッチ対策)
  Wire.setTimeout(3000);


  // 2つ目のI2Cバス (Wire1 / I2C1) のカスタムピンを設定
  Wire1.setSDA(14); // 例: GP26
  Wire1.setSCL(15); // 例: GP27
  // ★ I2C1のピンの内部プルアップを有効化
  //pinMode(14, INPUT_PULLUP);
  //pinMode(15, INPUT_PULLUP);
  // Wire1.begin() を呼び出すことで I2C1 を初期化
  Wire1.begin();
  Wire1.setClock(400000);
  // タイムアウトを延長 (クロックストレッチ対策)
  Wire1.setTimeout(3000);


  Serial.println("BNO055 Two Bus Test");

  // 1つ目のセンサーの初期化
  if(!bno1.begin()) 
  { 
    Serial.println("Ooops, no BNO055 (I2C0) detected ... Check your wiring or I2C ADDR!"); 
  } else {
    delay(1000);
    bno1.setExtCrystalUse(true);
  }

  // 2つ目のセンサーの初期化
  if(!bno2.begin()) 
  { 
    Serial.println("Ooops, no BNO055 (I2C1) detected ... Check your wiring or I2C ADDR!"); 
  } else {
    delay(1000);
    bno2.setExtCrystalUse(true);
  }
}

void loop(void)
{
  // // データを一時保存する配列を作成
  // // (w, x, y, z) を持つ Quaternion 型の配列
  // imu::Quaternion quats[2];

  // // 1つ目のセンサーからデータを取得
  // imu::Quaternion quat1 = bno1.getQuat(); 
  // quats[0] = quat1;

  // // 2つ目のセンサーからデータを取得
  // imu::Quaternion quat2 = bno2.getQuat(); 
  // quats[1] = quat2;

  // // 2つのデータを送信
  // for (uint8_t i = 0; i < 2; i++) {
  //   // 配列からデータを取り出して送信
  //   Serial.print(i);
  //   Serial.print(",");
  //   Serial.print(quats[i].w(), 4);
  //   Serial.print(",");
  //   Serial.print(quats[i].x(), 4);
  //   Serial.print(",");
  //   Serial.print(quats[i].y(), 4);
  //   Serial.print(",");
  //   Serial.println(quats[i].z(), 4);
  // }

  // 1つ目のセンサーからデータを取得
  imu::Quaternion quat1 = bno1.getQuat(); 

  Serial.print("1"); // センサー1を示すポート番号 (仮)
  Serial.print(",");
  Serial.print(quat1.w(), 4); 
  Serial.print(",");
  Serial.print(quat1.x(), 4);
  Serial.print(",");
  Serial.print(quat1.y(), 4);
  Serial.print(",");
  Serial.println(quat1.z(), 4);

  delay(10);

  // 2つ目のセンサーからデータを取得
  imu::Quaternion quat2 = bno2.getQuat(); 

  Serial.print("2"); // センサー2を示すポート番号 (仮)
  Serial.print(",");
  Serial.print(quat2.w(), 4); 
  Serial.print(",");
  Serial.print(quat2.x(), 4);
  Serial.print(",");
  Serial.print(quat2.y(), 4);
  Serial.print(",");
  Serial.println(quat2.z(), 4);

  delay(BNO055_SAMPLERATE_DELAY_MS); 
}