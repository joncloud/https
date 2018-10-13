# https
## Description
`https` is a simple CLI for sending HTTP requests.

## Licensing
Released under the MIT License. See the [LICENSE][] File for further details.

[license]: LICENSE.md

## Installation
Install `https` as a global .NET tool using
```bash
dotnet tool install --global https --version 0.1.0-beta
```

## Usage
Urls without a protocol, i.e., `http://` or `https://`, by default will be assigns the `https://` protocol.

```bash
https [method] [uri] [options] [content]
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
    "User-Agent": "dotnet-https/0.1.0.0",
    "X-Content": "content-x"
  },
  "json": {
    "hello": "world"
  },
  "origin": "50.53.112.92",
  "url": "https://httpbin.org/put"
}
```
