﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsMmdDataIO.Vmd
{
    public interface IElementKeyFrame : IKeyFrame
    {
        string Name { get; set; }
    }
}
