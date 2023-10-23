# Need certificates for interacting with Google API endpoints:
FROM alpine:latest as alpine

RUN apk add -U --no-cache ca-certificates

# Actual containers here:
FROM scratch

LABEL maintainer "tdg@google.com"

COPY --from=alpine /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/
COPY llama-linux-amd64-static-upx /
COPY vmtouch-linux-amd64-static-upx /
COPY gameprompt-server-linux-amd64-static-upx /

EXPOSE 50005
ENTRYPOINT ["/gameprompt-server-linux-amd64-static-upx"]
