﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public class ScreenManager
    {
        public static ScreenManager Instance = new ScreenManager();
        private ScreenManager() { }

        GraphicsDevice Device;
        ContentManager Content;
        public Point ScreenSize { get; set; }
        Stack<GameScreen> screenStack = new Stack<GameScreen>();
        public GameScreen CurrentScreen { get; private set; }
        Dictionary<string, Texture2D> TextureProvider = new Dictionary<string, Texture2D>();
        Dictionary<string, SpriteFont> FontProvider = new Dictionary<string, SpriteFont>();
        readonly static string[] texturesToLoad = 
        {
            "img/twp_pixel",
            "img/twp_circle",
            "img/twp_corner",
            "img/twp_ending_left",
            "img/twp_ending_top",
            "img/twp_hexagon",
            "img/twp_square",
            "img/twp_sun",
            "img/twp_elimination",
            "img/twp_triangle1..3"
        };
        readonly static string[] fontsToLoad =
        {
            "font/fnt_constantia12",
            "font/fnt_constantia36"
        };
        System.Text.RegularExpressions.Regex texIsMultiple = new System.Text.RegularExpressions.Regex(@"\d+\.\.\d+");
        System.Text.RegularExpressions.Regex texMultipleGetName = new System.Text.RegularExpressions.Regex(@".+(?=\d+\.\.)");

        FadeTransition transitionAnimation = new FadeTransition(15, 20, 10);
        Texture2D texPixel;

        public void AddScreen(GameScreen screen, bool replaceCurrent = false, bool doFadeAnimation = false)
        {
            if (doFadeAnimation)
            {
                Action callback = null;
                callback = () =>
                {
                    _addScreen(screen, replaceCurrent);
                    transitionAnimation.FadeOutComplete -= callback;
                };
                transitionAnimation.FadeOutComplete += callback;
                transitionAnimation.Restart();
            }
            else
                _addScreen(screen, replaceCurrent);
        }

        private void _addScreen(GameScreen screen, bool replaceCurrent)
        {
            if (replaceCurrent)
                screenStack.Pop();
            screenStack.Push(screen);
            CurrentScreen = screen;
            CurrentScreen.LoadContent(TextureProvider, FontProvider);

            if (screen is PanelGameScreen pgs)
                pgs.LoadNewPanel(PanelGenerator.GeneratePanel());
        }

        public void Initialize(GraphicsDevice device)
        {
            Device = device;
        }
        public void LoadContent(ContentManager contentManager)
        {
            Content = contentManager;

            // Load all textures from the list
            foreach (string texName in texturesToLoad)
            {
                if (!texIsMultiple.IsMatch(texName))
                    TextureProvider.Add(texName, Content.Load<Texture2D>(texName));
                // If texture is specified in the form of "name0..20", it means that we should load "name0", "name1" ... "name20"
                else
                {
                    string name = texMultipleGetName.Match(texName).Value;
                    int[] range = Array.ConvertAll(texIsMultiple.Match(texName).Value.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries), int.Parse);
                    for (int i = range[0]; i <= range[1]; i++)
                        TextureProvider.Add(name + i, Content.Load<Texture2D>(name + i));
                }
            }
            texPixel = TextureProvider["img/twp_pixel"];

            // Load all fonts from the list
            foreach (string fontName in fontsToLoad)
                FontProvider.Add(fontName, Content.Load<SpriteFont>(fontName));
        }
        public void Update(GameTime gameTime)
        {
            CurrentScreen?.Update(gameTime);
            transitionAnimation.Update();

            if (InputManager.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Enter))
                AddScreen(new PanelGameScreen(ScreenSize, Device), true, true);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            CurrentScreen?.Draw(spriteBatch);

            if (transitionAnimation.IsActive)
                spriteBatch.Draw(texPixel, new Rectangle(Point.Zero, ScreenSize), Color.Black * transitionAnimation.Opacity);
        }
    }
}
