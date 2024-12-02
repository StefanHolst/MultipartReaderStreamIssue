# Steps to Reproduce

1. Start the project
2. Upload the `123.zip` file to `http://localhost:5205/anywhere`

> Note any zip file will do, if you don't want to use the provided `123.zip` file.

## Curl
```shell
curl -X POST 'localhost:5205/anywhere' --form 'file=@./MultipartReaderStreamIssue/123.zip'
```

## HTTP
```http request
@MultipartReaderStreamIssue_HostAddress = http://localhost:5205

POST {{MultipartReaderStreamIssue_HostAddress}}/anywhere/
Accept: application/json
Content-Type: multipart/form-data; boundary=boundary

--boundary
Content-Disposition: form-data; name="file"; filename="123.zip"

< ./123.zip
--boundary
```