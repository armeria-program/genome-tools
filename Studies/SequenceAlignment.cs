﻿using System.Collections.Generic;

namespace Studies
{
    public class SequenceAlignment<T>
    {
        public SequenceAlignment(List<AlignedPair<T>> alignedPairs)
        {
            AlignedPairs = alignedPairs;
        }

        public List<AlignedPair<T>> AlignedPairs { get; }
    }
}