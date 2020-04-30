﻿//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using RuntimeCommon;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;
using Microsoft.IdentityModel.Tokens;

namespace RuntimeTests
{
    public class ValidateTokens
    {
        public static void Run(string[] args)
        {
            IdentityModelEventSource.ShowPII = true;
            var testRuns = TestConfig.SetupTestRuns(
                new List<TestExecutor>
                {
                    TokenTestExecutors.JsonWebTokenHandler_ValidateToken,
                    TokenTestExecutors.JwtSecurityTokenHandler_ValidateToken,
                    TokenTestExecutors.Saml2SecurityTokenHandler_ValidateToken,
                    TokenTestExecutors.SamlSecurityTokenHandler_ValidateToken,
                });

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {                
                Audience = TestData.Audience,
                Claims = TestData.ClaimsDictionary,
                Issuer = TestData.Issuer,
                Subject = TestData.Subject,
                SigningCredentials = TestData.RsaSigningCredentials_2048Sha256
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var jwt = jwtTokenHandler.CreateEncodedJwt(securityTokenDescriptor);
            var samlTokenHandler = new SamlSecurityTokenHandler();
            var samlToken = samlTokenHandler.CreateToken(securityTokenDescriptor);
            var saml = samlTokenHandler.WriteToken(samlToken);
            var saml2TokenHandler = new Saml2SecurityTokenHandler();
            var saml2Token = saml2TokenHandler.CreateToken(securityTokenDescriptor);
            var saml2 = saml2TokenHandler.WriteToken(saml2Token);

            var testConfig = TestConfig.ParseArgs(args);
            var tokenTestData = new TokenTestRunData
            {
                JwtSecurityTokenHandler = new JwtSecurityTokenHandler(),
                JsonWebTokenHandler = new JsonWebTokenHandler(),
                JwtToken = jwt,
                NumIterations = testConfig.NumIterations,
                Saml2Token = saml2,
                SamlToken = saml,
                SamlSecurityTokenHandler = samlTokenHandler,
                Saml2SecurityTokenHandler = saml2TokenHandler,
                TokenValidationParameters = TestData.RsaTokenValidationParameters_2048_Public
            };

            // run each test to set any static data
            foreach(var testRun in testRuns)
                testRun.TestExecutor(tokenTestData);

            var assemblyVersion = typeof(JwtSecurityTokenHandler).Assembly.GetName().Version.ToString();
#if DEBUG
            var prefix = "DEBUG";
#else
            var prefix = "RELEASE";
#endif
            testConfig.Version = $"{prefix}-{assemblyVersion}";
            var logName = $"ValidateTokens-{testConfig.Version}_{DateTime.Now.ToString("yyyy.MM.dd.hh.mm.ss")}.txt";
            var directory = testConfig.LogDirectory;
            var logFile = Path.Combine(directory, logName);
            Directory.CreateDirectory(directory);

            TestRunner.Run(testConfig, testRuns, tokenTestData);
            File.WriteAllText(logFile, testConfig.Logger.Logs);
        }
    }
}
