﻿using UAlbion.Core;

namespace UAlbion.TestCommon
{
    public class MockWindow : IWindow
    {
        public MockWindow(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
    }
}