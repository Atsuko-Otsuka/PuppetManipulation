#include <Wire.h>

void setup() {
  Wire.setSDA(20); // GP20をSDAに設定
  Wire.setSCL(21); // GP21をSCLに設定
  Wire.begin();
  Serial.begin(9600);
  //while(!Serial);
  delay(1000);
}

bool slavePresent(byte adr) {
  Wire.beginTransmission(adr);
  return(Wire.endTransmission() == 0);
}

void loop() {
  Serial.println("I2C slave device list.");
  for(byte adr = 1; adr < 127; adr++) {
    if(slavePresent(adr)) {
      if(adr < 16) Serial.print("0");
      Serial.print(adr,HEX);
      Serial.print(" ");
    }
  }
  Serial.println("\nDone.");
  delay(5000);
}
