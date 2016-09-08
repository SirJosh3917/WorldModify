#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;

namespace OpenTK.Platform.MacOS {
	
	/// \internal
	/// <summary> Describes a Carbon window. </summary>
	sealed class CarbonWindowInfo : IWindowInfo {
		
		public IntPtr WindowRef;
		internal bool goFullScreenHack = false;
		internal bool goWindowedHack = false;

		public CarbonWindowInfo(IntPtr windowRef) {
			WindowRef = windowRef;
		}

		/// <summary>Returns a System.String that represents the current window.</summary>
		/// <returns>A System.String that represents the current window.</returns>
		public override string ToString() {
			return String.Format("MacOS.CarbonWindowInfo: Handle {0}", WindowRef);
		}
		
		public IntPtr WinHandle { 
			get { return WindowRef; }
		}

		public void Dispose() {
		}
	}
}