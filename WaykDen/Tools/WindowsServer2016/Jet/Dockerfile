ARG FROM_IMAGE=mcr.microsoft.com/windows/servercore:1803
FROM ${FROM_IMAGE}

LABEL maintainer "Devolutions Inc."

WORKDIR "C:\\wayk"

COPY devolutions-jet.exe .

EXPOSE 8080

ENTRYPOINT ["c:\\wayk\\devolutions-jet.exe"]