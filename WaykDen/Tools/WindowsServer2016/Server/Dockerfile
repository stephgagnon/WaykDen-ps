ARG FROM_IMAGE=mcr.microsoft.com/windows/servercore:1803
FROM ${FROM_IMAGE}

LABEL maintainer "Devolutions Inc."

WORKDIR "C:\\wayk"

COPY den-server.exe .
COPY frontend ./host/www/

EXPOSE 4491
EXPOSE 443
EXPOSE 10255

ENTRYPOINT ["c:\\wayk\\den-server.exe"]