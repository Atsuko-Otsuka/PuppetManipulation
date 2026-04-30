/************************************************************************
 * ADXL367 3-axis Accelerometer, Serial Demo
 * - Pico W 2022 / RP2040
 * - SPI0 remapped to GPIO2-5
 * - Wi-Fi/UDP functionality has been removed for testing.
 ************************************************************************/
#include <SPI.h> // SPI 通信を行うためのライブラリをインクルード

// --- Wi-Fi関連のライブラリは不要なためコメントアウト ---
// #include <WiFi.h>
// #include <WiFiUDP.h>

const uint8_t PIN_SCK  = 2; // SPI クロック信号のピン
const uint8_t PIN_MOSI = 3; // SPI の送信ピン (Master Out Slave In)
const uint8_t PIN_MISO = 4; // SPI の受信ピン (Master In Slave Out)
const uint8_t PIN_CS1  = 5; // チップセレクト1 (CS) のピン、デバイス選択用
const uint8_t PIN_CS2  = 1; // チップセレクト2 (CS) のピン、デバイス選択用

/* ADXL367 commands & regs */
const uint8_t CMD_WRITE = 0x0A; // レジスタへの書き込みコマンド
const uint8_t CMD_READ  = 0x0B; // レジスタの読み出しコマンド
const uint8_t REG_DEVID = 0x00; // デバイス ID のレジスタ
const uint8_t REG_POWER = 0x2D; // 電源管理用のレジスタ
const uint8_t REG_XL    = 0x08; // X 軸データの最下位バイト (ここからバースト読み出し)

// --- Wi-Fi/UDP関連の変数は不要なため削除 ---

// レジスタへデータを書き込む関数
void writeReg(uint8_t addr, uint8_t data, uint8_t csPin) {
  digitalWrite(csPin, LOW);
  SPI.transfer(CMD_WRITE);
  SPI.transfer(addr);
  SPI.transfer(data);
  digitalWrite(csPin, HIGH);
}

//指定のレジスタからデータを読み取る関数
uint8_t readReg(uint8_t addr, uint8_t csPin) {
  digitalWrite(csPin, LOW);
  SPI.transfer(CMD_READ);
  SPI.transfer(addr);
  uint8_t v = SPI.transfer(0x00);
  digitalWrite(csPin, HIGH);
  return v;
}

// X, Y, Z 軸の加速度データを読み取る関数
void readXYZ(int8_t &x, int8_t &y, int8_t &z, uint8_t csPin) {
  digitalWrite(csPin, LOW);
  SPI.transfer(CMD_READ);
  SPI.transfer(REG_XL);
  uint8_t xm = SPI.transfer(0x00);
  uint8_t ym = SPI.transfer(0x00);
  uint8_t zm = SPI.transfer(0x00);
  digitalWrite(csPin, HIGH);
  x = (int8_t)xm;
  y = (int8_t)ym;
  z = (int8_t)zm;
}

// 初期化処理
void setup() {
  Serial.begin(115200);
  while (!Serial && millis() < 5000) {}
  Serial.println("=== ADXL367 RAW SPI Demo (Serial Test) ===");

  SPI.setRX (PIN_MISO);
  SPI.setSCK(PIN_SCK);
  SPI.setTX (PIN_MOSI);
  pinMode(PIN_CS1, OUTPUT);
  pinMode(PIN_CS2, OUTPUT);
  digitalWrite(PIN_CS1, HIGH);
  digitalWrite(PIN_CS2, HIGH);
  SPI.begin();
  SPI.beginTransaction(SPISettings(1'000'000, MSBFIRST, SPI_MODE0));

  // 両センサのデバイスIDチェック＆測定モード設定
  for (int i = 0; i < 2; ++i) {
    uint8_t cs = (i == 0 ? PIN_CS1 : PIN_CS2);
    uint8_t id = readReg(REG_DEVID, cs);
    Serial.printf("Sensor %d ID = 0x%02X\n", i+1, id);
    if (id != 0xAD) {
      Serial.printf("ERROR: Sensor %d not found!\n", i+1);
      while (1) delay(500);
    }
    // 測定モードへ
    writeReg(REG_POWER, 0x02, cs);
    delay(10);
  }
  Serial.println("Both sensors in measurement mode.");

  // --- Wi-Fi接続処理は全て削除 ---
}

// ループ処理 (加速度データの取得)
void loop() {
  static uint32_t prev = 0;
  if (millis() - prev < 500) return;
  prev = millis();

  int8_t x1, y1, z1, x2, y2, z2;
  readXYZ(x1, y1, z1, PIN_CS1);
  readXYZ(x2, y2, z2, PIN_CS2);

  // --- UDP 送信の代わりに、シリアルに直接出力 ---
  String payload = String(x1) + "," + y1 + "," + z1 + "," + x2 + "," + y2 + "," + z2;
  
  Serial.print("DATA> ");
  Serial.println(payload);
}