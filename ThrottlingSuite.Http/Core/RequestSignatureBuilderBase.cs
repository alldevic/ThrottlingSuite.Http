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

using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Http;

namespace ThrottlingSuite.Core
{
    /// <summary>
    /// An abstract class implements IRequestSignatureBuilder interface and provides basics of HTTP request signature building functionality.
    /// </summary>
    public abstract class RequestSignatureBuilderBase : IRequestSignatureBuilder
    {
        /// <summary>
        /// The configuration object used by the class.
        /// </summary>
        protected ThrottlingConfiguration Configuration;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RequestSignatureBuilderBase()
        {
        }

        /// <summary>
        /// Initializes class instance with its configuration.
        /// </summary>
        /// <param name="configuration">The configuration object.</param>
        public void Init(ThrottlingConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Computes the HTTP request signature.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>Returns a signature as a string.</returns>
        public string ComputeRequestSignature(HttpRequestMessage request)
        {
            if (Configuration == null)
                throw new Exception("Configiration must be initialized before calling this method.");

            //compute request signature
            string reqDescription = ComputeRequestDescription(request);
            return ComputeRequestSignature(reqDescription);
        }

        /// <summary>
        /// Computes the HTTP request long description.
        /// </summary>
        /// <param name="request">The HTTP request object.</param>
        /// <returns>Returns a description as a string.</returns>
        protected abstract string ComputeRequestDescription(HttpRequestMessage request);

        /// <summary>
        /// Computes the HTTP request signature from its long description.
        /// </summary>
        /// <param name="value">The HTTP request long description.</param>
        /// <returns>Returns a signature as a string.</returns>
        protected string ComputeRequestSignature(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty; //use an empty string - same meaning, less issues
            if (value.Length <= 60)
                return value.ToLower(); //use AS IS for performance reason: makes no sense to go through hashing for small strings

            MD5CryptoServiceProvider cryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            StringBuilder builder = new StringBuilder(value.Length.ToString().PadLeft(8, '0') + "-");

            bytes = cryptoServiceProvider.ComputeHash(bytes);

            foreach (byte b in bytes)
                builder.Append(b.ToString("x2").ToLower());

            return builder.ToString();
        }
    }
}
