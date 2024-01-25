#include <Wire.h>
#include <MPU6050.h>
#include <vector_type.h>

const int FilterWindow = 3;

template <typename T>
class SimpleFilter{
  T valuesSum = 0;
  T* valuesBuffer = new T[FilterWindow];
  int currentValue = 0;
  
  public:
  T calculate(T value){
    valuesSum += value;
    valuesSum -= valuesBuffer[currentValue%FilterWindow];
    valuesBuffer[currentValue%FilterWindow] = value;
    currentValue++;
    return valuesSum / FilterWindow;
  }

  T& getValuesSum(){
    return valuesSum;
  }

  T* getArray(){
    return valuesBuffer;
  }
};

class Timer{
  unsigned long startTime = 0;

public:
  void start(){
    startTime = micros();
  }

  double stop(){
    return (micros() - startTime) / 100000.0;
  }
};

SimpleFilter<vec3_t> rotationFilter;
MPU6050 mpu;



void setup() {
    Serial.begin(9600);
    initializeMPU6050();
    vec3_t* array = rotationFilter.getArray();
    for( int i = 0; i < FilterWindow; i++ )
      array[i] = vec3_t(0,0,0);
    rotationFilter.getValuesSum() = vec3_t(0,0,0);
}

void loop() {
  static vec3_t rotation = getRotationByAcceleration(vec3_t(getAcceleration().x, getAcceleration().y, 0));
  static double dt = 0.0;
  Timer timer;
  timer.start();
  vec3_t acceleration = getAcceleration();
  vec3_t gyroAcceleration = getRotation();
  vec3_t accelerationRotation = getRotationByAcceleration(acceleration);
  vec3_t gyroRotation = getRotationByGyroAcceleration(rotation, gyroAcceleration, dt);

  rotation = (accelerationRotation*0.10 + gyroRotation*0.90);
  if( rotation.mag() < 500.0 && rotation.mag() > -500.0)
    rotation = rotationFilter.calculate(rotation);
  rotation.z = gyroRotation.z;


  Serial.println(String(rotation.x) + "," + String(rotation.y) + "," + String(rotation.z));
  delay(5);
  dt = timer.stop();
}

float unwrapAngle(float angle){
  float wrapped1 = angle < 0.0 ? (360.0 + angle) : angle;
  float wrapped2 = abs(wrapped1 - 360.0) < 0.8 ? 0.0 : wrapped1;
  return angle;
}

vec3_t getAcceleration(){
  const double g = 9.8066;
  int16_t ax, ay, az;
  mpu.getAcceleration(&ax, &ay, &az);
  double gax = ax / 2048.0*g;
  double gay = ay / 2048.0*g;
  double gaz = az / 2048.0*g;
  return vec3_t(gax, gay, gaz);
}

vec3_t getRotation(){
  int16_t rx, ry, rz;
  mpu.getRotation(&rx, &ry, &rz);
  double anglex = rx / 131.0;
  double angley = ry / 131.0;
  double anglez = rz / 131.0;
  return vec3_t(anglex, angley, anglez);
}

vec3_t getRotationByAcceleration(vec3_t acceleration){
  vec3_t accelerationRotation = vec3_t(0, 0, 0);  
  accelerationRotation.x = degrees(atan2(acceleration.y, acceleration.z));
  accelerationRotation.y = degrees(atan2(acceleration.x, acceleration.z));
  accelerationRotation.z = degrees(atan2(acceleration.z, acceleration.x));
  float angleX = unwrapAngle(accelerationRotation.x);
  float angleY = unwrapAngle(accelerationRotation.y);
  float angleZ = 0;
  return vec3_t(angleX, -angleY, angleZ);
}

vec3_t getRotationByGyroAcceleration(vec3_t currentRotation, vec3_t acceleration, double dt){  
  // integration
  return currentRotation +(acceleration * dt) * 0.78260869565; // don't know why i need correct it by magik number
}


void initializeMPU6050(){
    mpu.initialize(ACCEL_FS::A16G, GYRO_FS::G250DPS);
    mpu.CalibrateAccel(50);
    mpu.CalibrateGyro(50);
}
