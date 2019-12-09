FROM mcr.microsoft.com/windows/servercore:1803
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN [Net.ServicePointManager]::SecurityProtocol=[System.Net.SecurityProtocolType]::Tls12;\
	Invoke-WebRequest \
        -Uri "https://github.com/containous/traefik/releases/download/v1.7.8/traefik_windows-amd64.exe" \
        -OutFile "/traefik.exe";

EXPOSE 80
ENTRYPOINT ["/traefik"]

# Metadata
LABEL org.opencontainers.image.vendor="Containous" \
    org.opencontainers.image.url="https://traefik.io" \
    org.opencontainers.image.title="Traefik" \
    org.opencontainers.image.description="A modern reverse-proxy" \
    org.opencontainers.image.version="v2.1.0-rc3" \
    org.opencontainers.image.documentation="https://docs.traefik.io"