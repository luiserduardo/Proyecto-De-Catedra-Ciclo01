using System;
using System.Collections.Generic;     // HashSet<Key>
using System.IO.Ports;                // SerialPort
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;        // DispatcherTimer
using System.Text.RegularExpressions;  // Regex

namespace PROYECTO_FINAL.vistas
{
    public partial class vistaJuegoPong : UserControl
    {
        // Constantes tipo double (para velocidad de pelota y paletas)
        private const double BALL_SPEED = 4.0;
        private const double PADDLE_SPEED = 6.0;

        // DispatcherTimer: Controla el ciclo del juego
        private readonly DispatcherTimer gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };

        // SerialPort: Comunicación serial con Arduino
        private SerialPort? serialPort;

        // Variables tipo double: posiciones y velocidades de la pelota
        private double ballPosX, ballPosY, ballVelX, ballVelY;

        // Variables tipo int: puntajes de cada jugador
        private int score1, score2;

        // bool: Estado del juego (activo/inactivo)
        private bool gameRunning;

        // HashSet<Key>: conjunto de teclas presionadas actualmente
        private readonly HashSet<Key> pressedKeys = new HashSet<Key>();

        // DateTime y otros int: control envío serial y última posición enviada
        private DateTime lastSendTime = DateTime.MinValue;
        private const int SERIAL_SEND_INTERVAL_MS = 100;
        private int lastBallX = -1, lastBallY = -1, lastP1Y = -1, lastP2Y = -1;

        // Constructor: inicializa componentes y eventos
        public vistaJuegoPong()
        {
            InitializeComponent();
            this.Focusable = true;
            this.Loaded += (s, e) =>
            {
                Keyboard.Focus(this);
                ResetGame();
            };
            this.GotFocus += UserControl_GotFocus;
            gameTimer.Tick += GameLoopTick;
            GameCanvas.MouseLeftButtonDown += GameCanvas_MouseLeftButtonDown;
            UpdateControlsState();
        }

        // Función: mantiene el foco de teclado en el control
        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }

        // Función: permite enfocar el control con clic de mouse
        private void GameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.Focus(this);
        }

        // Función: valida el formato del puerto serial (usa Regex)
        private void Control_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender == txtSerialPort && !Regex.IsMatch(txtSerialPort.Text, @"^COM\d+$"))
                MessageBox.Show("Formato inválido. Ejemplo: COM3");
        }

        // Función: reinicia el juego; resetea posiciones, puntajes y limpia el estado
        private void ResetGame()
        {
            gameRunning = false;
            gameTimer.Stop();
            score1 = 0;
            score2 = 0;
            ScorePlayer1.Text = "0";
            ScorePlayer2.Text = "0";
            CenterBall();
            CenterPaddles();
            pressedKeys.Clear();
            InitializeBallVelocity();
            UpdateControlsState();

            try
            {
                if (serialPort?.IsOpen == true)
                    serialPort.WriteLine("RESET");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar RESET: {ex.Message}");
            }
        }

        // Función: habilita/deshabilita controles de la UI según el estado del juego/puerto
        private void UpdateControlsState()
        {
            txtSerialPort.IsEnabled = !gameRunning;
            btnConectar.IsEnabled = !gameRunning && (serialPort == null || !serialPort.IsOpen);
            btnDesconectar.IsEnabled = !gameRunning && (serialPort != null && serialPort.IsOpen);
            txtSerialPort.Opacity = gameRunning ? 0.5 : 1.0;
            btnConectar.Opacity = gameRunning ? 0.5 : 1.0;
            btnDesconectar.Opacity = gameRunning ? 0.5 : 1.0;
        }

        // Función: inicializa la velocidad de la pelota (usa Random)
        private void InitializeBallVelocity()
        {
            var rand = new Random();
            ballVelX = rand.Next(2) == 0 ? BALL_SPEED : -BALL_SPEED;
            ballVelY = (rand.NextDouble() * 2 - 1) * BALL_SPEED;
        }

        // Función: centra la pelota en el canvas
        private void CenterBall()
        {
            ballPosX = (GameCanvas.ActualWidth - Ball.ActualWidth) / 2;
            ballPosY = (GameCanvas.ActualHeight - Ball.ActualHeight) / 2;
            Canvas.SetLeft(Ball, ballPosX);
            Canvas.SetTop(Ball, ballPosY);
        }

        // Función: centra ambas paletas en el canvas
        private void CenterPaddles()
        {
            Canvas.SetTop(Paddle1, (GameCanvas.ActualHeight - Paddle1.ActualHeight) / 2);
            Canvas.SetTop(Paddle2, (GameCanvas.ActualHeight - Paddle2.ActualHeight) / 2);
        }

        // Función: ciclo principal del juego (llamada por DispatcherTimer)
        private void GameLoopTick(object? sender, EventArgs e)
        {
            if (!gameRunning) return;
            ProcessInput();
            UpdateBallPosition();
            CheckCollisions();
            SendGameStateToArduino();
        }

        // Función: procesa teclas presionadas y mueve las paletas (usa HashSet<Key>)
        private void ProcessInput()
        {
            double deltaTime = 1.0;
            MovePaddle(Paddle1, Key.W, Key.S, deltaTime);
            MovePaddle(Paddle2, Key.Up, Key.Down, deltaTime);
        }

        // Función: mueve una paleta según las teclas y mantiene su posición dentro del canvas
        private void MovePaddle(UIElement paddle, Key upKey, Key downKey, double delta)
        {
            double currentTop = Canvas.GetTop(paddle);
            if (pressedKeys.Contains(upKey)) currentTop -= PADDLE_SPEED * delta;
            if (pressedKeys.Contains(downKey)) currentTop += PADDLE_SPEED * delta;
            currentTop = Math.Clamp(currentTop, 0, GameCanvas.ActualHeight - ((FrameworkElement)paddle).ActualHeight);
            Canvas.SetTop(paddle, currentTop);
        }

        // Función: mueve la pelota según su velocidad
        private void UpdateBallPosition()
        {
            ballPosX += ballVelX;
            ballPosY += ballVelY;
            Canvas.SetLeft(Ball, ballPosX);
            Canvas.SetTop(Ball, ballPosY);
        }

        // Función: detecta colisiones y actualiza puntajes
        // Usa estructuras Rect para colisión y variables int/double para posiciones
        private void CheckCollisions()
        {
            if (ballPosY <= 0 || ballPosY + Ball.ActualHeight >= GameCanvas.ActualHeight)
                ballVelY *= -1;

            Rect ballRect = new Rect(Canvas.GetLeft(Ball), Canvas.GetTop(Ball), Ball.ActualWidth, Ball.ActualHeight);
            Rect paddle1Rect = new Rect(Canvas.GetLeft(Paddle1), Canvas.GetTop(Paddle1), Paddle1.ActualWidth, Paddle1.ActualHeight);
            Rect paddle2Rect = new Rect(Canvas.GetLeft(Paddle2), Canvas.GetTop(Paddle2), Paddle2.ActualWidth, Paddle2.ActualHeight);

            if (ballRect.IntersectsWith(paddle1Rect))
                ballVelX = Math.Abs(ballVelX);
            else if (ballRect.IntersectsWith(paddle2Rect))
                ballVelX = -Math.Abs(ballVelX);

            if (ballPosX <= 0)
            {
                score2++;
                ScorePlayer2.Text = score2.ToString();
                CheckWinner();
                if (gameRunning) ResetBall();
            }
            else if (ballPosX + Ball.ActualWidth >= GameCanvas.ActualWidth)
            {
                score1++;
                ScorePlayer1.Text = score1.ToString();
                CheckWinner();
                if (gameRunning) ResetBall();
            }
        }

        // Función: verifica si algún jugador ganó
        private void CheckWinner()
        {
            if (score1 >= 5)
            {
                gameRunning = false;
                gameTimer.Stop();
                UpdateControlsState();
                MessageBox.Show("¡JUGADOR 1 GANA!");
            }
            else if (score2 >= 5)
            {
                gameRunning = false;
                gameTimer.Stop();
                UpdateControlsState();
                MessageBox.Show("¡JUGADOR 2 GANA!");
            }
        }

        // Función: resetea la pelota tras un punto
        private void ResetBall()
        {
            CenterBall();
            InitializeBallVelocity();
        }

        // Función: envía el estado del juego al Arduino solo si hubo cambios
        // Utiliza int para conversión de posiciones y SerialPort para la comunicación
        private void SendGameStateToArduino()
        {
            if ((DateTime.Now - lastSendTime).TotalMilliseconds < SERIAL_SEND_INTERVAL_MS)
                return;

            int ballX = (int)Math.Clamp((ballPosX / GameCanvas.ActualWidth) * 8, 0, 7);
            int ballY = (int)Math.Clamp((ballPosY / GameCanvas.ActualHeight) * 8, 0, 7);
            int p1Y = (int)Math.Clamp((Canvas.GetTop(Paddle1) / GameCanvas.ActualHeight) * 6, 0, 6);
            int p2Y = (int)Math.Clamp((Canvas.GetTop(Paddle2) / GameCanvas.ActualHeight) * 6, 0, 6);

            if (ballX == lastBallX && ballY == lastBallY && p1Y == lastP1Y && p2Y == lastP2Y)
                return;

            lastBallX = ballX;
            lastBallY = ballY;
            lastP1Y = p1Y;
            lastP2Y = p2Y;
            lastSendTime = DateTime.Now;

            try
            {
                if (serialPort?.IsOpen == true)
                    serialPort.WriteLine($"B:{ballX},{ballY};P1:{p1Y};P2:{p2Y};");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serial: {ex.Message}");
            }
        }

        // Función: conecta al puerto serie (usa SerialPort y Regex para validación)
        private void AssignPort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string portName = txtSerialPort.Text.Trim().ToUpper();
                if (!Regex.IsMatch(portName, @"^COM\d+$")) throw new FormatException();

                if (serialPort != null && serialPort.IsOpen) serialPort.Close();

                serialPort = new SerialPort(portName, 115200) { ReadTimeout = 300 };
                serialPort.Open();
                MessageBox.Show($"Conectado a {portName}");
                UpdateControlsState();
            }
            catch (FormatException)
            {
                MessageBox.Show("Formato inválido. Ejemplo: COM3");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}");
            }
        }

        // Función: desconecta el puerto serie
        private void DisconnectPort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Close();
                    MessageBox.Show("Puerto desconectado");
                    UpdateControlsState();
                }
                else
                {
                    MessageBox.Show("No hay puerto conectado");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desconectar: {ex.Message}");
            }
        }

        // Función: inicia el juego (activa el timer)
        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            gameRunning = true;
            gameTimer.Start();
            UpdateControlsState();
            Keyboard.Focus(this);
        }

        // Función: pausa el juego (detiene el timer)
        private void PauseGame_Click(object sender, RoutedEventArgs e)
        {
            gameRunning = false;
            UpdateControlsState();
        }

        // Función: reinicia el juego (llama a ResetGame)
        private void ResetGame_Click(object sender, RoutedEventArgs e) => ResetGame();

        // Funciones: control de teclas presionadas (usa HashSet<Key>)
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsGameControlKey(e.Key) && !pressedKeys.Contains(e.Key))
            {
                pressedKeys.Add(e.Key);
                e.Handled = true;
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (pressedKeys.Contains(e.Key))
            {
                pressedKeys.Remove(e.Key);
                e.Handled = true;
            }
        }

        // Función: verifica si una tecla es de control de paletas
        private bool IsGameControlKey(Key key) =>
            key == Key.W || key == Key.S || key == Key.Up || key == Key.Down;

        // Función: limpieza al descargar el control, cierra el puerto y detiene timer
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (serialPort?.IsOpen == true) serialPort.Close();
            gameTimer.Stop();
        }
    }
}