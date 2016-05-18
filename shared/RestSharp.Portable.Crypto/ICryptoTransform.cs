#pragma warning disable SA1005
#pragma warning disable SA1027
#pragma warning disable SA1028
#pragma warning disable SA1116
#pragma warning disable SA1119
#pragma warning disable SA1120
#pragma warning disable SA1131
#pragma warning disable SA1201
#pragma warning disable SA1202
#pragma warning disable SA1203
#pragma warning disable SA1306
#pragma warning disable SA1400
#pragma warning disable SA1401
#pragma warning disable SA1502
#pragma warning disable SA1512
#pragma warning disable SA1515
#pragma warning disable SA1600
#pragma warning disable SA1616

//
// RestSharp.Portable.Crypto ICryptoTransform interface
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace RestSharp.Portable.Crypto
{
    interface ICryptoTransform : IDisposable
    {
        bool CanReuseTransform
        {
            get;
        }

        bool CanTransformMultipleBlocks
        {
            get;
        }

        int InputBlockSize
        {
            get;
        }

        int OutputBlockSize
        {
            get;
        }

        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

        byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);
    }
}
