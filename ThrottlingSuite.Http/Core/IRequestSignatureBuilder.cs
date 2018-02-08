#region "Copyright (C) Lenny Granovsky. 2012-2014"
//
//                Copyright (C) Lenny Granovsky. 2012-2014. 
//
//   Permission is hereby granted, free of charge, to any person obtaining a copy of 
//   this software and associated documentation files (the "Software"), to deal in 
//   the Software without restriction, including without limitation the rights to 
//   use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
//   of the Software, and to permit persons to whom the Software is furnished to 
//   do so, subject to the following conditions:

//   The above copyright notice and this permission notice shall be included in 
//   all copies or substantial portions of the Software.

//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//   INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//   PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//   HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//   OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Net.Http;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// Interface that must be implemented by the class providing HTTP request signature building functionality.
    /// </summary>
    public interface IRequestSignatureBuilder
    {
        /// <summary>
        /// Initializes class instance with its configuration.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        void Init(ThrottlingConfiguration configuration);
        /// <summary>
        /// Computes the HTTP request signature.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>Returns a signature as a string.</returns>
        string ComputeRequestSignature(HttpRequestMessage request);
    }
}
