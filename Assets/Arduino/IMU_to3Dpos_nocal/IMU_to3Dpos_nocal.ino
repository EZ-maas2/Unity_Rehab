#include <Wire.h>
#include <WiFi.h>
#include <WiFiUdp.h>
#include <ICM20948_WE.h>
#include <math.h>



#define ICM20948_ADDR 0x69 // Default I2C address
ICM20948_WE myIMU1 = ICM20948_WE(0x69); // IMU1
ICM20948_WE myIMU2 = ICM20948_WE(0x68); // IMU2

// Wi-Fi Configuration
const char* ssid = "lrz"; //"TP-Link_1FB8";       // Replace with your Wi-Fi SSID
const char* password = "";   // Replace with your Wi-Fi password
const char* udpAddress = "10.158.102.159"; //"192.168.0.101";    // Replace with your PC's local IP
const int udpPort = 6969;                // Port number for UDP communication

WiFiUDP udp; 

float ax1, ay1, az1, ax2, ay2, az2;
float vx_rel = 0.0, vy_rel = 0.0, vz_rel = 0.0;
float x_rel = 0.0, y_rel = 0.0, z_rel = 0.0;
float ax_prev = 0, ay_prev = 0, az_prev =0; // acc in m/s^2
float dt = 0.0;
unsigned long prevTime = 0;

void setup() {
  Serial.begin(115200);
  WiFi.begin(ssid, password);
    Serial.print("Connecting to Wi-Fi");
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println("\nConnected to Wi-Fi");
    Serial.print("ESP32 IP Address: ");
    Serial.println(WiFi.localIP());

  Wire.begin();
  while (!Serial);
  Serial.println("Initializing IMUs...");

  if (!myIMU1.init()) {
    Serial.println("IMU1 not responding.");
    while (1);
  }
  if (!myIMU2.init()) {
    Serial.println("IMU2 not responding.");
    while (1);
  }
  Serial.println("Both IMUs connected.");

  myIMU1.setAccRange(ICM20948_ACC_RANGE_2G);
  myIMU1.setGyrRange(ICM20948_GYRO_RANGE_250);
  myIMU1.setAccDLPF(ICM20948_DLPF_6);
  myIMU1.setGyrDLPF(ICM20948_DLPF_6);

  myIMU2.setAccRange(ICM20948_ACC_RANGE_2G);
  myIMU2.setGyrRange(ICM20948_GYRO_RANGE_250);
  myIMU2.setAccDLPF(ICM20948_DLPF_6);
  myIMU2.setGyrDLPF(ICM20948_DLPF_6);

  udp.begin(udpPort);
  prevTime = millis();
}

void loop() {
  // Read both IMUs
  myIMU1.readSensor();
  myIMU2.readSensor();

  // Get accelerometer data
  xyzFloat acc1 = myIMU1.getGValues(); // Acceleration (G) IMU1
  xyzFloat acc2 = myIMU2.getGValues(); // Acceleration (G) IMU2

  // Convert acceleration from G to m/s^2
  ax1 = acc1.x * 9.81;
  ay1 = acc1.y * 9.81;
  az1 = acc1.z * 9.81;

  ax2 = acc2.x * 9.81;
  ay2 = acc2.y * 9.81;
  az2 = acc2.z * 9.81;

  // Get gyroscope data (angular velocity in degrees/sec)
  xyzFloat gyro1 = myIMU1.getGyrValues();
  xyzFloat gyro2 = myIMU2.getGyrValues();

  // Calculate time delta
  unsigned long currentTime = millis();
  dt = (currentTime - prevTime) / 1000.0;
  prevTime = currentTime;

  // Convert gyroscope data from degrees/sec to radians/sec
  float wx1 = gyro1.x * (PI / 180.0);
  float wy1 = gyro1.y * (PI / 180.0);
  float wz1 = gyro1.z * (PI / 180.0);

  float wx2 = gyro2.x * (PI / 180.0);
  float wy2 = gyro2.y * (PI / 180.0);
  float wz2 = gyro2.z * (PI / 180.0);

  // Compute relative angular velocity
  float dWx = wx2 - wx1; // Difference in angular velocity (X)
  float dWy = wy2 - wy1; // Difference in angular velocity (Y)
  float dWz = wz2 - wz1; // Difference in angular velocity (Z)

  // Compute relative rotation matrix R_rel (small angle approximation)
  float R_rel[3][3] = {
    {1, -dt * dWz, dt * dWy},
    {dt * dWz, 1, -dt * dWx},
    {-dt * dWy, dt * dWx, 1}
  };

  // Transform IMU2 acceleration into IMU1's frame
  float ax2_ref = R_rel[0][0] * ax2 + R_rel[0][1] * ay2 + R_rel[0][2] * az2;
  float ay2_ref = R_rel[1][0] * ax2 + R_rel[1][1] * ay2 + R_rel[1][2] * az2;
  float az2_ref = R_rel[2][0] * ax2 + R_rel[2][1] * ay2 + R_rel[2][2] * az2;

  // Subtract IMU1's acceleration to calculate relative acceleration
  float ax_rel = ax2_ref - ax1;
  float ay_rel = ay2_ref - ay1;
  float az_rel = az2_ref - az1;

   // Integrate acceleration to get velocity
  if (abs(ax_rel - ax_prev) > 0.2 || abs(ay_rel - ay_prev) > 0.2 || abs(az_rel - az_prev) > 0.2) {
    vx_rel += ax_rel * dt;
    vy_rel += ay_rel * dt;
    vz_rel += az_rel * dt;

    // Integrate velocity to get position
    x_rel += vx_rel * dt;
    y_rel += vy_rel * dt;
    z_rel += vz_rel * dt;
  }
  ax_prev = ax_rel;
  ay_prev = ay_rel;
  az_prev = az_rel;
  // Print relative acceleration (in IMU1's reference frame)
  char buffer1[100];
  sprintf(buffer1, "Relative Acc: X:%.2f Y:%.2f Z:%.2f", ax_rel, ay_rel, az_rel);
  Serial.println(buffer1);

  // Print normal acceleration of IMU2 (before transformation) and IMU1
  char udpmessage[100];
  //sprintf(udpmessage, "Relative Pos: X:%.2f Y:%.2f Z:%.2f", x_rel, y_rel, z_rel);
  sprintf(udpmessage, "%.2f, %.2f, %.2f", x_rel, y_rel, z_rel);
  Serial.println(udpmessage);

  udp.beginPacket(udpAddress, udpPort);
  udp.write((uint8_t *)udpmessage, strlen(udpmessage));
  udp.endPacket();
  Serial.print("Bytes sent: ");

  delay(100);
}
