﻿/// <summary>
/// OpenAPI generated class
/// </summary>
namespace WestervilleFoodBot.API
{
    using System;
    using RestSharp;

    /// <summary>
    /// A delegate to ExceptionFactory method
    /// </summary>
    /// <param name="methodName">Method name</param>
    /// <param name="response">Response</param>
    /// <returns>Exceptions</returns>
    public delegate Exception ExceptionFactory(string methodName, IRestResponse response);
}
