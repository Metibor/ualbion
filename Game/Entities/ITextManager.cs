﻿using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Text;

namespace UAlbion.Game.Entities
{
    public interface ITextManager
    {
        Vector2 Measure(TextBlock block);
        PositionedSpriteBatch BuildRenderable(TextBlock block, DrawLayer layer, object caller);
        IEnumerable<TextBlock> SplitBlocksToSingleWords(IEnumerable<TextBlock> blocks);
        string GetTextFromTextEvent(TextEvent textEvent);
    }
}