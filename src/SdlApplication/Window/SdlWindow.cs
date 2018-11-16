using System;
using System.Threading;
using Clipping2D.Clipping;
using Clipping2D.Drawer;
using SdlApplication.Drawer;
using SdlApplication.Figure;
using SDL2;

namespace SdlApplication.Window
{
    public class SdlWindow
    {
        private readonly int _renderLoopTimeoutMs = 10;
        private readonly int _offsetFromBorders = 30;
        private readonly double _defaultRotationAngle = 0;
        private readonly int _timerInterval = 40;

        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly string _title;

        private object _workWithFigures = new object();
        private IntPtr _renderer;
        private IntPtr _window;
        private Timer _timer;
        private bool _isTimerWork;
        private MoveDirection _trapezeMoveDirection;
        private MoveDirection _ellipseMoveDirection;
        private MovablePolygon2D _currentFigure;
        private MovablePolygon2D _rectangle;
        private MovablePolygon2D _trapeze;
        private MovablePolygon2D _ellipse;
        private IPolygonDrawer _rectabgleDrawer;
        private IPolygonDrawer _trapezeDrawer;
        private IPolygonDrawer _ellipseDrawer;

        public SdlWindow(string title, int screenWidth, int screenHeight)
        {
            _title = title;
            _screenHeight = screenHeight;
            _screenWidth = screenWidth;
            _trapezeMoveDirection = MoveDirection.Right;
            _ellipseMoveDirection = MoveDirection.Left;
            _rectabgleDrawer = DrawerFactory.SquareDrawer;
            _trapezeDrawer = DrawerFactory.SquareDrawer;
            _ellipseDrawer = DrawerFactory.RoundDrawer;
        }

        public void Open()
        {
            var thred = new Thread(() =>
            {
                SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
                _window = SDL.SDL_CreateWindow(_title, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
                    _screenWidth, _screenHeight, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
                _renderer = SDL.SDL_CreateRenderer(_window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

                InitializeFigures();
                InitializeTimer();
                WindowProcedure();

                SDL.SDL_DestroyRenderer(_renderer);
                SDL.SDL_DestroyWindow(_window);
                SDL.SDL_Quit();
            });
            thred.Start();
            thred.Join();
        }

        private void InitializeFigures()
        {
            int width, height;
            SDL.SDL_GetWindowSize(_window, out width, out height);

            _rectangle = FiguresFactory.CreateRectangle(
                width / 2, height / 2, _defaultRotationAngle,
                _offsetFromBorders, width - _offsetFromBorders, _offsetFromBorders, height - _offsetFromBorders,
                width - 300, height - 300
            );
            _trapeze = FiguresFactory.CreateTrapeze(
                115, height / 2 + 15, _defaultRotationAngle,
                _offsetFromBorders, width - _offsetFromBorders, _offsetFromBorders, height - _offsetFromBorders,
                200, 100
            );
            _ellipse = FiguresFactory.CreateEllipse(
                width - 100, height / 2, _defaultRotationAngle,
                _offsetFromBorders, width - _offsetFromBorders, _offsetFromBorders, height - _offsetFromBorders,
                100, 57
            );
            _currentFigure = _trapeze;
            PerformClipping();
        }

        private void InitializeTimer()
        {
            _timer = new Timer(TimerProcedure, null, 0, _timerInterval);
            _isTimerWork = true;
        }

        private void DeleteTimer()
        {
            _timer.Dispose();
            _isTimerWork = false;
        }

        private void ChangeTimerState()
        {
            if (_isTimerWork)
            {
                DeleteTimer();
            }
            else
            {
                InitializeTimer();
            }
        }

        private void WindowSizeChanged()
        {
            int width, height;
            SDL.SDL_GetWindowSize(_window, out width, out height);


            _rectangle = FiguresFactory.CreateRectangle(
                width / 2, height / 2, _defaultRotationAngle,
                _offsetFromBorders, width - _offsetFromBorders, _offsetFromBorders, height - _offsetFromBorders,
                width - 300, height - 300
            );
            _trapeze.SetMovementBorders(
                _offsetFromBorders, width - _offsetFromBorders, _offsetFromBorders, height - _offsetFromBorders
            );
            _ellipse.SetMovementBorders(
                _offsetFromBorders, width - _offsetFromBorders, _offsetFromBorders, height - _offsetFromBorders
            );
        }

        private void PerformClipping()
        {
            _trapeze.ResetClipping();
            _ellipse.ResetClipping();
            _trapeze.ClipByPolygon(_rectangle, ClippingType.Inside);
            _trapeze.ClipByPolygon(_ellipse, ClippingType.External);
            _ellipse.ClipByPolygon(_rectangle, ClippingType.Inside);
        }

        private void MoveFigures()
        {
            _trapeze.Rotate(RotateDirection.Right);
            _ellipse.Rotate(RotateDirection.Left);

            if (!_trapeze.Move(_trapezeMoveDirection))
            {
                _trapezeMoveDirection = _trapezeMoveDirection == MoveDirection.Left
                    ? MoveDirection.Right
                    : MoveDirection.Left;
            }

            if (!_ellipse.Move(_ellipseMoveDirection))
            {
                _ellipseMoveDirection = _ellipseMoveDirection == MoveDirection.Left
                    ? MoveDirection.Right
                    : MoveDirection.Left;
            }
        }

        private void TimerProcedure(object arg)
        {
            lock (_workWithFigures)
            {
                MoveFigures();
            }
        }

        private void WindowProcedure()
        {
            bool exit = false;
            while (!exit)
            {
                SDL.SDL_Event sdlEvent;
                SDL.SDL_PollEvent(out sdlEvent);
                switch (sdlEvent.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                    {
                        exit = true;
                        break;
                    }
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    {
                        switch (sdlEvent.window.windowEvent)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                                lock (_workWithFigures)
                                {
                                    WindowSizeChanged();
                                }
                                break;
                        }

                        break;
                    }
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        var key = sdlEvent.key;

                        if (!_isTimerWork)
                        {
                            switch (key.keysym.sym)
                            {
                                case SDL.SDL_Keycode.SDLK_w:
                                    _currentFigure.Move(MoveDirection.Up);
                                    break;
                                case SDL.SDL_Keycode.SDLK_a:
                                    _currentFigure.Move(MoveDirection.Left);
                                    break;
                                case SDL.SDL_Keycode.SDLK_s:
                                    _currentFigure.Move(MoveDirection.Down);
                                    break;
                                case SDL.SDL_Keycode.SDLK_d:
                                    _currentFigure.Move(MoveDirection.Right);
                                    break;
                                case SDL.SDL_Keycode.SDLK_q:
                                    _currentFigure.Rotate(RotateDirection.Left);
                                    break;
                                case SDL.SDL_Keycode.SDLK_e:
                                    _currentFigure.Rotate(RotateDirection.Right);
                                    break;
                            }
                        }

                        switch (key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_SPACE:
                                ChangeTimerState();
                                break;
                            case SDL.SDL_Keycode.SDLK_1:
                                _currentFigure = _trapeze;
                                break;
                            case SDL.SDL_Keycode.SDLK_2:
                                _currentFigure = _ellipse;
                                break;
                            case SDL.SDL_Keycode.SDLK_F1:
                                _rectabgleDrawer = DrawerFactory.SquareDrawer;
                                _trapezeDrawer = DrawerFactory.SquareDrawer;
                                _ellipseDrawer = DrawerFactory.RoundDrawer;
                                break;
                            case SDL.SDL_Keycode.SDLK_F2:
                                _rectabgleDrawer = DrawerFactory.UniversalDrawer;
                                _trapezeDrawer = DrawerFactory.UniversalDrawer;
                                _ellipseDrawer = DrawerFactory.UniversalDrawer;
                                break;

                            }

                        break;
                    }
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        if (sdlEvent.button.button == SDL.SDL_BUTTON_LEFT)
                        {
                            lock (_workWithFigures)
                            {
                                _currentFigure.MoveTo(sdlEvent.button.x, sdlEvent.button.y);
                            }
                        }

                        break;
                    }
                }

                _trapeze.CalculateCurrentEdges();
                _ellipse.CalculateCurrentEdges();
                PerformClipping();
                DrawFigures();
                Thread.Sleep(_renderLoopTimeoutMs);
            }
        }

        // Формат цвета в HEX коде:
        //     0xRRGGBB00
        //  где R: от 00 до FF
        //      G: от 00 до FF
        //      B: от 00 до FF 
        private void DrawFigures()
        {
            SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255);
            SDL.SDL_RenderClear(_renderer);
            SDL.SDL_SetRenderDrawColor(_renderer, 255, 255, 255, 255);

            _rectangle.Draw(_renderer, _rectabgleDrawer);
            _trapeze.Draw(_renderer, _trapezeDrawer);
            _ellipse.Draw(_renderer, _ellipseDrawer);

            SDL.SDL_RenderPresent(_renderer);
        }
    }
}