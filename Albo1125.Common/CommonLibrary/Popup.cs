using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Rage;
using Object = System.Object;

namespace Albo1125.Common.CommonLibrary
{
    /// <summary>
    /// Represents a popup dialog that can be displayed to the user.
    /// </summary>
    public class Popup
    {
        private static readonly List<Popup> PopupQueue = new List<Popup>();
        private static readonly List<GameFiber> PopupFibersToDelete = new List<GameFiber>();
        private static bool _cleanGameFibersRunning;

        /// <summary>
        /// Cleans up the GameFibers that are no longer needed.
        /// </summary>
        private static void CleanGameFibers()
        {
            _cleanGameFibersRunning = true;
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Sleep(0);
                    foreach (var gameFiber in PopupFibersToDelete.ToArray())
                    {
                        if (gameFiber.IsAlive)
                        {
                            gameFiber.Abort();
                        }
                        PopupFibersToDelete.Remove(gameFiber);
                    }
                }
            });
        }

        private static readonly Stopwatch Timer = new Stopwatch();

        public List<string> PopupLines = new List<string>();

        public string PopupText
        {
            set
            {
                PopupLines = value.WrapText(720, "Arial Bold", 15.0f, out _popupTextLineHeight);
                AddAnswersToText();
            }
        }
        private List<string> _answersAsDisplayed = new List<string>();

        private double _popupTextLineHeight;
        public string PopupTitle = "";
        public bool PauseGame;
        private List<string> _answers = new List<string>();
        public bool ShowEnterConfirmation = true;
        public bool ForceDisplay;
        public int IndexOfGivenAnswer = -1;
        public bool ShuffleAnswers;
        public Action<Popup> ShownCallback;
        public int Delay;
        private GameFiber _gameFiber;
        public bool HasDisplayed { get; private set; }

        public bool IsDisplaying { get; private set; }

        public Popup() { }
        
        
        public Popup(string title, string text, bool pauseGame, bool showConfirmation, Action<Popup> shownCallback = null, int delay = 0, bool forceDisplay = false)
        {
            PopupTitle = title;
            PopupText = text;
            PauseGame = pauseGame;
            ShowEnterConfirmation = showConfirmation;
            ForceDisplay = forceDisplay;
            ShownCallback = shownCallback;
            Delay = delay;
            AddAnswersToText();
        }

        public Popup(string title, string text, List<string> answers, bool shuffleAnswers, bool pauseGame, Action<Popup> shownCallback = null, int delay = 0, bool forceDisplay = false)
        {
            PopupTitle = title;
            PopupText = text;
            _answers = answers;
            PauseGame = pauseGame;
            ForceDisplay = forceDisplay;
            ShuffleAnswers = shuffleAnswers;
            ShownCallback = shownCallback;
            Delay = delay;
            ShowEnterConfirmation = false;
            AddAnswersToText();
        }

        public Popup(string title, List<string> lines, List<string> answers, bool shuffleAnswers, bool pauseGame, bool showConfirmation, Action<Popup> shownCallback = null, int delay = 0, bool forceDisplay = false)
        {
            PopupTitle = title;
            PopupLines = lines;
            _answers = answers;
            PauseGame = pauseGame;
            ShowEnterConfirmation = showConfirmation;
            ForceDisplay = forceDisplay;
            ShownCallback = shownCallback;
            Delay = delay;
            AddAnswersToText();
        }

        /// <summary>
        /// Adds the answers to the popup text.
        /// </summary>
        /// <remarks>
        /// The answers are added to the popup text as options for the user to choose.
        /// </remarks>
        private void AddAnswersToText()
        {
            if (_answers == null) return;
            
            _answersAsDisplayed = ShuffleAnswers ? _answers.Shuffle() : new List<string>(_answers);
            
            for (var i = 0; i < _answers.Count; i++)
            {
                PopupLines.AddRange(("[" + (i + 1) + "] " + _answers[i]).WrapText(720, "Arial Bold", 15.0f, out _popupTextLineHeight));
            }
        }

        /// <summary>
        /// Displays a popup message.
        /// </summary>
        public void Display()
        {
            if (!_cleanGameFibersRunning)
            {
                CleanGameFibers();
            }
            HasDisplayed = false;
            IndexOfGivenAnswer = -1;
            PopupQueue.Add(this);
            if (_gameFiber != null && PopupFibersToDelete.Contains(_gameFiber))
            {
                PopupFibersToDelete.Remove(_gameFiber);
            }
            Game.LogTrivial("Adding " + PopupTitle + " popup to queue.");
            
            //TODO: Extract this Method
            _gameFiber = new GameFiber(delegate
            {
                
                while (!ForceDisplay)
                {
                    GameFiber.Yield();
                    if (CommonVariables.DisplayTime || Game.IsPaused) continue;
                    
                    if (PopupQueue.Count > 0 && PopupQueue[0] == this)
                    {
                        break;
                    }

                    if (PopupQueue.Count == 0)
                    {
                        break;
                    }
                }

                CommonVariables.DisplayTime = true;
                if (PauseGame)
                {
                    Game.IsPaused = true;
                }
                if (ShowEnterConfirmation)
                {
                    PopupLines.AddRange(("Press Enter to close.").WrapText(720, "Arial Bold", 15.0f, out _popupTextLineHeight));
                }
                IsDisplaying = true;
                GameFiber.Sleep(Delay);
                PopupQueue.Remove(this);
                Game.RawFrameRender += DrawPopup;
                Game.LogTrivial("Drawing " + PopupTitle + " popup message");

                Timer.Restart();
                if (ShowEnterConfirmation)
                {
                    while (IsDisplaying)
                    {
                        if (PauseGame)
                        {
                            Game.IsPaused = true;
                        }
                        GameFiber.Yield();
                        if (Timer.ElapsedMilliseconds > 25000)
                        {
                            Game.DisplayNotification("A textbox is currently being shown in the centre of your screen. If you can't see it, RPH had an issue initializing with DirectX and your RPH console won't work either - ask for support on the RPH Discord (link at www.ragepluginhook.net");
                            Timer.Restart();
                        }

                        if (!Game.IsKeyDown(Keys.Enter)) continue;
                        
                        Game.LogTrivial("ClosePopup is pressed");
                        Hide();
                        break;
                    }
                }

                else if (_answers != null && _answers.Count > 0)
                {
                    while (IsDisplaying)
                    {
                        if (PauseGame)
                        {
                            Game.IsPaused = true;
                        }
                        GameFiber.Yield();
                        if (Timer.ElapsedMilliseconds > 25000)
                        {
                            Game.DisplayNotification("A textbox is currently being shown in the centre of your screen. If you can't see it, RPH had an issue initializing with DirectX and your RPH console won't work either - ask for support on the RPH Discord (link at www.ragepluginhook.net");
                            Timer.Restart();
                        }
                        
                        //TODO:Convert Code below to switch statement via Game.GetKeyboardState().PressedKeys
                        if (Game.IsKeyDown(Keys.D1))
                        {
                            if (_answersAsDisplayed.Count >= 1)
                            {
                                IndexOfGivenAnswer = _answers.IndexOf(_answersAsDisplayed[0]);
                                Hide();

                            }
                        }
                        if (Game.IsKeyDown(Keys.D2))
                        {
                            if (_answersAsDisplayed.Count >= 2)
                            {
                                IndexOfGivenAnswer = _answers.IndexOf(_answersAsDisplayed[1]);
                                Hide();
                            }
                        }
                        if (Game.IsKeyDown(Keys.D3))
                        {
                            if (_answersAsDisplayed.Count >= 3)
                            {
                                IndexOfGivenAnswer = _answers.IndexOf(_answersAsDisplayed[2]);
                                Hide();
                            }
                        }
                        if (Game.IsKeyDown(Keys.D4))
                        {
                            if (_answersAsDisplayed.Count >= 4)
                            {
                                IndexOfGivenAnswer = _answers.IndexOf(_answersAsDisplayed[3]);
                                Hide();
                            }
                        }
                        if (Game.IsKeyDown(Keys.D5))
                        {
                            if (_answersAsDisplayed.Count >= 5)
                            {
                                IndexOfGivenAnswer = _answers.IndexOf(_answersAsDisplayed[4]);
                                Hide();
                            }
                        }
                        if (Game.IsKeyDown(Keys.D6))
                        {
                            if (_answersAsDisplayed.Count >= 6)
                            {
                                IndexOfGivenAnswer = _answers.IndexOf(_answersAsDisplayed[5]);
                                Hide();
                            }
                        }
                    }
                }
                Timer.Stop();
            });
            _gameFiber.Start();
            
            
        }

        /// <summary>
        /// Hides the popup dialog if it is currently being displayed.
        /// </summary>
        /// <remarks>
        /// This method stops rendering the popup dialog and resumes the game if it was paused.
        /// It also invokes the shown callback, if provided, and marks the popup as having been displayed.
        /// </remarks>
        /// <seealso cref="Popup.Display"/>
        public void Hide()
        {
            if (!IsDisplaying) return;
            
            Game.RawFrameRender -= DrawPopup;
            CommonVariables.DisplayTime = false;
                
            if (PauseGame)
            {
                Game.IsPaused = false;
            }
            IsDisplaying = false;
            HasDisplayed = true;
            PopupFibersToDelete.Add(_gameFiber);
            ShownCallback?.Invoke(this);
        }

        /// <summary>
        /// Displays a popup dialog to the user.
        /// </summary>
        private void DrawPopup(Object sender, GraphicsEventArgs e)
        {
            if (!IsDisplaying) return;
            
            var drawRect = new Rectangle(Game.Resolution.Width / 4, Game.Resolution.Height / 7, 750, 200);
            var drawBorder = new Rectangle(Game.Resolution.Width / 4 - 5, Game.Resolution.Height / 7 - 5, 760, 210);
            e.Graphics.DrawRectangle(drawBorder, Color.FromArgb(90, Color.Black));
            e.Graphics.DrawRectangle(drawRect, Color.Black);

            e.Graphics.DrawText(PopupTitle, "Aharoni Bold", 18.0f, new PointF(drawBorder.X + 5, drawBorder.Y + 5), Color.White, drawBorder);
            double lineModifier = 0;
            foreach (var line in PopupLines)
            {
                e.Graphics.DrawText(line, "Arial Bold", 15.0f, new PointF(drawRect.X, (float)(drawRect.Y + 35 + lineModifier)), Color.White, drawRect);
                lineModifier += _popupTextLineHeight + 2;
            }
        }
    }
}
