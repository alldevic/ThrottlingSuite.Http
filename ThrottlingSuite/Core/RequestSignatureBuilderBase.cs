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
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace ThrottlingSuite.Core
{
    public abstract class RequestSignatureBuilderBase : IRequestSignatureBuilder
    {
        protected ThrottlingConfiguration Configuration;

        public RequestSignatureBuilderBase()
        {
        }

        public void Init(ThrottlingConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public string ComputeRequestSignature(HttpContext context)
        {
            if (this.Configuration == null)
                throw new Exception("Configiration must be initialized before calling this method.");

            //compute request signature
            string reqDescription = this.ComputeRequestDescription(context);
            return this.ComputeRequestSignature(reqDescription);
        }

        protected abstract string ComputeRequestDescription(HttpContext context);

        protected string ComputeRequestSignature(string value)
        {
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
