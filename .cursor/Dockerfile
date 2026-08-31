# Base image for the Hanekawa Cloud Agent environment.
# Ships the .NET 10 SDK (matching the projects' <TargetFramework>net10.0</TargetFramework>)
# plus the local development services the bot needs: PostgreSQL and Redis.
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble

ENV DEBIAN_FRONTEND=noninteractive

# System dependencies:
# - postgresql / postgresql-contrib: relational store the bot migrates on startup
# - redis-server: distributed cache used by the infrastructure layer
# - git / curl / ca-certificates: baseline tooling for a custom image
# - sudo: lets the non-root agent user manage the local service daemons
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        git \
        curl \
        ca-certificates \
        sudo \
        postgresql \
        postgresql-contrib \
        redis-server \
    && rm -rf /var/lib/apt/lists/*

# Run as a non-root user with passwordless sudo, matching Cloud Agent conventions.
RUN useradd --create-home --shell /bin/bash ubuntu \
    && echo 'ubuntu ALL=(ALL) NOPASSWD:ALL' > /etc/sudoers.d/ubuntu \
    && chmod 0440 /etc/sudoers.d/ubuntu

# Quieter, faster, non-interactive dotnet CLI.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

USER ubuntu
WORKDIR /home/ubuntu
