ARG FROM_IMAGE=mcr.microsoft.com/windows/servercore:1803
FROM ${FROM_IMAGE}
SHELL ["powershell"]

LABEL maintainer "Devolutions Inc."

WORKDIR "C:\\wayk"

COPY lucid.exe .
COPY ./src/host ./host
COPY frontend ./host/www/

EXPOSE 4242

ENTRYPOINT ["c:\\wayk\\lucid.exe"]