// #include <Wire.h>
// #include <Adafruit_Sensor.h>
// #include <Adafruit_BNO055.h>
// #include <utility/imumaths.h>

// /*
//  * BNO055のインスタンスを作成します。
//  * I2Cアドレスはデフォルトの0x28です。
//  * もしADRジャンパをはんだ付けしている場合は 0x29 に変更してください。
//  */
// Adafruit_BNO055 bno = Adafruit_BNO055(-1, 0x28);

// void setup(void) {
//   Serial.begin(115200);
//   Serial.println("BNO055 All Sensor Data Test");
//   Serial.println("--------------------------------");
  
//   // Pico WのカスタムI2Cピンを設定
//   Wire.setSDA(20);
//   Wire.setSCL(21);

//   // BNO055センサーを初期化
//   if (!bno.begin()) {
//     Serial.print("BNO055が検出できませんでした。配線かI2Cアドレスを確認してください。");
//     while (1);
//   }
  
//   delay(1000);
// }

// void loop(void) {
//   // 加速度センサーのベクトルデータを取得 (単位: m/s^2)
//   imu::Vector<3> accelerometer = bno.getVector(Adafruit_BNO055::VECTOR_ACCELEROMETER);

//   // ジャイロスコープのベクトルデータを取得 (単位: rad/s)
//   imu::Vector<3> gyroscope = bno.getVector(Adafruit_BNO055::VECTOR_GYROSCOPE);

//   // 地磁気センサー（磁力計）のベクトルデータを取得 (単位: uT - マイクロテスラ)
//   imu::Vector<3> magnetometer = bno.getVector(Adafruit_BNO055::VECTOR_MAGNETOMETER);

//   // --- 取得した値をシリアルモニタに表示 ---

//   // 加速度
//   Serial.print("Accel X: ");
//   Serial.print(accelerometer.x());
//   Serial.print("Y: ");
//   Serial.print(accelerometer.y());
//   Serial.print("Z: ");
//   Serial.print(accelerometer.z());
//   //Serial.println(" (m/s^2)");

//   // ジャイロ
//   Serial.print("Gyro X: ");
//   Serial.print(gyroscope.x());
//   Serial.print("Y: ");
//   Serial.print(gyroscope.y());
//   Serial.print("Z: ");
//   Serial.print(gyroscope.z());
//   //Serial.println(" (rad/s)");

//   // 地磁気
//   Serial.print("Mag X: ");
//   Serial.print(magnetometer.x());
//   Serial.print("Y: ");
//   Serial.print(magnetometer.y());
//   Serial.print("Z: ");
//   Serial.print(magnetometer.z());
//   //Serial.println(" (uT)");
  
//   Serial.println(""); // 見やすくするために改行
  
//   delay(500); // 0.5秒ごとにデータを更新
// }


#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BNO055.h>
#include <utility/imumaths.h>

/*
 * BNO055のインスタンスを作成します。
 * I2Cアドレスはデフォルトの0x28です。
 * もしADRジャンパをはんだ付けしている場合は 0x29 に変更してください。
 */
Adafruit_BNO055 bno = Adafruit_BNO055(-1, 0x28);

void setup(void) {
  Serial.begin(115200);
  Serial.println("BNO055 All Sensor Data Test");
  Serial.println("--------------------------------");
  
  // Pico WのカスタムI2Cピンを設定
  // Wire.setSDA(20);
  // Wire.setSCL(21);
  
  // XIAO ESP32C6の場合
  Wire.begin();

  // BNO055センサーを初期化
  if (!bno.begin()) {
    Serial.print("BNO055が検出できませんでした。配線かI2Cアドレスを確認してください。");
    while (1);
  }
  
  delay(1000);
}

void loop(void) {
  // 加速度センサーのベクトルデータを取得 (単位: m/s^2)
  imu::Vector<3> accelerometer = bno.getVector(Adafruit_BNO055::VECTOR_ACCELEROMETER);

  // ジャイロスコープのベクトルデータを取得 (単位: rad/s)
  imu::Vector<3> gyroscope = bno.getVector(Adafruit_BNO055::VECTOR_GYROSCOPE);

  // 地磁気センサー（磁力計）のベクトルデータを取得 (単位: uT - マイクロテスラ)
  imu::Vector<3> magnetometer = bno.getVector(Adafruit_BNO055::VECTOR_MAGNETOMETER);

  // --- 取得した値をシリアルモニタに表示 ---

  // 加速度
  Serial.print(accelerometer.x());
  Serial.print(",");
  Serial.print(accelerometer.y());
  Serial.print(",");
  Serial.print(accelerometer.z());
  Serial.print(",");
  //ジャイロ
  Serial.print(gyroscope.x());
  Serial.print(",");
  Serial.print(gyroscope.y());
  Serial.print(",");
  Serial.print(gyroscope.z());
  Serial.print(",");
  // 地磁気
  Serial.print(magnetometer.x());
  Serial.print(",");
  Serial.print(magnetometer.y());
  Serial.print(",");
  Serial.println(magnetometer.z());
  

  
  
  // Serial.println(""); // 見やすくするために改行
  
  delay(500); // 0.5秒ごとにデータを更新
}
