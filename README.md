# https
[![NuGet](https://img.shields.io/nuget/v/https.svg)](https://www.nuget.org/packages/https/)
[![Build Status](https://dev.azure.com/joncloud/joncloud-github/_apis/build/status/joncloud.https?branchName=master)](https://dev.azure.com/joncloud/joncloud-github/_build/latest?definitionId=13&branchName=master)

## Description
`https` is a simple CLI for sending HTTP requests.

## Licensing
Released under the MIT License. See the [LICENSE][] File for further details.

[license]: LICENSE.md

## Installation
Install `https` as a global .NET tool using
```bash
dotnet tool install --global https --version 0.2.0-*
```

## Usage
Urls without a protocol, i.e., `http://` or `https://`, by default will be assigns the `https://` protocol.

```bash
Usage: https <METHOD> <URI> [options] [content]

Submits HTTP requests. For example https put httpbin.org/put hello=world

Arguments:
  <METHOD>    HTTP method, i.e., get, head, post
  <URI>       URI to send the request to. Leaving the protocol off the URI defaults to https://

Options:
  --form                Renders the content arguments as application/x-www-form-urlencoded
  --help                Show command line help.
  --ignore-certificate  Prevents server certificate validation.
  --json                Renders the content arguments as application/json.
  --timeout=<VALUE>     Sets the timeout of the request using System.TimeSpan.TryParse (https://docs.microsoft.com/en-us/dotnet/api/system.timespan.parse)
  --version             Displays the application verison.
  --xml=<ROOT_NAME>     Renders the content arguments as application/xml using the optional xml root name.

Content:
Repeat as many content arguments to create content sent with the HTTP request. Alternatively pipe raw content send as the HTTP request content.
  <KEY>=<VALUE>
```

For example `https put httpbin.org/put hello=world` will output:
```
HTTP/1.1 200 OK
Connection: keep-alive
Server: gunicorn/19.9.0
Date: Sat, 13 Oct 2018 03:09:13 GMT
Access-Control-Allow-Origin: *
Access-Control-Allow-Credentials: true
Via: 1.1 vegur
Content-Type: application/json
Content-Length: 423
{
  "args": {},
  "data": "{\"hello\":\"world\"}",
  "files": {},
  "form": {},
  "headers": {
    "Connection": "close",
    "Content-Length": "17",
    "Content-Type": "application/json; charset=utf-8",
    "Host": "httpbin.org",
    "User-Agent": "dotnet-https/0.1.1.0",
    "X-Content": "content-x"
  },
  "json": {
    "hello": "world"
  },
  "origin": "50.53.112.92",
  "url": "https://httpbin.org/put"
}
```
