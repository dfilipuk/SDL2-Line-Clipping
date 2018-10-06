using System;
using System.Threading;
using SdlApplication.Figure;
using SDL2;

namespace SdlApplication.Window
{
    public class SdlWindow
    {
        private readonly int _renderLoopTimeoutMs = 10;

        private readonly int _screenWidth;
        private readonly int _screenHeight;
        private readonly string _title;

        private IntPtr _renderer;
        private IntPtr _window;

        private GenericFigure _currentFigure;
        private GenericFigure _rectangle;
        private GenericFigure _trapeze;
        private GenericFigure _ellipse;

        public SdlWindow(string title, int screenWidth, int screenHeight)
        {
            _title = title;
            _screenHeight = screenHeight;
            _screenWidth = screenWidth;
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

            _rectangle = new Rectangle(width / 2, height / 2, 0, width - 60, height - 60);
            _trapeze = new Trapeze(100, height / 2, 0, 200, 100);
            _ellipse = new Ellipse(width - 100, height / 2, 0, 100, 50);
            _currentFigure = _trapeze;
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
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        var key = sdlEvent.key;
                        switch (key.keysym.sym)
                        {
                            case SDL.SDL_Keycode.SDLK_w:
                                _currentFigure.Move(MoveDirection.Up);
                                _currentFigure.CalculateCurrentPosition();
                                break;
                            case SDL.SDL_Keycode.SDLK_a:
                                _currentFigure.Move(MoveDirection.Left);
                                _currentFigure.CalculateCurrentPosition();
                                break;
                            case SDL.SDL_Keycode.SDLK_s:
                                _currentFigure.Move(MoveDirection.Down);
                                _currentFigure.CalculateCurrentPosition();
                                break;
                            case SDL.SDL_Keycode.SDLK_d:
                                _currentFigure.Move(MoveDirection.Right);
                                _currentFigure.CalculateCurrentPosition();
                                break;
                            case SDL.SDL_Keycode.SDLK_q:
                                _currentFigure.Rotate(RotateDirection.Left);
                                _currentFigure.CalculateCurrentPosition();
                                break;
                            case SDL.SDL_Keycode.SDLK_e:
                                _currentFigure.Rotate(RotateDirection.Right);
                                _currentFigure.CalculateCurrentPosition();
                                break;
                            case SDL.SDL_Keycode.SDLK_1:
                                _currentFigure = _trapeze;
                                break;
                            case SDL.SDL_Keycode.SDLK_2:
                                _currentFigure = _ellipse;
                                break;
                            }
                        break;
                    }
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        if (sdlEvent.button.button == SDL.SDL_BUTTON_LEFT)
                        {
                            // do smth
                        }
                        else
                        if (sdlEvent.button.button == SDL.SDL_BUTTON_RIGHT)
                        {
                            // do smth
                        }
                        break;
                    }
                }
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

            _rectangle.Draw(_renderer);
            _trapeze.Draw(_renderer);
            _ellipse.Draw(_renderer);

            SDL.SDL_RenderPresent(_renderer);
        }
    }
}
