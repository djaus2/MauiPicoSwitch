#include <SerialBT.h> 

#define debounce 2000;

bool lastState = false;
bool ready = false;
unsigned long startTime = 0;
unsigned long Debounce = debounce;

#define SWITCH_PIN16 16
#define SWITCH_PIN18 18
#define SWITCH_PIN20 20

#define LED_PIN26  26
#define LED_PIN27  27
#define LED_PIN28  28


bool doneReady = false;
bool waiting4switch16 = false;
bool waiting4switch18 = false;
bool waiting4switch20 = false;

void setup() 
{ 
  pinMode(SWITCH_PIN16, INPUT_PULLUP);
  pinMode(SWITCH_PIN18, INPUT_PULLUP);
  pinMode(SWITCH_PIN20, INPUT_PULLUP);

  pinMode(LED_PIN26, OUTPUT);
  pinMode(LED_PIN27, OUTPUT);
  pinMode(LED_PIN28, OUTPUT);

  digitalWrite(LED_PIN26, LOW);
  digitalWrite(LED_PIN27, LOW);
  digitalWrite(LED_PIN28, LOW);
 7,
  Serial.begin((9600));
  while(!Serial);
  SerialBT.setName("PicoW uPPERCASE"); // 00:00:00:00:00:00"); 
  SerialBT.begin(); 
  while(!SerialBT);
  //Flush
  while (SerialBT.available()) {
    char c = SerialBT.read();
  } 
  Serial.println(("started"));
  ready = false;
  //lastState = !digitalRead(SWITCH_PIN);
  startTime = millis()  ;
  waiting4switch16 = false;
  waiting4switch18 = false;
  waiting4switch20 = false;
  doneReady = false;
} 

void loop() 
{ 
  if (SerialBT) 
  { 
    //Serial.println(("\t\t\t\tserialBT"));
    
    if(ready)
    {
      if(waiting4switch16)
      {
        if(!doneReady)
          Serial.println("Ready");
        doneReady = true;
        bool currentState = !digitalRead(SWITCH_PIN16); // Active low
        if (currentState != lastState) 
        {
          lastState = currentState;
          if(!lastState)
          {
            SerialBT.write('A'); 
            Serial.println('A'); 
            digitalWrite(LED_PIN26, HIGH);
            startTime = millis();
          }
          else
          {
            SerialBT.write('B'); 
            Serial.println('B'); 
            digitalWrite(LED_PIN26, LOW);

            ready = false;
            doneReady = false;
            waiting4switch16 = false;
          }
        }
      }
      else if(waiting4switch18)
      {
        if(!doneReady)
          Serial.println("Ready");
        doneReady = true;
        bool currentState = !digitalRead(SWITCH_PIN18); // Active low
        if (currentState != lastState) 
        {
          lastState = currentState;
          if(!lastState)
          {
            SerialBT.write('C'); 
            Serial.println('C'); 
            digitalWrite(LED_PIN27, HIGH);
            startTime = millis();
          }
          else
          {
            SerialBT.write('D'); 
            Serial.println('D'); 
            digitalWrite(LED_PIN27, LOW);

            ready = false;
            doneReady = false;
            waiting4switch18 - false;
          }
        }
      }
      else if(waiting4switch20)
      {
        if(!doneReady)
          Serial.println("Ready");
        doneReady = true;
        bool currentState = !digitalRead(SWITCH_PIN20); // Active low
        if (currentState != lastState) 
        {
          lastState = currentState;
          if(!lastState)
          {
            SerialBT.write('E'); 
            Serial.println('E'); 
            startTime = millis();
            digitalWrite(LED_PIN28, HIGH);
          }
          else
          {
            SerialBT.write('F'); 
            Serial.println('F'); 
            digitalWrite(LED_PIN28, LOW);

            ready = false;
            doneReady = false;
            waiting4switch20 - false;
          }
        }
      }
    } 
    else 
    {
      if (SerialBT.available()) 
      { 
        Serial.println("---Avail");
        unsigned long currentTime = millis();
        currentTime -= startTime;
        if(currentTime > Debounce )
        { 
          char c = SerialBT.read(); 
          Serial.print("========");
          Serial.println(c);
          c = toupper(c);
          //Flush incomming commands
          while (SerialBT.available()) {
            char c = SerialBT.read();
          }
          if(c=='R')
          {
            ready=true;
            waiting4switch16=true;
            waiting4switch18=false;
            waiting4switch20=false;
            lastState = !digitalRead(SWITCH_PIN16);
            Serial.print("LastState: ");
            Serial.println(lastState);
            SerialBT.println('K');
          }
          else if(c=='S')
          {
            ready=true;
            waiting4switch16=false;
            waiting4switch18=true;
            waiting4switch20=false;
            lastState = !digitalRead(SWITCH_PIN18);
            Serial.print("LastState: ");
            Serial.println(lastState);
            SerialBT.println('K');
          }
          else if(c=='T')
          {
            ready=true;
            waiting4switch16=false;
            waiting4switch18=false;
            waiting4switch20=true;
            lastState = !digitalRead(SWITCH_PIN20);
            Serial.print("LastState: ");
            Serial.println(lastState);
            SerialBT.println('K');
          }
        }         
      }
    }
  } 
}