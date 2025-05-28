#include <LedControl.h>

const int DIN_PIN = 11;
const int CS_PIN = 10;
const int CLK_PIN = 13;

LedControl matrix = LedControl(DIN_PIN, CLK_PIN, CS_PIN, 1);

#define MAX_INPUT 64
char inputData[MAX_INPUT];
byte inputPos = 0;

int ballX = 0, ballY = 0;
int p1Y = 0, p2Y = 0;

int modoActual = 0; // 0 = juego, 1 = figura
unsigned long tiempoFiguraMostrada = 0;
const unsigned long DURACION_FIGURA = 5000;

const byte trianglePattern[8] PROGMEM = {
  0b00001000,
  0b00011100,
  0b00111110,
  0b11111111,
  0b00000000,
  0b00000000,
  0b00000000,
  0b00000000
};

const byte heartPattern[8] PROGMEM = {
  0b01100110,
  0b11111111,
  0b11111111,
  0b01111110,
  0b00111100,
  0b00011000,
  0b00001000,
  0b00000000
};

const byte catPattern[8] PROGMEM = {
  0b00100100,
  0b00100100,
  0b00111100,
  0b01011010,
  0b11111111,
  0b01000100,
  0b01000100,
  0b00111100
};

const byte hiPattern[8] PROGMEM = {
  0b10101010,
  0b10101010,
  0b10101010,
  0b10101010,
  0b00101010,
  0b00101010,
  0b00101010,
  0b00101010
};

const byte squarePattern[8] PROGMEM = {
  0b11111111,
  0b10000001,
  0b10000001,
  0b10000001,
  0b10000001,
  0b10000001,
  0b10000001,
  0b11111111
};

void setup() {
  Serial.begin(115200);
  matrix.shutdown(0, false);
  matrix.setIntensity(0, 15);
  matrix.clearDisplay(0);
  // Serial.println("Arduino iniciado"); // Solo para debug
}

void loop() {
  while (Serial.available() > 0) {
    char c = Serial.read();
    if (c == '\n') {
      inputData[inputPos] = '\0';

      // NUEVO: Procesar comando de brillo
      if (strncmp(inputData, "#BRILLO:", 8) == 0) {
        int nivel = atoi(inputData + 8);
        nivel = constrain(nivel, 0, 15); // El rango válido para setIntensity es 0-15
        matrix.setIntensity(0, nivel);
        // Opcional: Serial.println("Brillo ajustado a: " + String(nivel));
      }
      // Procesar comando de juego
      else if (strncmp(inputData, "B:", 2) == 0) {
        modoActual = 0;
        parseData(inputData);
        drawGame();
      }
      else if (strcmp(inputData, "RESET") == 0) {
        modoActual = 0;
        matrix.clearDisplay(0);
      }
      else if (inputData[0] == '$') {
        modoActual = 1;
        processFigureCommand(inputData);
        tiempoFiguraMostrada = millis();
      }
      inputPos = 0;
    } else {
      if (inputPos < MAX_INPUT - 1) {
        inputData[inputPos++] = c;
      } else {
        inputPos = 0;
        while (Serial.available() > 0 && Serial.read() != '\n') { }
        break;
      }
    }
  }

  // MODO FIGURA
  if (modoActual == 1 && millis() - tiempoFiguraMostrada >= DURACION_FIGURA) {
    modoActual = 0;
    matrix.clearDisplay(0);
  }
}

void parseData(char *data) {
  int bx = -1, by = -1, p1 = -1, p2 = -1;
  int r = sscanf(data, "B:%d,%d;P1:%d;P2:%d;", &bx, &by, &p1, &p2);
  if (r == 4) {
    ballX = constrain(bx, 0, 7);
    ballY = constrain(by, 0, 7);
    p1Y = constrain(p1, 0, 6);
    p2Y = constrain(p2, 0, 6);
  }
}

void drawGame() {
  matrix.clearDisplay(0);
  matrix.setLed(0, p1Y, 0, true);
  matrix.setLed(0, p1Y + 1, 0, true);
  matrix.setLed(0, p2Y, 7, true);
  matrix.setLed(0, p2Y + 1, 7, true);
  matrix.setLed(0, ballY, ballX, true);
}

void processFigureCommand(char *command) {
  if (strcmp(command, "$TRIANGLE") == 0) mostrarFigura(trianglePattern);
  else if (strcmp(command, "$HEART") == 0) mostrarFigura(heartPattern);
  else if (strcmp(command, "$CAT") == 0) mostrarFigura(catPattern);
  else if (strcmp(command, "$HI") == 0) mostrarFigura(hiPattern);
  else if (strcmp(command, "$SQUARE") == 0) mostrarFigura(squarePattern);
}

void mostrarFigura(const byte datos[8]) {
  matrix.clearDisplay(0);
  for (int fila = 0; fila < 8; fila++) {
    byte filaData = pgm_read_byte(&datos[fila]);
    matrix.setRow(0, fila, filaData);
  }
}